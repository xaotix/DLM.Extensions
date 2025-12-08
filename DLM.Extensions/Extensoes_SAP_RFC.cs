using DLM;
using DLM.db;
using DLM.sap;
using DLM.vars;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_SAP_RFC
    {
        public static void GetMaterialBase(this List<SAP_Material> materiais)
        {
            var pck_mats = materiais.FindAll(x => x.SAP > 100000).Quebrar(200);
            foreach (var pck in pck_mats)
            {
                var bases = DLM.SAP.MontarMateriaisExplodidos(pck.Select(x => x.SAP).ToList());
                var bases_sap = bases.FindAll(x => x.Codigo != "").ToList();
                foreach (var base_sap in bases_sap)
                {
                    var mp = pck.Find(x => x.SAP == base_sap.Pai.Int());

                    if (mp != null)
                    {
                        mp.SAP_Base = base_sap.Codigo.Int();
                    }
                }
            }
        }
        public static DLM.db.Tabela GetTabelaRFC(this IRfcFunction funcao, string tabela)
        {
            try
            {
                var rfc_tbl = funcao.GetTable(tabela);
                if(rfc_tbl != null)
                {
                    var tbl = rfc_tbl.GetTabelaRFC();
                    tbl.Nome = tabela;
                    return tbl;
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return new Tabela(tabela);
        }
        public static DLM.db.Linha GetLinhaRFC(this IRfcFunction funcao, string estrutura)
        {
            try
            {
                var value = funcao.GetStructure(estrutura);
                return value.GetLinhaRFC();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return new Linha();
        }
        public static DLM.db.Celula GetCelulaRFC(this IRfcFunction funcao, string objeto)
        {
            try
            {
                var value = funcao.GetValue(objeto);

                return new Celula(objeto, value);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            return new Celula(objeto);
        }
        private static DLM.db.Tabela GetTabelaRFC(this IRfcTable table)
        {
            var retorno = new DLM.db.Tabela();
            retorno.Nome = table.Metadata.Name;
            if (retorno.Nome == "")
            {
                retorno.Nome = table.Metadata.LineType.Name;
            }

            foreach (var linha in table.ToList())
            {
                retorno.Add(linha.GetLinhaRFC());
            }

            return retorno;

        }
        public static DLM.db.Linha GetLinhaRFC(this IRfcStructure line)
        {
            var nl = new DLM.db.Linha();
            var values = line.ToList();

            foreach (var value in values)
            {
                nl.Add(value.GetCelulaRFC());
            }

            return nl;
        }
        public static DLM.db.Celula GetCelulaRFC(this IRfcField value)
        {
            var type_cel = Celula_Tipo_Valor.NULL;
            var valor = value.GetValue();
            var valorstr = "";
            if (!valor.IsNullOrEmpty())
            {
                valorstr = valor.ToString();
                var type = value.Metadata.DataType;
                type_cel = Celula_Tipo_Valor.Texto;
                switch (type)
                {
                    case RfcDataType.BYTE:
                        type_cel = Celula_Tipo_Valor.Texto;
                        break;

                    case RfcDataType.XSTRING:
                    case RfcDataType.STRING:
                        type_cel = Celula_Tipo_Valor.Texto;
                        break;
                    case RfcDataType.CHAR:
                    case RfcDataType.NUM:
                        if (valorstr.ESoNumero())
                        {
                            type_cel = Celula_Tipo_Valor.Inteiro;
                        }
                        else
                        {
                            type_cel = Celula_Tipo_Valor.Texto;
                        }
                        break;


                    case RfcDataType.BCD:
                    case RfcDataType.FLOAT:
                    case RfcDataType.DECF16:
                    case RfcDataType.DECF34:
                        type_cel = Celula_Tipo_Valor.Decimal;
                        break;

                    case RfcDataType.INT1:
                    case RfcDataType.INT2:
                    case RfcDataType.INT4:
                    case RfcDataType.INT8:
                    case RfcDataType.DTWEEK:
                    case RfcDataType.TSECOND:
                    case RfcDataType.DTMONTH:
                    case RfcDataType.TMINUTE:
                        type_cel = Celula_Tipo_Valor.Inteiro;
                        break;

                    case RfcDataType.UTCLONG:
                    case RfcDataType.UTCSECOND:
                    case RfcDataType.UTCMINUTE:
                    case RfcDataType.DATE:
                    case RfcDataType.CDAY:
                    case RfcDataType.DTDAY:
                        type_cel = Celula_Tipo_Valor.Data;
                        break;

                    case RfcDataType.TIME:
                        type_cel = Celula_Tipo_Valor.Hora;
                        break;

                    case RfcDataType.STRUCTURE:
                    case RfcDataType.TABLE:
                    case RfcDataType.CLASS:
                    case RfcDataType.UNKNOWN:
                        break;
                }
            }
            else
            {
                valorstr = null;
            }

            var nc = new Celula(value.Metadata.Name, valorstr, type_cel);
            return nc;
        }
    }
}

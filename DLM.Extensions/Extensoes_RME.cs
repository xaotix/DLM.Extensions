using Conexoes.Macros.Escada;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conexoes
{
    public static class Extensoes_RME
    {
        public static RME GetRMDB(this RME rme)
        {
            RME _RMDB = null;
            if (rme.id_codigo > 0)
            {
                _RMDB = DBases.GetBancoRM().GetRME(rme.id_codigo);
            }
            if (_RMDB == null)
            {
                _RMDB = DBases.GetBancoRM().GetRME(rme.CODIGOFIM);
            }

            return _RMDB;
        }
        public static bool Comprimento_Pode(this RME rme, double Valor, bool setar = true)
        {
            if (!rme.VARIAVEL) { "Peça com comprimento fixo".Alerta(); return false; }
            if (rme.COMP_MAX > 0 && Valor > rme.COMP_MAX) { $"Comprimento maior que o máximo [{rme.COMP_MAX}]".Alerta(); return false; }
            if (Valor < rme.COMP_MIN) { $"Comprimento menor que o mínimo [{rme.COMP_MIN}]".Alerta(); return false; }
            if (setar)
            {
                rme.COMP = Valor;
            }
            return true;
        }
        public static List<RME> GetCorrespondentes(this RME rme)
        {
            return DBases.GetBancoRM().GetRMEs().FindAll(x => x.FAMILIA == rme.FAMILIA && x.SUBSTITUTO == "");
        }
        public static bool CORRESPONDE(this RME rme, string valor)
        {
            var cfim = rme.CODIGOFIM.ToUpper().Replace(" ", "");
            string valor2 = valor.ToUpper().Replace(" ", "").Replace("*", "");
            if (valor2 == rme.MARCA_CUSTOM.ToUpper().Replace(" ", ""))
            {
                return true;
            }
            else if (valor2 == rme.CODIGOFIM)
            {
                return true;
            }
            else if (valor2 == rme.GETCODIGOFIM(false))
            {
                return true;
            }
            else if (valor2 == rme.COD_DB)
            {
                return true;
            }
            else if (valor2 == rme.CHAVE)
            {
                return true;
            }

            foreach (var rm in rme.SUBSTITUIDAS())
            {
                if (rm.COD_DB == valor2)
                {
                    return true;
                }
            }

            return false;
        }

        public static DLM.cam.ReadCAM GetInfo(this RME rme)
        {
            DLM.cam.ReadCAM pp = new DLM.cam.ReadCAM();
            pp.Tratamento = rme.FICHA_PINTURA;
            pp.Material = string.Join(" ", rme.GetMateriais());
            pp.Data = DateTime.Now.ToShortDateString();
            pp.Descricao = rme.PERFIL;
            pp.Espessura = rme.ESP;
            pp.Largura = rme.CORTE;
            pp.Marca = rme.CODIGOFIM;
            pp.Quantidade = rme.Quantidade;
            pp.Peso = rme.PESOUNIT;

            return pp;
        }

        public static List<RME> GetIrmas(this RME rme)
        {
            if (rme.DESTINO == "RMU")
            {
                return DBases.GetBancoRM().GetRMUs().FindAll(x => x.FAMILIA == rme.FAMILIA);
            }
            else
            {
                return DBases.GetBancoRM().GetRMEs().FindAll(x => x.FAMILIA == rme.FAMILIA);
            }
        }

        public static Arquivo GetPDFPadrao(this RME rme)
        {
            var pdf1 = new Arquivo("");
            try
            {

                if (Cfg.Init.SAP_PADRONIZADOS.Exists())
                {
                    pdf1 = new Arquivo(Cfg.Init.SAP_PADRONIZADOS + rme.ZPP_DENOMIN.Replace(Cfg.Init.RM_SufixPos, rme.COD_DB) + ".PDF");
                    if (pdf1.Exists())
                    {
                        return pdf1;
                    }
                }
                if (Cfg.Init.CAMINHO_SERVICOS_TECNICOS.Exists())
                {
                    pdf1 = new Arquivo(Cfg.Init.CAMINHO_SERVICOS_TECNICOS + rme.ZPP_DENOMIN.Replace(Cfg.Init.RM_SufixPos, rme.COD_DB) + ".PDF");
                    if (pdf1.Exists())
                    {
                        return pdf1;
                    }
                }

                return pdf1;
            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
                return pdf1;
            }
        }


        public static List<RMA> Juntar(this List<RMA> Origem, bool arredondar_multiplo = true)
        {
            var retorno = new List<RMA>();
            var distinct =
                                Origem.FindAll(x => x != null).GroupBy(x => x.ToString())
                                        .Select(g => g.First())
                                        .ToList();
            foreach (RMA r in distinct)
            {
                var nrma = new RMA(r);
                var iguais = Origem.FindAll(x => x.SAP == r.SAP);
                nrma.Quantidade = iguais.Sum(x => x.Quantidade);
                if (arredondar_multiplo && nrma.Multiplo > 0)
                {
                    nrma.SetQuantidadeMultipla(nrma.Quantidade);
                }
                List<string> OBS = Origem.FindAll(x => x != null).FindAll(X => X.SAP == r.SAP).Select(x => x.OBSERVACOES).Distinct().ToList().FindAll(x => x.Replace(" ", "") != "");
                nrma.OBSERVACOES = string.Join(", ", iguais.Select(x => x.OBSERVACOES).Select(x => x.CortarString(10)).Distinct().ToList()).CortarString(50);


                retorno.Add(nrma);
            }
            return retorno;
        }

        public static List<RMT> Juntar(this List<RMT> Origem)
        {
            var retorno = new List<RMT>();
            var distinct =
                                Origem
                                             .GroupBy(x => x.ToString())
                                             .Select(g => g.First())
                                             .ToList().OrderBy(x => x.Nome).ToList();
            foreach (RMT t in distinct)
            {
                RMT N = new RMT(t, t.Bobina, t.GetLinha_RMT_User());
                N.Quantidade = Origem.FindAll(x => x.ToString() == t.ToString()).Sum(y => y.Qtd);
                retorno.Add(N);
            }
            var lista_fim = retorno.GroupBy(x => x.NomeFim).ToList();
            foreach (var pcs in lista_fim)
            {
                for (int i = 1; i < pcs.ToList().Count; i++)
                {
                    pcs.ToList()[i].SUFIX = $"{i.getLetra()}";
                }
            }
            return retorno;
        }
        public static List<RME> Juntar(this List<RME> Origem)
        {
            var retorno = new List<RME>();
            /*11.06.2021 - botei ele considerar o programa, para diferenciar peças com Nomes iguais, mas com coordenadas diferentes.*/
            var distinct = Origem.FindAll(x => x != null)
                                                .GroupBy(x => x.ToString() + "@@@@" + string.Join("|", x.Programa))
                                                .Select(g => g.First())
                                                .ToList().OrderBy(x => x.CODIGOFIM).ToList();
            foreach (RME t in distinct)
            {
                var nPeca = t.Clonar(Origem.FindAll(x => x != null).FindAll(x => x.ToString() == t.ToString()).Sum(y => y.Quantidade));
                retorno.Add(nPeca);
            }
            var lista_fim = retorno.GroupBy(x => x.CODIGOFIM).ToList();
            foreach (var pcs in lista_fim)
            {
                for (int i = 1; i < pcs.ToList().Count; i++)
                {
                    pcs.ToList()[i].SUFIX = $"_{i.getLetra()}";
                }
            }
            return retorno;
        }
    }
}

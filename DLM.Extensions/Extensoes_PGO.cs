using Conexoes;
using DLM.orc;
using DLM.orc.Peca;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLM
{
    public static class Extensoes_PGO
    {
        public static List<LT_PMP> GetPMP(this RME m)
        {
            var retorno = new List<LT_PMP>();
            foreach (var pos in m.Posicoes)
            {
                if (pos.NORMT != TAB_NORMT.PERFIL_I_SOLDADO)
                {
                    try
                    {
                        var novo = new LT_PMP(pos, m);
                        retorno.Add(novo);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return retorno;
        }
        public static List<LT_PMP> GetPMP(this RMA m)
        {
            var retorno = new List<LT_PMP>();
            var novo = new LT_PMP(m);
            retorno.Add(novo);

            return retorno;
        }
        public static List<LT_PMP> GetPMP(this RMT m)
        {
            var retorno = new List<LT_PMP>();
            var novo = new LT_PMP(m);
            retorno.Add(novo);

            return retorno;
        }
        public static List<LT_PMP> Explodir(this List<LT_PMP> materiais)
        {
            var mats = materiais.GroupBy(x => x.SAP).ToList();
            var kanbans = mats.FindAll(x => x.Key.Length == 12).ToList();
            var skanban = DLM.SAP.MontarMateriaisExplodidos(new db.Tabela(kanbans.Select(x => new DLM.db.Linha("MATERIAL", x.Key, "WERKS", "1202")).ToList()));

            var sub_bobinas = mats.FindAll(x => x.Key.Int() > 1100000 && x.Key.Int() < 9000000).ToList();
            var s_sub_bobinas = DLM.SAP.MontarMateriaisExplodidos(new db.Tabela(sub_bobinas.Select(x => new DLM.db.Linha("MATERIAL", x.Key, "WERKS", "1202")).ToList()));

            foreach (var sap in kanbans)
            {
                var sks = skanban.FindAll(x => x.Pai == sap.Key && x.Codigo != "");
                if (sks.Count > 0)
                {
                    foreach (var mat in sap)
                    {
                        mat.SAP_SubMateriais = sks;
                    }
                }
                else
                {
                    foreach (var mat in sap)
                    {
                        mat.SAP_Descricao = "Código de material não encontrado no SAP.";
                    }
                }
            }

            foreach (var sap in sub_bobinas)
            {
                var sks = s_sub_bobinas.FindAll(x => x.Pai == sap.Key && x.Codigo != "");
                if (sks.Count > 0)
                {
                    foreach (var mat in sap)
                    {
                        mat.SAP_SubMateriais = sks;
                    }
                }
                else
                {
                    foreach (var mat in sap)
                    {
                        mat.SAP_Descricao = "Código de material não encontrado no SAP.";
                    }
                }
            }

            DLM.SAP.SetInfosSAP(materiais);

            var retorno = new List<LT_PMP>();
            foreach (var material in materiais)
            {
                var ms = new List<LT_PMP>();
              
                if (material.SAP_SubMateriais.Count > 0)
                {
                    foreach (var sub in material.SAP_SubMateriais)
                    {
                        var nmat = new LT_PMP(sub, material);
                        ms.Add(nmat);
                    }
                }
                else
                {
                    ms.Add(material);
                }
                retorno.AddRange(ms);
            }
            return retorno;
        }

        public static List<LT_PMP> GetPMP(this List<PGO_Peca> pcs)
        {
            return pcs.SelectMany(x => x.GetPMP()).ToList();
        }
        public static List<LT_PMP> GetPMP(this PGO_Peca pc)
        {
            var retorno = new List<LT_PMP>();
            if (pc.ItemRM is RMA)
            {
                var p = pc.ItemRM as RMA;
                var novo = new LT_PMP(p);
                retorno.Add(novo);
            }
            else if (pc.ItemRM is RME)
            {
                var m = pc.ItemRM as RME;
                foreach (var pos in m.Posicoes)
                {
                    if (pos.NORMT != TAB_NORMT.PERFIL_I_SOLDADO)
                    {
                        try
                        {
                            var novo = new LT_PMP(pos, m);
                            retorno.Add(novo);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            else if (pc.ItemRM is RMT)
            {
                var m = pc.ItemRM as RMT;
                var novo = new LT_PMP(m);
                retorno.Add(novo);
            }
            else
            {
            }
            return retorno;
        }

        public static List<PGO_Peca> JuntarPecas(this List<Range> ranges)
        {
            var pcs = ranges.SelectMany(x => x.Pecas).ToList();
            return pcs.Juntar();
        }
        public static List<DLM.orc.PGO_Peca> Juntar(this List<DLM.orc.PGO_Peca> Pecas)
        {
            var peso_antes = Pecas.Sum(x => x.PesoTotal);


            var Retorno = new List<DLM.orc.PGO_Peca>();
            var grps = Pecas.GroupBy(x => x.Chave).ToList().Select(x => x.ToList()).ToList();
            foreach (var lista in grps)
            {
                if (lista.Count == 1)
                {
                    Retorno.Add(lista.First());
                }
                else
                {
                    var n_peca = new PGO_Peca(lista);
                    Retorno.Add(n_peca);
                }
            }

            var grp_pecas = Retorno.GroupBy(x => $"{x.Codigo}@{x.Peso_Unitario_Custom}").ToList().Select(x => x.ToList()).ToList();
            foreach (var n_peca in grp_pecas)
            {
                double saldo = 0;
                var pcs = n_peca.ToList().OrderBy(x => x.Quantidade).ToList();
                var pc1 = pcs.Last();
                if (!pc1.ForcarPeso)
                {
                    foreach (var pc in pcs)
                    {
                        var qtd = pc.Quantidade.ArredondarMultiplo(pc.Multiplo);
                        if (qtd != pc.Quantidade)
                        {
                            saldo += (pc.Quantidade - qtd);
                            pc.Quantidade = qtd;
                        }
                    }
                    if (saldo != 0)
                    {
                        var qtd_saldo = saldo.ArredondarMultiplo(pc1.Multiplo);

                    requantificar:
                        pc1.Quantidade = pc1.Quantidade + qtd_saldo;
                        if (pc1.Quantidade < 0)
                        {
                            qtd_saldo = pc1.Quantidade;
                            pc1.Quantidade = 0;
                            pcs = pcs.OrderBy(x => x.Quantidade).ToList();
                            pc1 = pcs.Last();
                            goto requantificar;
                        }
                    }
                }
                else
                {
                    var peso_total = n_peca.Sum(x => x.PesoTotal);
                    var qtd_total = n_peca.Sum(x => x.Quantidade);
                    var qtd_arred = qtd_total.ArredondarMultiplo(pc1.Multiplo);
                    if (qtd_arred == 0)
                    {
                        qtd_arred = 1;
                    }
                    var peso_un = peso_total / qtd_arred;

                    foreach (var pc in pcs)
                    {
                        var qtd = pc.Quantidade.ArredondarMultiplo(pc.Multiplo);
                        saldo += (pc.Quantidade - qtd);
                        pc.Quantidade = qtd;
                        pc.Peso_Unitario_Custom = peso_un;
                    }

                    if (saldo != 0)
                    {
                        var qtd_saldo = saldo.ArredondarMultiplo(pc1.Multiplo);

                    requantificar:
                        pc1.Quantidade = pc1.Quantidade + qtd_saldo;
                        if (pc1.Quantidade < 0)
                        {
                            qtd_saldo = pc1.Quantidade;
                            pc1.Quantidade = 0;
                            pcs = pcs.OrderBy(x => x.Quantidade).ToList();
                            pc1 = pcs.Last();
                            goto requantificar;
                        }
                    }
                }


            }



            Retorno = Retorno.OrderBy(x => x.PEP).ThenBy(x => x.Codigo).ToList();

            Retorno = Retorno.FindAll(x => x.Quantidade > 0);

            var peso_depois = Retorno.Sum(x => x.PesoTotal);

            return Retorno;
        }
        public static List<DLM.orc.PGO_PEP_FERT> Juntar(this List<DLM.orc.PGO_PEP_FERT> Lista)
        {

            return Lista.GroupBy(x => x.WERKS + "@" + x.FERT).Select(x => x.First().Clonar()).ToList();
            //var Retorno = new List<DLM.orc.PGO_PEP_FERT>();
            //foreach (var de_para in Lista)
            //{
            //    if (Retorno.Find(x => x.WERKS == de_para.WERKS && x.FERT == de_para.FERT) == null)
            //    {
            //        var novo = de_para.Clonar();
            //        var ocorrencias = Lista.FindAll(x => x.WERKS == de_para.WERKS && x.FERT == de_para.FERT);
            //        Retorno.Add(novo);
            //    }
            //}
            //return Retorno.OrderBy(x => x.ToString()).ToList();
        }
    }
}

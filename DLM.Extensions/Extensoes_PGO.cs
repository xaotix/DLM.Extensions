using Conexoes;
using DLM.orc;
using DLM.orc.Peca;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLM
{
    public static class Extensoes_PGO
    {
        public static List<LT_PMP> GetPMP(this RME obj)
        {
            var retorno = new List<LT_PMP>();
            foreach (var pos in obj.Posicoes)
            {
                if (pos.NORMT != TAB_NORMT.PERFIL_I_SOLDADO)
                {
                    try
                    {
                        var novo = new LT_PMP(pos, obj);
                        retorno.Add(novo);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return retorno;
        }
        public static List<LT_PMP> GetPMP(this RMA obj)
        {
            return new List<LT_PMP>() { new LT_PMP(obj) };
        }
        public static List<LT_PMP> GetPMP(this RMT obj)
        {
            return new List<LT_PMP>() { new LT_PMP(obj) };
        }
        public static List<LT_PMP> Explodir(this List<LT_PMP> materiais)
        {
            var mats = materiais.GroupBy(x => x.SAP).ToList();
            var kanbans = mats.FindAll(x => x.Key.LenghtStr() == 12).ToList();
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

        public static void VincularRotas(this List<PGO_Obra> lista_agrupado)
        {
            var obras_end = lista_agrupado.GroupBy(x => x.Cidade + ";" + x.Estado + ";" + x.Pais).ToList();
            obras_end = obras_end.OrderBy(x => x.Key).ToList();

            //var w = Conexoes.Utilz.Wait(obras_end.Count, $"Procurando rotas das ({obras_end.Count}) obras...");
            /*carrega todas as rotas*/
            foreach (var obra_end in obras_end)
            {
                var chave = obra_end.Key.Split(';');

                var obras = obra_end.ToList();
                var endereco = DBases.GetPGO().GetRota(chave[0], chave[1], chave[2]);

                if (endereco != null)
                {
                    if (endereco.id <= 0)
                    {

                    }
                    else
                    {
                        foreach (var ob in obras)
                        {
                            ob.SetSalvaRota(endereco);
                        }
                    }

                }
                //w.somaProgresso();
            }
            //w.Close();
        }

        public static List<LT_PMP> GetPMP(this List<PGO_Peca> pcs)
        {
            return pcs.SelectMany(x => x.GetPMP()).ToList();
        }
        public static List<LT_PMP> GetPMP(this object obj)
        {
            if (obj is RME)
            {
                return (obj as RME).GetPMP();
            }
            else if (obj is RMA)
            {
                return (obj as RMA).GetPMP();
            }
            else if (obj is RMT)
            {
                return (obj as RMT).GetPMP();
            }
            else if (obj is PGO_Peca)
            {
                return (obj as PGO_Peca).ItemRM.GetPMP();
            }
            return new List<LT_PMP>();
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
            var grps = Pecas.GroupBy(x => x.Chave).ToList();
            foreach (var lista in grps)
            {
                if (lista.Count() == 1)
                {
                    Retorno.Add(lista.First());
                }
                else
                {
                    var n_peca = new PGO_Peca(lista.ToList());
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

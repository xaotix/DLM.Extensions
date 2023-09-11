﻿using Conexoes.Macros.Escada;
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
        public static double GetPesoTotal(this List<object> objetos)
        {
            double peso = 0;

            peso += objetos.FindAll(x => x is RME).Cast<RME>().Sum(x => x.PESOTOT);
            peso += objetos.FindAll(x => x is RMA).Cast<RMA>().Sum(x => x.PESOTOT);

            return peso;
        }


        public static void GetPecas(this List<RME_Macro> Lista, out List<object> pecas)
        {
            pecas = new List<object>();


            var retorno_RMEs = new List<RME>();
            var retorno_RMAs = new List<RMA>();
            var retorno_RMUs = new List<RME>();


            var correntes_macros = Lista.FindAll(x => x.GetObjeto() is Macros.Corrente).Select(x => x.GetObjeto() as Macros.Corrente).ToList();
            var tirantes = Lista.FindAll(x => x.GetObjeto() is Macros.Tirante).Select(x => x.GetObjeto() as Macros.Tirante).ToList();
            var zenitais = Lista.FindAll(x => x.GetObjeto() is Macros.Zenital).Select(x => x.GetObjeto() as Macros.Zenital).ToList();
            var purlins = Lista.FindAll(x => x.GetObjeto() is Macros.Purlin).Select(x => x.GetObjeto() as Macros.Purlin).ToList();
            var contraventos = Lista.FindAll(x => x.GetObjeto() is Macros.Contravento).Select(x => x.GetObjeto() as Macros.Contravento).ToList();
            var medaluxes = Lista.FindAll(x => x.GetObjeto() is Macros.Medalux).Select(x => x.GetObjeto() as Macros.Medalux).ToList();
            var escadas_marinheiro = Lista.FindAll(x => x.GetObjeto() is Macros.Escada.Marinheiro).Select(y => y.GetObjeto()).Cast<Macros.Escada.Marinheiro>().ToList();


            var pacote_marinheiro = new Macros.Escada.PacoteMarinheiro(escadas_marinheiro);
            retorno_RMAs.AddRange(pacote_marinheiro.getPecas().FindAll(x => x is RMA).Cast<RMA>());
            retorno_RMEs.AddRange(pacote_marinheiro.getPecas().FindAll(x => x is RME).Cast<RME>());




            foreach (var obj in contraventos)
            {
                retorno_RMAs.AddRange(obj.getPecas().GetRMAs());
                retorno_RMEs.AddRange(obj.getPecas().GetRMEs());
            }

            foreach (var obj in medaluxes)
            {
                obj.getPecas();
                retorno_RMAs.AddRange(obj.getPecas().GetRMAs());
                retorno_RMEs.AddRange(obj.getPecas().GetRMEs());
                retorno_RMUs.AddRange(obj.getPecas().GetRMUs());
            }


            if (correntes_macros.Sum(x => x.Qtd) > 0)
            {
                var par_corrente = DBases.GetBancoRM().GetParafuso(12, 38, "GALVANIZADO");
                if (par_corrente != null)
                {
                    if (par_corrente.GetPORCA() != null && par_corrente.GetARRUELA() != null)
                    {

                        RMA prCTR = new RMA(par_corrente, correntes_macros.FindAll(X => X.Parafusos).Sum(X => X.QuantidadeParafusos), "[MACRO CORRENTES]");
                        if (prCTR.Quantidade > 0)
                        {
                            retorno_RMAs.Add(prCTR);
                            retorno_RMAs.Add(prCTR.GetPORCA());
                            retorno_RMAs.Add(prCTR.GetARRUELA());
                        }
                    }
                }
                var CrF46 = correntes_macros.FindAll(x => x.Fixacao == Fixacao.F46);
                var CrF76 = correntes_macros.FindAll(x => x.Fixacao == Fixacao.F76);
                var CrF156 = correntes_macros.FindAll(x => x.Fixacao == Fixacao.F156);
                if (CrF46.Count > 0 && CrF46.Sum(X => X.Qtd * 2) > 0)
                {
                    var supF46 = DBases.GetBancoRM().GetRMA("F46");
                    if (supF46 != null)
                    {
                        retorno_RMAs.Add(new RMA(supF46, CrF46.Sum(X => X.Qtd * 2), "[MACRO CORRENTES]"));
                    }
                }

                if (CrF76.Count > 0 && CrF76.Sum(X => X.Qtd * 2) > 0)
                {
                    var supF76 = DBases.GetBancoRM().GetRMA("F76");
                    if (supF76 != null)
                    {
                        retorno_RMAs.Add(new RMA(supF76, CrF46.Sum(X => X.Qtd * 2), "[MACRO CORRENTES]"));
                    }
                }

                if (CrF156.Count > 0 && CrF156.Sum(X => X.Qtd * 2) > 0)
                {
                    var supF156 = DBases.GetBancoRM().GetRMA("F156");
                    if (supF156 != null)
                    {
                        retorno_RMAs.Add(new RMA(supF156, CrF46.Sum(X => X.Qtd * 2), "[MACRO CORRENTES]"));
                    }
                }


               foreach(var corrente in correntes_macros)
                {
                    if (corrente.Diagonal != null)
                    {
                        if (corrente.Comprimento >= corrente.Diagonal.COMP_MIN && corrente.Comprimento <= corrente.Diagonal.COMP_MAX)
                        {
                            var rme = corrente.Diagonal.Clonar();
                            rme.COMP = corrente.Comprimento;
                            rme.OBSERVACOES = "[MACRO]" + corrente.Observacoes;
                            rme.FICHA_PINTURA = corrente.Tratamento;
                            rme.Quantidade = corrente.Qtd;
                            retorno_RMEs.Add(rme);
                        }
                    }
                }
            }



            foreach (var purlin in purlins)
            {
                var pc = purlin.GetPeca();
                if (pc != null)
                {
                    var nova = pc.Clonar(purlin.Quantidade, purlin.Comprimento);
                    nova.User = purlin.User;
                    nova.FICHA_PINTURA = purlin.Pintura;
                    nova.Quantidade = purlin.Quantidade;
                    nova.PREFIX = purlin.Sequencia.ToString().PadLeft(3, '0');
                    if (purlin.Material != nova.MATERIAL)
                    {
                        nova.TIPO_ACO_CUSTOM = purlin.Material;
                    }

                    nova.OBSERVACOES = purlin.Observacoes;
                    nova.TIPO_ACO_CUSTOM = purlin.Material;

                    var linhas = nova.Linhas;

                    if (linhas.Count == 2)
                    {
                        nova.PDF_CUSTOM = true;

                        int furos = purlin.GetFurosVista(false).Count;

                        if (nova.COMP <= purlin.Comp_Min)
                        {
                            linhas[0].MAKTX = "PERFIL DOBRADO";
                            linhas[1].MAKTX = purlin.Perfil_Dobrado;
                            linhas[1].NORMT = TAB_NORMT.PERFIL_DOBRADO;
                            linhas[1].ZPP_CORTE = purlin.Corte;
                            linhas[1].QTD_FUROS_Custom = furos;
                        }

                        linhas[1].ZPP_TIPOACO = purlin.Material;
                        retorno_RMEs.Add(nova);
                    }
                }
            }



            if(tirantes.Sum(x=>x.Qtd)>0)
            {
                //TIRANTES
                int qtdSFT = tirantes.Sum(x => x.Qtd * 2);
                if (qtdSFT > 0)
                {
                    var SFT = DBases.GetBancoRM().GetRMA("SFT01");
                    var POR = DBases.GetBancoRM().GetPorca("3/8", "GALVANIZADO");
                    var ARR = DBases.GetBancoRM().GetArruela("3/8", "GALVANIZADO");
                    if (SFT != null)
                    {
                        retorno_RMAs.Add(new RMA(SFT, qtdSFT, "[MACRO TIRANTES]"));
                    }
                    if (POR != null && ARR != null)
                    {
                        retorno_RMAs.Add(new RMA(POR, qtdSFT, "[MACRO TIRANTES]"));
                        retorno_RMAs.Add(new RMA(ARR, qtdSFT, "[MACRO TIRANTES]"));
                    }
                }

                var tr_comps = tirantes.Select(x => x.Comprimento).Distinct().ToList();
                var nTR = DBases.GetBancoRM().GetRME($"03TR{Cfg.Init.RM_SufixComp}");

                if (nTR != null)
                {
                    foreach (var tr in tr_comps)
                    {
                        var corr = tirantes.FindAll(x => x.Comprimento == tr);
                        var tr_trats = tirantes.Select(x => x.Tratamento).Distinct().ToList();
                        foreach (var trat in tr_trats)
                        {
                            if (tr <= nTR.COMP_MAX && tr >= nTR.COMP_MIN)
                            {
                                var novo = nTR.Clonar(corr.FindAll(x => x.Tratamento == trat).Sum(x => x.Qtd), tr);
                                novo.FICHA_PINTURA = trat;
                                novo.User = Global.UsuarioAtual;
                                retorno_RMEs.Add(novo);
                            }
                        }
                    }
                }
            }
            if (zenitais.Sum(x => x.Qtd) > 0)
            {
                double qtd_MASTIC = zenitais.FindAll(x => x.Considerar_Selante_e_Mastic).Sum(x => x.Qtd * DBases.GetBancoRM().ZENITAL_MASTIC);
                if (qtd_MASTIC > 0)
                {
                    qtd_MASTIC = Math.Round(qtd_MASTIC / DBases.GetBancoRM().ZENITAL_MASTIC_RENDIMENTO);
                }
                double qtd_SELANTE = zenitais.FindAll(x => x.Considerar_Selante_e_Mastic).Sum(x => x.Qtd * DBases.GetBancoRM().ZENITAL_SELANTE);
                if (qtd_SELANTE > 0)
                {
                    qtd_SELANTE = Math.Round(qtd_MASTIC / DBases.GetBancoRM().ZENITAL_SELANTE_RENDIMENTO);
                }

                int qtd_REBITE = zenitais.Sum(x => x.Qtd_Rebite);

                if (qtd_MASTIC > 0)
                {
                    var parMastic = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_MASTIC_CODIGO);
                    if (parMastic != null)
                    {
                        retorno_RMAs.Add(new RMA(parMastic, qtd_MASTIC, "[MACRO ZENITAL]"));
                    }
                }

                if (qtd_SELANTE > 0)
                {
                    var parSelante = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_SELANTE_CODIGO);
                    if (parSelante != null)
                    {
                        retorno_RMAs.Add(new RMA(parSelante, qtd_SELANTE, "[MACRO ZENITAL]"));
                    }
                }

                if (qtd_REBITE > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_REBITE_CODIGO);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_REBITE, "[MACRO ZENITAL]"));
                    }
                }

                int qtd_PARAFUSO_CABECA_INOX = zenitais.FindAll(x => x.Tipo_Parafuso == Tipo_Parafuso.Cabeça_Inox && x.Considerar_Parafuso).Sum(x => x.Qtd_Parafusos);
                int qtd_PARAFUSO_NORMAL = zenitais.FindAll(x => x.Tipo_Parafuso == Tipo_Parafuso.Normal && x.Considerar_Parafuso).Sum(x => x.Qtd_Parafusos);
                int qtd_PARAFUSO_TODO_INOX = zenitais.FindAll(x => x.Tipo_Parafuso == Tipo_Parafuso.Todo_Inox && x.Considerar_Parafuso).Sum(x => x.Qtd_Parafusos);
                if (qtd_PARAFUSO_CABECA_INOX > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_PARAFUSOS_CODIGO_CABECA_INOX);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_CABECA_INOX, "[MACRO ZENITAL]"));
                    }
                }
                if (qtd_PARAFUSO_NORMAL > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_PARAFUSOS_CODIGO_NORMAL);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_NORMAL, "[MACRO ZENITAL]"));
                    }
                }
                if (qtd_PARAFUSO_TODO_INOX > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_PARAFUSOS_CODIGO_TODO_INOX);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_TODO_INOX, "[MACRO ZENITAL]"));
                    }
                }

                var ZENITAL = DBases.GetBancoRM().GetRMU(DBases.GetBancoRM().ZENITAL_CODIGO);
                var PS4 = DBases.GetBancoRM().GetRMU(DBases.GetBancoRM().ZENITAL_PS4_CODIGO);
                if (ZENITAL != null)
                {
                    var T = new RME(ZENITAL)
                    {
                        User = Global.UsuarioAtual,
                        Quantidade = zenitais.Sum(X => X.Qtd),
                        OBSERVACOES = "[MACRO ZENITAL]"

                    };
                    retorno_RMUs.Add(T);
                }
                if (PS4 != null)
                {
                    var T = new RME()
                    {
                        User = Global.UsuarioAtual,
                        Quantidade = (zenitais.Sum(X => X.Qtd) * DBases.GetBancoRM().ZENITAL_PS4).Int(),
                        OBSERVACOES = "[MACRO ZENITAL]"

                    };
                    retorno_RMUs.Add(T);
                }

                int qtd_Az1 = zenitais.Sum(x => x.Qtd_AZ1);
                int qtd_Az2 = zenitais.Sum(x => x.Qtd_AZ2);
                int qtd_Az4_1840 = zenitais.Sum(x => x.Qtd_AZ4_1840);
                int qtd_Az4_620 = zenitais.Sum(x => x.Qtd_AZ4_620);

                if (qtd_Az1 > 0)
                {
                    var pc = DBases.GetBancoRM().GetRMU("AZ1");
                    if (pc != null)
                    {
                        var pcs = zenitais.FindAll(x => x.Qtd_AZ1 > 0 && x.Inverter_Cores);
                        var pcs2 = zenitais.FindAll(x => x.Qtd_AZ1 > 0 && !x.Inverter_Cores);
                        var bobinas1 = pcs.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        var bobinas2 = pcs2.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        foreach (var bob in bobinas1)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ1));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ1));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                    }
                }

                if (qtd_Az2 > 0)
                {
                    var pc = DBases.GetBancoRM().GetRMU("AZ2");
                    if (pc != null)
                    {
                        var pcs = zenitais.FindAll(x => x.Qtd_AZ2 > 0 && x.Inverter_Cores);
                        var pcs2 = zenitais.FindAll(x => x.Qtd_AZ2 > 0 && !x.Inverter_Cores);
                        var bobinas1 = pcs.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        var bobinas2 = pcs2.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        foreach (var bob in bobinas1)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ2));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ2));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                    }
                }
                if (qtd_Az4_1840 > 0)
                {
                    var pc = DBases.GetBancoRM().GetRMU("AZ4-1840P");
                    if (pc != null)
                    {
                        var pcs = zenitais.FindAll(x => x.Qtd_AZ4_1840 > 0 && x.Inverter_Cores);
                        var pcs2 = zenitais.FindAll(x => x.Qtd_AZ4_1840 > 0 && !x.Inverter_Cores);
                        var bobinas1 = pcs.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        var bobinas2 = pcs2.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        foreach (var bob in bobinas1)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_1840));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_1840));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                    }
                }
                if (qtd_Az4_620 > 0)
                {
                    var pc = DBases.GetBancoRM().GetRMU("AZ4-620P");
                    if (pc != null)
                    {
                        var pcs = zenitais.FindAll(x => x.Qtd_AZ4_620 > 0 && x.Inverter_Cores);
                        var pcs2 = zenitais.FindAll(x => x.Qtd_AZ4_620 > 0 && !x.Inverter_Cores);
                        var bobinas1 = pcs.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        var bobinas2 = pcs2.Select(x => x.Bobina).ToList().GroupBy(x => x.ToString()).Select(g => g.First()).ToList();
                        foreach (var bob in bobinas1)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_620));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_620));
                            npc.OBSERVACOES = "[MACRO ZENITAL]";
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                    }
                }
            }

            pecas.AddRange(retorno_RMAs);
            pecas.AddRange(retorno_RMEs);
            pecas.AddRange(retorno_RMUs);

        }


        public static List<RMA> GetRMAs(this List<object> lista)
        {
            return lista.FindAll(x => x is RMA).Cast<RMA>().ToList();
        }
        public static List<RME> GetRMUs(this List<object> lista)
        {
            return lista.FindAll(x => x is RME).Cast<RME>().ToList().FindAll(x => x.DESTINO == "RMU").ToList();
        }
        public static List<RME> GetRMEs(this List<object> lista)
        {
            return lista.FindAll(x=>x is RME).Cast<RME>().ToList().FindAll(x => x.DESTINO == "RME").ToList();
        }
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

using Conexoes.Macros.Escada;
using DLM.encoder;
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
        public static void SincronizarDB(this List<RME> lista)
        {
            foreach (var rme in lista)
            {
                rme.Carregar_Codigo(false);
                rme.SincronizarDB();
            }
        }
        public static double GetPesoTotal(this List<object> objetos)
        {
            double peso = 0;

            peso += objetos.Get<RME>().Sum(x => x.PESOTOT);
            peso += objetos.Get<RMA>().Sum(x => x.PESOTOT);

            return peso;
        }
        public static string Tratar(this string valor, RME RME)
        {
            if (RME == null) { return valor; }

            string retorno = valor
                                .Replace(Cfg.Init.RM_SufixFicha, RME.FICHA_PINTURA)
                                .Replace(Cfg.Init.RM_SufixMaterial, RME.MAT)
                                .Replace(Cfg.Init.RM_SufixPeso, RME.PESOUNIT.String(Cfg.Init.DECIMAIS_Peso))
                                .Replace(Cfg.Init.RM_SufixComp, RME.COMP.String(0, RME.QTDCARACTCOMP))
                                .Replace(Cfg.Init.RM_SufixEsp, RME.ESP.String())
                                .Replace(Cfg.Init.RM_SufixLarg, RME.LARG.String())
                                .Replace(Cfg.Init.RM_SufixAba, RME.ABA.String())
                                .Replace(Cfg.Init.RM_SufixAba2, RME.ABA2.String())
                                .Replace(Cfg.Init.RM_SufixAba3, RME.ABA3.String())
                                .Replace(Cfg.Init.RM_SufixAbaEspecial, RME.ABA_ESPECIAL.String())

                                .Replace(Cfg.Init.RM_SufixSecao, RME.SECAO.String())
                                .Replace(Cfg.Init.RM_SufixCorte, RME.CORTE.String())

                                .Replace(Cfg.Init.RM_SufixFuracoes, RME.QTD_FUROS.ToString())

                                .Replace(Cfg.Init.RM_SufixPos, RME.CODIGOFIM)
                                .Replace(Cfg.Init.RM_SufixQtd, RME.Quantidade.ToString());


            return retorno;
        }

        public static void GetPecas(this List<RME_Macro> Lista, out List<object> _pecas)
        {
            _pecas = new List<object>();


            var retorno_RMEs = new List<RME>();
            var retorno_RMAs = new List<RMA>();
            var retorno_RMUs = new List<RME>();


            var objetos = Lista.Select(x => x.GetObjeto()).ToList();
            var correntes = objetos.Get<Macros.Corrente>();
            var tirantes = objetos.Get<Macros.Tirante>();
            var zenitais = objetos.Get<Macros.Zenital>();
            var purlins = objetos.Get<Macros.Purlin>();
            var ctv1 = objetos.Get<Macros.Contravento>();
            var ctv2 = objetos.Get<DLM.macros.CTV2>();
            var medaluxes = objetos.Get<Macros.Medalux>(); 
            var escadasEM1 = objetos.Get<Marinheiro>(); 
            var pacoteEM1 = new PacoteMarinheiro(escadasEM1);
            var escadasEM2 = objetos.Get<DLM.macros.EM2>();
            var pacoteEM2 = new DLM.macros.EM2Pacote(escadasEM2);

            retorno_RMAs.AddRange(pacoteEM1.getPecas().Get<RMA>());
            retorno_RMEs.AddRange(pacoteEM1.getPecas().Get<RME>());


            var txt_macro_medalux = "[MACRO ZENITAL]";
            var txt_macro_corrente = "[MACRO CORRENTE]";
            var txt_macro_tirante = "[MACRO TIRANTES]";
            var txt_macro_em2 = "[MACRO EM2]";

            var pcsEM2 = pacoteEM2.Pecas.GroupBy(x => x.Nome_Padronizado).OrderBy(x=>x.Key).ToList();
            foreach(var pc in pcsEM2)
            {
                var igual = DBases.GetBancoRM().GetPeca(pc.Key);
                if(igual!=null)
                {
                    if(igual is RME)
                    {
                        foreach(var p1 in pc.ToList())
                        {
                            var nrm = igual.As<RME>().Clonar(p1.Qtd, p1.Comp, p1.Nome);
                            nrm.OBSERVACOES = txt_macro_em2;
                            nrm.FICHA_PINTURA = p1.Tratamento;
                            retorno_RMEs.Add(nrm);
                        }
                    }
                    else if(igual is RMA)
                    {
                        var pecas = pc.ToList();
                        var qtd = pecas.Sum(x => x.Qtd);

                        var PAR = igual.As<RMA>().Clonar(qtd, txt_macro_em2);
                        retorno_RMAs.Add(PAR);

                        if (PAR.TIPO == "PAR")
                        {
                            var POR = PAR.GetPOR().Clonar(qtd, txt_macro_em2);
                            if (POR != null)
                            {
                                retorno_RMAs.Add(POR);
                            }
                            else
                            {
                                _pecas.Add(new Report("Peça Não encontrada", $"EM2 => Porca para o parafuso: {pc.Key}", TipoReport.Critico));
                            }

                            var ARR = PAR.GetARR().Clonar(qtd, txt_macro_em2);
                            if (ARR != null)
                            {
                                retorno_RMAs.Add(ARR);
                            }
                            else
                            {
                                _pecas.Add(new Report("Peça Não encontrada", $"EM2 => Arruela para o parafuso: {pc.Key}", TipoReport.Critico));
                            }
                        }


                    }
                    else
                    {

                    }
                }
                else
                {
                    _pecas.Add(new Report("Peça Não encontrada", $"EM2 => {pc.Key}", TipoReport.Critico));
                }
            }


            foreach (var obj in ctv1)
            {
                retorno_RMAs.AddRange(obj.getPecas().GetRMAs());
                retorno_RMEs.AddRange(obj.getPecas().GetRMEs());
            }

            foreach (var purlin in purlins)
            {
                var pcPurlin = purlin.getPeca();
                if (pcPurlin != null)
                {
                    var nova = pcPurlin.Clonar(purlin.Quantidade, purlin.Comprimento);
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
                else
                {
                    _pecas.Add(new Report($"Purlin, id_peca={purlin.id_peca}", "Peça Não encontrada", TipoReport.Critico));
                }
            }

            foreach (var obj in medaluxes)
            {
                retorno_RMAs.AddRange(obj.getPecas().GetRMAs());
                retorno_RMEs.AddRange(obj.getPecas().GetRMEs());
                retorno_RMUs.AddRange(obj.getPecas().GetRMUs());
            }


            if (correntes.Sum(x => x.Quantidade) > 0)
            {
                var par_corrente = DBases.GetBancoRM().GetParafuso(12, 38, "GALVANIZADO");
                if (par_corrente != null)
                {
                    if (par_corrente.GetPOR() != null && par_corrente.GetARR() != null)
                    {
                        var prCTR = new RMA(par_corrente, correntes.FindAll(x => x.Parafusos).Sum(x => 4 * x.Quantidade), txt_macro_corrente);
                        prCTR.OBSERVACOES = txt_macro_corrente;
                        if (prCTR.Quantidade > 0)
                        {
                            retorno_RMAs.Add(prCTR);
                            retorno_RMAs.Add(prCTR.GetPOR().Clonar(prCTR.Quantidade));
                            retorno_RMAs.Add(prCTR.GetARR().Clonar(prCTR.Quantidade));
                        }
                    }
                }
                var CrF46 = correntes.FindAll(x => x.Fixacao == Corrente_Fixacao.F46);
                var CrF76 = correntes.FindAll(x => x.Fixacao == Corrente_Fixacao.F76);
                var CrF156 = correntes.FindAll(x => x.Fixacao == Corrente_Fixacao.F156);

                var qtdF46 = CrF46.Sum(X => X.Quantidade * 2);
                var qtdF76 = CrF76.Sum(X => X.Quantidade * 2);
                var qtdF156 = CrF156.Sum(X => X.Quantidade * 2);

                if (CrF46.Count > 0 && qtdF46 > 0)
                {
                    var supF46 = DBases.GetBancoRM().GetRME("F46");
                    if (supF46 != null)
                    {
                        supF46.Quantidade = qtdF46;
                        supF46.OBSERVACOES = txt_macro_corrente;
                        retorno_RMEs.Add(supF46);
                    }
                    else
                    {
                        _pecas.Add(new Report("Peça não encontrada", $"Macro Correntes => F46", TipoReport.Critico));
                    }
                }

                if (CrF76.Count > 0 && qtdF76 > 0)
                {
                    var supF76 = DBases.GetBancoRM().GetRME("F76");
                    if (supF76 != null)
                    {
                        supF76.Quantidade = qtdF76;
                        supF76.OBSERVACOES = txt_macro_corrente;
                        retorno_RMEs.Add(supF76);
                    }
                    else
                    {
                        _pecas.Add(new Report("Peça não encontrada", $"Macro Correntes => F76", TipoReport.Critico));
                    }
                }

                if (CrF156.Count > 0 && qtdF156 > 0)
                {
                    var supF156 = DBases.GetBancoRM().GetRME("F156");
                    if (supF156 != null)
                    {
                        supF156.Quantidade = qtdF156;
                        supF156.OBSERVACOES = txt_macro_corrente;
                        retorno_RMEs.Add(supF156);
                    }
                    else
                    {
                        _pecas.Add(new Report("Peça não encontrada", $"Macro Correntes => F156", TipoReport.Critico));
                    }
                }


               foreach(var corrente in correntes)
                {
                    if (corrente.GetDiagonal() != null)
                    {
                        if (corrente.CompCorrente >= corrente.GetDiagonal().COMP_MIN && corrente.CompCorrente <= corrente.GetDiagonal().COMP_MAX)
                        {
                            var rme = corrente.GetDiagonal().Clonar();
                            rme.COMP = corrente.CompCorrente;
                            rme.OBSERVACOES = txt_macro_corrente;
                            rme.FICHA_PINTURA = corrente.Tratamento;
                            rme.Quantidade = corrente.Quantidade;
                            retorno_RMEs.Add(rme);
                        }
                        else
                        {
                            _pecas.Add(new Report("Comprimento inválido", $"Vão digitado é maior ou menor que possível para a diagonal: {corrente}",TipoReport.Critico));
                        }
                    }
                }
            }

            if(tirantes.Sum(x=>x.Quantidade)>0)
            {
                var tipos_sft = new List<string>();
                tipos_sft.AddRange(tirantes.Select(x => x.Fixacao_1));
                tipos_sft.AddRange(tirantes.Select(x => x.Fixacao_2));
                tipos_sft = tipos_sft.Distinct().ToList();

                foreach(var sftNome in tipos_sft)
                {
                    var qtdSFT = tirantes.FindAll(x => x.Fixacao_1 == sftNome).Sum(x => x.Quantidade) + tirantes.FindAll(x => x.Fixacao_2 == sftNome).Sum(x => x.Quantidade);
                    if(qtdSFT>0)
                    {
                        var SFT = DBases.GetBancoRM().GetRME(sftNome);
                        if (SFT != null)
                        {
                            SFT.Quantidade = qtdSFT;
                            SFT.OBSERVACOES = txt_macro_tirante;
                            retorno_RMEs.Add(SFT);
                        }
                        else
                        {
                            _pecas.Add(new Report("Peça não encontrada", $"Macro Tirantes => {sftNome}", TipoReport.Critico));
                        }
                    }
                }

                //TIRANTES
                int qtdSftPorcas = tirantes.Sum(x => x.Quantidade * 2);
                if (qtdSftPorcas > 0)
                {
                   
                    var POR = DBases.GetBancoRM().GetPorca("3/8", "GALVANIZADO");
                    var ARR = DBases.GetBancoRM().GetArruela("3/8", "GALVANIZADO");
                    
                    if (POR != null)
                    {
                        retorno_RMAs.Add(new RMA(POR, qtdSftPorcas, txt_macro_tirante));
                    }
                    else
                    {
                        _pecas.Add(new Report("Peça não encontrada", $"Macro Tirantes => {POR}", TipoReport.Critico));
                    }

                    if (ARR!=null)
                    {
                        retorno_RMAs.Add(new RMA(ARR, qtdSftPorcas, txt_macro_tirante));
                    }
                    else
                    {
                        _pecas.Add(new Report("Peça não encontrada", $"Macro Tirantes => {ARR}", TipoReport.Critico));
                    }
                }

                var tr_comps = tirantes.Select(x => x.Comprimento).Distinct().ToList();
                var nTR = DBases.GetBancoRM().GetRME($"03TR{Cfg.Init.RM_SufixComp}");

                if (nTR != null)
                {
                    foreach (var tr_comp in tr_comps)
                    {
                        var corr = tirantes.FindAll(x => x.Comprimento == tr_comp);
                        var tr_trats = tirantes.Select(x => x.Tratamento).Distinct().ToList();
                        foreach (var trat in tr_trats)
                        {
                            if (tr_comp <= nTR.COMP_MAX && tr_comp >= nTR.COMP_MIN)
                            {
                                var novo = nTR.Clonar(corr.FindAll(x => x.Tratamento == trat).Sum(x => x.Quantidade), tr_comp);
                                novo.FICHA_PINTURA = trat;
                                novo.User = Global.UsuarioAtual;
                                retorno_RMEs.Add(novo);
                            }
                            else
                            {
                                _pecas.Add(new Report("Comprimento inválido", $"Vão digitado é maior ou menor que possível para o Tirante: {tr_comp}", TipoReport.Critico));

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
                        retorno_RMAs.Add(new RMA(parMastic, qtd_MASTIC, txt_macro_medalux));
                    }
                }

                if (qtd_SELANTE > 0)
                {
                    var parSelante = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_SELANTE_CODIGO);
                    if (parSelante != null)
                    {
                        retorno_RMAs.Add(new RMA(parSelante, qtd_SELANTE, txt_macro_medalux));
                    }
                }

                if (qtd_REBITE > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_REBITE_CODIGO);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_REBITE, txt_macro_medalux));
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
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_CABECA_INOX, txt_macro_medalux));
                    }
                }
                if (qtd_PARAFUSO_NORMAL > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_PARAFUSOS_CODIGO_NORMAL);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_NORMAL, txt_macro_medalux));
                    }
                }
                if (qtd_PARAFUSO_TODO_INOX > 0)
                {
                    var par = DBases.GetBancoRM().GetRMA(DBases.GetBancoRM().ZENITAL_PARAFUSOS_CODIGO_TODO_INOX);
                    if (par != null)
                    {
                        retorno_RMAs.Add(new RMA(par, qtd_PARAFUSO_TODO_INOX, txt_macro_medalux));
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
                        OBSERVACOES = txt_macro_medalux
                    };
                    retorno_RMUs.Add(T);
                }
                if (PS4 != null)
                {
                    var T = new RME()
                    {
                        User = Global.UsuarioAtual,
                        Quantidade = (zenitais.Sum(X => X.Qtd) * DBases.GetBancoRM().ZENITAL_PS4).Int(),
                        OBSERVACOES = txt_macro_medalux

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
                            npc.OBSERVACOES = txt_macro_medalux;
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ1));
                            npc.OBSERVACOES = txt_macro_medalux;
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
                            npc.OBSERVACOES = txt_macro_medalux;
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ2));
                            npc.OBSERVACOES = txt_macro_medalux;
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
                            npc.OBSERVACOES = txt_macro_medalux;
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_1840));
                            npc.OBSERVACOES = txt_macro_medalux;
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
                            npc.OBSERVACOES = txt_macro_medalux;
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                        foreach (var bob in bobinas2)
                        {
                            var npc = pc.Clonar(pcs.Sum(x => x.Qtd_AZ4_620));
                            npc.OBSERVACOES = txt_macro_medalux;
                            npc.Inverter_Cor = true;
                            npc.SetBobina(bob);
                            retorno_RMUs.Add(npc);
                        }
                    }
                }
            }

            _pecas.AddRange(retorno_RMAs);
            _pecas.AddRange(retorno_RMEs);
            _pecas.AddRange(retorno_RMUs);

        }


        public static List<RMA> GetRMAs(this List<object> lista)
        {
            return lista.Get<RMA>();
        }
        public static List<RME> GetRMUs(this List<object> lista)
        {
            return lista.Get<RME>().FindAll(x => x.DESTINO == "RMU").ToList();
        }
        public static List<RME> GetRMEs(this List<object> lista)
        {
            return lista.Get<RME>().FindAll(x => x.DESTINO == "RME").ToList();
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
                return DBases.GetBancoRM().GetRMUs().FindAll(x => x.FAMILIA == rme.FAMILIA && x.VARIAVEL == rme.VARIAVEL);
            }
            else
            {
                return DBases.GetBancoRM().GetRMEs().FindAll(x => x.FAMILIA == rme.FAMILIA && x.VARIAVEL == rme.VARIAVEL);
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
            foreach (RMA rm in distinct)
            {
                var nrma = new RMA(rm);
                var iguais = Origem.FindAll(x => x.SAP == rm.SAP);
                nrma.Quantidade = iguais.Sum(x => x.Quantidade);
                if (arredondar_multiplo && nrma.Multiplo > 0)
                {
                    nrma.SetQuantidadeMultipla(nrma.Quantidade);
                }
                List<string> OBS = Origem.FindAll(x => x != null).FindAll(X => X.SAP == rm.SAP).Select(x => x.OBSERVACOES).Distinct().ToList().FindAll(x => x.Replace(" ", "") != "");
                nrma.OBSERVACOES = string.Join(", ", iguais.Select(x => x.OBSERVACOES).Distinct().ToList()).CortarString(50);


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
                N.Quantidade = Origem.FindAll(x => x.ToString() == t.ToString()).Sum(y => y.Quantidade);
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
            foreach (RME rm in distinct)
            {
                var nPeca = rm.Clonar(Origem.FindAll(x => x != null).FindAll(x => x.ToString() == rm.ToString()).Sum(y => y.Quantidade));
                nPeca.OBSERVACOES = rm.OBSERVACOES;
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

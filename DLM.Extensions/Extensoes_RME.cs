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
        public static void SincronizarDB(this List<RME> _lista)
        {
            var erros = new List<Report>();
            foreach (var rme in _lista)
            {
                if (rme.id_db <= 0)
                    rme.Carregar_Codigo(false);
            }

            var tipos = _lista.GroupBy(x => x.id_db).ToList();
            foreach (var tipo in tipos)
            {
                var igual = Conexoes.DBases.GetBancoRM().GetRM(tipo.Key);
                if (igual != null)
                {
                    foreach (var pc in tipo.ToList())
                    {
                        pc.SincronizarDB(igual);
                    }
                }
                else
                {
                    erros.Add(tipo.First().ToString(), "Peça não encontrada no banco de dados");
                }
            }
        }
        public static void SincronizarDB(this List<RMT> _lista)
        {
            var erros = new List<Report>();

            var tipos = _lista.GroupBy(x => x.id_db).ToList();
            foreach (var tipo in tipos)
            {
                var igual = Conexoes.DBases.GetBancoRM().GetRMT(tipo.Key);
                if (igual != null)
                {
                    foreach (var pc in tipo.ToList())
                    {
                        pc.SincronizarDB(igual);
                    }
                }
                else
                {
                    erros.Add(tipo.First().ToString(), "Peça não encontrada no banco de dados");
                }
            }
        }
        public static void SincronizarDB(this List<RMA> _lista)
        {
            var erros = new List<Report>();

            var tipos = _lista.GroupBy(x => x.id_db).ToList();
            foreach (var tipo in tipos)
            {
                var igual = Conexoes.DBases.GetBancoRM().GetRMA(tipo.Key);
                if (igual != null)
                {
                    foreach (var pc in tipo.ToList())
                    {
                        pc.SincronizarDB(igual);
                    }
                }
                else
                {
                    erros.Add(tipo.First().ToString(), "Peça não encontrada no banco de dados");
                }
            }
        }
        public static double GetPesoTotal(this List<object> _objetos)
        {
            double peso = 0;

            peso += _objetos.Get<RME>().Sum(x => x.PESOTOT);
            peso += _objetos.Get<RMA>().Sum(x => x.PESOTOT);

            return peso;
        }
        public static string Tratar(this string _valor, RME _rme)
        {
            if (_rme == null) { return _valor; }

            string retorno = _valor
                                .Replace(Cfg.Init.RM_SufixFicha, _rme.FICHA_PINTURA)
                                .Replace(Cfg.Init.RM_SufixMaterial, _rme.MATERIAL)
                                .Replace(Cfg.Init.RM_SufixPeso, _rme.PESOUNIT.String(Cfg.Init.DECIMAIS_Peso))
                                .Replace(Cfg.Init.RM_SufixComp, _rme.COMP_USER.String(0, _rme.QTDCARACTCOMP))
                                .Replace(Cfg.Init.RM_SufixEsp, _rme.ESP.String(2))
                                .Replace(Cfg.Init.RM_SufixLarg, _rme.LARG.String(2))
                                .Replace(Cfg.Init.RM_SufixAba, _rme.ABA.String(2))
                                .Replace(Cfg.Init.RM_SufixAba2, _rme.ABA2.String(2))
                                .Replace(Cfg.Init.RM_SufixAba3, _rme.ABA3.String(2))
                                .Replace(Cfg.Init.RM_SufixAbaEspecial, _rme.ABA_ESPECIAL.String(2))

                                .Replace(Cfg.Init.RM_SufixSecao, _rme.SECAO.String(2))
                                .Replace(Cfg.Init.RM_SufixCorte, _rme.CORTE.String(2))

                                .Replace(Cfg.Init.RM_SufixFuracoes, _rme.QTD_FUROS.ToString())

                                .Replace(Cfg.Init.RM_SufixPos, _rme.CODIGOFIM)
                                .Replace(Cfg.Init.RM_SufixQtd, _rme.Quantidade.ToString());


            return retorno;
        }

        public static List<object> GetPecas(this List<RME_Macro> _lista, out List<RME> retorno_purlins)
        {

            var retorno = new List<object>();
            retorno_purlins = new List<RME>();


            var objetos = _lista.Select(x => x.GetObjeto()).ToList();
            var correntes = objetos.Get<DLM.macros.Corrente>();
            var tirantes = objetos.Get<DLM.macros.Tirante>();
            var purlins = objetos.Get<Macros.Purlin>();
            var ctv1 = objetos.Get<Macros.Contravento>();
            var ctv2 = objetos.Get<DLM.macros.CTV2>();
            var escadasEM1 = objetos.Get<Marinheiro>();
            var pacoteEM1 = new PacoteMarinheiro(escadasEM1);
            var escadasEM2 = objetos.Get<DLM.macros.EM2>();
            var pacoteEM2 = new DLM.macros.EM2Pacote(escadasEM2);
            var pacoteCTV = new DLM.macros.CTV2Pacote(ctv2);

            retorno.AddRange(pacoteEM1.getPecas().Get<RMA>());
            retorno.AddRange(pacoteEM1.getPecas().Get<RME>());


            var txt_macro_corrente = "[MACRO CORRENTE]";
            var txt_macro_tirante = "[MACRO TIRANTES]";
            var txt_macro_ctv2 = "[MACRO CTV2]";
            var txt_macro_em2 = "[MACRO EM2]";

            var pcsEM2 = pacoteEM2.Pecas.GroupBy(x => x.Nome_Padronizado).OrderBy(x => x.Key).ToList();

            foreach (var pc in pacoteCTV.Pecas)
            {
                object igual = DBases.GetBancoRM().GetPeca(pc.NomePadronizado);
                if (igual is RME)
                {
                    var nrm = igual.As<RME>().Clonar(pc.Quantidade, pc.Comprimento);
                    nrm.Observacoes = txt_macro_ctv2;
                    nrm.FICHA_PINTURA = pc.Tratamento;
                    nrm.User = Cfg.Init.GetUser().Alias;
                    retorno.Add(nrm);
                }
                else if (igual is RMA)
                {
                    var PAR = igual.As<RMA>().Clonar(pc.Quantidade, txt_macro_ctv2);
                    retorno.Add(PAR);
                }
                else
                {
                    retorno.Add(new Report("Peça Não encontrada", $"CTV2 => {pc.Nome}", TipoReport.Critico));
                }
            }

            foreach (var pc in pcsEM2)
            {
                var igual = DBases.GetBancoRM().GetPeca(pc.Key);
                if (igual != null)
                {
                    if (igual is RME)
                    {
                        foreach (var p1 in pc.ToList())
                        {
                            var nrm = igual.As<RME>().Clonar(p1.Qtd, p1.Comp, p1.Nome);
                            nrm.Observacoes = txt_macro_em2;
                            nrm.FICHA_PINTURA = p1.Tratamento;
                            nrm.User = Cfg.Init.GetUser().Alias;

                            retorno.Add(nrm);
                        }
                    }
                    else if (igual is RMA)
                    {
                        var pecas = pc.ToList();
                        var qtd = pecas.Sum(x => x.Qtd);

                        var PAR = igual.As<RMA>().Clonar(qtd, txt_macro_em2);
                        retorno.Add(PAR);

                        if (PAR.TIPO == "PAR")
                        {
                            var POR = PAR.GetPOR().Clonar(qtd, txt_macro_em2);
                            if (POR != null)
                            {
                                retorno.Add(POR);
                            }
                            else
                            {
                                retorno.Add(new Report("Peça Não encontrada", $"EM2 => Porca para o parafuso: {pc.Key}", TipoReport.Critico));
                            }

                            var ARR = PAR.GetARR().Clonar(qtd, txt_macro_em2);
                            if (ARR != null)
                            {
                                retorno.Add(ARR);
                            }
                            else
                            {
                                retorno.Add(new Report("Peça Não encontrada", $"EM2 => Arruela para o parafuso: {pc.Key}", TipoReport.Critico));
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    retorno.Add(new Report("Peça Não encontrada", $"EM2 => {pc.Key}", TipoReport.Critico));
                }
            }

            /*todo = é necessário melhorar essa parte.*/
            retorno.AddRange(ctv1.SelectMany(x => x.getPecas().GetRMAs()).ToList().Juntar());
            retorno.AddRange(ctv1.SelectMany(x => x.getPecas().GetRMEs()).ToList().Juntar());
            retorno.AddRange(ctv1.SelectMany(x => x.getPecas().Get<Report>()));



            foreach (var purlin in purlins)
            {
                var pcPurlin = purlin.getPeca();
                if (pcPurlin != null)
                {
                    var nova = pcPurlin.Clonar(purlin.Quantidade, purlin.Comprimento);
                    nova.User = purlin.User;
                    nova.FICHA_PINTURA = purlin.Pintura;
                    nova.Quantidade = purlin.Quantidade;
                    nova.PREFIX = purlin.Sequencia;
                    nova.MARCA_CUSTOM = pcPurlin.MARCA_CUSTOM;
                    nova.PDF_CUSTOM = pcPurlin.PDF_CUSTOM;
                    if (purlin.Material != nova.MATERIAL)
                    {
                        nova.TIPO_ACO_CUSTOM = purlin.Material;
                    }

                    nova.Observacoes = purlin.Observacoes;
                    nova.TIPO_ACO_CUSTOM = purlin.Material;


                    if (nova.Posicoes.Count == 1)
                    {
                        nova.PDF_CUSTOM = true;
                        int furos = purlin.GetFurosVista(false).Count;
                        if (nova.COMP_USER <= purlin.Comp_Min)
                        {
                            nova.Marca.MAKTX = "PERFIL DOBRADO";
                            nova.Posicoes[0].MAKTX = purlin.Perfil_Dobrado;
                            nova.Posicoes[0].NORMT = TAB_NORMT.PERFIL_DOBRADO;
                            nova.Posicoes[0].ZPP_CORTE = purlin.Corte;
                            nova.Posicoes[0].QTD_FUROS_CUSTOM = furos;
                        }
                        nova.Posicoes[0].ZPP_TIPOACO = purlin.Material;
                        retorno.Add(nova);
                    }
                }
                else
                {
                    retorno.Add(new Report($"Purlin, id_peca={purlin.id_peca}", "Peça Não encontrada", TipoReport.Critico));
                }
            }

            if (correntes.Sum(x => x.Quantidade) > 0)
            {
                var par_corrente = DBases.GetBancoRM().GetParafuso(12, 38, Cfg.Init.CTR_TRAT);
                if (par_corrente != null)
                {
                    if (par_corrente.GetPOR() != null && par_corrente.GetARR() != null)
                    {
                        var prCTR = new RMA(par_corrente, correntes.FindAll(x => x.Parafusos).Sum(x => 4 * x.Quantidade), txt_macro_corrente);
                        prCTR.Observacoes = txt_macro_corrente;
                        prCTR.User = Cfg.Init.GetUser().Alias;
                        if (prCTR.Quantidade > 0)
                        {
                            retorno.Add(prCTR);



                            retorno.Add(prCTR.GetPOR().Clonar(prCTR.Quantidade, txt_macro_corrente));
                            var qtd_arr = correntes.FindAll(x => x.Parafusos && x.Arruela).Sum(x => 4 * x.Quantidade);

                            if (qtd_arr > 0)
                            {
                                retorno.Add(prCTR.GetARR().Clonar(qtd_arr, txt_macro_corrente));
                            }
                        }
                    }
                }

                var fixacoes = correntes.GroupBy(x => $"{x.Fixacao}@{x.Tratamento}").ToList();
                foreach (var fixacao in fixacoes)
                {
                    var pcs = fixacao.ToList();
                    var total = pcs.Sum(x => x.Quantidade * 2);
                    if (total > 0)
                    {
                        var frs = pcs.First();
                        var tipo = DBases.GetBancoRM().GetRME(frs.Fixacao.ToString());
                        if (tipo != null)
                        {
                            var nf = tipo.Clonar(total);
                            nf.Observacoes = txt_macro_corrente;
                            nf.FICHA_PINTURA = frs.Tratamento;
                            retorno.Add(nf);
                        }
                        else
                        {
                            retorno.Add(new Report("Peça não encontrada", $"Macro Correntes => {frs.Fixacao.ToString()}", TipoReport.Critico));
                        }
                    }

                }

                var ctrs = correntes.GroupBy(x => $"{x.id_db}@{x.Tratamento}").ToList();
                foreach (var ctr in ctrs)
                {
                    var frst = ctr.First();
                    var diagonal = frst.GetDiagonal();
                    if (diagonal != null)
                    {
                        var comps = ctr.ToList().GroupBy(x => x.CompCorrente).ToList();
                        foreach (var comp in comps)
                        {
                            var nctr = diagonal.Clonar(comp.Sum(x => x.Quantidade), comp.Key);
                            nctr.FICHA_PINTURA = frst.Tratamento;
                            nctr.Observacoes = txt_macro_corrente;
                            nctr.User = Cfg.Init.GetUser().Alias;
                            retorno.Add(nctr);
                        }
                    }
                    else
                    {
                        retorno.Add(new Report("Peça não encontrada", $"Macro Correntes => RME id={frst.id_db}", TipoReport.Critico));
                    }
                }


            }

            if (tirantes.Sum(x => x.Quantidade) > 0)
            {
                var ntrPOR = DBases.GetBancoRM().GetPorca(Cfg.CTV2.TRR03DIAM, Cfg.CTV2.TRR03TRAT);
                var ntrARR = DBases.GetBancoRM().GetArruela(Cfg.CTV2.TRR03DIAM, Cfg.CTV2.TRR03TRAT);
                var tiposTR = tirantes.GroupBy(x => $"{x.NomePadronizado}").ToList();
                foreach (var tipoTR in tiposTR)
                {
                    var nTR = DBases.GetBancoRM().GetRME(tipoTR.Key);
                    var trrs = tipoTR.ToList();

                    var tipos_sft = new List<string>();
                    tipos_sft.AddRange(trrs.Select(x => x.Fixacao_1 + "@" + x.Tratamento));
                    tipos_sft.AddRange(trrs.Select(x => x.Fixacao_2 + "@" + x.Tratamento));
                    tipos_sft = tipos_sft.Distinct().ToList();

                    foreach (var sftNome in tipos_sft)
                    {
                        var txt = sftNome.Split('@').ToList();
                        var qtdSFT = trrs.FindAll(x => x.Fixacao_1 == txt[0] && x.Tratamento == txt[1]).Sum(x => x.Quantidade) +
                                     trrs.FindAll(x => x.Fixacao_2 == txt[0] && x.Tratamento == txt[1]).Sum(x => x.Quantidade);
                        if (qtdSFT > 0)
                        {
                            var nsft = DBases.GetBancoRM().GetRME(txt[0]);
                            if (nsft != null)
                            {
                                nsft.Quantidade = qtdSFT;
                                nsft.Observacoes = txt_macro_tirante;
                                nsft.FICHA_PINTURA = txt[1];
                                nsft.User = Cfg.Init.GetUser().Alias;
                                retorno.Add(nsft);
                            }
                            else
                            {
                                retorno.Add(new Report("Peça não encontrada", $"Macro Tirantes => {sftNome}", TipoReport.Critico));
                            }
                        }
                    }

                    //TIRANTES
                    int qtdSftPorcas = trrs.Sum(x => x.Quantidade * 2);
                    if (qtdSftPorcas > 0)
                    {
                        if (ntrPOR != null)
                        {
                            retorno.Add(new RMA(ntrPOR, qtdSftPorcas, txt_macro_tirante));
                        }
                        else
                        {
                            retorno.Add(new Report("Peça não encontrada", $"Macro Tirantes => {ntrPOR}", TipoReport.Critico));
                        }

                        if (ntrARR != null)
                        {
                            retorno.Add(new RMA(ntrARR, qtdSftPorcas, txt_macro_tirante));
                        }
                        else
                        {
                            retorno.Add(new Report("Peça não encontrada", $"Macro Tirantes => {ntrARR}", TipoReport.Critico));
                        }
                    }

                    var tr_comps = trrs.Select(x => x.Comprimento).Distinct().ToList();

                    if (nTR != null)
                    {
                        var trts = trrs.GroupBy(x => $"{x.Comprimento}@{x.Tratamento}");
                        foreach (var trt in trts)
                        {
                            var tr_comp = trt.Key.Split('@')[0].Double();
                            var trat = trt.Key.Split('@')[1];

                            if (tr_comp <= nTR.COMP_MAX && tr_comp >= nTR.COMP_MIN)
                            {
                                var novo = nTR.Clonar(trt.ToList().Sum(x => x.Quantidade), tr_comp);
                                novo.FICHA_PINTURA = trat;
                                novo.User = Cfg.Init.GetUser().Alias;
                                novo.Observacoes = txt_macro_tirante;
                                retorno.Add(novo);
                            }
                            else
                            {
                                retorno.Add(new Report("Comprimento inválido", $"Vão digitado é maior ou menor que possível para o Tirante: {tr_comp}", TipoReport.Critico));
                            }

                        }

                    }
                    else
                    {
                        retorno.Add(new Report("Peça não encontrada", $"Macro Tirantes => {tipoTR.Key}", TipoReport.Critico));
                    }
                }
            }

            return retorno;
        }
        public static List<RMA> GetRMAs(this List<object> _lista)
        {
            return _lista.Get<RMA>();
        }
        public static List<RME> GetRMUs(this List<object> _lista)
        {
            return _lista.Get<RME>().FindAll(x => x.DESTINO == "RMU").ToList();
        }
        public static List<RME> GetRMEs(this List<object> _lista)
        {
            return _lista.Get<RME>().FindAll(x => x.DESTINO == "RME").ToList();
        }
        public static RME GetRMDB(this RME _rme)
        {
            RME _RMDB = null;
            if (_rme.id_db > 0)
            {
                _RMDB = DBases.GetBancoRM().GetRM(_rme.id_db);
            }
            return _RMDB;
        }
        public static bool Comprimento_Pode(this RME _rme, double _valor, bool _setar = true)
        {
            if (!_rme.VARIAVEL) { "Peça com comprimento fixo".Alerta(); return false; }
            if (_rme.COMP_MAX > 0 && _valor > _rme.COMP_MAX) { $"Comprimento maior que o máximo [{_rme.COMP_MAX}]".Alerta(); return false; }
            if (_valor < _rme.COMP_MIN) { $"Comprimento menor que o mínimo [{_rme.COMP_MIN}]".Alerta(); return false; }
            if (_setar)
            {
                _rme.SetComprimento(_valor);
            }
            return true;
        }
        public static List<RME> GetCorrespondentes(this RME rme)
        {
            return DBases.GetBancoRM().GetRMEs().FindAll(x => x.TIPO == rme.TIPO && x.SUBSTITUTO == "");
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
            else if (valor2 == $"{rme.TIPO}_{rme.COD_DB}")
            {
                return true;
            }
            else if (valor2 == $"{rme.TIPO}_{rme.CODIGOFIM}")
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
            pp.Material = rme.MATERIAL;
            pp.Data = DateTime.Now.ToShortDateString();
            pp.Descricao = rme.PERFIL;
            pp.Espessura = rme.ESP;
            pp.Largura = rme.CORTE;
            pp.Marca = rme.CODIGOFIM;
            pp.Quantidade = rme.Quantidade.Int();
            pp.Peso = rme.PESOUNIT;

            return pp;
        }

        public static List<RME> GetIrmas(this RME rme)
        {
            if (rme.DESTINO == "RMU")
            {
                return DBases.GetBancoRM().GetRMUs().FindAll(x => x.TIPO == rme.TIPO && x.VARIAVEL == rme.VARIAVEL);
            }
            else
            {
                return DBases.GetBancoRM().GetRMEs().FindAll(x => x.TIPO == rme.TIPO && x.VARIAVEL == rme.VARIAVEL);
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
                                Origem.FindAll(x => x != null).GroupBy(x => x.SAP);
            foreach(var p in distinct)
            {
                if(p.ToList().Count>1)
                {
                    retorno.Add(p.First().Clonar(p.Sum(x => x.Quantidade), string.Join(", ", p.Select(x => x.Observacoes)).Esquerda(30)));
                }
                else
                {
                    retorno.Add(p.First());
                }
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
                                             .ToList().OrderBy(x => x.COD_DB).ToList();
            foreach (RMT t in distinct)
            {
                RMT N = new RMT(t, t.Bobina, t.GetLinha_RMT_User());
                N.Quantidade = Origem.FindAll(x => x.ToString() == t.ToString()).Sum(y => y.Quantidade);
                retorno.Add(N);
            }
            var lista_fim = retorno.GroupBy(x => x.CODIGOFIM).ToList();
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
            var distinct = Origem.FindAll(x => x != null)
                                                .GroupBy(x => x.ToString()).ToList();

            foreach (var p in distinct)
            {
                if (p.Count() == 1)
                {
                    retorno.Add(p.First());
                }
                else
                {
                    var frst = p.First();


                    var qtd = p.ToList().Sum(x => x.Quantidade);

                    var clone = frst.Clonar(qtd, frst.COMP_USER, null, frst.Bobina);
                    clone.PREFIX = frst.PREFIX;
                    clone.SUFIX = frst.SUFIX;
                    clone.Observacoes = frst.Observacoes;

                    retorno.Add(clone);
                }

            }
            //foreach (var rm in distinct)
            //{

            //    var nPeca = rm.Clonar(Origem.FindAll(x => x != null).FindAll(x => x.ToString() == rm.ToString()).Sum(y => y.Quantidade));
            //    nPeca.Observacoes = rm.Observacoes;
            //    retorno.Add(nPeca);
            //}
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

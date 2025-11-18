using DLM.desenho;
using DLM.encoder;
using DLM.vars;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Conexoes
{
    public static class ExtensoesPDF
    {
        public static bool InserirTabelasPDFsCAMs(this Pacotes pacotes, bool horizontal = false)
        {
            var cams = pacotes.GetCAMs().FindAll(x => x.IsMesa());
            var pos_mesas = pacotes.GetPosicoes_NaoRM_CHGrossa().FindAll(x => x.NORMT == TAB_NORMT.VIGA_MESA).FindAll(x => x.GetCam() != null);
            pos_mesas = pos_mesas.FindAll(x => x.Marca.Pacote.Tipo == Pacote_Tipo.MBS);


            var erros = new List<Report>();
            var cams_mesas = cams.FindAll(x => x.Formato.LIV1.Furacoes.Count > 0);
            cams_mesas.AddRange(pos_mesas.Select(x => x.GetCam()));
            cams_mesas = cams_mesas.GroupBy(x => x.Nome).Select(x => x.First()).ToList();

            if (cams_mesas.Count == 0)
            {
                return true;
            }
            var programas = new List<string>();
            var destino = pacotes.PastaPDF.GetSubPasta(Cfg.Init.FLANGES);
            foreach (var cam in cams_mesas)
            {
                try
                {
                    var pdf_origem = $"{pacotes.PastaPDF}{cam.Nome}.PDF";
                    var pdf_destino = $"{destino}{cam.Nome}.PDF";

                    if (!pdf_origem.Exists())
                    {
                        erros.Add($"Arquivo PDF não encontrado: {pdf_origem}");
                        continue;
                    }

                    var furos = cam.Formato.LIV1.Furacoes.GetGages();

                    programas.Add("$");
                    programas.Add("@CAB");
                    programas.Add($"NOME:{cam.Nome}");
                    programas.Add($"COMP:{cam.Comprimento.Round(0)}");
                    programas.Add($"ESP:{cam.Espessura.String(2)}");
                    programas.Add($"PUNCOES:{furos.Count}");
                    programas.Add($"RECORTE:{(cam.TemRecorte ? "SIM" : "NAO")}");
                    programas.Add("@GAGES");


                    foreach (var furo in furos)
                    {
                        programas.Add($"{furo.ToString()}{(furo.Gages.Count > 1 ? "[!]" : "")}");
                    }

                    programas.Add("$");

                    if (furos.Count == 0)
                    {
                        continue;
                    }

                    var tabelas = new List<TabelaPDF>();
                    var furacoes = new List<List<string>>();
                    furacoes.Add(new List<string> { "X", "G  ", "Ø  ", "EF " });
                    for (int i = 0; i < furos.Count; i++)
                    {
                        var fr = furos[i];
                        var fa = 0.0;
                        if (i > 0)
                        {
                            fa = (fr.X - furos[i - 1].X);
                        }
                        if (fa > 1.5 | i == 0)
                        {
                            furacoes.Add(new List<string> { fr.X.String(0, 5), string.Join(",", fr.Gages), fr.Diametro.Replace("Ø", ""), fa > 0 ? fa.String(0) : "" });
                        }
                    }
                    if (!horizontal)
                    {
                        furacoes = furacoes.Inverter();
                    }

                    var pdf = new PdfReader(pdf_origem);
                    var tamanho = pdf.GetPageSizeWithRotation(1);
                    var w = tamanho.Width / 842;
                    var h = tamanho.Height / 595;
                    var fonte = 8 * w;

                    if (!horizontal)
                    {
                        w = w * 750;
                        h = h * 20;
                    }
                    else
                    {
                        fonte = 9 * w;
                        w = w * 25;
                        h = h * 60;
                    }


                    var tabela_furos = new TabelaPDF(new P3d(w, h), furacoes, fonte);
                    tabelas.Add(tabela_furos);

                    pdf.AddTabelas(pdf_destino, tabelas);
                }
                catch (Exception ex)
                {
                    erros.Add(new Report(ex));
                }
            }
            string arq_destino = $"{destino}RESUMO.FLANGES";
            if (programas.Count > 0)
            {
                Conexoes.Utilz.Arquivo.Gravar(arq_destino, programas);
            }

            erros.Show();

            return erros.Count == 0;
        }

        public static void AddTabelas(this PdfReader pdf, string PDF_Destino, List<TabelaPDF> tabelas)
        {
            try
            {

                var arquivo_destino = new FileStream(PDF_Destino, FileMode.Create, FileAccess.Write, FileShare.None);

                var tamanho = pdf.GetPageSizeWithRotation(1);

                var stamper = new PdfStamper(pdf, arquivo_destino);
                var cor = new BaseColor(System.Drawing.Color.White);
                #region Tabela1
                foreach (var tabela in tabelas)
                {
                    var largura = tabela.Colunas.Max(x => x.Sum(y => y.LenghtStr()));
                    largura = largura * 4;
                    var table = new PdfPTable(tabela.Colunas.Count);
                    var fonte = new Font(FontFactory.GetFont("MonoSpace").BaseFont, (float)tabela.TamFonte);

                    table.HorizontalAlignment = tabela.AlinhamentoHorizontal;
                    table.WidthPercentage = 100;

                    var larguras = new List<List<Chunk>>();
                    for (int L = 0; L < tabela.Linhas; L++)
                    {
                        var ls = new List<Chunk>();
                        for (int C = 0; C < table.NumberOfColumns; C++)
                        {
                            string valor = "";
                            try
                            {
                                if (L < tabela.Colunas[C].Count)
                                {
                                    valor = tabela.Colunas[C][L];
                                }
                                var chk = new Chunk(valor, fonte);
                                ls.Add(chk);

                                var celula = new PdfPCell(new Phrase(chk));
                                celula.BackgroundColor = cor;
                                table.AddCell(celula);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        larguras.Add(ls);
                    }



                    var tams = larguras.Inverter().Select(x => x.Select(y => (float)(y.GetWidthPoint() * 1.3)).ToList()).ToList();
                    var tamanhos_finais = tams.Select(x => x.Max()).ToArray();

                    var total_largura = (float)(tamanhos_finais.Sum());

                    table.SetWidths(tamanhos_finais);
                    table.TotalWidth = total_largura;
                    table.LockedWidth = true;


                    /*coordenada Y de cima para baixo*/
                    table.WriteSelectedRows(0, tabela.Linhas, (float)tabela.Origem.X, (float)(tamanho.Height - tabela.Origem.Y), stamper.GetOverContent(1));
                }
                #endregion
                stamper.Close();
            }
            catch (Exception ex)
            {
                ex.Alerta();
            }
        }
    }
}

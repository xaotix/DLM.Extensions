using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conexoes
{
    public static class ExtensoesPDF
    {
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
                    var largura = tabela.Colunas.Max(x => x.Sum(y => y.Length));
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

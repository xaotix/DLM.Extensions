using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conexoes
{
    public class LinkWeb
    {
        public string Url { get; set; }
        public string Arquivo_Local { get; set; }
        public LinkWeb() { }
    }
    public static class Web
    {

        public static void DownloadSiteFiles(string url, string pasta_raiz_destino, int max_segundos)
        {
            // Cria um token de cancelamento com timeout de 10 segundos
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                try
                {
                    // Executa GetLinksArquivos em uma Task
                    var task = Task.Run(() => GetLinksArquivos(url, pasta_raiz_destino, cts.Token), cts.Token);

                    // Aguarda a conclusão ou timeout
                    if (task.Wait(TimeSpan.FromSeconds(max_segundos)))
                    {
                        var links = task.Result;
                        if (links.Count > 0)
                        {
                            Download(links);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tempo limite excedido. Operação cancelada.");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operação cancelada pelo token.");
                }
            }
        }

        //public static void DownloadSiteFiles(string url, string pasta_raiz_destino)
        //{
        //    var links = GetLinksArquivos(url,pasta_raiz_destino);
        //    if(links.Count>0)
        //    {
        //        Download(links);
        //    }
        //}
        public static List<LinkWeb> GetLinksArquivos(string root_url, string pasta_raiz_destino, CancellationToken token)
        {
            var retorno = new List<LinkWeb>();
            try
            {
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(root_url);

                    // Regex para capturar href="..."
                    Regex regex = new Regex("href=\"([^\"]+)\"", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(html);


                    if (!pasta_raiz_destino.AsPasta().CreateDirectory().IsNullOrEmpty())
                    {
                        foreach (Match match in matches)
                        {
                            string link = match.Groups[1].Value;


                            token.ThrowIfCancellationRequested(); // verifica se foi cancelado

                            // Ignorar links inválidos
                            if (string.IsNullOrWhiteSpace(link)) continue;
                            if (link.StartsWith("?")) continue;
                            if (link == "/") continue;
                            if (link.StartsWith("../")) continue; // evita Parent Directory

                            // Montar URL absoluta corretamente
                            var url = new Uri(new Uri(root_url), link);
                            string full_url = url.AbsoluteUri;
                            string arquivo_local = Path.Combine(pasta_raiz_destino, url.LocalPath.Replace(@"/", @"\").TrimStart(@"\"));



                            if (root_url.StartsW(full_url))
                            {
                                continue;
                            }

                            if (link.EndsWith("/"))
                            {
                                // É diretório → chamada recursiva
                                token.ThrowIfCancellationRequested(); // verifica se foi cancelado
                                retorno.AddRange(GetLinksArquivos(full_url, pasta_raiz_destino, token));
                            }
                            else
                            {
                                try
                                {

                                    retorno.Add(new LinkWeb() { Arquivo_Local = arquivo_local, Url = full_url });

                                }
                                catch (Exception ex)
                                {
                                    ex.Alerta(full_url);
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                ex.Alerta();
            }
            return retorno;
        }


        public static void Download(List<LinkWeb> links)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    foreach (var link in links)
                    {
                        if (link.Arquivo_Local.Delete())
                        {
                            byte[] data = client.DownloadData(link.Url);
                            Directory.CreateDirectory(Path.GetDirectoryName(link.Arquivo_Local));
                            File.WriteAllBytes(link.Arquivo_Local, data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Alerta();
            }
        }





        //     public static void DownloadSiteFiles(string url, string pasta_destino)
        //     {
        //try
        //{
        //             using (WebClient client = new WebClient())
        //             {
        //                 string html = client.DownloadString(url);

        //                 // Regex para capturar href="..."
        //                 Regex regex = new Regex("href=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        //                 var matches = regex.Matches(html);


        //                 if(!pasta_destino.AsPasta().CreateDirectory().IsNullOrEmpty())
        //                 {
        //                     foreach (Match match in matches)
        //                     {
        //                         string link = match.Groups[1].Value;

        //                         // Ignorar links inválidos
        //                         if (string.IsNullOrWhiteSpace(link)) continue;
        //                         if (link.StartsWith("?")) continue;
        //                         if (link == "/") continue;
        //                         if (link.StartsWith("../")) continue; // evita Parent Directory

        //                         // Montar URL absoluta corretamente
        //                         var urls = new Uri(new Uri(url), link);
        //                         string fullUrl = urls.AbsoluteUri;
        //                         string localFilePath = Path.Combine(pasta_destino, urls.LocalPath.Replace(@"/", @"\").TrimStart(@"\"));

        //                         if(url.StartsW(fullUrl))
        //                         {
        //                             continue;
        //                         }

        //                         if (link.EndsWith("/"))
        //                         {
        //                             // É diretório → chamada recursiva
        //                             DownloadSiteFiles(fullUrl, pasta_destino);
        //                         }
        //                         else
        //                         {
        //                             try
        //                             {
        //                                 if (localFilePath.Delete())
        //                                 {
        //                                     byte[] data = client.DownloadData(fullUrl);
        //                                     Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
        //                                     File.WriteAllBytes(localFilePath, data);
        //                                 }

        //                             }
        //                             catch (Exception ex)
        //                             {
        //                                 ex.Alerta(fullUrl);
        //                             }
        //                         }
        //                     }
        //                 }


        //             }
        //         }
        //catch (Exception ex)
        //{
        //             ex.Alerta();
        //}
        //     }


    }
}
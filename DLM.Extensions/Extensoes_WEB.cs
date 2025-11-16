using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conexoes
{
    public static class Web
    {
        public static void DownloadSiteFiles(string url, string pasta_destino)
        {
			try
			{
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(url);

                    // Regex para capturar href="..."
                    Regex regex = new Regex("href=\"([^\"]+)\"", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(html);

                   
                    if(!pasta_destino.AsPasta().CreateDirectory().IsNullOrEmpty())
                    {
                        foreach (Match match in matches)
                        {
                            string link = match.Groups[1].Value;

                            // Ignorar links inválidos
                            if (string.IsNullOrWhiteSpace(link)) continue;
                            if (link.StartsWith("?")) continue;
                            if (link == "/") continue;
                            if (link.StartsWith("../")) continue; // evita Parent Directory

                            // Montar URL absoluta corretamente
                            var urls = new Uri(new Uri(url), link);
                            string fullUrl = urls.AbsoluteUri;
                            string localFilePath = Path.Combine(pasta_destino, urls.LocalPath.Replace(@"/", @"\").TrimStart(@"\"));

                            if (link.EndsWith("/"))
                            {
                                // É diretório → chamada recursiva
                                DownloadSiteFiles(fullUrl, pasta_destino);
                            }
                            else
                            {
                                try
                                {
                                    if (localFilePath.Delete())
                                    {
                                        byte[] data = client.DownloadData(fullUrl);
                                        Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
                                        File.WriteAllBytes(localFilePath, data);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    ex.Alerta(fullUrl);
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
        }


    }
}
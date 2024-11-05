using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conexoes
{
    public static class Extensoes_Arquivo
    {
        public static string SalvarArquivo(this string extensao, string mensagem = "Salvar o arquivo")
        {
            var saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = $"{extensao.ToUpper()}| *.{extensao.Replace("*.", "").Replace("*", "")}";
            saveFileDialog1.Title = mensagem + extensao;
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                return saveFileDialog1.FileName;
            }
            return null;
        }
        public static string AbrirString(this string extensao)
        {
            return Conexoes.Utilz.Abrir_String(extensao);
        }
        public static void Gravar(this List<List<string>> linhas, string extensao = "csv")
        {
            linhas.Select(x => string.Join(";", x)).ToList().Gravar(extensao);
        }
        public static void Gravar(this List<string> linhas, string extensao = "csv")
        {
        retentar:
            var destino = extensao.SalvarArquivo();
            try
            {
                if (destino != null)
                {
                    Utilz.Arquivo.Gravar(destino, linhas);
                    if (destino.Exists())
                    {
                        try
                        {

                            destino.Abrir();
                        }
                        catch (Exception ex)
                        {
                            ex.Alerta();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if ($"Não foi possível criar o arquivo. \n{ex.Message}\nDeseja tentar novamente?".Pergunta())
                {
                    goto retentar;
                }
            }

        }
    }
}

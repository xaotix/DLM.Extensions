using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Conexoes
{
    public static class Extensoes_Arquivo
    {
        public static string SalvarArquivo(this string extensao, string mensagem = "Salvar o arquivo", string arquivo = "", string pasta_raiz = "")
        {
            var saveFileDialog1 = new SaveFileDialog();
            if (!pasta_raiz.IsNullOrEmpty())
            {
                saveFileDialog1.InitialDirectory = pasta_raiz;
            }
            else
            {
                saveFileDialog1.RestoreDirectory = true;
            }


            saveFileDialog1.Filter = $"{extensao.ToUpper()}| *.{extensao.Replace("*.", "").Replace("*", "")}";
            saveFileDialog1.Title = mensagem + extensao;
            if (!arquivo.IsNullOrEmpty())
            {
                saveFileDialog1.FileName = arquivo;
            }
            var result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (saveFileDialog1.FileName.Length > 0)
                {
                    return saveFileDialog1.FileName;
                }
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

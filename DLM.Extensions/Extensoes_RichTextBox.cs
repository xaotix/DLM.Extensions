using Conexoes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace DLM
{
    public static class Extensoes_RichTextBox
    {
        public static void Set(this System.Windows.Controls.RichTextBox rich, List<double> valores)
        {
            var txt = new TextRange(rich.Document.ContentStart, rich.Document.ContentEnd);
            txt.Text = string.Join("\n", valores);
        }
        public static List<double> GetValores(this System.Windows.Controls.RichTextBox rich)
        {
            var txt = new TextRange(rich.Document.ContentStart, rich.Document.ContentEnd);
            var l = txt.Text.Replace("\r", "").Split('\n').ToList().FindAll(x => x.Replace(" ", "") != "").FindAll(x => x.ESoNumero()).Select(x => x.Double()).ToList();

            return l.ToList();
        }
        public static string RTF_para_TXT(this string valor)
        {
            try
            {
                var rtBox = new System.Windows.Forms.RichTextBox();
                rtBox.Rtf = valor;
                string plainText = rtBox.Text;
                return plainText;
            }
            catch (Exception)
            {


            }
            return "";
        }
    }
}

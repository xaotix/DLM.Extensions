using DLM.ini;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conexoes
{
    public static class Extensoes_Prompt
    {
        public static int StringMaxLenght => 500;
        public static int Decimais => 5;
        private static string MainPrompt(string titulo, string Valor, bool gravar_carregar, string chave, bool multilinha, int maxlenght)
        {
            if (maxlenght <= 0)
            {
                maxlenght = StringMaxLenght;
            }
            string retorno = null;
            string arq = Cfg.Init.DIR_APPDATA + @"cfguser_prompt.dlm";
            if (chave.Length == 0)
            {
                chave = titulo.ToUpper().RemoverCaracteresEspeciais();
            }
            if (gravar_carregar)
            {
                Valor = INI.Get(arq, "Prompt", chave, Valor);
            }
            if (multilinha)
            {
                Janelas.Texto mm = new Janelas.Texto(Valor);
                mm.Title = titulo;
                mm.ShowDialog();

                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    retorno = mm.Input.Text.CortarString(maxlenght, false);
                }
            }
            else
            {
                var mm = new Digita_Texto(titulo, maxlenght, Valor, titulo);
                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    retorno = mm.caixa_texto.Text;
                    if (gravar_carregar && retorno != "")
                    {
                        INI.Set(arq, "Prompt", chave, retorno);
                    }
                }
            }
            return retorno;
        }

        public static List<string> PromptLista(this string titulo, int max_lenght = 100)
        {
            var mm = new ListaStringManual(titulo, max_lenght);
            mm.ShowDialog();
            List<string> retorno = new List<string>();
            if (mm.DialogResult.HasValue && mm.DialogResult.Value)
            {
                retorno = mm.Itens;
            }
            return retorno;
        }

        public static string Prompt(this string valor, bool multi_line, string titulo, int  max_lenght = 0)
        {
            return MainPrompt(titulo,valor,false,"",true, max_lenght);
        }
        public static string Prompt(this string valor, string titulo = "Digite", int max_lenght = 0)
        {
            var status = false;
            return Prompt(valor, out status, titulo);
        }
        public static string Prompt(this string valor, out bool status, string titulo = "Digite")
        {
            status = false;
            var retorno = MainPrompt(titulo, valor, false, "", false, 0);
            if (retorno == null)
            {
                return null;
            }
            else
            {
                status = true;
                return retorno;
            }
        }
        public static string Prompt(this string valor, string titulo, string chave, int max_lenght = 0)
        {
            var retorno = MainPrompt(titulo, valor, true, chave, false, max_lenght);
            return retorno;
        }

        public static int Prompt(this int valor, string titulo = "Digite")
        {
            var status = false;
            var retorno = valor.Prompt(out status, titulo);
            return retorno;
        }
        public static int Prompt(this int valor, out bool status, string titulo = "Digite")
        {
            status = false;
            var retorno = MainPrompt(titulo, valor.ToString(), false, "", false,15);
            if (retorno == null)
            {
                return -1;
            }
            else
            {
                status = true;
                return retorno.Int();
            }
        }


        public static double Prompt(this double valor, string titulo = "Digite", int decimais = 0)
        {
            var status = false;
            var retorno = valor.Prompt(out status, decimais);
            return retorno;
        }
        public static double Prompt(this double valor, out bool status, int decimais = -1, string titulo = "Digite")
        {
            if(decimais<0)
            {
                decimais = Decimais;
            }
            status = false;
            var retorno = MainPrompt(titulo, valor.String(decimais), false, "", false, 15);
            if (retorno == null)
            {
                return 0;
            }
            if (retorno.Length > 0)
            {
                status = true;
                return retorno.Double(decimais);
            }
            return 0;
        }
        public static double Prompt(this double valor, string titulo, string chave,int decimais = -1)
        {
            if(decimais<0)
            {
                decimais = Decimais;
            }
            var retorno = MainPrompt(titulo, valor.ToString(), true, chave, false, 0);
            return retorno.Double(decimais);
        }
    }
}

using DLM.ini;
using DLM.vars;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Conexoes
{
    public static class Extensoes_Prompt
    {
        public static int StringMaxLenght => 500;
        public static int Decimais => 5;
        private static object MainPrompt(string titulo, object Valor, bool gravar_carregar, string chave, bool multilinha, int maxlenght, string string_format = null)
        {
            if (maxlenght <= 0)
            {
                maxlenght = StringMaxLenght;
            }
            object retorno = Valor;
            string arq = Cfg.Init.DIR_APPDATA + @"cfguser_prompt.dlm";
            if (chave.Length == 0)
            {
                chave = titulo.ToUpper().RemoverCaracteresEspeciais();
            }
            if (gravar_carregar)
            {
                Valor = INI.Get(arq, "Prompt", chave, Valor.ToString());
            }
            if (multilinha)
            {
                var mm = new Janelas.Texto(Valor.ToString());
                mm.Title = titulo;
                mm.ShowDialog();

                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    retorno = mm.Input.Text.Esquerda(maxlenght, false);
                }
            }
            else if (Valor is string)
            {
                var mm = new Digita_Texto(titulo, maxlenght, Valor, titulo);
                mm.caixa_texto.TextWrapping = System.Windows.TextWrapping.WrapWithOverflow;
                mm.caixa_texto.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Visible;

                mm.caixa_texto.MaxLength = maxlenght;
                if (maxlenght > 100)
                {
                    mm.caixa_texto.Height = 150;
                    mm.caixa_texto.Width = 450;
                    mm.caixa_texto.AcceptsReturn = true;
                }
                else
                {
                    mm.caixa_texto.AcceptsReturn = false;
                    mm.caixa_texto.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
                }

                mm.ShowDialog();
                if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                {
                    retorno = mm.caixa_texto.Text;
                    if (gravar_carregar && retorno != null)
                    {
                        INI.Set(arq, "Prompt", chave, retorno);
                    }
                }
                else
                {
                    retorno = null;
                }
            }
            else
            {
                if (Valor is double)
                {
                    var mm = new DLM.WPF.Pompt.PromptDouble(Valor.Double(), titulo, string_format);
                    mm.Title = titulo;

                    mm.ShowDialog();
                    if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                    {
                        retorno = mm.MVC.Valor;
                        if (gravar_carregar && retorno != null)
                        {
                            INI.Set(arq, "Prompt", chave, retorno);
                        }
                    }
                    else
                    {
                        retorno = null;
                    }

                }
                else
                {
                    var mm = new Digita_Texto(titulo, maxlenght, Valor, titulo);
                    mm.ShowDialog();
                    if (mm.DialogResult.HasValue && mm.DialogResult.Value)
                    {
                        retorno = mm.caixa_texto.Text;
                        if (gravar_carregar && retorno != null)
                        {
                            INI.Set(arq, "Prompt", chave, retorno);
                        }
                    }
                    else
                    {
                        retorno = null;
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
        public static string Prompt(this string valor, string titulo = "Digite", int max_lenght = 50, bool multilinha = false, bool salvar_carregar = false, [CallerMemberName] string salvar_carregar_chave = "")
        {
            var retorno = MainPrompt(titulo, valor, salvar_carregar, "", multilinha, max_lenght);
            if (retorno == null)
            {
                return null;
            }
            else
            {
                return retorno.ToString();
            }
        }
        public static int? Prompt(this int valor, string titulo = "Digite")
        {
            var retorno = ((double)valor).Prompt(titulo, 0, "N0");
            if (retorno != null)
            {
                return (int)retorno;
            }
            return null;
        }
        public static double? Prompt(this double valor, string titulo = "Digite", int decimais = -1, string string_format = null, bool gravar_carregar = false, string chave = "")
        {
            if (decimais < 0)
            {
                decimais = Decimais;
            }
            var retorno = MainPrompt(titulo, valor, gravar_carregar, chave, false, 15, string_format);
            if (retorno == null)
            {
                return null;
            }
            else
            {
                return retorno.Double(decimais);
            }
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Conexoes
{
    public static class Extensoes_String
    {
        public static string GetKey(this string txt)
        {
            if(txt !=null)
            {
                if(txt.Length>0)
                {
                    return txt.ToUpper().Replace(" ", "").Replace(".", "");
                }
            }
            return txt;
        }
        public static bool StartsW(this string txt, params string[] valores)
        {
            foreach(var valor in valores)
            {
            var tem = txt.TrimStart().StartsWith(valor);
                if (tem)
                {
                    return tem;
                }
            }
            return false;
        }
        public static string getLetra(this int indice)
        {
            return getLetra((long)indice);
        }
        public static string getLetra(this long indice)
        {
            char[] alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            long max = alfabeto.Count() - 1;
            if (indice <= max)
            {
                return alfabeto[indice].ToString();
            }
            else
            {
                string retorno = "";
                long sub = indice;
                long resto = indice;

                while (sub > 0)
                {

                    resto = sub % max;
                    sub = (long)Math.Round((double)(sub / max));
                    retorno = alfabeto[resto] + retorno;
                }

                return retorno;

            }
        }
        public static bool IsLower(this string valor)
        {
            return valor.Any(char.IsLower);
        }
        public static bool ESoNumero(this string str)
        {
            if (str == null)
            {
                return false;
            }
            if (str.Length == 0)
            {
                return false;
            }
            foreach (char c in str.ToUpper().Replace(",", "").Replace(".", "").Replace(" ", "").Replace("-", "").Replace("+", "").Replace("E", ""))
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
        public static bool CaracteresEspeciais(this string valor)
        {
            var regex2 = new Regex("^[a-zA-Z0-9]*$");
            return !regex2.IsMatch(valor.Replace("-", "").Replace("_", "").Replace(" ", ""));
        }
        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
        public static string TrimEnd(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }
        public static string RemoveAspas(this string txt)
        {
            return txt.Replace(@"""", "");
        }
        public static string Esquerda(this string Origem, int MaxComp, bool pontilhado = false)
        {
            string txt = Origem;
            if (txt.Length > MaxComp)
            {
                txt = Origem.Substring(0, MaxComp) + (pontilhado ? ".." : "");
            }
            return txt;
        }

        public static string Direita(this string Origem, int Comp)
        {
            string txt = Origem;
            if(Comp<txt.Length)
            {
                return txt.Substring(txt.Length - Comp, Comp);
            }

            return Origem;
        }


        public static string RemoverNumeros(this string Nome)
        {
            return Regex.Replace(Nome, @"[\d-]", string.Empty);
        }
        public static string RemoverTextos(this string Nome, bool manter_sinais = false)
        {
            if (manter_sinais)
            {
                return Regex.Replace(Nome, "[^0-9.+-]", "");
            }
            else
            {
                return Regex.Replace(Nome, "[^0-9.]", "");
            }
        }
        public static string RemoverCaracteresEspeciais(this string texto)
        {
            if (texto == null) { return null; }
            texto = texto.TrimStart().TrimEnd();
            if (texto.Length == 0)
            {
                return "";
            }
            var normalizar = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizar.Length);

            for (int i = 0; i < normalizar.Length; i++)
            {
                char c = normalizar[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var ret = stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);

            return Regex.Replace(ret, @"[^0-9a-zA-Z-]+", "_");
        }
    }
}

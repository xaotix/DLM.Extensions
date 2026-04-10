using Conexoes.Janelas;
using DLM.db;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace Conexoes
{
    public static class Extensoes_Enum
    {
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member == null)
                return value.ToString();
            var attribute = member.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
    public static class Extensoes_HTML
    {
        public static string ToStringNull(this object item)
        {
            if (item.IsNullOrEmpty())
            {
                return null;
            }
            else if (item is Celula)
            {
                var valor = ((Celula)item).Valor;

                return valor.NotNullOrEmpty() ? valor : null;
            }
            return item.ToString();
        }
        public static string RemoverAtributoHTML(this string html, string atributo, string substituto = "")
        {
            // Regex para encontrar o atributo dentro de qualquer tag
            string pattern = $@"\s{atributo}\s*=\s*""[^""]*""";

            // Remove todas as ocorrências do atributo
            string resultado = Regex.Replace(html, pattern, substituto, RegexOptions.IgnoreCase);

            return resultado;
        }
        public static string RemoverTagHtml(this string html, string tag, string substituto = "")
        {
            // Regex para remover a tag de abertura e fechamento
            string pattern = $@"</?{tag}\b[^>]*>";
            string resultado = Regex.Replace(html, pattern, substituto, RegexOptions.IgnoreCase);

            return resultado;
        }
    }
    public static class Extensoes_String
    {
        public static string Sanitize(this string str)
        {
            if(str!=null)
            {
                return str.TrimEnd().TrimStart().ToUpper().RemoverCaracteresEspeciais().Replace(" ","_");
            }
            return str;
        }
        public static List<string> SplitTabulation(this string str)
        {
            string[] partes = str.Split(new[] { "\r\n", "\n", "\r", "\t", "\v", "\f", "\u2028", "\u2029" }, StringSplitOptions.RemoveEmptyEntries);
            return partes.ToList();
        }
        public static string Upper(this string txt)
        {
            return txt?.ToUpper();
        }
        public static bool Contem(this object item, string valor, double porcentagem = 70)
        {
            if (item == null) { return false; }
            if (valor.IsNullOrEmpty()) { return true; }
            else
            {



                valor = valor.Upper().TrimStart().TrimEnd();
                var descricao_item = item.ToString().Upper();

                if (item is Celula)
                {
                    var cel = item as Celula;
                    descricao_item = $"{cel.ColunaUpper}={cel.ToString()}";
                }

                if (valor == descricao_item)
                {
                    return true;
                }
                else if (descricao_item.Contem(valor))
                {
                    return true;
                }
                else
                {
                    var chaves = valor.Replace("  ", " ").Split(' ').ToList().FindAll(y => y.Replace(" ", "").Count() > 2);

                    int cc = 0;
                    foreach (string chave in chaves)
                    {
                        if (descricao_item.Contem(chave))
                        {
                            cc++;
                        }
                    }

                    if (cc > 0)
                    {
                        double x = 100 * cc / chaves.Count().Double();
                        return (x >= porcentagem);
                    }
                    else
                    {
                        return false;
                    }

                }
            }
        }
        public static string ToPEP(this string valor)
        {
            var retorno = "";
            var pep = valor.Replace(" ", "").Replace("-", "").Replace(".", "");
            //10-123456.P00.001.30A.F2
            //setor atividade
            if (pep.LenghtStr() > 1)
            {
                retorno += $"{pep.Substring(0, 2)}";
            }
            //contrato
            if (pep.LenghtStr() > 7)
            {
                retorno += $"-{pep.Substring(2, 6)}";
            }
            //pedido
            if (pep.LenghtStr() > 10)
            {
                retorno += $".{pep.Substring(8, 3)}";
            }
            //etapa
            if (pep.LenghtStr() > 13)
            {
                retorno += $".{pep.Substring(11, 3)}";
            }
            //sub-etapa
            if (pep.LenghtStr() > 16)
            {
                retorno += $".{pep.Substring(14, 3)}";
            }
            //pep
            if (pep.LenghtStr() > 18)
            {
                retorno += $".{pep.Substring(17, 2)}";
            }

            return retorno;
        }
        public static int LenghtStr(this string valor)
        {
            if (valor != null)
            {
                if (valor is string)
                {
                    return ((string)valor).Length;
                }
                return valor.ToString().Length;
            }

            return 0;
        }
        public static bool NotNullOrEmpty(this object valor, bool decimais = true)
        {
            return !valor.IsNullOrEmpty(decimais);
        }
        public static bool IsNullOrEmpty(this object valor, bool decimais = true)
        {
            if (valor == null) { return true; }



            var str = valor.ToString();
            if (valor is Celula)
            {
                str = ((Celula)valor).Valor;
            }

            if (decimais)
            {
                if (str.LenghtStr() == 1)
                {
                    if (str == "") { return true; }
                    if (str == " ") { return true; }
                    if (str == "0") { return true; }

                    if (str == ".") { return true; }
                    if (str == ",") { return true; }
                    if (str == "'") { return true; }
                }

                else
                {
                    if (str == "0.0") { return true; }
                    if (str == "0,0") { return true; }
                    if (str.Replace("0000-00-00", "").LenghtStr() == 0) { return true; }
                    if (str.Replace("0", "").Replace(",", "").Replace(".", "").LenghtStr() == 0) { return true; }
                    if (str == "0.0d") { return true; }
                }
            }
            else if (str.LenghtStr() == 2)
            {
                if (str == "[]") { return true; }
                else if (str == "{}") { return true; }
            }
            return false;
        }
        public static string GetKey(this string txt)
        {
            if (!txt.IsNullOrEmpty(false))
            {
                return txt.Upper().Replace(" ", "").Replace(".", "");
            }
            return txt;
        }
        public static string FirstCharToUpper(this string text)
        {

            if (text == null)
            {
                return null;
            }
            else if (text.LenghtStr() == 0)
            {
                return "";
            }
            else if (text.LenghtStr() == 1)
            {
                return text.Upper();
            }
            else
            {
                var str_join = "";
                var strs = text.Split(' ').ToList();
                for (int i = 0; i < strs.Count; i++)
                {
                    var st = strs[i];
                    if (i > 0)
                    {
                        str_join += " ";
                    }
                    if (st.LenghtStr() > 0)
                    {
                        if (st.LenghtStr() == 1)
                        {
                            str_join += st.Upper();
                        }
                        else
                        {
                            str_join += char.ToUpper(st[0]) + st.Substring(1).ToLower();
                        }
                    }
                    else
                    {
                        str_join += " ";
                    }
                }
                return str_join;
            }
        }
        public static bool StartsW(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (text.TrimStart().StartsWith(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool EndsW(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (text.TrimStart().EndsWith(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool EqualsOne(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (value == text)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool NotEquals(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (value == text)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool Contem(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (text.TrimStart().Contains(value))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public static string Remover(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return text;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    text = text.Replace(value, "");
                }
            }

            return text;
        }
        public static string Substituir(this string text, string new_value, params string[] old_values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return text;
            }
            foreach (var value in old_values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    text = text.Replace(value, new_value);
                }
            }

            return text;
        }
        public static bool ContemTudo(this string text, params string[] values)
        {
            if (text.IsNullOrEmpty(false))
            {
                return false;
            }
            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty(false))
                {
                    if (!text.TrimStart().Contains(value))
                    {
                        return false;
                    }
                }
            }
            return true;
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
                    sub = (long)((double)(sub / max)).Round(0);
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
            if (str.LenghtStr() == 0)
            {
                return false;
            }
            foreach (char c in str.Upper().Remover(",", ".", " ", "-", "+", "E"))
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
            string result = target;
            while (result.StartsW(trimString))
            {
                result = result.Substring(trimString.LenghtStr());
            }

            return result;
        }
        public static string TrimEnd(this string target, string trimString)
        {
            string result = target;
            while (result.EndsW(trimString))
            {
                result = result.Substring(0, result.LenghtStr() - trimString.LenghtStr());
            }

            return result;
        }
        public static string RemoveAspas(this string txt)
        {
            return txt.Replace(@"""", "");
        }

        /// <summary>
        /// Remove caracteres duplicados que estão lado a lado
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="remover"></param>
        /// <returns></returns>
        public static string RemoverDuplicatas(this string texto, string remover)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            string t = regex.Replace(texto, remover);
            return t;
        }
        public static string Esquerda(this string stxt, int MaxComp, bool pontilhado = false)
        {
            string txt = stxt;
            if (txt.LenghtStr() > MaxComp)
            {
                txt = txt.Substring(0, MaxComp) + (pontilhado ? "..." : "");
            }
            return txt;
        }

        public static string CortarStringDireita(this string txt, int comp)
        {
            if (txt.Length > comp)
            {
                return txt.Substring(txt.Length - comp);
            }
            else
            {
                return "";
            }
        }

        public static string Direita(this string stxt, int comp)
        {
            string txt = stxt;
            if (comp < txt.LenghtStr())
            {
                return txt.Substring(txt.LenghtStr() - comp, comp);
            }

            return txt;
        }


        public static string RemoverNumeros(this string txt)
        {
            return Regex.Replace(txt, @"[\d-]", string.Empty);
        }
        public static string RemoverTextos(this string txt, bool manter_sinais = false)
        {
            if (manter_sinais)
            {
                return Regex.Replace(txt, "[^0-9.+-]", "");
            }
            else
            {
                return Regex.Replace(txt, "[^0-9.]", "");
            }
        }
        /// <summary>
        /// Remove os espaços iniciais e finais
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string TrimTxt(this string txt)
        {
            if (txt.NotNullOrEmpty())
            {
                return txt.TrimStart().TrimEnd();
            }
            return txt;
        }
        public static string NormalizarTexto(this string txt)
        {
            if (txt.IsNullOrEmpty())
                return txt;

            txt = txt.Replace("°", "o");
            txt = Regex.Replace(txt, "[\u2010-\u2015\u2212\u00AD]", "-");
            // Normaliza para decompor caracteres acentuados (ex: "é" -> "e" + acento)
            string sem_acento = txt.Normalize(NormalizationForm.FormD);

            // Remove marcas de acento (diacríticos)
            var sem_acento_diacritico = new StringBuilder();
            foreach (var c in sem_acento)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sem_acento_diacritico.Append(c);
            }

            // Normaliza de volta para FormC
            string retorno = sem_acento_diacritico.ToString().Normalize(NormalizationForm.FormC);

            // Remove caracteres especiais, deixando apenas letras, números e espaço
            //semAcento = Regex.Replace(semAcento, @"[^a-zA-Z0-9\s]", "");
            retorno = Regex.Replace(retorno, @"[^a-zA-Z0-9\s/\\,.!?%*+-:@]", "");

            // Substitui múltiplos espaços por apenas um
            retorno = Regex.Replace(retorno, @"\s+", " ");

            // Remove espaços iniciais
            return retorno.TrimStart().TrimEnd();
        }
        public static string RemoverCaracteresEspeciais(this string txt)
        {
            if (txt == null) { return null; }
            txt = txt.TrimStart().TrimEnd();
            if (txt.LenghtStr() == 0)
            {
                return "";
            }
            var normalizar = txt.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizar.LenghtStr());

            for (int i = 0; i < normalizar.LenghtStr(); i++)
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


        private static readonly object _randomLock = new object();
        private static readonly Random _random = new Random();

        private const string CharsDefault = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string RandomString(this int length, string chars = CharsDefault)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "O comprimento deve ser maior que zero.");
            if (string.IsNullOrEmpty(chars))
                throw new ArgumentException("O conjunto de caracteres nao pode ser vazio.", "chars");

            char[] buffer = new char[length];
            lock (_randomLock)
            {
                for (int i = 0; i < length; i++)
                    buffer[i] = chars[_random.Next(chars.Length)];
            }
            return new string(buffer);
        }
    }
}

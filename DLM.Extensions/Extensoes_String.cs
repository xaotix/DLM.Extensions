using DLM.db;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Conexoes
{
    public static class Extensoes_HTML
    {
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
        public static bool Contem(this object item, string valor, double porcentagem = 70)
        {
            if (item == null) { return false; }
            if (valor.IsNullOrEmpty()) { return true; }
            else
            {



                valor = valor.ToUpper().TrimStart().TrimEnd();
                var descricao_item = item.ToString().ToUpper();

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
                else if (str.LenghtStr() == 2)
                {
                    if (str == "[]") { return true; }
                    else if (str == "{}") { return true; }
                }
                else
                {
                    if (str == "0.0") { return true; }
                    if (str.Replace("0000-00-00", "").LenghtStr() == 0) { return true; }
                    if (str.Replace("0", "").Replace(",","").Replace(".","").LenghtStr() == 0) { return true; }
                    if (str == "0.0d") { return true; }
                }
            }
            return false;
        }
        public static string GetKey(this string txt)
        {
            if (!txt.IsNullOrEmpty(false))
            {
                return txt.ToUpper().Replace(" ", "").Replace(".", "");
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
                return text.ToUpper();
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
                            str_join += st.ToUpper();
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
        public static string Esquerda(this string Origem, int MaxComp, bool pontilhado = false)
        {
            string txt = Origem;
            if (txt.LenghtStr() > MaxComp)
            {
                txt = Origem.Substring(0, MaxComp) + (pontilhado ? "..." : "");
            }
            return txt;
        }

        public static string Direita(this string Origem, int Comp)
        {
            string txt = Origem;
            if (Comp < txt.LenghtStr())
            {
                return txt.Substring(txt.LenghtStr() - Comp, Comp);
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
            if (texto.LenghtStr() == 0)
            {
                return "";
            }
            var normalizar = texto.Normalize(NormalizationForm.FormD);
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
    }
}

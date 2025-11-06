using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Conexoes
{
    public static class Extensoes_Double
    {
        public static bool IsValid(this double d)
        {
            if (Double.IsNaN(d) || Double.IsPositiveInfinity(d) || Double.IsNegativeInfinity(d))
            {
                return false;
            }
            return true;
        }
        public static string ToPCT(this double num, int decimais = 2)
        {
            if (num == 0)
            {
                return "";
            }
            return num.ToString($"P{decimais}");
        }
        public static string ToFin(this double num, int decimais = 3)
        {
            bool neg = false;
            if (num < 0)
            {
                neg = true;
                num = num.Abs();
            }

            var ret = num.ToString($"N{decimais}");

            if (neg)
            {
                ret = $"({ret})";
            }
            return ret;
        }
        public static string ToPeso(this object value)
        {
            var _value = "";
            var negativo = false;
            var culture = Utilz._BR;
            if (value is double doubleValue)
            {
                if (doubleValue < 0)
                {
                    negativo = true;
                }

                doubleValue = doubleValue.Abs();
                if (doubleValue == 0)
                {
                    _value = "";
                }
                else if (doubleValue > 9999)
                {
                    _value = string.Format(culture, "{0:N2} mil t", doubleValue / 1000);
                }
                else if (doubleValue > 999)
                {
                    _value = string.Format(culture, "{0:N0} t", doubleValue);
                }
                else if (doubleValue > 99)
                {
                    _value = string.Format(culture, "{0:N1} t", doubleValue);
                }
                else if (doubleValue > 9)
                {
                    _value = string.Format(culture, "{0:N2} t", doubleValue);
                }
                else if (doubleValue > 1)
                {
                    _value = string.Format(culture, "{0:N3} t", doubleValue);
                }
                else if (doubleValue > 0)
                {
                    _value = string.Format(culture, "{0:N0} Kg", doubleValue * 1000);
                }
                else
                {
                    _value = "";
                }
            }
            else
            {
                _value = "";
            }

            if (negativo)
            {
                _value = $"-{_value}";
            }

            return _value;
        }
        public static string ToMoeda(this object value, string prefix = "R$ ")
        {
            string _value = "";
            bool negativo = false;
            var culture = Utilz._BR;
            if (value is double doubleValue)
            {
                if (doubleValue < 0)
                {
                    negativo = true;
                    doubleValue = Math.Abs(doubleValue);
                }

                if (doubleValue == 0)
                {
                    _value = "";
                }
                else if (doubleValue > 999_999_999)
                {
                    _value = $"{(doubleValue / 1_000_000_000).ToString("0.###", culture)} bi";
                }
                else if (doubleValue > 99_999_999)
                {
                    _value = $"{(doubleValue / 1_000_000).ToString("0.##", culture)} mi";
                }
                else if (doubleValue > 999_999)
                {
                    _value = $"{(doubleValue / 1_000_000).ToString("0.###", culture)} mi";
                }
                else if (doubleValue > 9_999)
                {
                    _value = $"{(doubleValue / 1_000).ToString("0.#", culture)} mil";
                }
                else if (doubleValue > 999)
                {
                    _value = $"{(doubleValue / 1_000).ToString("0.##", culture)} mil";
                }
                else
                {
                    _value = $"{doubleValue.ToString("0.##", culture)}";
                }
            }

            _value = $"{prefix}{_value}";

            if (negativo && !string.IsNullOrEmpty(_value))
            {
                _value = $"({_value})";
            }

            return _value;
        }
        public static string ToKMB(this double num, string prefix = "")
        {
            var retorno = "";
            var neg = num < 0;
            if (neg)
            {
                num = num.Abs();
            }
            if (num == 0)
            {
                retorno = "";
            }
            else if (num > 999999999 || num < -999999999)
            {
                retorno = num.ToString("0,,,.### bi", CultureInfo.InvariantCulture);
            }
            else if (num > 999999 || num < -999999)
            {
                retorno = num.ToString("0,,.## mi", CultureInfo.InvariantCulture);
            }
            else if (num > 999 || num < -999)
            {
                retorno = num.ToString("0,.K", CultureInfo.InvariantCulture);
            }
            else
            {
                retorno = prefix + (num/1000).ToString("N1") +  " K";
            }

            retorno = $"{prefix}{retorno}";

            if (neg)
            {
                retorno = $"({retorno})";
            }

            return retorno;
        }
        public static string ToKg(this double num)
        {
            if (num == 0)
            {
                return "";
            }
            else if (num > 999 | num < -999)
            {
                num = num / 1000;

                return num.ToString("N0") + " t";
            }

            return num.ToString("N0") + " kg";
        }
        public static double Stretch_X(this double mm_comp, double mm_metro_adicional)
        {
            var metros = mm_comp / 1000;
            var soma = metros * mm_metro_adicional;
            return soma + mm_comp;
        }
        public static List<double> MediaMovel(this List<double> valores)
        {
            var mediaMovel = new List<double>();

            // Calcula a média móvel para cada período
            for (int i = 0; i < valores.Count; i++)
            {
                // Calcula a média para o período atual
                double media = valores.Take(i + 1).Average();

                // Adiciona a média ao resultado
                mediaMovel.Add(media);
            }

            return mediaMovel;
        }
        public static List<double> GetRange(this double max, double comp, double min = 0)
        {
            var retorno = new List<double>();
            var valor = min;

            if (comp > 0)
            {
                while (valor < max)
                {
                    retorno.Add(valor);
                    valor += comp;
                }

                if (retorno.Count > 0)
                {
                    retorno.Add(max);
                }
            }




            return retorno;
        }
        public static List<double> Quebrar(this double Comp, double Comp_Max, double Comp_Min, double Transpasse)
        {
            var Retorno = new List<double>();
            if (Comp < Comp_Max)
            {
                Retorno.Add(Comp);
                return Retorno;
            }
            if (Comp < Comp_Min)
            {
                Retorno.Add(Comp_Min);
                return Retorno;
            }
            double inicio = 0;
            int qtd = (int)Math.Floor(Comp / (Comp_Max - Transpasse));
            double resto = Comp - ((Comp_Max - Transpasse) * qtd);
            if (resto < Comp_Min && resto > 0)
            {
                qtd = qtd - 1;
                resto = Comp - ((Comp_Max - Transpasse) * qtd);
            }

            if (resto > Comp_Max && (resto - Comp_Min) > Comp_Min)
            {
                inicio = Comp_Min;
                resto = resto - Comp_Min;
            }
            else if (resto > Comp_Max && (resto / 2) > Comp_Min)
            {
                inicio = resto / 2;
                resto = resto / 2;
            }


            if (inicio > 0)
            {
                Retorno.Add(inicio);
            }

            for (int i = 0; i < qtd; i++)
            {
                Retorno.Add(Comp_Max);
            }
            if (resto > 0)
            {

                Retorno.Add(resto);
            }
            return Retorno.OrderByDescending(x => x).ToList();
        }
        public static double Abs(this double valor)
        {
            return Math.Abs(valor);
        }
        public static bool E_Multiplo(this double valor, double divisor)
        {
            if (divisor != 0)
            {
                double divisao = (valor * 10000 / divisor * 10000);
                return divisao % 1 == 0;
            }
            else
            {
                return true;
            }
        }
        public static bool E_Inteiro(this double valor)
        {
            return (valor % 1) == 0;
        }

        public static bool E_Par(this int valor)
        {
            return valor % 2 == 0;
        }
        public static bool Igual(this double valor, double comparar, double tol = 1)
        {
            return valor >= comparar - tol && valor <= comparar + tol;
        }
        public static double Porcentagem(this double Valor, double Maximo, int Decimais = 2)
        {
            try
            {
                var t = (100 * Valor / Maximo).Round(Decimais);
                if (t >= 0)
                {
                    return t;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public static double Round(this double valor, int decimais)
        {
            if (decimais > 10)
            {
                decimais = 10;
            }
            else if (decimais < 0)
            {
                decimais = 4;
            }
            return Math.Round(valor, decimais);
        }
        public static float Round(this float valor, int decimais)
        {
            if (decimais > 10)
            {
                decimais = 10;
            }
            return (float)Math.Round(valor, decimais);
        }

        public static double ArredondarMultiplo(this double valor, double multiplo)
        {
            var valor_fim = valor;

            var neg = valor_fim < 0;
            if (neg)
            {
                valor_fim = valor_fim.Abs();
            }
            if (multiplo <= 0)
            {
                valor_fim = valor;
            }
            else if (multiplo == 1)
            {
                valor_fim = valor.Round(0);
            }
            else if (valor % multiplo == 0)
            {
                valor_fim = valor;
            }
            else if (valor < multiplo)
            {
                valor_fim = multiplo;
            }
            else
            {
                var diferenca = ((multiplo + valor % multiplo) - multiplo);
                if (diferenca > 0)
                {
                    if (diferenca / multiplo > 0.5)
                    {
                        valor_fim = (multiplo - valor % multiplo) + valor;
                    }
                    else
                    {
                        valor_fim = valor - ((multiplo + valor % multiplo) - multiplo);
                    }

                    if (valor_fim < 0)
                    {

                    }
                }
            }


            if (valor_fim == 0)
            {
                valor_fim = multiplo;
            }
            if (valor_fim < valor.Round(1) && multiplo > 1)
            {
                valor_fim += multiplo;
            }

            if (neg)
            {
                valor_fim *= -1;
            }
            return valor_fim;
        }
        public static double ArredondarMultiplo(this double valor, int multiplo)
        {
            if (multiplo == 0)
            {
                return valor;
            }

            if (valor % multiplo == 0) return valor;

            var retorno = Math.Ceiling(valor / multiplo) * multiplo;
            if (retorno < valor)
            {
                retorno += multiplo;
            }
            return retorno;
        }

        public static List<List<double>> AgruparPorDistancia(this List<double> valores, double dist)
        {
            var lista = new List<double>();
            lista.AddRange(valores);
            lista = lista.OrderBy(x => x).ToList();

            var pacotes = new List<List<double>>();

            for (int i = 0; i < lista.Count; i++)
            {
                var pacote = new List<double>();
                double dist0 = 0;
                while (dist0 <= dist)
                {
                    pacote.Add(lista[i]);
                    i++;

                    if (i == lista.Count)
                    {
                        pacotes.Add(pacote);
                        goto fim;
                    }
                    else
                    {
                        dist0 = lista[i] - pacote[0];
                    }
                }
                i--;
                pacotes.Add(pacote);
            }
        fim:

            return pacotes;
        }
    }
}

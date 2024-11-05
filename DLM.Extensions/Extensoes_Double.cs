using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_Double
    {
        public static string ToPCT(this double num, int decimais = 2)
        {
            return num.ToString($"P{decimais}");
        }
        public static string ToFin(this double num, int decimais = 3)
        {
            bool neg = false;
            if(num<0)
            {
                neg = true;
                num = num.Abs();
            }

            var ret = num.ToString($"N{decimais}");

            if(neg)
            {
                ret = $"({ret})";
            }
            return ret;
        }
        public static string ToKMB(this double num)
        {
            if (num > 999999999 || num < -999999999)
            {
                return num.ToString("0,,,.### bi", CultureInfo.InvariantCulture);
            }
            else if (num > 999999 || num < -999999)
            {
                return num.ToString("0,,.## mi", CultureInfo.InvariantCulture);
            }
            else if (num > 999 || num < -999)
            {
                return num.ToString("0,.K", CultureInfo.InvariantCulture);
            }
            else
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
        }
        public static string ToKg(this double num)
        {
            if(num > 999 | num < -999)
            {
                num = num / 1000;

                return num.ToString("N0") + " Ton";
            }

            return num.ToString("N0") + " Kg";
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

            if(comp>0)
            {
                while (valor < max)
                {
                    retorno.Add(valor);
                    valor += comp;
                }

                if(retorno.Count>0)
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
                var t = Math.Round(100 * Valor / Maximo, Decimais);
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

        public static double Round(this double valor, int decimais = 4)
        {
            return Math.Round(valor, decimais);
        }

        public static double ArredondarMultiplo(this double valor, double multiplo)
        {
            var valor_fim = valor;

            var neg = valor_fim < 0;
            if(neg)
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
            else if(valor<multiplo)
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

            
            if(valor_fim == 0)
            {
                valor_fim = multiplo;
            }
            if (valor_fim < valor.Round(1) && multiplo>1)
            {
                valor_fim += multiplo;
            }

            if(neg)
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

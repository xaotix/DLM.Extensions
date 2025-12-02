using DLM;
using DLM.vars;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Conexoes
{
    public static class Extensoes_Conversao
    {
        public static string String(this double Valor, int decimais, int padleft = 0, char padding = '0')
        {
            if (decimais >= 0)
            {
                return Valor.Round(decimais).ToString($"F{decimais}", CultureInfo.InvariantCulture).PadLeft(padleft, padding);
            }
            else
            {
                return Valor.ToString().PadLeft(padleft, padding);
            }
        }

        public static string String(this int Valor, int padleft = 0, char padding = '0')
        {
            return Valor.ToString().PadLeft(padleft, padding);
        }
        public static string String(this DateTimeOffset? Valor, string format = "dd/MM/yyyy")
        {
            return Valor != null ? Valor.Value.String(format) : "";
        }
        public static string String(this DateTimeOffset Valor, string format = "dd/MM/yyyy")
        {
            return Valor != null ? Valor.ToString(format) : "";
        }
        public static string String(this DateTime? Valor, string format = "dd/MM/yyyy")
        {
            return Valor != null ? Valor.Value.ToString(format) : "";
        }
        public static string String(this string Valor, int padleft = 0, char padding = '0')
        {
            return Valor.ToString().PadLeft(padleft, padding);
        }
        public static string String<TEnum>(this TEnum value, int padLeft = 0, char padding = '0') where TEnum : Enum
        {
            return value.ToString().PadLeft(padLeft, padding);
        }

        public static string String(this long Valor, int padleft = 0, char padding = '0')
        {
            return Valor.ToString().PadLeft(padleft, padding);
        }
        public static string String(this long? valor, int padleft = 0, char padding = '0')
        {
            return valor != null ? valor.Value.String(padleft, padding) : null;
        }
        public static double Double(this decimal? valor)
        {
            return valor != null ? (double)valor.Value : 0;
        }
        public static PesoStr PesoStr(this string valor, int decimais = 8)
        {
            return new PesoStr(valor.Double(decimais));
        }
        public static RSStr RSStr(this string valor, int decimais = 8)
        {
            return new RSStr(valor.Double(decimais));
        }
        public static double? DoubleNull<T>(this T comp, int decimais = 8)
        {
            if (comp == null) { return null; }
            return comp.Double(decimais);
        }
        public static double Double<T>(this T comp, int Decimais = 8)
        {
            bool negativo = false;
            double valor_final_retorno = 0;
            if (comp == null)
            {
                return 0;
            }

            if (Decimais > 10)
            {
                Decimais = 10;
            }

            if (comp is decimal or double or float)
            {
                return Convert.ToDouble(comp).Round(Decimais);
            }

            double valor_final = 0;
            try
            {
                var str = comp.ToString().Replace(" ", "").Replace("%", "").Replace("@", "").Replace("#", "");
                if (str.EndsW("-") | str.StartsW("-"))
                {
                    str = str.TrimEnd('-').TrimStart("-");
                    negativo = true;
                }

                if (double.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._BR, out valor_final))
                {

                }
                else if (double.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._US, out valor_final))
                {
                }
            }
            catch (Exception)
            {
            }
            valor_final_retorno = valor_final;
            if (Decimais >= 0)
            {
                if ((valor_final % 1) != 0)
                {
                    valor_final_retorno = valor_final.Round(Decimais);
                }
            }


            if (negativo)
            {
                return -valor_final_retorno;
            }
            else
            {
                return valor_final_retorno;
            }
        }
        public static decimal? DecimalNull<T>(this T comp, int decimais = 8)
        {
            if(comp == null) { return null; }
            return comp.Decimal(decimais);
        }
        public static decimal Decimal<T>(this T comp, int Decimais = 8)
        {
            bool negativo = false;
            decimal valor_final_retorno = 0;
            if (comp == null)
            {
                return 0;
            }

            if (Decimais > 10)
            {
                Decimais = 10;
            }

            if (comp is decimal or double or float)
            {
                return Convert.ToDecimal(comp).Round(Decimais);
            }

            decimal valor_final = 0;
            try
            {
                var str = comp.ToString().Replace(" ", "").Replace("%", "").Replace("@", "").Replace("#", "");
                if (str.EndsW("-") | str.StartsW("-"))
                {
                    str = str.TrimEnd('-').TrimStart("-");
                    negativo = true;
                }

                if (decimal.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._BR, out valor_final))
                {

                }
                else if (decimal.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._US, out valor_final))
                {
                }
            }
            catch (Exception)
            {
            }
            valor_final_retorno = valor_final;
            if (Decimais >= 0)
            {
                if ((valor_final % 1) != 0)
                {
                    valor_final_retorno = valor_final.Round(Decimais);
                }
            }


            if (negativo)
            {
                return -valor_final_retorno;
            }
            else
            {
                return valor_final_retorno;
            }
        }


        public static long? LongNull<T>(this T comp)
        {
            if (comp == null) { return null; }
            string comps = comp.ToString();
            if (comps == "") { comps = "0"; }
            try
            {
                return Convert.ToInt64(Math.Ceiling(Double(comps.Replace(".", ","))));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static long Long<T>(this T comp)
        {
            if (comp == null) { return 0; }
            string comps = comp.ToString();
            if (comps == "") { comps = "0"; }
            try
            {
                return Convert.ToInt64(Math.Ceiling(Double(comps.Replace(".", ","))));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static int? IntNull<T>(this T comp)
        {

            if (comp == null) { return null; }
            string comps = comp.ToString().Replace(" ", "");
            if (comps == "") { comps = "0"; }
            try
            {
                return Convert.ToInt32(Math.Ceiling(Double(comps.Replace(".", ","))));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static int Int<T>(this T comp)
        {

            if (comp == null) { return 0; }
            string comps = comp.ToString().Replace(" ", "");
            if (comps == "") { comps = "0"; }
            try
            {
                return Convert.ToInt32(Math.Ceiling(Double(comps.Replace(".", ","))));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static double Double(this double vlr, int decimais = 0)
        {
            return vlr.Round(decimais);
        }
        public static double Double(this float vlr, int decimais = 0)
        {
            return vlr.Round(decimais);
        }
        public static double Double(this int vlr)
        {
            return (double)vlr;
        }
        public static int Int(this double vlr)
        {
            return (int)vlr.Round(0);
        }
        public static bool? BooleanNull<T>(this T obj)
        {
            if(obj == null) { return null; }
            return obj.Boolean();
        }
        public static bool Boolean<T>(this T obj)
        {
            if (obj == null) { return false; }
            var valor = obj.ToString();

            if (valor == "1" | valor == "X" | valor == "x" | valor == "Y" | valor == "S" | valor == "true" | valor == "True")
            {
                return true;
            }

            valor = valor.ToUpper().Replace(" ", "");
            if (valor == "TRUE" | valor == "1" | valor == "X" | valor == "YES" | valor == "SIM" | valor == "Y" | valor == "S" | valor == "ON")
            {
                return true;
            }
            return false;
        }
        public static DateTime? DataNull<T>(this T Data)
        {
            if (Data != null)
            {
                return GetDateTime(Data.ToString());
            }

            return null;
        }

        public static DateTime? GetDateTime(this string vlr)
        {
            if (vlr.LenghtStr() > 0)
            {
                try
                {
                    if (!vlr.Contem("0000") && !vlr.Contem("0001"))
                    {
                        if (vlr.Contem(@"/", "-"))
                        {
                            var pcs = vlr.Split('/', '-', ' ', 'T').Select(x => x.Int()).ToList();
                            if (pcs.Count() >= 3)
                            {
                                var dt = new DateTime();
                                if (pcs[0] > 1000)
                                {
                                    dt = new DateTime(pcs[0], pcs[1], pcs[2]);
                                }
                                else
                                {
                                    dt = new DateTime(pcs[2], pcs[1], pcs[0]);
                                }

                                if (pcs.Count() > 3)
                                {
                                    var times = vlr.Split(' ', 'T');
                                    if (times.Count() > 1)
                                    {
                                        if (times[1].Replace(":", "").Replace("0", "").LenghtStr() > 0)
                                        {
                                            var hrs = times[1].Split(':', '-');
                                            if (hrs.Count() >= 3)
                                            {
                                                dt = dt.AddHours(hrs[0].Double());
                                                dt = dt.AddMinutes(hrs[1].Double());
                                                dt = dt.AddSeconds(hrs[2].Double());
                                            }
                                        }
                                    }
                                }

                                return dt;
                            }
                        }
                        return Convert.ToDateTime(vlr);
                    }
                }
                catch (Exception ex)
                {
                    DLM.log.Log(ex);
                }
            }
            return null;
        }

        public static DateTime Data<T>(this T Data)
        {
            if (Data != null)
            {
                var dt = GetDateTime(Data.ToString());
                if (dt != null)
                {
                    return dt.Value;
                }

            }

            return Cfg.Init.DataDummy;
        }
        public static double Double(this XElement x, int decimais = 9)
        {
            return x.Value.ToString().Double(decimais);
        }

    }
}

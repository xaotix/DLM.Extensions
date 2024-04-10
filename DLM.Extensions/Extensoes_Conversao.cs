using DLM.vars;
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
            return Valor.Round(decimais).ToString($"F{decimais}", CultureInfo.InvariantCulture).PadLeft(padleft, padding);
        }
        public static string String(this int Valor, int padleft = 0, char padding = '0')
        {
            return Valor.ToString().PadLeft(padleft, padding);
        }

        public static double Double<T>(this T comp, int Decimais = 8)
        {
            bool negativo = false;
            double valor_final_retorno = 0;
            if (comp == null)
            {
                return 0;
            }

            if (Decimais > 15)
            {
                Decimais = 15;
            }

            try
            {
                var str = comp.ToString().Replace(" ", "").Replace("%", "").Replace("@", "").Replace("#", "");
                if (str.EndsWith("-"))
                {
                    str = str.TrimEnd('-');
                    negativo = true;

                }
                else if (str.StartsW("-"))
                {
                    str = str.TrimStart('-');
                    negativo = true;
                }

                double valor_final;
                if (double.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._BR, out valor_final))
                {
                    try
                    {
                        if ((valor_final % 1) != 0)
                        {
                            valor_final_retorno = Math.Round(valor_final, Decimais);
                        }
                        else
                        {

                            valor_final_retorno = valor_final;
                        }

                    }
                    catch (Exception)
                    {

                    }
                }

                else if (double.TryParse(str, System.Globalization.NumberStyles.Float, Utilz._US, out valor_final))
                {
                    try
                    {

                        valor_final_retorno = Math.Round(valor_final, Decimais);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
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
            return Math.Round(vlr, decimais);
        }
        public static double Double(this float vlr, int decimais = 0)
        {
            return Math.Round(vlr, decimais);
        }
        public static double Double(this int vlr)
        {
            return (double)vlr;
        }
        public static int Int(this double vlr)
        {
            return (int)Math.Round(vlr);
        }
        public static bool Boolean<T>(this T obj)
        {
            if (obj == null) { return false; }
            var valor = obj.ToString().ToUpper().Replace(" ", "");
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
                var vlr = Data.ToString();
                if (vlr.Length > 0)
                {
                    try
                    {
                        if (!vlr.Contains("0000") && !vlr.Contains("0001"))
                        {
                            if (vlr.Contains(@"/") | vlr.Contains("-"))
                            {
                                var pcs = vlr.Split('/', '-', ' ').Select(x => x.Int()).ToList();
                                if (pcs.Count() >= 3)
                                {
                                    if (pcs[0] > 1000)
                                    {
                                        return new DateTime(pcs[0], pcs[1], pcs[2]);
                                    }
                                    else
                                    {
                                        return new DateTime(pcs[2], pcs[1], pcs[0]);
                                    }
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

            }

            return null;
        }
        public static DateTime Data<T>(this T Data)
        {
            if (Data != null)
            {
                var vlr = Data.ToString();
                if(vlr.Length>0)
                {
                    try
                    {
                        if (!vlr.Contains("0000") && !vlr.Contains("0001"))
                        {
                            if (vlr.Contains(@"/") | vlr.Contains("-"))
                            {
                                var pcs = vlr.Split('/', '-', ' ').Select(x => x.Int()).ToList();
                                if (pcs.Count() >= 3)
                                {
                                    if (pcs[0] > 1000)
                                    {
                                        return new DateTime(pcs[0], pcs[1], pcs[2]);
                                    }
                                    else
                                    {
                                        return new DateTime(pcs[2], pcs[1], pcs[0]);
                                    }
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
                
            }

            return Cfg.Init.DataDummy();
        }
        public static double Double(this XElement x, int decimais = 9)
        {
            return x.Value.ToString().Double(decimais);
        }

    }
}

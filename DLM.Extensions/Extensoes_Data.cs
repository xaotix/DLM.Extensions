using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Data
    {
        public static string ToShortDateString(this DateTime? dateTime)
        {
            if (dateTime != null)
            {
                return dateTime.Value.ToShortDateString();
            }
            return "";
        }
        public static DateTime LastDay(this DateTime data)
        {
            int diasNoMes = DateTime.DaysInMonth(data.Year, data.Month);
            var last = new DateTime(data.Year, data.Month, diasNoMes);
            return last;
        }
        public static DateTime? LastDay(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.LastDay();
            }
            return null;
        }
        public static DateTime? FirstDay(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.FirstDay();
            }
            return null;
        }
        public static DateTime FirstDay(this DateTime data)
        {
            var ret = new DateTime(data.Year, data.Month, 1);

            return ret;
        }
        public static List<DateTime> GetDatasMes(this DateTime data)
        {
            return GetRangeDatas(data.FirstDay(), data.LastDay());
        }
        public static List<DateTime> GetRangeDatas(this DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                var ssstart = new DateTime(startDate.Ticks);
                var ssend = new DateTime(endDate.Ticks);
                endDate = ssstart;
                startDate = ssend;
            }
            if ((endDate - startDate).Days < 1)
            {
                return new List<DateTime>();
            }
            var allDates = new List<DateTime>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                allDates.Add(date);



            return allDates;

        }
        public static List<string> GetRangeMeses(this DateTime inicio, DateTime fim)
        {
            var retorno = new List<string>();

            var dt = inicio.FirstDay();
            while (dt < fim)
            {
                retorno.Add(dt.ToString("MM/yyyy"));
                dt = dt.AddMonths(1);
            }

            return retorno;
        }
        public static List<int> GetRangeAnos(this DateTime inicio, DateTime fim)
        {
            var retorno = new List<int>();

            var ff = new DateTime(fim.Year, 01, 01);
            var ii = new DateTime(inicio.Year, 01, 01);
            while (ff > ii)
            {
                retorno.Add(ff.Year);
                ff = ff.AddYears(-1);
            }
            retorno.Add(fim.Year);
            retorno = retorno.Distinct().ToList();

            return retorno;
        }
        public static double GetPorcentagem(this DateTime data, DateTime inicio, DateTime fim)
        {
            if (data < inicio)
            {
                return 0;
            }
            else if (data > fim)
            {
                return 1;
            }
            else
            {
                var tempo_total = fim - inicio;
                var tempo_data = data - inicio;

                return tempo_data.TotalDays / tempo_total.TotalDays;
            }
        }
        public static DateTime? GetMin(this List<DateTime> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy);
            if (datas.Count > 0)
            {
                return datas.Min();
            }
            return null;
        }
        public static DateTime? GetMin(this List<DateTime?> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x != null).FindAll(x => x > DLM.vars.Cfg.Init.DataDummy);
            if (datas.Count > 0)
            {
                return datas.Min();
            }
            return null;
        }
        public static DateTime? GetMax(this List<DateTime> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy);
            if (datas.Count > 0)
            {
                return datas.Max();
            }
            return null;
        }
        public static DateTime? GetMax(this List<DateTime?> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy);
            if (datas.Count > 0)
            {
                return datas.Max();
            }
            return null;
        }

        public static int DiasUteis(this DateTime? inicio, DateTime? fim)
        {
            if (inicio != null && fim != null)
            {
                return inicio.Value.DiasUteis(fim.Value);
            }
            return 0;
        }
        public static int DiasUteis(this DateTime inicio, DateTime fim)
        {
            int days = 0;
            int daysCount = 0;
            days = inicio.Subtract(fim).Days;

            //Módulo 
            if (days < 0)
                days = days * -1;

            for (int i = 1; i <= days; i++)
            {
                inicio = inicio.AddDays(1);
                //Conta apenas dias da semana.
                if (inicio.DayOfWeek != DayOfWeek.Sunday && inicio.DayOfWeek != DayOfWeek.Saturday)
                    daysCount++;
            }
            return daysCount;
        }
    }
}

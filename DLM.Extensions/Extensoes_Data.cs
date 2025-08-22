using System;
using System.Collections.Generic;
using System.Globalization;
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
        public static DateTime LastDayOfMonth(this DateTime data)
        {
            int diasNoMes = DateTime.DaysInMonth(data.Year, data.Month);
            var last = new DateTime(data.Year, data.Month, diasNoMes);
            return last;
        }
        public static DateTime FirstDayOfMonth(this DateTime data)
        {
            var ret = new DateTime(data.Year, data.Month, 1);

            return ret;
        }
        public static int Week(this DateTime? data, DayOfWeek primeiroDiaSemana = DayOfWeek.Monday)
        {
            if (data != null)
            {
                return data.Value.Week(primeiroDiaSemana);
            }
            return 0;
        }
        public static int Week(this DateTime data, DayOfWeek primeiroDiaSemana = DayOfWeek.Monday)
        {
            var cultura = CultureInfo.InvariantCulture;
            var calendario = cultura.Calendar;

            // Define a regra ISO 8601: semana começa na segunda e a primeira semana tem pelo menos 4 dias
            CalendarWeekRule regraSemana = CalendarWeekRule.FirstFourDayWeek;

            return calendario.GetWeekOfYear(data, regraSemana, primeiroDiaSemana);
        }
        public static DateTime FirstDayOfWeek(this DateTime data)
        {
            // Considerando que a semana começa no domingo
            int diferenca = data.DayOfWeek - DayOfWeek.Sunday;
            if (diferenca < 0)
                diferenca += 7;

            return data.AddDays(-diferenca).Date;
        }
        public static DateTime LastDayOfWeek(this DateTime data)
        {
            // Considerando que a semana termina no sábado
            int diferenca = DayOfWeek.Saturday - data.DayOfWeek;
            if (diferenca < 0)
                diferenca += 7;

            return data.AddDays(diferenca).Date;
        }

        public static DataRange IntervaloSemana(this DateTime data, DayOfWeek primeiroDiaSemana = DayOfWeek.Monday)
        {
            return Conexoes.Utilz.Calendario.IntervaloSemana(data.Year, data.Week(), primeiroDiaSemana);
        }

        public static bool MaisRecente(this DateTime anterior, DateTime maisrecente)
        {
            int a = DateTime.Compare(anterior, maisrecente);
            return (a < 0);
        }

        public static DateTime CTimeToDate(this long ctime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(ctime).ToLocalTime();
        }
        public static long DateToCTime(this DateTime data)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((data.ToUniversalTime() - epoch).TotalSeconds);
        }


        public static List<DateTime> GetDatasMes(this DateTime data)
        {
            return GetRangeDatas(data.FirstDayOfMonth(), data.LastDayOfMonth());
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

            var dt = inicio.FirstDayOfMonth();
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
            retorno.Add(inicio.Year);
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
        public static DateTime? LastDayOfMonth(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.LastDayOfMonth();
            }
            return null;
        }
        public static DateTime? LastDayOfWeek(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.LastDayOfWeek();
            }
            return null;
        }
        public static DateTime? FirstDayOfMonth(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.FirstDayOfMonth();
            }
            return null;
        }
        public static DateTime? FirstDayOfWeek(this DateTime? data)
        {
            if (data != null)
            {
                return data.Value.FirstDayOfWeek();
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
    }
}

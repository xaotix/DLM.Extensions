using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Navigation;

namespace Conexoes
{
    public static class Extensoes_Data
    {
        public static bool ActualMonth(this DateTime data)
        {
            return data.Month == DateTime.Now.Month && data.Year == DateTime.Now.Year;
        }
        public static bool ActualMonth(this DateTime? data)
        {
            return data != null ? data.Value.ActualMonth() : false;
        }
        public static string DayOfWeek(this DateTime data)
        {
            CultureInfo cultura = Conexoes.Utilz._BR;
            return cultura.DateTimeFormat.GetDayName(data.DayOfWeek);
        }
        public static int Months(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }
        public static DateTime LastDayOfWeek(this int ano, int semana, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            return ano.FirstDayOfWeek(semana, primeiroDiaSemana).AddDays(7);
        }
        public static DateTime FirstDayOfWeek(this int ano, int semana, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            // Cria um objeto DateTime para o primeiro dia do ano
            var primeiroDiaDoAno = new DateTime(ano, 1, 1);

            // Obtém o CultureInfo atual para determinar o primeiro dia da semana
            var culture = CultureInfo.CurrentCulture;

            // Calcula o primeiro dia da primeira semana do ano
            // Encontra o primeiro dia da semana (ex.: segunda-feira)
            int diasParaPrimeiroDiaSemana = (int)primeiroDiaSemana - (int)primeiroDiaDoAno.DayOfWeek;
            if (diasParaPrimeiroDiaSemana > 0)
                diasParaPrimeiroDiaSemana -= 7;

            DateTime inicioPrimeiraSemana = primeiroDiaDoAno.AddDays(diasParaPrimeiroDiaSemana);

            // Calcula o primeiro dia da semana desejada
            DateTime primeiroDiaDaSemana = inicioPrimeiraSemana.AddDays((semana - 1) * 7);

            // Verifica se a data resultante está dentro do ano especificado
            if (primeiroDiaDaSemana.Year > ano)
                throw new ArgumentException("A semana especificada está fora do ano informado.");

            return primeiroDiaDaSemana;
        }
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
        public static DateTime LastDayOfYear(this DateTime data)
        {
            var last = new DateTime(data.Year, 12, 31);
            return last;
        }
        public static DateTime FirstDayOfYear(this DateTime data)
        {
            var last = new DateTime(data.Year, 01, 01);
            return last;
        }
        public static DateTime AddWeeks(this DateTime data, int weeks, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            var firs = data.FirstDayOfWeek(primeiroDiaSemana);
            while (weeks > 0)
            {
                firs = firs.AddDays(7);
                weeks--;
            }
            return firs;
        }
        public static DateTime FirstDayOfMonth(this DateTime data)
        {
            var ret = new DateTime(data.Year, data.Month, 1);

            return ret;
        }
        public static int Week(this DateTime? data, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            if (data != null)
            {
                return data.Value.Week(primeiroDiaSemana);
            }
            return 0;
        }
        public static int Week(this DateTime data, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            var cultura = CultureInfo.InvariantCulture;
            var calendario = cultura.Calendar;

            // Define a regra ISO 8601: semana começa na segunda e a primeira semana tem pelo menos 4 dias
            CalendarWeekRule regraSemana = CalendarWeekRule.FirstFourDayWeek;

            return calendario.GetWeekOfYear(data, regraSemana, primeiroDiaSemana);
        }
        public static DateTime FirstDayOfWeek(this DateTime data, DayOfWeek primeiroDiaSemana = System.DayOfWeek.Sunday)
        {
            // Considerando que a semana começa no domingo
            int diferenca = data.DayOfWeek - primeiroDiaSemana;
            if (diferenca < 0)
                diferenca += 7;

            return data.AddDays(-diferenca).Date;
        }
        public static DateTime LastDayOfWeek(this DateTime data)
        {
            // Considerando que a semana termina no sábado
            int diferenca = System.DayOfWeek.Saturday - data.DayOfWeek;
            if (diferenca < 0)
                diferenca += 7;

            return data.AddDays(diferenca).Date;
        }

        public static DataRange IntervaloSemana(this DateTime data)
        {
            var semana = data.Week();
            return new DataRange(data.Year, semana);
        }
        public static DataRange IntervaloSemana(this int ano, int semana)
        {
            return new DataRange(ano, semana);
        }

        public static bool MostRecent(this DateTime anterior, DateTime maisrecente)
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

        public static string WeekStr(this DateTime data)
        {
            var week = data.Week();
            return $"{data.Year.String(4)}.S{week.String(2)}";
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
                retorno.Add(dt.String("MM/yyyy"));
                dt = dt.AddMonths(1);
            }

            return retorno;
        }
        public static List<string> GetRangeSemanas(this DateTime inicio, DateTime fim)
        {
            var retorno = new List<string>();

            var data = inicio.FirstDayOfWeek();
            while (data < fim)
            {
                var semana = data.Week();

                retorno.Add(data.WeekStr());
                data = data.AddDays(7);
            }

            retorno.Add(fim.WeekStr());
            retorno = retorno.Distinct().ToList();
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
                if (inicio.DayOfWeek != System.DayOfWeek.Sunday && inicio.DayOfWeek != System.DayOfWeek.Saturday)
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

        public static DateTime? GetMax(this IEnumerable<DateTime> dateTimes)
        {
            return dateTimes.ToList().GetMax();
        }
        public static DateTime? GetMax(this IEnumerable<DateTime?> dateTimes)
        {
            return dateTimes.ToList().GetMax();
        }

        public static DateTime? GetMin(this IEnumerable<DateTime> dateTimes)
        {
            return dateTimes.ToList().GetMin();
        }
        public static DateTime? GetMin(this IEnumerable<DateTime?> dateTimes)
        {
            return dateTimes.ToList().GetMin();
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

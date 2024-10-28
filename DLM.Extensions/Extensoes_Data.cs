using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Data
    {
        public static DateTime LastDay(this DateTime data)
        {
            int diasNoMes = DateTime.DaysInMonth(data.Year, data.Month);
            var last = new DateTime(data.Year, data.Month, diasNoMes);
            return last;
        }
        public static DateTime FirstDay(this DateTime data)
        {
            var ret = new DateTime(data.Year, data.Month, 1);

            return ret;
        }
        public static List<string> GetRangeMeses(this DateTime inicio, DateTime fim)
        {
            var retorno = new List<string>();

            var dt = inicio.FirstDay();
            while(dt<fim)
            {
                retorno.Add(dt.ToString("MM/yyyy"));
                dt = dt.AddMonths(1);
            }
            
            return retorno;
        }
        public static double GetPorcentagem(this DateTime data, DateTime inicio, DateTime fim)
        {
            if(data<inicio)
            {
                return 0;
            }
            else if( data > fim)
            {
                return 1;
            }
            else
            {
                var tempo_total = fim - inicio;
                var tempo_data = data-inicio;

                return tempo_data.TotalDays / tempo_total.TotalDays;
            }
        }
        public static DateTime? GetMin(this List<DateTime> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy());
            if(datas.Count>0)
            {
                return datas.Min();
            }
            return null;
        }
        public static DateTime? GetMin(this List<DateTime?> dateTimes)
        {
            var datas = dateTimes.FindAll(x=>x!=null).FindAll(x => x > DLM.vars.Cfg.Init.DataDummy());
            if (datas.Count > 0)
            {
                return datas.Min();
            }
            return null;
        }
        public static DateTime? GetMax(this List<DateTime> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy());
            if (datas.Count > 0)
            {
                return datas.Max();
            }
            return null;
        }
        public static DateTime? GetMax(this List<DateTime?> dateTimes)
        {
            var datas = dateTimes.FindAll(x => x > DLM.vars.Cfg.Init.DataDummy());
            if (datas.Count > 0)
            {
                return datas.Max();
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Data
    {
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

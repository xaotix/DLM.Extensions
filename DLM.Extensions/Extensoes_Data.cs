using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Data
    {
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

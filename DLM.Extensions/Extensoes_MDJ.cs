using DLM.mdj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conexoes
{
   public static class Extensoes_MDJ
    {
        public static List<double> XS(this List<MDJ_Furo> furos)
        {
            return furos.Select(x => x.X).Distinct().ToList().OrderBy(x => x).ToList();
        }
        public static List<double> EntreFuros(this List<MDJ_Furo> furos)
        {
            var ret = new List<double>();
            var frs = XS(furos);
            if (frs.Count > 1)
            {
                for (int i = 1; i < frs.Count; i++)
                {
                    ret.Add(Math.Round(frs[i] - frs[i - 1]));
                }
            }
            return ret;
        }
    }
}

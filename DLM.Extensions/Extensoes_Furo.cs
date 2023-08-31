using Conexoes;
using DLM.desenho;
using DLM.encoder;
using System.Collections.Generic;
using System.Linq;

namespace DLM.cam
{
    public static class Extensoes_Furo
    {
        public static TecnoPlotFuro GetDB(this Furo fr)
        {
            return Conexoes.DBases.GetDBTecnoPlot().Get(fr.Diametro, fr.Oblongo);
        }
        public static List<Furo> AlinharX(this List<Furo> fros, double tolerancia)
        {
            var grp_X = fros.Select(x => x.Origem.X.Round(0)).Distinct().ToList();
            List<Furo> lista_controle = new List<Furo>();
            List<Furo> lista_alinhada = new List<Furo>();
            foreach (var fr in grp_X)
            {
                var frs = fros.FindAll(x => lista_controle.Find(y => y.ToString() == x.ToString()) == null).FindAll(x => x.Origem.X.Round(0) == fr);
                lista_controle.AddRange(frs);
                lista_alinhada.AddRange(frs.Select(x => x.SetX(fr)));
                var frs_prox = fros.FindAll(x => lista_controle.Find(y => y.ToString() == x.ToString()) == null).FindAll(x => x.Origem.X.Round(0) >= fr - tolerancia && x.Origem.X.Round(0) <= fr + tolerancia);
                lista_controle.AddRange(frs_prox);
                lista_alinhada.AddRange(frs_prox.Select(x => x.SetX(fr)));
            }
            return lista_alinhada;
        }
    }
}

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
        public static List<Furo> ChapaParaMesa(this List<Furo> items, double algura, double largura_mesa)
        {
            var retorno = new List<Furo>();
            if (items.Count >= 3)
            {
                var meio = largura_mesa / 2;
                foreach (var item in items)
                {
                    var nliv = new Furo(item.Origem.X, algura, item.Diametro,item.Oblongo,item.Angulo, item.Origem.Y + meio);
                    retorno.Add(nliv);
                }
            }

            return retorno;
        }
        public static List<Furo> AlinharX(this List<Furo> fros, double tolerancia)
        {
            var grp_X = fros.Select(x => x.Origem.X.Round(0)).Distinct().ToList();
            var lista_controle = new List<Furo>();
            var lista_alinhada = new List<Furo>();
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

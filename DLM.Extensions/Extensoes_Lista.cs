using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_Lista
    {
        public static List<T> Mirror<T>(this List<T> Origem)
        {
            List<T> retorno = new List<T>();
            for (int i = Origem.Count - 1; i >= 0; i--)
            {
                retorno.Add(Origem[i]);
            }

            return retorno;
        }
        public static List<T> Get<T>(this List<object> items)
        {
            return items.FindAll(x => x is T).Cast<T>().ToList();
        }
        public static List<T> Simplificar<T>(this List<T> Origem)
        {
            return Origem.GroupBy(x => x).Select(g => g.First()).ToList();
        }
        public static T MaiorOcorrencia<T>(this List<T> lista)
        {
            if (lista.Count == 1)
            {
                return lista[0];
            }
            else if (lista.Count == 0)
            {
                return default(T);
            }

            return (from i in lista
                    group i by i into grp
                    orderby grp.Count() descending
                    select grp.Key).First();
        }
        public static List<List<T>> Inverter<T>(this List<List<T>> ts)
        {
            return ts
                    .SelectMany(inner => inner.Select((item, index) => new { item, index }))
                    .GroupBy(i => i.index, i => i.item)
                    .Select(g => g.ToList())
                    .ToList();
        }
        public static List<List<List<T>>> Quebrar<T>(this List<List<T>> locations, int maximo = 30)
        {
            var list = new List<List<List<T>>>();

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
        public static List<List<T>> Quebrar<T>(this List<T> locations, int maximo = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
        public static int GetPosition<T>(this T objeto, List<T> objetos)
        {
            for (int i = 0; i < objetos.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(objetos[i], objeto))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

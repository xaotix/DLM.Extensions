using DLM.db;
using DLM.encoder;
using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Conexoes
{
    public static class Extensoes_Lista
    {
        public static List<Report> Agrupar(this IEnumerable<Report> _reports)
        {
            return _reports.GroupBy(x => x.Descricao).Select(x => new Report(x.ToList())).ToList();
        }
        public static DLM.db.Tabela GetTabela<T>(this List<T> Origem, [System.Runtime.CompilerServices.CallerMemberName] string nome = "")
        {
            return new DLM.db.Tabela(Origem.Select(x => x.GetLinha()).ToList(), nome);
        }
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
        public static List<T> RemoveAll<T>(this List<T> items, List<T> remove)
        {
            foreach (var item in remove)
            {
                items.Remove(item);
            }
            return items;
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

        public static List<List<T>> Quebrar<T>(this IEnumerable<T> lista, int maximo = 30)
        {
            var list = new List<List<T>>();

            var locations = new List<T>(lista);

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
        public static List<List<Linha>> Quebrar(this Tabela locations, int maximo = 30)
        {
            var list = new List<List<Linha>>();

            for (int i = 0; i < locations.Count; i += maximo)
            {
                list.Add(locations.ToList().GetRange(i, Math.Min(maximo, locations.Count - i)));
            }

            return list;
        }
        public static int GetPosition<T>(this T objeto, List<T> objetos)
        {
            return objetos.IndexOf(objeto);
        }

        public static List<T> MoveTop<T>(this List<T> list, List<T> mover)
        {
            List<T> result = new List<T>(list);
            var orderedMovers = mover.OrderBy(m => list.IndexOf(m)).ToList();

            foreach (var item in orderedMovers) result.Remove(item);
            result.InsertRange(0, orderedMovers);

            return result;
        }

        public static List<T> MoveBottom<T>(this List<T> list, List<T> mover)
        {
            List<T> result = new List<T>(list);
            var orderedMovers = mover.OrderBy(m => list.IndexOf(m)).ToList();

            foreach (var item in orderedMovers) result.Remove(item);
            result.AddRange(orderedMovers);

            return result;
        }

        public static List<T> MoveUP<T>(this List<T> list, List<T> mover)
        {
            List<T> result = new List<T>(list);
            // Ordena de cima pra baixo. Itens no topo precisam se mover primeiro para não encavalar.
            var orderedMovers = mover.OrderBy(m => list.IndexOf(m)).ToList();

            for (int i = 0; i < orderedMovers.Count; i++)
            {
                var item = orderedMovers[i];
                int index = result.IndexOf(item);

                // Só move para cima se não estiver no topo absoluto possível para aquele grupo
                if (index > i)
                {
                    result.RemoveAt(index);
                    result.Insert(index - 1, item);
                }
            }
            return result;
        }

        public static List<T> MoveDown<T>(this List<T> list, List<T> mover)
        {
            List<T> result = new List<T>(list);
            // Ordena de baixo para cima. Itens no fundo precisam descer primeiro.
            var orderedMovers = mover.OrderByDescending(m => list.IndexOf(m)).ToList();

            for (int i = 0; i < orderedMovers.Count; i++)
            {
                var item = orderedMovers[i];
                int index = result.IndexOf(item);

                // Só move para baixo se não bater no limite inferior do grupo
                if (index < result.Count - 1 - i)
                {
                    result.RemoveAt(index);
                    result.Insert(index + 1, item);
                }
            }
            return result;
        }
        public static List<T> Move<T>(this List<T> list, List<T> mover, int places)
        {
            var nlist = new List<T>(list);
            var indexes = mover.Select(item => nlist.IndexOf(item)).ToList();

            if (indexes.Any(idx => idx == -1))
                throw new ArgumentException("Todos os itens a serem movidos devem estar presentes na lista original.");

            // Removendo os itens para reposicioná-los corretamente
            foreach (var item in mover)
            {
                nlist.Remove(item);
            }

            // Calculando novos índices, garantindo que estejam dentro dos limites
            var newIndexes = indexes.Select(idx => Math.Max(0, Math.Min(nlist.Count, idx + places))).ToList();

            // Inserindo os itens nos novos locais
            for (int i = 0; i < mover.Count; i++)
            {
                nlist.Insert(newIndexes[i], mover[i]);
            }

            return nlist;
        }


    }
}

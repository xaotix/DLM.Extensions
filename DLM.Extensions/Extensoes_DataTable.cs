using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Conexoes
{
    public static class ExtensoesDataTable
    {
        public static List<List<object>> ToList(this DataTable dataTable)
        {
            var valores = dataTable.Rows.Cast<DataRow>().ToList().Select(x => x.ItemArray.ToList()).ToList();
            return valores;
        }
        public static bool IsNullOrEmpty(this DataTable table)
        {
            if (table == null) return true;
            if (table.Rows.Count < 1) return true;
            return false;
        }
        public static T ToObject<T>(this DataTable table) where T : new()
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (table.Rows.Count < 1) throw new ArgumentException("No rows in DataTable");
            foreach (DataRow r in table.Rows)
            {
                return ToObject<T>(r);
            }
            return default(T);
        }
        public static T ToObject<T>(this DataRow row) where T : new()
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            T item = new T();
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            foreach (var property in properties)
            {
                property.SetValue(item, row[property.Name], null);
            }
            return item;
        }
        public static List<dynamic> ToListDynamic(this DataTable table)
        {
            List<dynamic> ret = new List<dynamic>();
            if (table == null || table.Rows.Count < 1) return ret;

            foreach (DataRow curr in table.Rows)
            {
                dynamic dyn = new ExpandoObject();
                foreach (DataColumn col in table.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[col.ColumnName] = curr[col];
                }
                ret.Add(dyn);
            }

            return ret;
        }
        public static dynamic ToDynamic(this DataTable table)
        {
            dynamic ret = new ExpandoObject();
            if (table == null || table.Rows.Count < 1) return ret;
            if (table.Rows.Count != 1) throw new ArgumentException("DataTable must contain only one row.");

            foreach (DataRow curr in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    var dic = (IDictionary<string, object>)ret;
                    dic[col.ColumnName] = curr[col];
                }

                return ret;
            }

            return ret;
        }
        public static List<Dictionary<string, object>> ToListDictionary(this DataTable table)
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            if (table == null || table.Rows.Count < 1) return ret;

            foreach (DataRow curr in table.Rows)
            {
                Dictionary<string, object> currDict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    currDict.Add(col.ColumnName, curr[col]);
                }

                ret.Add(currDict);
            }

            return ret;
        }
        public static Dictionary<string, object> ToDictionary(this DataTable table)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            if (table == null || table.Rows.Count < 1) return ret;
            if (table.Rows.Count != 1) throw new ArgumentException("DataTable must contain only one row.");

            foreach (DataRow curr in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    ret.Add(col.ColumnName, curr[col]);
                }

                return ret;
            }

            return ret;
        }

        public static DataTable ToDataTable(this string arquivo)
        {
            var linhas = Utilz.Arquivo.Ler(arquivo).Select(x => x.Split(";".ToCharArray()).ToList().Select(y => y.TrimEnd().TrimStart()).ToList()).ToList();
            return linhas.ToDataTable();
        }

        public static DataTable ToDataTable(this List<List<string>> linhas)
        {
            var retorno = new DataTable();
            if (linhas.Count > 0)
            {
                var colunas = linhas.Max(x => x.Count);
                for (int i = 0; i < colunas; i++)
                {
                    retorno.Columns.Add(new DataColumn($"COLUNA_{i}"));
                }
                foreach (var l in linhas)
                {
                    var linha = retorno.NewRow();
                    for (int i = 0; i < colunas; i++)
                    {
                        if (l.Count > i)
                        {
                            linha[i] = l[i];
                        }
                    }
                    retorno.Rows.Add(linha);
                }
                return retorno;
            }
            else
            {
                return new DataTable();
            }
        }
    }
}

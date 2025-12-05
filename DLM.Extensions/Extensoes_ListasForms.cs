using DLM.db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Conexoes
{
    public static class Extensoes_ListasForms
    {
        public static List<T> Selecoes<T>(this System.Windows.Controls.ListBox lista)
        {
            try
            {
                var retorno = lista.SelectedItems.Cast<T>().ToList();
                if (retorno != null)
                {
                    return retorno;
                }

                return new List<T>();
            }
            catch (Exception)
            {

                return new List<T>();
            }
        }
        public static List<T> Selecoes<T>(this System.Windows.Controls.DataGrid lista)
        {
            try
            {
                var retorno = lista.SelectedItems.Cast<T>().ToList();
                if (retorno != null)
                {
                    return retorno;
                }

                return new List<T>();
            }
            catch (Exception)
            {

                return new List<T>();
            }
        }
        public static T Selecao<T>(this System.Windows.Controls.DataGrid lista)
        {

            if (lista.SelectedItems.Count > 0)
            {
                try
                {
                    return lista.SelectedItems.Cast<T>().ToList().First();
                }
                catch (Exception)
                {
                }
            }

            return (T)Convert.ChangeType(null, typeof(T));
        }
        public static T Selecao<T>(this object objeto)
        {

            if (objeto is DataGrid)
            {
                return (objeto as DataGrid).Selecao<T>();
            }
            else if (objeto is ListView)
            {
                return (objeto as ListView).Selecao<T>();
            }
            else if (objeto is ComboBox)
            {
                return (objeto as ComboBox).Selecao<T>();
            }


            return (T)Convert.ChangeType(null, typeof(T));
        }
        public static T Selecao<T>(this RoutedEventArgs e)
        {
            return (T)(e.OriginalSource as FrameworkElement).DataContext;
        }
        public static T Selecao<T>(this System.Windows.Controls.ListBox lista)
        {

            if (lista.SelectedItems.Count > 0)
            {
                try
                {
                    return lista.SelectedItems.Cast<T>().ToList().First();
                }
                catch (Exception)
                {
                }
            }

            return (T)Convert.ChangeType(null, typeof(T));
        }
        public static T Selecao<T>(this System.Windows.Controls.ComboBox lista)
        {

            if (lista.SelectedItem != null)
            {
                try
                {
                    return lista.SelectedItem.As<T>();
                }
                catch (Exception)
                {
                }
            }

            return (T)Convert.ChangeType(null, typeof(T));
        }
        public static List<T> ToList<T>(this System.Windows.Controls.DataGrid lista)
        {
            try
            {
                return lista.Items.Cast<T>().ToList();
            }
            catch (Exception)
            {

                return new List<T>();
            }
        }
        public static List<T> ToList<T>(this System.Windows.Controls.ListView lista)
        {
            try
            {
                return lista.Items.Cast<T>().ToList();
            }
            catch (Exception)
            {

                return new List<T>();
            }
        }
        public static List<T> SelectedItemsToList<T>(this System.Windows.Controls.ListView lista)
        {
            try
            {
                return lista.SelectedItems.Cast<T>().ToList();
            }
            catch (Exception)
            {

                return new List<T>();
            }
        }
        public static void Exportar(this System.Windows.Controls.DataGrid lista)
        {
            lista.GetTabela().GerarExcel(null, true, true);
        }

        private static Tabela GetTabela(this DataGrid lista, string nome = null)
        {
            var tabela = new Tabela(nome!=null?nome:lista.Name);

            var linhas = lista.Items.Cast<DataRowView>().ToList().Select(x => x.Row).ToList().Select(x => x.ItemArray.ToList()).ToList();

            if (linhas.Count > 0)
            {

                var headers = lista.Items.Cast<DataRowView>().ToList()[0].Row.Table.Columns
                            .Cast<DataColumn>().Select(c => (object)c.ColumnName).ToList();


                foreach (var linha in linhas)
                {
                    var l = new Linha();
                    for (var i = 0; i < linha.Count; i++)
                    {
                        l.Add(headers[i].ToString(), linha[i]);
                    }
                    tabela.Add(l);
                }
            }

            return tabela;
        }

        public static void AlimentaDataGrid(this System.Windows.Forms.DataGridView lista, Tabela tabela)
        {
            lista.Rows.Clear();
            lista.Columns.Clear();
            int c = 1;
            foreach (string Cab in tabela.GetColunas())
            {
                lista.Columns.Add("N_" + c, Cab);
                c++;
            }


            foreach (var linha in tabela)
            {
                lista.Rows.Add(linha.Valores().ToArray());
            }

        }
        public static void AliemntaDataGrid(this System.Windows.Controls.DataGrid lista, Tabela tabela)
        {
            lista.ItemsSource = null;
            lista.ItemsSource = tabela.GetDataTable().AsDataView();
        }


    }
}

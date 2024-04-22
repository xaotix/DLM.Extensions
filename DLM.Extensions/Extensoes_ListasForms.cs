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
            else if(objeto is ComboBox)
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

            if (lista.SelectedItem!=null)
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
            if (lista == null) { return; }
            try
            {
                List<object> itens = lista.Items.Cast<object>().ToList();
                if (itens.Count > 0)
                {
                    var destino = "xlsx".SalvarArquivo();
                    if (destino == null) { return; }
                    itens.GetTabela(true).GerarExcel(destino, true, true);
                }

            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }

            return;
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


            foreach (var linha in tabela.Linhas)
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

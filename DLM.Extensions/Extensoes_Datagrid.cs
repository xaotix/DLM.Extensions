using DLM.db;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Conexoes
{
    public static class ExtensoesDatagrid
    {

        /// <summary>
        /// Retorna um Dataview com as mesmas colunas que estão aparecendo no datagrid.
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static DataView GetDataView(this DataGrid dg)
        {
            if (dg == null) throw new ArgumentNullException(nameof(dg));

            // Commit edits to ensure valores atuais
            dg.CommitEdit(DataGridEditingUnit.Row, true);
            dg.CommitEdit();

            var dt = new DataTable();

            // selecionar colunas visíveis e ordenar por DisplayIndex
            var visibleCols = dg.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            // criar colunas no DataTable com os headers
            foreach (var col in visibleCols)
            {
                var header = col.Header?.ToString() ?? string.Empty;
                // garantir nomes únicos
                var colName = MakeUniqueColumnName(dt, header);
                dt.Columns.Add(colName, typeof(object));
            }

            // iterar itens visíveis no DataGrid (respeita CollectionView)
            foreach (var item in dg.Items)
            {
                if (item == CollectionView.NewItemPlaceholder) continue;

                var values = new object[visibleCols.Count];
                for (int i = 0; i < visibleCols.Count; i++)
                {
                    var col = visibleCols[i];
                    object cellValue = null;

                    // DataGridBoundColumn (Binding)
                    if (col is DataGridBoundColumn boundCol)
                    {
                        var binding = boundCol.Binding as Binding;
                        if (binding != null && !string.IsNullOrEmpty(binding.Path?.Path))
                        {
                            cellValue = GetPropertyValueByPath(item, binding.Path.Path);
                        }
                        else
                        {
                            // fallback: tentar GetCellContent
                            cellValue = GetCellDisplayedText(dg, col, item);
                        }
                    }
                    // DataGridTemplateColumn (template)
                    else if (col is DataGridTemplateColumn templateCol)
                    {
                        cellValue = GetCellDisplayedText(dg, col, item);
                    }
                    else
                    {
                        // outros tipos de coluna
                        cellValue = GetCellDisplayedText(dg, col, item);
                    }

                    values[i] = cellValue ?? DBNull.Value;
                }

                dt.Rows.Add(values);
            }

            return dt.DefaultView;
        }

        private static string MakeUniqueColumnName(DataTable dt, string baseName)
        {
            var name = string.IsNullOrWhiteSpace(baseName) ? "Column" : baseName;
            var unique = name;
            int idx = 1;
            while (dt.Columns.Contains(unique))
            {
                unique = $"{name}_{idx++}";
            }
            return unique;
        }

        private static object GetCellDisplayedText(DataGrid dg, DataGridColumn column, object item)
        {
            // GetCellContent pode retornar null se a linha não estiver gerada (virtualização)
            var content = column.GetCellContent(item);
            if (content == null)
            {
                // fallback: tentar ler via binding se for DataGridBoundColumn
                if (column is DataGridBoundColumn boundCol)
                {
                    var binding = boundCol.Binding as Binding;
                    if (binding != null && !string.IsNullOrEmpty(binding.Path?.Path))
                        return GetPropertyValueByPath(item, binding.Path.Path);
                }
                return null;
            }

            // se o conteúdo for um TextBlock (caso comum), retornar o texto
            if (content is TextBlock tb) return tb.Text;

            // se for ContentPresenter, tentar extrair visual
            if (content is ContentPresenter cp)
            {
                // procurar TextBlock dentro do ContentPresenter
                var tbInside = FindVisualChild<TextBlock>(cp);
                if (tbInside != null) return tbInside.Text;
            }

            // fallback para ToString do conteúdo
            return content.ToString();
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static object GetPropertyValueByPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path)) return null;

            var current = obj;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                if (current == null) return null;

                // suportar indexadores simples? (não implementado aqui)
                var type = current.GetType();
                var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) return null;
                current = prop.GetValue(current);
            }
            return current;
        }
        /// <summary>
        /// Gera o Excel com as colunas iguais do Datagrid
        /// </summary>
        /// <param name="datagrid"></param>
        public static void GerarExcel(this DataGrid datagrid)
        {
            var destino = "xlsx".SalvarArquivo();
            if (destino != null)
            {
                var data = datagrid.GetDataView();
                var tbl = data.GetTabela();
                tbl.GerarExcel(destino, true, true);
            }
        }
        public static Tabela GetTabela(this DataView dv)
        {
            var result = new Tabela();
            if (dv == null) return result;

            var table = dv.Table;
            var columnNames = new List<string>();
            foreach (DataColumn col in table.Columns)
                columnNames.Add(col.ColumnName);

            foreach (DataRowView drv in dv)
            {
                var linha = new Linha();
                foreach (var colName in columnNames)
                {
                    var raw = drv.Row[colName];
                    var valor = raw == DBNull.Value ? null : raw;
                    linha.Add(new Celula(colName,valor));
                }
                result.Add(linha);
            }

            return result;
        }

        public static void ShowHideColumns(this System.Windows.Controls.DataGrid data, string prompt = "Selecione as colunas que deseja visualizar/ocultar")
        {
            var columns = data.Columns.ToList();
            var headers = columns.Select(x => x.Header.ToString()).Distinct().ToList().FindAll(x => x != "");
            var selecao = headers.ListaSelecionarVarios(true, prompt);

            if (selecao.Count > 0)
            {
                foreach (var column in columns)
                {
                    var header = column.Header.ToString();
                    if (header != "")
                    {
                        var igual = selecao.Find(x => x == header);
                        if (igual != null)
                        {
                            column.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            column.Visibility = Visibility.Collapsed;
                        }
                    }

                }
            }
        }
        public static System.Windows.Controls.DataGrid GetParent(this System.Windows.Controls.DataGridCell cell)
        {
            DependencyObject datag = cell as DependencyObject;

            while (datag != null && !(datag is System.Windows.Controls.DataGrid))
            {
                datag = VisualTreeHelper.GetParent(datag);
            }

            if (datag == null) { return null; }

            return datag as System.Windows.Controls.DataGrid;
        }
        public static void Select(this DataGridCell cell)
        {
            var grid = cell.GetParent();
            int linha = grid.SelectedIndex;
            int coluna = grid.Columns.IndexOf(grid.CurrentColumn);
            grid.SetCell(linha, coluna);
            grid.BeginEdit();
        }
        public static System.Windows.Controls.DataGridCell SetCell(this System.Windows.Controls.DataGrid grid, int line, int column)
        {
            try
            {
                if (line >= 0 && column >= 0 && line < grid.Items.Count && column < grid.Columns.Count)
                {
                    DataGridCellInfo cell_info = new DataGridCellInfo(grid.Items[line], grid.Columns[column]);
                    grid.CurrentCell = cell_info;
                    grid.CurrentItem = cell_info.Item;
                    grid.SelectedItem = cell_info.Item;
                }
            }
            catch (Exception)
            {

            }

            return GetCurrentCell(grid);

        }
        public static System.Windows.Controls.DataGridCell GetCurrentCell(this System.Windows.Controls.DataGrid grid)
        {
            if (grid.CurrentColumn == null) { return null; }
            DataGridRow rowContainer = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
            if (rowContainer != null)
            {

                System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = rowContainer.GetChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>();
                System.Windows.Controls.DataGridCell cell = (System.Windows.Controls.DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(grid.Columns.IndexOf(grid.CurrentColumn));

                return cell;

            }
            return null;
        }
    }
}

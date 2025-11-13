using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Conexoes
{
    public static class ExtensoesDatagrid
    {
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

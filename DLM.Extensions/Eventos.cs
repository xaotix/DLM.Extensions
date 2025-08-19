using DataGridExtensions;
using DLM.cam;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Conexoes
{
    public static class Eventos
    {
        public static ItemsControlFilter SetFiltro(this System.Windows.Controls.ItemsControl _lista, System.Windows.Controls.TextBox _filtro)
        {
            var filtro = new ItemsControlFilter(_lista, _filtro);

            return filtro;
        }
        public static void ResetSource<T>(this System.Windows.Controls.DataGrid grid, List<T> lista)
        {
            grid.ItemsSource = null;
            var filter = grid.GetFilter();
            filter?.Clear();

            grid.ItemsSource = lista;
        }
        /// <summary>
        /// Adiciona um evento quando o ItemSource muda
        /// </summary>
        /// <param name="_list"></param>
        /// <param name="_itemsource_changed"></param>
        public static void ItemSourceChanged(this ItemsControl _list, EventHandler _itemsource_changed)
        {
            var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
            if (dpd != null)
            {
                dpd.AddValueChanged(_list, _itemsource_changed);
            }
        }
        public static void datagrid_navega_celulas(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int coluna_texto = 0;

            Direcao direcao = Direcao.Centro;

            var celula_atual = (System.Windows.Controls.DataGridCell)sender;
            if (celula_atual == null) { return; }
            var grid = celula_atual.GetParent();
            if (grid == null) { return; }
            int linha = grid.SelectedIndex;
            int coluna = grid.Columns.IndexOf(grid.CurrentColumn);

            if (e.Key == Key.Up) { direcao = Direcao.Cima; }
            else if ((e.Key == Key.Down) | e.Key == Key.Enter) { direcao = Direcao.Baixo; }
            else if ((e.Key == Key.Left) | (e.KeyboardDevice.IsKeyDown(Key.LeftShift) && e.Key == Key.Tab)) { direcao = Direcao.Esquerda; }
            else if ((e.Key == Key.Right) | e.Key == Key.Tab) { direcao = Direcao.Direita; } else { return; }

            var textos_anteriores = celula_atual.GetChildren<System.Windows.Controls.TextBox>().ToList();
            var texto_com_foco = textos_anteriores.Find(x => x.IsFocused);

            //se for na mesma linha
            for (int i = 0; i < textos_anteriores.Count; i++)
            {
                if (textos_anteriores[i].IsFocused)
                {
                    coluna_texto = i;
                    if (direcao == Direcao.Esquerda && i > 0)
                    {
                        //seleciona a caixa de texto anterior
                        coluna_texto = i - 1; textos_anteriores[coluna_texto].Focus(); return;
                    }
                    else if (direcao == Direcao.Direita && i < textos_anteriores.Count - 1)
                    {
                        //seleciona a caixa de texto próxima
                        coluna_texto = i + 1; textos_anteriores[coluna_texto].Focus(); return;
                    }
                }
            }

            switch (direcao)
            {
                case Direcao.Esquerda:
                    coluna--;
                    break;
                case Direcao.Direita:
                    coluna++;
                    break;
                case Direcao.Cima:
                    linha--;
                    break;
                case Direcao.Baixo:
                    linha++;
                    break;
                case Direcao.Centro:
                    break;
            }

            var celula_nova = grid.SetCell(linha, coluna);

            var texto = celula_nova.FindVisualChild<TextBox>();
            if (celula_nova.Content is TextBlock)
            {
                grid.BeginEdit();
            }


            var textos_novos = celula_nova.GetChildren<System.Windows.Controls.TextBox>().ToList();
            while (textos_novos.Count == 0 && celula_nova != null && (direcao == Direcao.Esquerda | direcao == Direcao.Direita) && coluna > 0 && linha > 0 && coluna < grid.Columns.Count && linha < grid.Items.Count)
            {
                celula_nova = grid.SetCell(linha, coluna);
                textos_novos = celula_nova.GetChildren<System.Windows.Controls.TextBox>().ToList();
                switch (direcao)
                {
                    case Direcao.Esquerda:
                        coluna--;
                        break;
                    case Direcao.Direita:
                        coluna++;
                        break;

                }
            }

            if (celula_nova is System.Windows.Controls.DataGridCell)
            {
                if (textos_novos.Count > 0 && textos_novos.Find(x => x.IsFocused) == null)
                {
                    if (direcao == Direcao.Esquerda)
                    {
                        //se for pra esquerda, ele pega a última caixa de texto da coluna
                        coluna_texto = textos_novos.Count - 1;
                    }
                    else if (direcao == Direcao.Direita)
                    {
                        coluna_texto = 0;
                    }

                    System.Windows.Controls.TextBox textBox = null;
                    if (textos_novos.Count > coluna_texto)
                    {
                        textBox = textos_novos[coluna_texto];
                    }
                    else
                    {
                        textBox = textos_novos[0];
                    }
                    textBox.Focus();
                }
            }



        }
        public static void datagrid_key_down(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //cancela o enter ir pra proxima linha
            //base.OnKeyDown(e);
            if (e.Key == Key.Enter | e.Key == Key.Tab | e.Key == Key.Up | e.Key == Key.Down)
            {
                e.Handled = true;
            }
        }
        public static void EventoExpandir(object sender, RoutedEventArgs e)
        {
            var sel = (TreeViewItem)e.Source;
            if (sel != null)
            {
                if (sel.Tag != null)
                {
                    if (sel.Tag is Pasta)
                    {
                        var p = (Pasta)sel.Tag;
                        if (!p.Atualizou)
                        {
                            p.Update();
                        }
                    }
                }
            }

        }
        public static void SetEventosDragAndDrop(this TreeViewItem _No)
        {
            _No.Expanded -= Eventos.EventoExpandir;
            _No.DragEnter -= Eventos.TreeView_DragEnter;
            _No.DragLeave -= Eventos.TreeView_DragLeave;

            _No.Expanded += Eventos.EventoExpandir;
            _No.DragEnter += Eventos.TreeView_DragEnter;
            _No.DragLeave += Eventos.TreeView_DragLeave;
            _No.Drop += Eventos.TreeView_Drop;
            //_No.SetBinding(TreeViewItem.ToolTipProperty, Utilz.GetBinding("ToolTip", _No.Tag));

            _No.AllowDrop = true;
        }
        public static void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            var sel = sender as TreeViewItem;
            sel.Background = System.Windows.Media.Brushes.Cyan;
        }
        public static void TreeView_DragLeave(object sender, DragEventArgs e)
        {
            var sel = sender as TreeViewItem;
            sel.Background = System.Windows.Media.Brushes.Transparent;


            while (sel != null)
            {
                if (sel.Parent is TreeViewItem)
                {
                    sel = sel.Parent as TreeViewItem;
                    sel.Background = System.Windows.Media.Brushes.Transparent;
                }
                else
                {
                    sel = null;
                }
            }
        }

        public static void TreeView_Drop(object sender, DragEventArgs e)
        {
            var treeview = sender as TreeViewItem;
            var pasta = treeview.Tag as Pasta;
            var arquivos = (List<Arquivo>)e.Data.GetData("Selecao");
            if(arquivos.Count>0 && pasta.Exists())
            {
                arquivos.Mover(pasta.Endereco);
            }
        }

        public static void Datagrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var list = sender as DataGrid;
                var selecao = list.SelectedItems.Cast<Arquivo>().ToList();
                var obj = new DataObject("Selecao", selecao);
                DragDrop.DoDragDrop(list, obj, DragDropEffects.Copy);
            }
        }



      



    }
}

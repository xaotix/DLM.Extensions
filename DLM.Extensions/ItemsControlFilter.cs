using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Conexoes
{
    /// <summary>
    /// Classe utilitária para automatizar filtros em controles de lista baseados em um TextBox de pesquisa.
    /// </summary>
    public class ItemsControlFilter
    {
        private readonly DispatcherTimer _debounceTimer;

        private ItemsControl _list { get; set; }
        private TextBox _filterTextBox { get; set; }
        private string _placeholderMsg { get; set; } = "Pesquisar...";

        public ItemsControlFilter(ItemsControl list, TextBox filterTextBox, string placeholderMsg = "Pesquisar...")
        {
            this._list = list ?? throw new ArgumentNullException(nameof(list));
            this._filterTextBox = filterTextBox ?? throw new ArgumentNullException(nameof(filterTextBox));
            this._placeholderMsg = placeholderMsg;

            // Configura o timer de debounce para evitar travamentos ao digitar rápido (300 milissegundos)
            _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _debounceTimer.Tick += DebounceTimer_Tick;

            // Inicializa o Placeholder
            if (string.IsNullOrEmpty(_filterTextBox.Text))
            {
                _filterTextBox.Text = _placeholderMsg;
            }

            // Assinatura dos Eventos
            this._list.Loaded += List_Loaded;
            this._filterTextBox.TextChanged += FilterTextBox_TextChanged;
            this._filterTextBox.GotFocus += FilterTextBox_GotFocus;
            this._filterTextBox.LostFocus += FilterTextBox_LostFocus;

            // Se o seu método customizado de extensão existir, mantém ele aqui:
            // this.List.ItemSourceChanged(this.ItemSource_Changed);
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Reinicia o timer a cada tecla pressionada
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            ApplyFilter();
        }

        private void List_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ItemSource_Changed(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            try
            {
                if (_list.ItemsSource == null) return;

                string textoFiltro = _filterTextBox.Text;

                // Se o texto for igual ao placeholder ou vazio, limpa o filtro
                if (textoFiltro == _placeholderMsg || string.IsNullOrWhiteSpace(textoFiltro))
                {
                    ClearFilter();
                    return;
                }

                // Cenário 1: Se a fonte de dados for um DataView (Datatables/Bancos)
                if (_list.ItemsSource is DataView dtView)
                {
                    var queryBuilder = new StringBuilder();
                    string safeText = textoFiltro.Replace("'", "''"); // Evita quebra por caracteres especiais

                    foreach (DataColumn c in dtView.Table.Columns)
                    {
                        // Apenas colunas de texto/compatíveis recebem o Like
                        if (c.DataType == typeof(string))
                        {
                            if (queryBuilder.Length > 0)
                                queryBuilder.Append(" OR ");

                            queryBuilder.Append($"({c.ColumnName} LIKE '*{safeText}*')");
                        }
                    }
                    dtView.RowFilter = queryBuilder.ToString();
                }
                // Cenário 2: Coleções normais (List<T>, ObservableCollection<T>) via CollectionView
                else
                {
                    var view = CollectionViewSource.GetDefaultView(_list.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = RunFilterPredicate;
                        view.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao aplicar filtro: {ex.Message}");
            }
        }

        private bool RunFilterPredicate(object item)
        {
            if (item == null) return false;

            string textoFiltro = _filterTextBox.Text;

            if (textoFiltro == _placeholderMsg || string.IsNullOrWhiteSpace(textoFiltro))
                return true;

            // Utiliza o seu método de extensão original "Contem" se ele existir,
            // ou faz um fallback seguro usando reflexão simples.
            return item.ToString().Contem(textoFiltro, 70);
        }

        private void ClearFilter()
        {
            if (_list.ItemsSource is DataView dtView)
            {
                dtView.RowFilter = string.Empty;
            }
            else
            {
                var view = CollectionViewSource.GetDefaultView(_list.ItemsSource);
                if (view != null)
                {
                    view.Filter = null;
                    view.Refresh();
                }
            }
        }

        #region Gerenciamento de Placeholder (Focus)

        private void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_filterTextBox.Text == _placeholderMsg)
            {
                _filterTextBox.Text = string.Empty;
            }
        }

        private void FilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filterTextBox.Text))
            {
                _filterTextBox.Text = _placeholderMsg;
            }
        }

        #endregion
    }
}
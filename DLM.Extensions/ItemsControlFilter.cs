using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Conexoes
{
    /// <summary>
    /// Classe que cria eventos para filtrar Listas
    /// </summary>
    public class ItemsControlFilter
    {
        public ItemsControl _list { get; private set; }
        public TextBox _filter { get; private set; }

        private bool _run_filter(object item)
        {
            if (_filter.Text == Msg | _filter.Text == "")
            {
                return true;
            }
            if (String.IsNullOrEmpty(_filter.Text))
                return true;

            return Utilz.Contem(item, _filter.Text);
        }
        private void _itemsource_changed(object sender, EventArgs e)
        {
            SetFilter();
        }

        private void SetFilter()
        {
            try
            {
                if (_list.ItemsSource != null)
                {
                    var view = CollectionViewSource.GetDefaultView(_list.ItemsSource);
                    view.Filter = null;
                    view.Filter = _run_filter;
                    view.Refresh();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void _text_changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_filter.Text == Msg)
                {
                    return;
                }
                SetFilter();
            }
            catch (Exception ex)
            {
            }
        }
        private void _loaded(object sender, RoutedEventArgs e)
        {
            SetFilter();
        }

        public ItemsControlFilter(ItemsControl _list, TextBox filter)
        {
            this._filter = filter;
            this._list = _list;

            this._list.Loaded += _loaded;
            this._filter.TextChanged += _text_changed;
            this._filter.GotFocus += _got_focus;
            this._filter.LostFocus += _leave;
            _filter.Text = Msg;


            _list.ItemSourceChanged(this._itemsource_changed);
        }

        private void _leave(object sender, RoutedEventArgs e)
        {
            if (_filter.Text == "")
            {
                _filter.Text = Msg;
            }
        }

        public string Msg = "Pesquisar...";

        private void _got_focus(object sender, RoutedEventArgs e)
        {
            if (_filter.Text == Msg)
            {
                _filter.Text = "";
            }
        }
    }
}

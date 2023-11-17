using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Conexoes
{
    public static class Extensoes_Tabcontrol
    {
        public static TabItem SelectNext(this TabControl tabs_controle)
        {
            var sel = tabs_controle.SelectedIndex + 1;
            if (sel <= tabs_controle.Items.Count)
            {
                tabs_controle.SelectedIndex = sel;
            }
            var selecao = tabs_controle.SelectedItem as TabItem;
            return selecao;
        }
        public static TabItem SelectPrevious(this TabControl tabs_controle)
        {
            var sel = tabs_controle.SelectedIndex - 1;
            if (sel >= 0)
            {
                tabs_controle.SelectedIndex = sel;
            }
            var selecao = tabs_controle.SelectedItem as TabItem;
            return selecao;
        }
        public static TabItem Selecao(this TabControl tab)
        {
            if (tab.SelectedItem is TabItem)
            {
                return tab.SelectedItem as TabItem;
            }
            return null;
        }
        public static List<TabItem> ToList(this TabControl tab)
        {
            var retorno = new List<TabItem>();

            foreach (TabItem tabItem in tab.Items)
            {
                retorno.Add(tabItem);
            }

            return retorno;
        }
    }
}

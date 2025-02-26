using Conexoes;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;


namespace DLM
{
    public static class Extensoes_Traducao
    {
        public static List<MenuItem> GetMenuItems(this System.Windows.Window window)
        {
            var retorno = new List<MenuItem>();
            var menus = window.GetChildren<Menu>();
            var contexts = window.GetChildren<System.Windows.FrameworkElement>().FindAll(x => x.ContextMenu != null);
            foreach (var menu in menus)
            {
                foreach (var sub in menu.Items)
                {
                    if (sub is MenuItem)
                    {
                        retorno.Add((MenuItem)sub);
                    }
                }
            }
            foreach (var menu in contexts)
            {
                foreach (var sub in menu.ContextMenu.Items)
                {
                    if (sub is System.Windows.Controls.MenuItem)
                    {
                        retorno.Add((MenuItem)sub);
                    }
                }
            }
            return retorno;
        }
        public static void ExportTexts(this System.Windows.Window window)
        {
            var arq = @$"{Cfg.Init.DIR_APPDATA}Translation\{Conexoes.Utilz.GetDLLInfo().FileName.getNome()}.{window.GetType().Name}.Lang";
            var menus = window.GetMenuItems();

            var retorno = new List<string>();

            foreach (var menu in menus)
            {
                if (menu.Header is string)
                {
                    retorno.Add($"{menu.Header.ToString()}|MenuItem");
                }
                else if (menu.Header is TextBlock)
                {
                    retorno.Add($"{(menu.Header as TextBlock).Text}|MenuItem");
                }
            }

            foreach (var item in window.GetChildren<Label>())
            {
                retorno.Add($"{item.Content.ToString()}|Label");
            }
            foreach (var item in window.GetChildren<TextBlock>())
            {
                retorno.Add($"{item.Text}|TextBlock");
            }

            foreach (var item in window.GetChildren<Button>())
            {
                if (item.Content is string)
                {
                    retorno.Add($"{item.Content.ToString()}|Button");
                }
                else if (item.Content is TextBlock)
                {
                    retorno.Add($"{(item.Content as TextBlock).Text}|Button");
                }
            }
            foreach (var item in window.GetChildren<GroupBox>())
            {
                if (item.Header is string)
                {
                    retorno.Add($"{item.Header.ToString()}|GroupBox");
                }
                else if (item.Header is TextBlock)
                {
                    retorno.Add($"{(item.Header as TextBlock).Text}|GroupBox");
                }
            }
            foreach (var datagrid in window.GetChildren<DataGrid>())
            {
                foreach (var item in datagrid.Columns)
                {
                    if (item.Header is string)
                    {
                        retorno.Add($"{item.Header.ToString()}|DataGridColumn");
                    }
                    else if (item.Header is TextBlock)
                    {
                        retorno.Add($"{(item.Header as TextBlock).Text}|DataGridColumn");
                    }
                }
            }
            foreach (var item in window.GetChildren<TabItem>())
            {
                if (item.Header is string)
                {
                    retorno.Add($"{item.Header.ToString()}|TabItem");
                }
            }
            Conexoes.Utilz.Arquivo.Gravar(arq, retorno);
        }

        public static void ImportTexts(this System.Windows.Window window, string folder)
        {
            if (!folder.Exists())
            {
                return;
            }
            var arq = @$"{folder}\{Conexoes.Utilz.GetDLLInfo().FileName.getNome()}.{window.GetType().Name}.Lang";

            var txts = Conexoes.Utilz.Arquivo.Ler(arq).FindAll(x => x.Replace("|", "").Replace(" ", "").Length > 0);
            if (txts.Count == 0) { return; }

            var menus = window.GetMenuItems();

            foreach (var item in menus)
            {
                if (item.Header is string)
                {
                    var txt = GetTranslation(txts, item.Header.ToString());
                    if (txt != null)
                    {
                        item.Header = txt;
                    }
                }
                else if (item.Header is TextBlock)
                {
                    var txt = GetTranslation(txts, (item.Header as TextBlock).Text);
                    if (txt != null)
                    {
                        (item.Header as TextBlock).Text = txt;
                    }
                }
            }

            foreach (var item in window.GetChildren<Label>())
            {
                var txt = GetTranslation(txts, item.Content.ToString());
                if (txt != null)
                {
                    item.Content = txt;
                }
            }
            foreach (var item in window.GetChildren<TextBlock>())
            {
                var txt = GetTranslation(txts, item.Text.ToString());
                if (txt != null)
                {
                    item.Text = txt;
                }
            }

            foreach (var item in window.GetChildren<Button>())
            {
                if (item.Content is string)
                {
                    var txt = GetTranslation(txts, item.Content.ToString());
                    if (txt != null)
                    {
                        item.Content = txt;
                    }
                }
                else if (item.Content is TextBlock)
                {
                    var txt = GetTranslation(txts, (item.Content as TextBlock).Text);
                    if (txt != null)
                    {
                        (item.Content as TextBlock).Text = txt;
                    }
                }
            }
            foreach (var item in window.GetChildren<GroupBox>())
            {
                if (item.Header is string)
                {
                    var txt = GetTranslation(txts, item.Header.ToString());
                    if (txt != null)
                    {
                        item.Header = txt;
                    }
                }
                else if (item.Header is TextBlock)
                {
                    var txt = GetTranslation(txts, (item.Header as TextBlock).Text);
                    if (txt != null)
                    {
                        (item.Header as TextBlock).Text = txt;
                    }
                }
            }
            foreach (var datagrid in window.GetChildren<DataGrid>())
            {
                foreach (var item in datagrid.Columns)
                {
                    if (item.Header is string)
                    {
                        var txt = GetTranslation(txts, item.Header.ToString());
                        if (txt != null)
                        {
                            item.Header = txt;
                        }
                    }
                    else if (item.Header is TextBlock)
                    {
                        var txt = GetTranslation(txts, (item.Header as TextBlock).Text);
                        if (txt != null)
                        {
                            (item.Header as TextBlock).Text = txt;
                        }
                    }
                }
            }
            foreach (var item in window.GetChildren<TabItem>())
            {
                if (item.Header is string)
                {
                    var txt = GetTranslation(txts, item.Header.ToString());
                    if (txt != null)
                    {
                        item.Header = txt;
                    }
                }
            }
        }

        private static string GetTranslation(List<string> txts, string value)
        {
            string txt = null;
            var trans = txts.Find(x => x.TrimStart("_").StartsWith($"{value.TrimStart("_")}|"));
            if (trans != null)
            {
                txt = trans.Split('|')[1];
            }

            return txt;
        }
    }
}

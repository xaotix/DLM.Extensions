using DLM;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Conexoes
{
    public static class Extensoes_WPF
    {
        public static void SetTitle(this Window window, string prefix)
        {
            window.Title = $"{prefix} - {Cfg.Init.GetNomeProduto()} - {Cfg.Init.GetVersao()} - [{Cfg.Init.SAP_Servidor}]";
        }
        public static void Sair(this System.Windows.Window window)
        {
            DLM.vars.Cfg_User.Init.Salvar();
            DLM.vars.Cfg.Init.Salvar();
            Cfg.Init.GetUser().SetOnline(false);
            Environment.Exit(0);
        }
        public static void SetIcones(this System.Windows.Window window)
        {
            var menus = window.GetChildren<System.Windows.Controls.Menu>();
            var contexts = window.GetChildren<System.Windows.FrameworkElement>().FindAll(x => x.ContextMenu != null);
            foreach (var menu in menus)
            {
                menu.SetIcones();
            }

            foreach (var menu in contexts)
            {
                menu.ContextMenu.SetIcones();
            }
        }
        private static void SetIcones(this System.Windows.Controls.Menu menu)
        {
            foreach (var pp in menu.Items)
            {
                if (pp is System.Windows.Controls.MenuItem)
                {
                    (pp as System.Windows.Controls.MenuItem).SetIcones();
                }
            }
        }
        private static void SetIcones(this System.Windows.Controls.ContextMenu menu)
        {
            foreach (var pp in menu.Items)
            {
                if (pp is System.Windows.Controls.MenuItem)
                {
                    (pp as System.Windows.Controls.MenuItem).SetIcones();
                }
            }
        }
        private static void SetIcones(this System.Windows.Controls.MenuItem menu)
        {
            menu.VerticalContentAlignment = VerticalAlignment.Center;
            menu.VerticalAlignment = VerticalAlignment.Center;
            string chave = "";


            if (menu.Header is string || menu.ToolTip is string)
            {
                chave = (menu.Header is string ? menu.Header as string : menu.ToolTip as string).Replace("_", "");

                chave = chave.Upper().Replace(" ", "");

                if (chave.IsNullOrEmpty())
                {
                    chave = menu.ToolTip.ToString();
                }

                if (chave != "")
                {

                    //menu.VerticalContentAlignment = VerticalAlignment.Center;
                    //menu.VerticalAlignment = VerticalAlignment.Center;
                    //menu.Background = System.Windows.Media.Brushes.Transparent;
                    if (menu.Header.NotNullOrEmpty())
                    {
                        var txt = new TextBlock();
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        txt.Text = (menu.Header as string).TrimStart("_");
                        txt.FontSize = menu.FontSize;
                        txt.FontFamily = menu.FontFamily;
                        txt.Background = System.Windows.Media.Brushes.Transparent;
                        txt.Margin = new Thickness(2, 0, 0, 0);
                        txt.ToolTip = menu.ToolTip;
                        menu.Header = txt;
                    }
                    else
                    {
                        menu.Header = null;
                    }

                    var Source = BufferImagem.GetIcone(chave);


                    menu.Icon = new System.Windows.Controls.Image()
                    {
                        Source = Source,
                        ToolTip = menu.ToolTip,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Width = 16,
                        Height = 16
                    };

                }
            }




            foreach (var pp in menu.Items)
            {
                if (pp is System.Windows.Controls.MenuItem)
                {
                    (pp as System.Windows.Controls.MenuItem).SetIcones();
                }
            }

        }

        public static void Inicializar(this System.Windows.Window window)
        {
            Cfg.Init.GetUser().SetOnline(true);

            window.Closed += Window_Closed;
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            DLM.vars.Cfg_User.Init.Salvar();
            DLM.vars.Cfg.Init.Salvar();
            Cfg.Init.GetUser().SetOnline(false);
            Environment.Exit(0);
        }
    }
}

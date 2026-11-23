using DLM;
using DLM.vars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Conexoes
{
    public static class Extensoes_WPF
    {
        public static Task<bool?> ShowAsync(this Window window)
        {
            var tcs = new TaskCompletionSource<bool?>();

            //window.Closed += (s, e) =>
            //{
            //    tcs.TrySetResult(window.DialogResult);
            //};

            window.Show();

            return tcs.Task;
        }
        public static void SetTitle(this Window window, string middle = null)
        {
            var tit = $"{Cfg.Init.GetNomeProduto()} - {Cfg.Init.GetVersao()}";
            
            if(middle.NotNullOrEmpty())
            {
                tit += $" - {middle}";
            }
            if (!Cfg.Init.SOFTWARE_TERCEIROS)
            {
                tit += $" - [SAP {Cfg.Init.SAP_Servidor} {Cfg.Init.SAP_Servidor_IP}] [MySQL {Cfg_User.Init.MySQL_Servidor}]";
            }
            window.Title = tit;
        }
        public static void Sair(this System.Windows.Window window)
        {
            DLM.vars.Cfg_User.Init.Salvar();
            DLM.vars.Cfg.Init.Salvar();
            Cfg.Init.GetUser().SetOnline(false);
            Environment.Exit(0);
        }
        // SetIcones movido e otimizado em DLM.Icones (Conexoes.ExtensoesMenuIcones)

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

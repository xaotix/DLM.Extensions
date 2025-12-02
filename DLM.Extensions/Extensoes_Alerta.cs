using DLM.encoder;
using DLM.vars;
using DLM.WPF;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace Conexoes
{
    public static class Extensoes_Alerta
    {
        public static bool Pergunta(this string Pergunta, string Titulo = "Confirme")
        {
            return System.Windows.MessageBox.Show(
                             Pergunta,
                             Titulo,
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Question,
                             MessageBoxResult.Yes, System.Windows.MessageBoxOptions.ServiceNotification) == MessageBoxResult.Yes;
        }
        public static void Alerta(this string mensagem, int tempo, string titulo = "")
        {
            if (!mensagem.IsNullOrEmpty())
            {
                var menu = new Janela_Timer(titulo, mensagem, tempo);
                menu.ShowDialog();
            }
        }
        public static void Alerta(this string mensagem, string titulo = "", MessageBoxImage icone = MessageBoxImage.Information)
        {
            var tipo = TipoReport.Critico;
            switch (icone)
            {
                case MessageBoxImage.Hand:
                    tipo = TipoReport.Critico;
                    break;
                case MessageBoxImage.None:
                case MessageBoxImage.Exclamation:
                case MessageBoxImage.Question:
                case MessageBoxImage.Asterisk:
                    tipo = TipoReport.Alerta;
                    break;
            }
            if (titulo == "")
            {
                titulo = Cfg.Init.GetNomeProduto();
            }
            if (mensagem.LenghtStr() > 300)
            {
                var report = new Report(titulo, mensagem, tipo);
                var reports = new List<Report>() { report };
                reports.Show();
            }
            else if (mensagem.LenghtStr() > 0)
            {
                switch (icone)
                {
                    case MessageBoxImage.None:
                        break;
                    case MessageBoxImage.Hand:
                        titulo = "[Erro] - " + titulo;
                        break;
                    case MessageBoxImage.Question:
                        break;
                    case MessageBoxImage.Exclamation:
                        titulo = "[Erro] - " + titulo;

                        break;
                    case MessageBoxImage.Asterisk:

                        break;
                }
                System.Windows.MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, icone);
            }

        }
        public static void Alerta(this UnhandledExceptionEventArgs ex, [CallerMemberName] String propertyName = "")
        {
            ex.ExceptionObject.ToString().JanelaErro(propertyName);
            DLM.log.Log(
                $"======================================================================================" +
                $"{DateTime.Now.ToString()}->" +
                $"{ex.ExceptionObject.ToString()}" +
                $"======================================================================================" +
                $"\n");
        }
        public static void Alerta(this Exception ex, string descricao = "", [CallerMemberName] String propertyName = "", string logfile = null)
        {
            if(propertyName == null)
            {
                propertyName = "Erro";
            }
            DLM.log.Log(ex, logfile, propertyName, descricao);
            var texto = ex.GetTexto();
            var desc = $"{descricao} [{propertyName}]";
            texto.JanelaErro(desc);
        }
        private static void JanelaErro(this string texto, string titulo)
        {
            new Action(() =>
            {
                var mm = new VerTextoReport();
                mm.texto.Text = $"{titulo}\n\n{texto}";
                mm.Title = $"Erro! {titulo.Esquerda(30)}";
                mm.Topmost = true;
                mm.texto.Foreground = System.Windows.Media.Brushes.Red;
                mm.texto.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                mm.texto.TextAlignment = TextAlignment.Left;
                mm.texto.VerticalAlignment = VerticalAlignment.Stretch;
                mm.Show();
            }).NovaInstancia();
        }
        public static void JanelaTexto(this string mensagem, string titulo = "", bool top = true, bool pendurar = false)
        {
            VerTextoReport mm = new VerTextoReport();
            mm.texto.Text = mensagem;
            mm.Title = titulo;
            mm.Topmost = top;
            mm.texto.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            mm.texto.TextAlignment = TextAlignment.Left;
            mm.texto.VerticalAlignment = VerticalAlignment.Stretch;
            if (pendurar)
            {
                mm.ShowDialog();
            }
            else
            {
                mm.Show();
            }



        }
        public static void NovaInstancia(this Action chamada)
        {
            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                chamada();
                System.Windows.Threading.Dispatcher.Run();
            }
            ));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.Start();
        }
        public static string GetTexto(this Exception ex, string adicional = "")
        {
            return adicional +
                $"Erro:\n" +
                ex.Message +

                "\n\n\n\nCódigo:\n" +
                ex.Source +
                "\nFrame:\n" +
                ex.StackTrace +
                "\nMensagem Interna:\n" +
                ex.InnerException;
        }
    }
}

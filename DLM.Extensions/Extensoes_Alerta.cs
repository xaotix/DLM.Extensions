using DLM.encoder;
using DLM.encoder.RTF.Tokens;
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
        public static bool Pergunta(this string message, string title = "Confirme")
        {
            bool result = false;

            var dispatcher = System.Windows.Application.Current?.Dispatcher
                             ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;

            dispatcher.Invoke(() =>
            {
                result = System.Windows.MessageBox.Show(
                    message,
                    title,
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question,
                    System.Windows.MessageBoxResult.Yes
                ) == System.Windows.MessageBoxResult.Yes;
            });

            return result;
        }
        public static void Alerta(this string mensagem, int tempo, string titulo = "")
        {
            if (!mensagem.IsNullOrEmpty())
            {
                var menu = new Janela_Timer(titulo, mensagem, tempo);
                menu.ShowDialog();
            }
        }
        public static void Alerta(this string mensagem, string titulo = "Alerta.", MessageBoxImage icone = MessageBoxImage.Information)
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
            if (propertyName == null)
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
                var report = new List<Report> { new Report(titulo, $"{titulo}\n\n{texto}", TipoReport.Critico) };
                report.Show();
                //var mm = new VerTextoReport();
                //mm.texto.Text = $"{titulo}\n\n{texto}";
                //mm.Title = $"Erro! {titulo.Esquerda(30)}";
                //mm.Topmost = true;
                //mm.texto.Foreground = System.Windows.Media.Brushes.Red;
                //mm.texto.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                //mm.texto.TextAlignment = TextAlignment.Left;
                //mm.texto.VerticalAlignment = VerticalAlignment.Stretch;
                //mm.Show();
            }).NovaInstancia();
        }
        public static void JanelaTexto(this string mensagem, string titulo = "", bool top = true, bool pendurar = false)
        {
            var mm = new VerTextoReport();
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
        public static string GetTexto(this Exception ex, string adicional = "", int nivel = 0)
        {
            if (ex == null) return adicional;

            var indent = new string(' ', nivel * 4);
            var retorno = new System.Text.StringBuilder();

            if (adicional.NotNullOrEmpty())
                retorno.AppendLine(adicional).AppendLine();

            // Cabecalho com nivel de aninhamento
            if (nivel > 0)
                retorno.AppendLine($"{indent}--- Excecao Interna (nivel {nivel}) ---");

            // Tipo da excecao
            retorno.AppendLine($"{indent}Tipo:       {ex.GetType().FullName}");

            // Mensagem principal
            if (ex.Message.NotNullOrEmpty())
                retorno.AppendLine($"{indent}Mensagem:   {ex.Message}");

            // Metodo que lancou
            if (ex.TargetSite != null)
                retorno.AppendLine($"{indent}Metodo:     {ex.TargetSite.DeclaringType?.FullName}.{ex.TargetSite.Name}");

            // Assembly/modulo de origem
            if (ex.Source.NotNullOrEmpty())
                retorno.AppendLine($"{indent}Origem:     {ex.Source}");

            // Codigo HResult (Win32 / COM / HRESULT)
            retorno.AppendLine($"{indent}HResult:    0x{ex.HResult:X8} ({ex.HResult})");

            // Link de ajuda, se disponivel
            if (ex.HelpLink.NotNullOrEmpty())
                retorno.AppendLine($"{indent}HelpLink:   {ex.HelpLink}");

            // Dados extras adicionados via ex.Data
            if (ex.Data != null && ex.Data.Count > 0)
            {
                retorno.AppendLine($"{indent}Dados:");
                foreach (System.Collections.DictionaryEntry entry in ex.Data)
                    retorno.AppendLine($"{indent}    [{entry.Key}] = {entry.Value}");
            }

            // Stack trace
            if (ex.StackTrace.NotNullOrEmpty())
            {
                retorno.AppendLine($"{indent}StackTrace:");
                foreach (var linha in ex.StackTrace.Split('\n'))
                    retorno.AppendLine($"{indent}    {linha.TrimEnd()}");
            }

            // AggregateException: itera todas as excecoes filhas
            var aggregate = ex as AggregateException;
            if (aggregate != null && aggregate.InnerExceptions.Count > 0)
            {
                retorno.AppendLine($"{indent}Excecoes Agregadas ({aggregate.InnerExceptions.Count}):");
                for (int i = 0; i < aggregate.InnerExceptions.Count; i++)
                {
                    retorno.AppendLine($"{indent}  [{i + 1}]:");
                    retorno.AppendLine(aggregate.InnerExceptions[i].GetTexto(nivel: nivel + 1));
                }
            }
            else if (ex.InnerException != null)
            {
                retorno.AppendLine(ex.InnerException.GetTexto(nivel: nivel + 1));
            }

            return retorno.ToString();
        }
    }
}

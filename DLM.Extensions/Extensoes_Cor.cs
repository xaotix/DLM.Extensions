using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Windows.Media;

namespace Conexoes
{
    public static class ExtensoesCor
    {
        /// <summary>
        /// Escala de Verde até Vermelho
        /// </summary>
        /// <param name="desvio"></param>
        /// <param name="escala"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static SolidColorBrush GetCorDesvio(this double desvio, double escala, double max)
        {
            // Se o desvio for zero ou negativo, retorna Verde Puro
            if (desvio <= 0)
                return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

            // Evita divisões por zero ou valores inconsistentes
            if (max <= 0) max = 0.01;
            if (escala <= 0) escala = 0.01;

            // Desvio maior ou igual ao máximo: Vermelho Puro
            if (desvio >= max)
                return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            // Calcula o degrau. Se a escala for maior que o desvio, usamos o próprio desvio
            // para evitar que o t caia sempre em 0.
            double degrau = desvio < escala ? desvio : Math.Floor(desvio / escala) * escala;

            double t = degrau / max;
            t = Math.Min(1, Math.Max(0, t)); // Garante que fique entre 0 e 1

            byte r, g;
            if (t <= 0.5)
            {
                // De Verde (0, 255, 0) para Amarelo (255, 255, 0)
                double local = t / 0.5;
                r = (byte)Math.Round(local * 255);
                g = 255;
            }
            else
            {
                // De Amarelo (255, 255, 0) para Vermelho (255, 0, 0)
                double local = (t - 0.5) / 0.5;
                r = 255;
                g = (byte)Math.Round(255 - (local * 255));
            }

            // Usamos FromArgb garantindo Alpha em 255 (totalmente opaco)
            return new SolidColorBrush(Color.FromArgb(255, r, g, 0));
        }
        public static Brush Inverter(this Brush cor)
        {
            var color = (SolidColorBrush)cor;
            return System.Drawing.Color.FromArgb(255 - color.Color.R, 255 - color.Color.G, 255 - color.Color.B).ToBrush();
        }
        public static SolidColorBrush Inverter(this SolidColorBrush color)
        {
            return System.Drawing.Color.FromArgb(255 - color.Color.R, 255 - color.Color.G, 255 - color.Color.B).ToBrush();
        }
        public static string ToHex(this SolidColorBrush color)
        {
            return $"#{color.Color.A:X2}{color.Color.R:X2}{color.Color.G:X2}{color.Color.B:X2}";
        }
        public static SolidColorBrush ToBrush(this System.Drawing.Color Color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B));
        }
        public static SolidColorBrush ToBrush(this System.Windows.Media.Color Color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B));
        }
        public static System.Windows.Media.Color ToColor(this System.Windows.Media.Brush cor)
        {
            return ((System.Windows.Media.SolidColorBrush)cor).Clone().Color;
        }
        public static System.Windows.Media.Color ToColor(this string hex)
        {
            if (hex == null)
            {
                return System.Windows.Media.Colors.Transparent;
            }
            // Remove o caractere '#' se estiver presente
            hex = hex.Replace("#", "");

            // Se a string tiver 6 caracteres, assume opacidade total (FF)
            if (hex.LenghtStr() == 6)
            {
                hex = "FF" + hex;
            }

            if (hex.LenghtStr() != 8)
            {
                return System.Windows.Media.Colors.Transparent;
            }

            // Converte os componentes ARGB
            byte a = Convert.ToByte(hex.Substring(0, 2), 16);
            byte r = Convert.ToByte(hex.Substring(2, 2), 16);
            byte g = Convert.ToByte(hex.Substring(4, 2), 16);
            byte b = Convert.ToByte(hex.Substring(6, 2), 16);

            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }
    }
}

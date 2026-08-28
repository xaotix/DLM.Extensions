using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Conexoes
{
    public static class ExtensoesCor
    {
        private static readonly SolidColorBrush PureGreen = CreateFrozenBrush(0, 255, 0);
        private static readonly SolidColorBrush PureRed = CreateFrozenBrush(255, 0, 0);

        /// <summary>
        /// Retorna Verde para desvio <= valorMax. 
        /// Acima de valorMax, faz gradiente de Amarelo até Vermelho.
        /// </summary>
        /// <param name="desvio">Valor atual do desvio.</param>
        /// <param name="valorMax">Limite máximo tolerado antes de começar a ficar amarelo.</param>
        /// <param name="limiteVermelho">Valor onde atinge o Vermelho puro (se 0, usa 2x o valorMax).</param>
        public static SolidColorBrush GetCorDesvio(this double desvio, double valorMax, double limiteVermelho = 0)
        {
            // 1. Até o valorMax é sempre Verde Puro
            if (desvio <= valorMax)
                return PureGreen;

            // Se não informar onde é o Vermelho puro, por padrão será o dobro do valorMax
            if (limiteVermelho <= valorMax)
                limiteVermelho = valorMax * 2;

            // 2. Desvio maior ou igual ao limite do vermelho: Vermelho Puro
            if (desvio >= limiteVermelho)
                return PureRed;

            // 3. Normalização percentual (0.0 no valorMax -> 1.0 no limiteVermelho)
            double t = (desvio - valorMax) / (limiteVermelho - valorMax);
            t = Math.Min(1.0, Math.Max(0.0, t)); // Clamp manual para .NET 4.8

            // 4. Interpolação de Amarelo (255, 255, 0) para Vermelho (255, 0, 0)
            // O Vermelho (R) fica fixo em 255, apenas o Verde (G) vai caindo até 0
            byte r = 255;
            byte g = (byte)Math.Round((1.0 - t) * 255);

            return CreateFrozenBrush(r, g, 0);
        }

        private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze(); // Otimização WPF
            return brush;
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

        private static readonly Dictionary<string, SolidColorBrush> _colorCache = new();

        public static SolidColorBrush GetCorAleatoria(this string chave)
        {
            if (string.IsNullOrEmpty(chave))
                chave = string.Empty;

            chave = chave.Trim().ToUpper();

            if (_colorCache.TryGetValue(chave, out var brushExistente))
            {
                return brushExistente.Clone();
            }

            // 1. Gera um Matiz (Hue) único baseado no Hash da chave (0 a 360 graus)
            // Usamos Math.Abs para evitar números negativos
            double hue = Math.Abs(chave.GetHashCode()) % 360;

            // 2. Definimos Saturação e Luminosidade fixas/controladas
            // Saturação alta (0.65 a 0.8) evita cores pastosas/acinzentadas
            // Luminosidade média (0.5 a 0.6) garante que não fique nem muito escuro, nem muito claro
            double saturation = 0.70;
            double lightness = 0.55;

            var (r, g, b) = HslToRgb(hue, saturation, lightness);

            var novoBrush = new SolidColorBrush(Color.FromRgb(r, g, b));
            novoBrush.Freeze();

            _colorCache[chave] = novoBrush;

            return novoBrush.Clone();
        }

        // Conversor auxiliar de HSL para RGB
        private static (byte r, byte g, byte b) HslToRgb(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r1, g1, b1;

            if (h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }

            byte r = (byte)((r1 + m) * 255);
            byte g = (byte)((g1 + m) * 255);
            byte b = (byte)((b1 + m) * 255);

            return (r, g, b);
        }
    }
}

using System;
using System.Windows.Media;

namespace Conexoes
{
    public static class ExtensoesCor
    {
        public static Brush Inverter(this Brush cor)
        {
            var color = (SolidColorBrush)cor;
            return System.Drawing.Color.FromArgb(255 - color.Color.R, 255 - color.Color.G, 255 - color.Color.B).ToBrush();
        }
        public static SolidColorBrush Inverter(this SolidColorBrush color)
        {
            return System.Drawing.Color.FromArgb(255 - color.Color.R, 255 - color.Color.G, 255 - color.Color.B).ToBrush();
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
            if (hex.Length == 6)
            {
                hex = "FF" + hex;
            }

            if (hex.Length != 8)
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

using System.Drawing;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    internal static class AutoColor
    {
        public static string ContrastColor(string color)
        {
            var rgb = color.StartsWith("#") ? FromHexCode(color) : Color.FromName(color);
            return GetColorBrightness(rgb) > 0.5 ? "#000" : "#fff";
        }

        private static Color FromHexCode(string color)
        {
            var hex = color.TrimStart('#');
            if(hex.Length == 3)
            {
                hex = string.Concat(hex.Select(c => new string(c, 2)));
            }

            return Color.FromArgb(
                int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }

        private static double GetColorBrightness(Color rgb) => 1 - (0.299 * rgb.R + 0.587 * rgb.G + 0.114 * rgb.B) / 255;
    }
}

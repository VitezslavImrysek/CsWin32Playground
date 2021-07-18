using Windows.Win32.Graphics.Direct2D;

namespace CsWin32Playground.Helpers
{
    internal static class ColorHelper
    {
        public static D2D1_COLOR_F ToD2Color(this System.Drawing.Color color)
        {
            return new D2D1_COLOR_F() 
            { 
                a = color.A / 255, 
                b = color.B / 255, 
                g = color.G / 255, 
                r = color.R / 255, 
            };
        }
    }
}

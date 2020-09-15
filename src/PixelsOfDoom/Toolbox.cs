using System.Drawing;

namespace PixelsOfDoom
{
    public static class Toolbox
    {
        public static bool IsSameRGB(this Color color, Color other)
        {
            return (color.R == other.R) && (color.G == other.G) && (color.B == other.B);
        }
    }
}

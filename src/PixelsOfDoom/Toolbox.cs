using System.Drawing;

namespace PixelsOfDoom
{
    public static class Toolbox
    {
        public static bool IsSameRGB(this Color color, Color other)
        {
            return (color.R == other.R) && (color.G == other.G) && (color.B == other.B);
        }

        public static Point Add(this Point point, Point other)
        {
            return new Point(point.X + other.X, point.Y + other.Y);
        }

        public static Point Mult(this Point point, Point mutiplier)
        {
            return new Point(point.X * mutiplier.X, point.Y * mutiplier.Y);
        }
    }
}

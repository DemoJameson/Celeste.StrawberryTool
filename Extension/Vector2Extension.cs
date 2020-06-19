using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class Vector2Extension {
        public static float Distance(this Vector2 point, Rectangle rect) {
            float xDist = MinXDistance(point, rect);
            float yDist = MinYDistance(point, rect);
            if (Math.Abs(xDist) < 0.5) {
                return yDist;
            }

            if (Math.Abs(yDist) < 0.5) {
                return xDist;
            }

            return (float) Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
        }

        private static float MinXDistance(Vector2 point, Rectangle rect) {
            if (rect.Left > point.X) {
                return rect.Left - point.X;
            }

            if (rect.Right < point.X) {
                return point.X - rect.Right;
            }

            return 0;
        }

        private static float MinYDistance(Vector2 point, Rectangle rect) {
            if (rect.Bottom < point.Y) {
                return point.Y - rect.Bottom;
            }

            if (rect.Top > point.Y) {
                return rect.Top - point.Y;
            }

            return 0;
        }
    }
}
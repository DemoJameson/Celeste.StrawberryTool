using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class CameraExtension {
        public static Vector2 GetIntersectionPoint(this Camera camera, Vector2 start, Vector2 end) {
            Line line1 = new Line(start, end);

            Line line2 = new Line(camera.TopLeft(), camera.BottomLeft());
            if (Line.Intersects(line1, line2)) {
                return Line.GetIntersectionPoint(line1, line2);
            }

            line2 = new Line(camera.TopRight(), camera.BottomRight());
            if (Line.Intersects(line1, line2)) {
                return Line.GetIntersectionPoint(line1, line2);
            }

            line2 = new Line(camera.TopLeft(), camera.TopRight());
            if (Line.Intersects(line1, line2)) {
                return Line.GetIntersectionPoint(line1, line2);
            }

            line2 = new Line(camera.BottomLeft(), camera.BottomRight());
            if (Line.Intersects(line1, line2)) {
                return Line.GetIntersectionPoint(line1, line2);
            }

            return default;
        }

        public static Vector2 Center(this Camera camera) {
            return new Vector2((camera.Left + camera.Right) / 2, (camera.Top + camera.Bottom) / 2);
        }

        private static Vector2 TopLeft(this Camera camera) {
            return new Vector2(camera.Left, camera.Top);
        }

        private static Vector2 TopRight(this Camera camera) {
            return new Vector2(camera.Right, camera.Top);
        }

        private static Vector2 BottomLeft(this Camera camera) {
            return new Vector2(camera.Left, camera.Bottom);
        }

        private static Vector2 BottomRight(this Camera camera) {
            return new Vector2(camera.Right, camera.Bottom);
        }
    }

    class Line {
        private Vector2 start;
        private Vector2 end;

        public Line(Vector2 start, Vector2 end) {
            this.start = start;
            this.end = end;
        }

        public static Vector2 GetIntersectionPoint(Line a, Line b) {
            //y = kx + m;
            //k = (y2 - y1) / (x2 - x1)
            float kA = (a.end.Y - a.start.Y) / (a.end.X - a.start.X);
            float kB = (b.end.Y - b.start.Y) / (b.end.X - b.start.X);

            //m = y - k * x
            float mA = a.start.Y - kA * a.start.X;
            float mB = b.start.Y - kB * b.start.X;

            float x = (mB - mA) / (kA - kB);
            float y = kA * x + mA;
            return new Vector2(x, y);
        }

        public static bool Intersects(Line a, Line b) {
            Vector2 intersect = GetIntersectionPoint(a, b);

            if (Vector2.Distance(a.start, intersect) < Vector2.Distance(a.start, a.end) &&
                Vector2.Distance(a.end, intersect) < Vector2.Distance(a.start, a.end)) {
                return true;
            }

            return false;
        }
    }
}
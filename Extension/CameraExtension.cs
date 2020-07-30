using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    public static class CameraExtension {
        public static Vector2 GetIntersectionPoint(this Camera camera, Vector2 start, Vector2 end, float margin = 0f) {
            Vector2 result = FindIntersection(camera.TopLeft(), camera.BottomLeft(), start, end);
            if (!result.Equals(default)) {
                return result + Vector2.UnitX * margin;
            }
            
            result = FindIntersection(camera.TopRight(), camera.BottomRight(), start, end);
            if (!result.Equals(default)) {
                return result - Vector2.UnitX * margin;
            }
            
            result = FindIntersection(camera.TopLeft(), camera.TopRight(), start, end);
            if (!result.Equals(default)) {
                return result + Vector2.UnitY * margin;
            }
            
            result = FindIntersection(camera.BottomLeft(), camera.BottomRight(), start, end);
            if (!result.Equals(default)) {
                return result - Vector2.UnitY * margin;
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

        private static Vector2 FindIntersection(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            Vector2 intersection = default;

            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                / denominator;
            if (float.IsInfinity(t1)) {
                // The lines are parallel (or close enough to it).
                return default;
            }

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            bool segments_intersect = t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;

            if (segments_intersect) {
                return intersection;
            }

            return default;
        }
    }
}
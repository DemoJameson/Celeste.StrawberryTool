using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.StrawberryTool.Extension {
    internal static class CameraExtension {
        public static Vector2? GetIntersectionPoint(this Camera camera, Vector2 start, Vector2 end, float margin = 0f) {
            float marginX = Math.Min(camera.Viewport.Width / 2f, margin);
            float marginY = Math.Min(camera.Viewport.Height / 2f, margin);
            Vector2 marginVector = new Vector2(marginX, marginY);

            Vector2[] borderPoints = {
                camera.TopLeft() + marginVector * new Vector2(1f, 1f),
                camera.TopRight() + marginVector * new Vector2(-1f, 1f),
                camera.BottomRight() + marginVector * new Vector2(-1f, -1f),
                camera.BottomLeft() + marginVector * new Vector2(1f, -1f)
            };

            for (int i = 0; i < borderPoints.Length; i++) {
                Vector2? result = FindIntersection(borderPoints[i], borderPoints[(i + 1) % borderPoints.Length],
                    start, end);
                if (result != null) {
                    return result;
                }
            }

            return null;
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

        private static Vector2? FindIntersection(
            Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
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
                return null;
            }

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            Vector2 intersection = new Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            bool segments_intersect = t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;

            if (segments_intersect) {
                return intersection;
            }

            return null;
        }
    }
}
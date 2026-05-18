using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Utility methods for evaluating Bezier curves.
    /// Used by projectile return paths, camera movements, and any curved interpolation.
    /// </summary>
    public static class BezierUtils
    {
        /// <summary>
        /// Evaluates a point on a quadratic Bezier curve.
        /// B(t) = (1-t)^2 * a + 2(1-t)t * b + t^2 * c
        /// </summary>
        /// <param name="start">The curve start point (t=0).</param>
        /// <param name="control">The control point that "pulls" the curve into an arc.</param>
        /// <param name="end">The curve end point (t=1).</param>
        /// <param name="t">Parameter from 0 to 1 representing position along the curve.</param>
        public static Vector3 QuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        /// <summary>
        /// Computes a control point for a curved arc between two positions.
        /// The arc is offset upward and perpendicular to the line between start and end.
        /// </summary>
        /// <param name="start">Arc start position.</param>
        /// <param name="end">Arc end position.</param>
        /// <param name="heightRatio">Upward offset as a ratio of distance (default 0.3 = 30%).</param>
        /// <param name="sideRatio">Sideways offset as a ratio of distance (default 0.15 = 15%).</param>
        public static Vector3 ComputeArcControlPoint(Vector3 start, Vector3 end, float heightRatio = 0.3f, float sideRatio = 0.15f)
        {
            Vector3 midpoint = (start + end) * 0.5f;
            Vector3 toEnd = (end - start).normalized;

            Vector3 perpendicular = Vector3.Cross(toEnd, Vector3.up).normalized;
            if (perpendicular.sqrMagnitude < 0.01f)
            {
                perpendicular = Vector3.Cross(toEnd, Vector3.right).normalized;
            }

            float distance = Vector3.Distance(start, end);
            return midpoint + Vector3.up * (distance * heightRatio) + perpendicular * (distance * sideRatio);
        }
    }
}

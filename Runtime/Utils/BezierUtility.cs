using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>Bezier curve evaluation helpers.</summary>
    public static class BezierUtility
    {
        /// <summary>Quadratic Bezier: p0 at t=0, p2 at t=1, bends toward p1.</summary>
        public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return (oneMinusT * oneMinusT * p0) + (2f * oneMinusT * t * p1) + (t * t * p2);
        }
    }
}

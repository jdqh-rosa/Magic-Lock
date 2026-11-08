using UnityEngine;

namespace MagicLock.Radial
{
    internal static class Helper
    {
        /// <summary>Point on a circle of <paramref name="radius"/> at <paramref name="deg"/> degrees (0 = right, 90 = up).</summary>
        public static Vector2 PolarToCart(float deg, float radius)
        {
            float rad = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        /// <summary>Destroy that also works outside Play Mode.</summary>
        public static void DestroySafe(Object obj)
        {
            if (!obj) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}

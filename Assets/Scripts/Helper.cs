using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Helper
{
    public static float CalculateCircm(float pRadius)
    {
        return (2 * Mathf.PI * pRadius);
    }

    public static float CalculateCircmDeg(float pRadius)
    {
        return 360 / CalculateCircm(pRadius);
    }

    public static Vector2 PolarToCart(float deg, float pRadius)
    {
        float x = pRadius * Mathf.Cos(deg * Mathf.Deg2Rad);
        float y = x * Mathf.Tan(deg * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }

    public static Vector2 CartesianToPol(float x, float y)
    {
        float radius = Mathf.Sqrt(x * x + y * y);
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return new Vector2(radius, angle);
    }

    public static Vector2 OrbitPoint(Vector2 pivot, Vector2 point, float angle)
    {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

        // translate point back to origin:
        point.x -= pivot.x;
        point.y -= pivot.y;

        // rotate point
        float xnew = point.x * cos - point.y * sin;
        float ynew = point.x * sin + point.y * cos;

        // translate point back:
        point.x = xnew + pivot.x;
        point.y = ynew + pivot.y;
        return point;
    }

}

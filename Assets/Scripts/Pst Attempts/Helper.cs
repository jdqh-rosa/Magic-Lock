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

    public static Vector2 CalculateDegPos(float deg, float pRadius)
    {
        float x = pRadius * Mathf.Cos(deg * Mathf.Deg2Rad);
        float y = x * Mathf.Tan(deg * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }
}

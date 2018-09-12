using UnityEngine;

public static class Bezier
{
    public static Vector3 CalculateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        t = Mathf.Clamp01(t);

        float mt = 1f - t;
        float t2 = t * t;
        float mt2 = mt * mt;
        return (a * mt2) + (b * 2f * mt * t) + (c * t2);
    }

    public static Vector3 GetQuadratictDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return
            2f * (1f - t) * (p1 - p0) +
            2f * t * (p2 - p1);
    }

    public static Vector3 CalculateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        t = Mathf.Clamp01(t);
        float t2 = t * t;
        float t3 = t * t * t;
        float mt = 1f - t;
        float mt2 = mt * mt;
        float mt3 = mt * mt * mt;
        return (a * mt3) + (3f * mt2 * t * b) + (3f * mt * t2 * c) + (t3 * d);
    }

    public static Vector3 GetCubicDerivative(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (b - a) +
            6f * oneMinusT * t * (c - b) +
            3f * t * t * (d - c);
    }
}

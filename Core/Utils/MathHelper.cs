public static class MathHelper
{
    public const float Pi = 3.1415927f;
    public const float TwoPi = Pi * 2f;

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static float SmoothStep(float edge0, float edge1, float t)
    {
        t = Clamp((t - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }
    
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (Pi / 180f);
    }
    
    /// <summary>
    /// Clamps a value between a minimum float and a maximum float.
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
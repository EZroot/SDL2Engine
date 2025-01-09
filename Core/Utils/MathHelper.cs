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
        t = Math.Clamp((t - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }
}
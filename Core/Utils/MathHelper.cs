using OpenTK.Mathematics;

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
    /// Generates a model matrix for a 2D object.
    /// </summary>
    /// <param name="position">The position of the object in 2D space.</param>
    /// <param name="scale">The uniform scale of the object. Defaults to 1.</param>
    /// <returns>The model matrix.</returns>
    public static Matrix4 GetMatrixTranslation(Vector3 position, float scale = 1.0f)
    {
        // Create the model matrix with translation and scale
        return Matrix4.CreateScale(scale, scale, 1.0f) * Matrix4.CreateTranslation(position);
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
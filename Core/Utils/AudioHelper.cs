public static class AudioHelper
{
    public static float NormalizeAmplitude(float amplitude)
    {
        float cappedAmplitude = Math.Clamp(amplitude, 0f, 5000f);
        // Normalize between 0 and 1
        return cappedAmplitude / 5000f;
    }
}
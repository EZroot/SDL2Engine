public static class ColorHelper
{
    public static uint GetColor(byte r, byte g, byte b, byte a = 255)
    {
        return (uint)((a << 24) | (b << 16) | (g << 8) | r);
    }
}
public static class ColorHelper
{
    public static uint GetColor(byte r, byte g, byte b, byte a = 255)
    {
        return (uint)((a << 24) | (b << 16) | (g << 8) | r);
    }
    
    public static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
    {
        int i;
        double f, p, q, t;
        if (s == 0)
        {
            // Achromatic (grey)
            return ((byte)(v * 255), (byte)(v * 255), (byte)(v * 255));
        }
        h /= 60; // sector 0 to 5
        i = (int)Math.Floor(h);
        f = h - i; // factorial part of h
        p = v * (1 - s);
        q = v * (1 - s * f);
        t = v * (1 - s * (1 - f));
        switch (i)
        {
            case 0:
                return ((byte)(v * 255), (byte)(t * 255), (byte)(p * 255));
            case 1:
                return ((byte)(q * 255), (byte)(v * 255), (byte)(p * 255));
            case 2:
                return ((byte)(p * 255), (byte)(v * 255), (byte)(t * 255));
            case 3:
                return ((byte)(p * 255), (byte)(q * 255), (byte)(v * 255));
            case 4:
                return ((byte)(t * 255), (byte)(p * 255), (byte)(v * 255));
            default: // case 5:
                return ((byte)(v * 255), (byte)(p * 255), (byte)(q * 255));
        }
    }
    
}
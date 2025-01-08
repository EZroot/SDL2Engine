using System.Diagnostics;

public static class Time
{
    private static Stopwatch _fpsStopwatch;
    private static int _frameCount;
    private static float _currentFps;

    private static Stopwatch _deltaStopwatch;
    private static float _deltaTime;

    public static float Fps => _currentFps;
    public static float DeltaTime => _deltaTime;

    static Time()
    {
        _fpsStopwatch = new Stopwatch();
        _fpsStopwatch.Start();
        _frameCount = 0;
        _currentFps = 0f;
        _deltaStopwatch = new Stopwatch();
        _deltaStopwatch.Start();
        _deltaTime = 0f;
    }

    /// <summary>
    /// Call this method once per frame to update FPS and DeltaTime.
    /// </summary>
    public static void Update()
    {
        _frameCount++;

        _deltaTime = (float)_deltaStopwatch.Elapsed.TotalSeconds;
        _deltaStopwatch.Restart();

        if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
        {
            _currentFps = _frameCount / (_fpsStopwatch.ElapsedMilliseconds / 1000f);
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }
    }
}

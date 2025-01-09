using System.Diagnostics;

public static class Time
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private static float _lastFrameTime;

    // frame rate (60 FPS = 1 / 60 = 0.0167 seconds per frame)
    private const float TARGET_FRAME_RATE = 60f;
    private const float TARGET_DELTA_TIME = 1f / TARGET_FRAME_RATE;

    public static float DeltaTime { get; private set; }
    public static float Fps { get; private set; }
    public static float DeltaFps => 1f / DeltaTime;

    private static int _frameCount = 0;
    private static float _fpsTimer = 0f;

    /// <summary>
    /// Update engine time values
    /// </summary>
    public static void Update()
    {
        float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
        float elapsedTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;

        DeltaTime = elapsedTime / TARGET_DELTA_TIME;

        if (DeltaTime > 0.1f)
            DeltaTime = 0.1f;

        _fpsTimer += elapsedTime;
        _frameCount++;

        if (_fpsTimer >= 1f)
        {
            Fps = _frameCount / _fpsTimer;
            _fpsTimer = 0f;
            _frameCount = 0;
        }
    }
}

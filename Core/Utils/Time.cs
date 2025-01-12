using System.Diagnostics;

public static class Time
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private static float _lastFrameTime;

    // Unity-like "target" frame rate settings
    private const float TARGET_FRAME_RATE = 60f;
    private const float TARGET_DELTA_TIME = 1f / TARGET_FRAME_RATE;

    /// <summary>
    /// Real, unscaled seconds between frames.
    /// </summary>
    public static float RawDeltaTime { get; private set; }

    /// <summary>
    /// Scaled delta time (Unity-like).
    /// If your FPS is exactly 60, this will be ~1.0f each frame.
    /// If you are below 60 FPS, this can become > 1.0f.
    /// </summary>
    public static float DeltaTime { get; private set; }

    public static float Fps { get; private set; }
    public static float DeltaFps => 1f / DeltaTime;
    public static double TotalTime => _stopwatch.Elapsed.TotalSeconds;

    private static int _frameCount = 0;
    private static float _fpsTimer = 0f;

    public static void Update()
    {
        float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
        float elapsedTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;

        // 1. Store the *real* unscaled time between frames
        RawDeltaTime = elapsedTime;

        // 2. Compute your "Unity-like" DeltaTime
        DeltaTime = elapsedTime / TARGET_DELTA_TIME;

        // Safety clamp
        if (DeltaTime > 0.1f)
            DeltaTime = 0.1f;

        // FPS calculation still uses the real elapsed time
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
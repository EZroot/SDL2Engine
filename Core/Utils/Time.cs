using System.Diagnostics;

public static class Time
{
    private static Stopwatch _stopwatch = Stopwatch.StartNew();
    private static float _lastFrameTime;

    public static float DeltaTime { get; private set; }
    public static float Fps => 1f / DeltaTime;

    /// <summary>
    /// Call this once per frame to update time values.
    /// </summary>
    public static void Update()
    {
        float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
        DeltaTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;

        // Cap DeltaTime to avoid huge jumps (e.g., when pausing the debugger)
        if (DeltaTime > 0.1f)
            DeltaTime = 0.1f;
    }
}

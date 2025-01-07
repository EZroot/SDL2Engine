using SDL2Engine.Core.Configuration.Components;

public class OnWindowResized : EventArgs
{
    public readonly WindowSettings WindowSettings;

    public OnWindowResized(WindowSettings windowSettings)
    {
        WindowSettings = windowSettings;
    }
}

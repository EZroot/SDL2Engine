using Microsoft.Extensions.DependencyInjection;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Physics;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Windowing;
using SDL2Engine.Core.Windowing.Interfaces;
using ImGuiWindowBuilder = SDL2Engine.Core.GuiRenderer.ImGuiWindowBuilder;

namespace SDL2Engine.Core;

/// <summary>
/// Bridge to run our game through the SDL2 Engine
/// </summary>
public class GameApp
{
    private readonly Engine _engine;

    public GameApp()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IServiceWindowConfig, WindowConfig>();
        services.AddSingleton<IServiceSysInfo, SysInfo>();

        services.AddSingleton<IServiceWindowService, WindowService>();
        services.AddSingleton<IServiceRenderService, RenderService>();
        services.AddSingleton<IServicePhysicsService, PhysicsService>();

        services.AddSingleton<IServiceGuiRenderService, ImGuiRenderService>();
        services.AddSingleton<IServiceGuiWindowBuilder, ImGuiWindowBuilder>();
        services.AddSingleton<IVariableBinder, ImGuiVariableBinder>();

        services.AddSingleton<IServiceAudioLoader, AudioLoader>();
        services.AddSingleton<IServiceImageLoader, ImageLoader>();
        services.AddSingleton<IServiceAssetManager, AssetManager>();
        services.AddSingleton<IServiceCameraService, CameraService>();

        var serviceProvider = services.BuildServiceProvider();
        
        _engine = new Engine(serviceProvider);
    }

    public void Run(IGame game)
    {
        _engine?.Run(game);
    }
}
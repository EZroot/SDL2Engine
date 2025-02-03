using Microsoft.Extensions.DependencyInjection;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Fonts;
using SDL2Engine.Core.Addressables.Fonts.Interfaces;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Networking;
using SDL2Engine.Core.Networking.Interfaces;
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

    /// <summary>
    /// If using OPENGL, you must use custom opengl bindings
    /// SDL Renderer makes things much easier, but no custom OPENGL 
    /// </summary>
    /// <param name="rendererType"></param>
    public GameApp(RendererType rendererType = RendererType.SDLRenderer, PipelineType pipelineType = PipelineType.Pipe2D)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IWindowConfig, WindowConfig>();
        services.AddSingleton<ISysInfo, SysInfo>();

        services.AddSingleton<IFontService, FontService>();
        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<IAudioService, AudioService>();
        
        services.AddSingleton<ICameraService, CameraService>();
        
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IRenderService, RenderService>();
        services.AddSingleton<IPhysicsService, PhysicsService>();

        services.AddSingleton<IGuiRenderService, ImGuiRenderService>();
        services.AddSingleton<IGuiWindowBuilder, ImGuiWindowBuilder>();
        services.AddSingleton<IVariableBinder, ImGuiVariableBinder>();
        
        services.AddSingleton<INetworkService, NetworkService>();

        var serviceProvider = services.BuildServiceProvider();
        
        _engine = new Engine(serviceProvider, rendererType, pipelineType);
    }

    public void Run(IGame game)
    {
        _engine?.Run(game);
    }
}

public enum PipelineType
{
    Pipe2D,
    Pipe3D
}

public enum RendererType
{
    SDLRenderer,
    OpenGlRenderer
}

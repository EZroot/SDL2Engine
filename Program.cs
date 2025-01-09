using Microsoft.Extensions.DependencyInjection;
using SDL2Engine.Core;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Windowing;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.GuiRenderer.Interfaces;

class Program
{
    static void Main(string[] args)
    {
            var services = new ServiceCollection();

            services.AddSingleton<IServiceWindowConfig, WindowConfig>();
            services.AddSingleton<IServiceSysInfo, SysInfo>();

            services.AddSingleton<IServiceWindowService, WindowService>();
            services.AddSingleton<IServiceRenderService, RenderService>();

            services.AddSingleton<IServiceGuiRenderService, ImGuiRenderService>();
            services.AddSingleton<IServiceGuiWindowService, ImGuiWindowBuilder>();
            services.AddSingleton<IVariableBinder, ImGuiVariableBinder>();
            
            services.AddSingleton<IServiceImageLoader, ImageLoader>();
            services.AddSingleton<IServiceAssetManager, AssetManager>();

            services.AddSingleton<Engine>();

            var serviceProvider = services.BuildServiceProvider();

            // ensure disposability
            using (var engine = serviceProvider.GetService<Engine>())
            {
                engine?.Run();
            }
    }
}

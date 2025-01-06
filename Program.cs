﻿using Microsoft.Extensions.DependencyInjection;
using SDL2Engine.Core;
using SDL2Engine.Core.Configuration;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Windowing;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.Addressables;

class Program
{
    static void Main(string[] args)
    {
            var services = new ServiceCollection();

            services.AddSingleton<IServiceWindowConfig, WindowConfig>();
            services.AddSingleton<IServiceWindowService, WindowService>();
            services.AddSingleton<IServiceRenderService, RenderService>();
            services.AddSingleton<IServiceGuiRenderService, ImGuiRenderService>();
            services.AddSingleton<IServiceImageLoader, ImageLoader>();

            services.AddSingleton<Engine>();

            var serviceProvider = services.BuildServiceProvider();

            //Ensure disposability
            using (var engine = serviceProvider.GetService<Engine>())
            {
                engine?.Run();
            }
    }
}

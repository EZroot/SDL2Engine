using System;
using SDL2;

class Program
{
    static void Main(string[] args)
    {
        // Initialize SDL
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine("SDL could not initialize! SDL_Error: " + SDL.SDL_GetError());
            return;
        }

        // Create window
        IntPtr window = SDL.SDL_CreateWindow(
            "SDL2 Engine >:)",
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            800,
            600,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
        );

        if (window == IntPtr.Zero)
        {
            Console.WriteLine("Window creation failed! SDL_Error: " + SDL.SDL_GetError());
            SDL.SDL_Quit();
            return;
        }

        // Create a renderer
        IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, 0);
        if (renderer == IntPtr.Zero)
        {
            Console.WriteLine("Renderer creation failed! SDL_Error: " + SDL.SDL_GetError());
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
            return;
        }

        // Main loop
        bool running = true;
        while (running)
        {
            // Process events
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    running = false;
                }
                else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
                         e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
                {
                    running = false;
                }
            }

            // Set render color (R, G, B, A) -> Blue
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 255, 255);
            // Clear the current rendering target with the set color
            SDL.SDL_RenderClear(renderer);
            // Present the current rendering
            SDL.SDL_RenderPresent(renderer);

            // Tiny delay to avoid maxing out CPU
            SDL.SDL_Delay(10);
        }

        // Cleanup
        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }
}

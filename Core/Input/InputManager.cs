using System.Diagnostics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SDL2;
namespace SDL2Engine.Core.Input
{
    public static class InputManager
    {
        private static HashSet<SDL.SDL_Keycode> _keysPressed = new();
        public static bool IsKeyPressed(SDL.SDL_Keycode key)
        {
            return _keysPressed.Contains(key);
        }

        public static void Update(SDL.SDL_Event e)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    io.MouseWheel += e.wheel.y;
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    int button = e.button.button;
                    bool isDown = e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;
                    if (button == SDL.SDL_BUTTON_LEFT) io.MouseDown[0] = isDown;
                    if (button == SDL.SDL_BUTTON_RIGHT) io.MouseDown[1] = isDown;
                    if (button == SDL.SDL_BUTTON_MIDDLE) io.MouseDown[2] = isDown;
                    break;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    unsafe
                    {
                        string text = Marshal.PtrToStringUTF8((IntPtr)e.text.text);
                        io.AddInputCharactersUTF8(text);
                    }
                    break;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    _keysPressed.Add(e.key.keysym.sym);
                    UpdateImGuiIO(io, e);
                    break;
                case SDL.SDL_EventType.SDL_KEYUP:
                    _keysPressed.Remove(e.key.keysym.sym);
                    UpdateImGuiIO(io, e);
                    break;
            }

            int mouseX, mouseY;
            uint mouseState = SDL.SDL_GetMouseState(out mouseX, out mouseY);
            io.MousePos = new System.Numerics.Vector2(mouseX, mouseY);

            io.MouseDown[0] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT)) != 0;
            io.MouseDown[1] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_RIGHT)) != 0;
            io.MouseDown[2] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_MIDDLE)) != 0;
        }

        private static void UpdateImGuiIO(ImGuiIOPtr io, SDL.SDL_Event e)
        {
            int keyIndex = (int)e.key.keysym.sym;
            bool down = e.type == SDL.SDL_EventType.SDL_KEYDOWN;

            // Hacky work around for backspaces in IMGUI because I'm retarded
            if (keyIndex == (int)SDL.SDL_Keycode.SDLK_BACKSPACE)
            {
                io.AddKeyEvent(ImGuiKey.Backspace, down);
                io.SetKeyEventNativeData(ImGuiKey.Backspace, (int)SDL.SDL_Keycode.SDLK_BACKSPACE, (int)SDL.SDL_GetScancodeFromKey(SDL.SDL_Keycode.SDLK_BACKSPACE));
            }

            if (keyIndex >= 0 && keyIndex < io.KeysData.Count)
            {
                ImGuiKeyData keyData = io.KeysData[keyIndex];
                keyData.Down = (byte)(down ? 1 : 0);
                keyData.DownDuration = down ? 0 : -1;
                io.KeysData[keyIndex] = keyData;
            }

            io.KeyCtrl = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != 0;
            io.KeyShift = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
            io.KeyAlt = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_ALT) != 0;
            io.KeySuper = (SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_GUI) != 0;
        }
    }
}
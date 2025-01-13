using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SDL2;
using System.Numerics;

namespace SDL2Engine.Core.Input
{
    public static class InputManager
    {
        private static HashSet<SDL.SDL_Keycode> _keysPressed = new();
        private static bool[] _mouseButtonsPressed = new bool[3]; // 0 = left, 1 = right, 2 = middle

        // Added properties to store mouse coordinates
        public static int MouseX { get; private set; }
        public static int MouseY { get; private set; }

        public static bool IsKeyPressed(SDL.SDL_Keycode key)
        {
            return _keysPressed.Contains(key);
        }

        public static bool IsMouseButtonPressed(uint button)
        {
            switch (button)
            {
                case SDL.SDL_BUTTON_LEFT:
                    return _mouseButtonsPressed[0];
                case SDL.SDL_BUTTON_RIGHT:
                    return _mouseButtonsPressed[1];
                case SDL.SDL_BUTTON_MIDDLE:
                    return _mouseButtonsPressed[2];
                default:
                    return false; 
            }
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
                    HandleMouseButtonEvent(e, io);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    MouseX = e.motion.x;
                    MouseY = e.motion.y;
                    io.MousePos = new Vector2(MouseX, MouseY);
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

            int currentMouseX, currentMouseY;
            uint mouseState = SDL.SDL_GetMouseState(out currentMouseX, out currentMouseY);

            // Update mouse coordinates if not already updated by motion event
            // This prevents overwriting motion event updates
            if (e.type != SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                MouseX = currentMouseX;
                MouseY = currentMouseY;
                io.MousePos = new Vector2(MouseX, MouseY);
            }

            _mouseButtonsPressed[0] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT)) != 0;
            _mouseButtonsPressed[1] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_RIGHT)) != 0;
            _mouseButtonsPressed[2] = (mouseState & SDL.SDL_BUTTON(SDL.SDL_BUTTON_MIDDLE)) != 0;

            io.MouseDown[0] = _mouseButtonsPressed[0];
            io.MouseDown[1] = _mouseButtonsPressed[1];
            io.MouseDown[2] = _mouseButtonsPressed[2];
        }

        private static void HandleMouseButtonEvent(SDL.SDL_Event e, ImGuiIOPtr io)
        {
            uint button = e.button.button;
            bool isDown = e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;

            switch (button)
            {
                case SDL.SDL_BUTTON_LEFT:
                    io.MouseDown[0] = isDown;
                    _mouseButtonsPressed[0] = isDown; 
                    break;
                case SDL.SDL_BUTTON_RIGHT:
                    io.MouseDown[1] = isDown;
                    _mouseButtonsPressed[1] = isDown;
                    break;
                case SDL.SDL_BUTTON_MIDDLE:
                    io.MouseDown[2] = isDown;
                    _mouseButtonsPressed[2] = isDown; 
                    break;
            }
        }

        private static void UpdateImGuiIO(ImGuiIOPtr io, SDL.SDL_Event e)
        {
            int keyIndex = (int)e.key.keysym.sym;
            bool down = e.type == SDL.SDL_EventType.SDL_KEYDOWN;

            // Special handling for backspace key not being assigned normally because I'm retarded
            if (keyIndex == (int)SDL.SDL_Keycode.SDLK_BACKSPACE)
            {
                io.AddKeyEvent(ImGuiKey.Backspace, down);
                io.SetKeyEventNativeData(
                    ImGuiKey.Backspace,
                    (int)SDL.SDL_Keycode.SDLK_BACKSPACE,
                    (int)SDL.SDL_GetScancodeFromKey(SDL.SDL_Keycode.SDLK_BACKSPACE));
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

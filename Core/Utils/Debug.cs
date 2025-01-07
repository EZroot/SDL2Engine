using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;
using SDL2;

namespace SDL2Engine.Core.Utils
{
    public static class Debug
    {
        private static bool IsDebugMode = true;
        private static bool IsDebugModePollEvents = false;
        private static bool IsDebugModeEventHub = true;
        private static bool IsShowingMethodNames = true;

        public static void LogEvents(string msg)
        {
            if (!IsDebugModeEventHub) return;
            Log($"<color=darkyellow>{msg}</color>");
        }

        // No worky ):
        // public static void LogEvents<TEventArgs>(TEventArgs e) where TEventArgs : EventArgs
        // {
        //     if (!IsDebugModeEventHub) return;
        //     var details = GetEventArgsDetails(e);
        //     Log($"<color=darkyellow>{details}</color>");
        // }

        public static void LogPollEvents(SDL.SDL_Event e)
        {
            if (!IsDebugModePollEvents) return;
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    Debug.Log($"Key event: {e.key.keysym.sym}, State: {e.key.state}");
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    Debug.Log($"Mouse motion: X={e.motion.x}, Y={e.motion.y}, DX={e.motion.xrel}, DY={e.motion.yrel}");
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    Debug.Log($"Mouse button event: Button={e.button.button}, X={e.button.x}, Y={e.button.y}, State={e.button.state}");
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    Debug.Log($"Mouse wheel event: X={e.wheel.x}, Y={e.wheel.y}");
                    break;

                case SDL.SDL_EventType.SDL_QUIT:
                    Debug.Log("Quit event triggered.");
                    break;

                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    Debug.Log($"Window event: {e.window.windowEvent}");
                    break;

                // case SDL.SDL_EventType.SDL_TEXTINPUT:
                //     Debug.Log($"Text input event: {System.Text.Encoding.UTF8.GetString(e.text.)}");
                //     break;

                default:
                    Debug.Log($"Unhandled event type: {e.type}");
                    break;
            }
        }
        public static T Throw<T>(Exception exception, string message = "")
        {
            LogError(message);
            throw exception;
        }

        public static void LogError(string message)
        {
            Log($"<color=red>Error: {message}</color>");
        }

        public static void LogException(string message, Exception ex)
        {
            Log($"<color=red>Exception: {message} - {ex.Message}</color>");
            throw ex;
        }

        public static void Log(string input)
        {
            var timeStamp = DateTime.Now.ToString("h:mm tt").Replace(" ", "");

            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame?.GetMethod();
            var callerClassName = "";
            var callerMethodName = "";

            if (method?.ReflectedType != null)
            {
                callerClassName = CleanGeneratedNames(method.ReflectedType.Name);
                callerMethodName = CleanGeneratedNames(method.Name);
            }

            if (IsDebugMode)
            {
                if (IsShowingMethodNames)
                    input = $"<color=magenta>{timeStamp}</color> <color=yellow>[{callerClassName}.{callerMethodName}]</color> " + input;
                else
                    input = $"<color=magenta>{timeStamp}</color> <color=yellow>[{callerClassName}]</color> " + input;
            }
            else
                input = $"<color=magenta>{timeStamp}</color> " + input;

            int currentIndex = 0;

            while (currentIndex < input.Length)
            {
                int openTagStart = input.IndexOf("<color=", currentIndex);
                if (openTagStart == -1)
                {
                    Console.Write(input.Substring(currentIndex));
                    break;
                }
                Console.Write(input.Substring(currentIndex, openTagStart - currentIndex));

                int openTagEnd = input.IndexOf(">", openTagStart);
                if (openTagEnd == -1)
                {
                    Console.Write(input.Substring(currentIndex));
                    break;
                }
                string colorName = input.Substring(openTagStart + 7, openTagEnd - (openTagStart + 7));
                ConsoleColor color;
                if (Enum.TryParse(colorName, true, out color))
                {
                    Console.ForegroundColor = color;
                }

                int closeTagStart = input.IndexOf("</color>", openTagEnd);
                if (closeTagStart == -1)
                {
                    Console.Write(input.Substring(openTagEnd + 1));
                    break;
                }

                Console.Write(input.Substring(openTagEnd + 1, closeTagStart - (openTagEnd + 1)));
                Console.ResetColor();

                currentIndex = closeTagStart + 8;
            }
            Console.WriteLine();
        }

        private static string CleanGeneratedNames(string name)
        {
            int genericIndex = name.IndexOf('`');
            if (genericIndex != -1)
            {
                name = name.Substring(0, genericIndex);
            }
            name = name.Replace('+', '.');

            Match meaningfulPart = Regex.Match(name, @"\<(.+?)\>");
            if (meaningfulPart.Success)
            {
                name = meaningfulPart.Groups[1].Value;
            }
            else
            {
                name = Regex.Replace(name, @"\.MoveNext$", "");
            }

            return name;
        }

        private static string GetEventArgsDetails<TEventArgs>(TEventArgs e) where TEventArgs : EventArgs
        {
            var properties = typeof(TEventArgs).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var eventArgsDetails = new StringBuilder();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(e, null);
                eventArgsDetails.Append($"{prop.Name}: {value}, ");
            }

            // Remove the last comma and space
            if (eventArgsDetails.Length > 0)
                eventArgsDetails.Length -= 2;

            return eventArgsDetails.ToString();
        }

    }
}

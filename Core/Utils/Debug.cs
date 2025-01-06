using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SDL2Engine.Core.Utils
{
    public static class Debug
    {
        private static bool IsDebugMode = true;
        private static bool IsShowingMethodNames = true;

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
            
            if(method?.ReflectedType != null)
            {
                callerClassName = CleanGeneratedNames(method.ReflectedType.Name);
                callerMethodName = CleanGeneratedNames(method.Name);
            }

            if (IsDebugMode)
            {
                if(IsShowingMethodNames)
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
    }
}

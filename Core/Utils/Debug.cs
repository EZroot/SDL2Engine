using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SDL2;
using ImGuiNET;
using System.Numerics;
using System.Drawing;

namespace SDL2Engine.Core.Utils
{
    public static class Debug
    {
        private const int MAX_LOG_ENTRIES = 1000;

        public static bool IsDebugMode = true;
        public static bool IsDebugModePollEvents = false;
        public static bool IsDebugModeEventHub = true;
        public static bool IsShowingMethodNames = true;

        private static readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private static readonly object _logLock = new object();
        private static string m_searchFilter = "";

        public static IReadOnlyList<LogEntry> LogEntries
        {
            get
            {
                lock (_logLock)
                {
                    return _logEntries.AsReadOnly();
                }
            }
        }

        public static void LogEvents(string msg)
        {
            if (!IsDebugModeEventHub) return;
            Log($"<color=DarkYellow>{msg}</color>", 2);
        }

        public static void LogPollEvents(SDL.SDL_Event e)
        {
            if (!IsDebugModePollEvents) return;
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    Log($"<color=DarkYellow>Key event: {e.key.keysym.sym}, State: {e.key.state}</color>", 2);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    Log($"<color=DarkYellow>Mouse motion: X={e.motion.x}, Y={e.motion.y}, DX={e.motion.xrel}, DY={e.motion.yrel}</color>", 2);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    Log($"<color=DarkYellow>Mouse button event: Button={e.button.button}, X={e.button.x}, Y={e.button.y}, State={e.button.state}</color>", 2);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    Log($"<color=DarkYellow>Mouse wheel event: X={e.wheel.x}, Y={e.wheel.y}</color>", 2);
                    break;

                case SDL.SDL_EventType.SDL_QUIT:
                    Log($"<color=DarkYellow>Quit event triggered.</color>", 2);
                    break;

                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    Log($"<color=DarkYellow>Window event: {e.window.windowEvent}</color>", 2);
                    break;

                default:
                    Log($"<color=DarkYellow>Unhandled event type: {e.type}</color>", 2);
                    break;
            }
        }

        public static T Throw<T>(Exception exception, string message = "") where T : Exception
        {
            LogError(message);
            throw exception;
        }

        public static void LogError(string message)
        {
            Log($"<color=Red>Error: {message}</color>", 2);
        }

        public static void LogException(string message, Exception ex)
        {
            Log($"<color=Red>Exception: {message} - {ex.Message}</color>", 2);
            throw ex;
        }

        /// <summary>
        /// Log Console Input
        /// Format with <color=value></color>
        /// </summary>
        /// <param name="input"></param>
        public static void Log(string input, int stackFrameIndex = 1)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(stackFrameIndex);
            WriteToConsole(input, frame);
        }

        private static List<(string text, Color color)> ParseColorTags(string input)
        {
            var spans = new List<(string text, Color color)>();
            string pattern = @"<color=(?<color>[a-zA-Z]+)>(?<text>.*?)<\/color>";
            var matches = Regex.Matches(input, pattern);

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    string beforeText = input.Substring(lastIndex, match.Index - lastIndex);
                    if (!string.IsNullOrWhiteSpace(beforeText))
                        spans.Add((beforeText, Color.White));
                }

                string colorName = match.Groups["color"].Value;
                string text = match.Groups["text"].Value;
                Color color = GetColorFromName(colorName);
                if (!string.IsNullOrWhiteSpace(text))
                    spans.Add((text, color));

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < input.Length)
            {
                string remainingText = input.Substring(lastIndex);
                if (!string.IsNullOrWhiteSpace(remainingText))
                    spans.Add((remainingText, Color.White));
            }

            return spans;
        }

        private static Color GetColorFromName(string colorName)
        {
            try
            {
                return Color.FromName(colorName);
            }
            catch
            {
                return Color.White;
            }
        }

        private static void WriteToConsole(string input, StackFrame? trace)
        {
            var timeStamp = DateTime.Now.ToString("h:mm tt").Replace(" ", "");
            var method = trace?.GetMethod();

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
                    input = $"<color=Magenta>{timeStamp}</color> <color=Yellow>[{callerClassName}.{callerMethodName}]</color> " + input;
                else
                    input = $"<color=Magenta>{timeStamp}</color> <color=Yellow>[{callerClassName}]</color> " + input;
            }
            else
                input = $"<color=Magenta>{timeStamp}</color> " + input;

            List<(string text, Color color)> parsedSpans = ParseColorTags(input);
            lock (_logLock)
            {
                var logEntry = new LogEntry();
                foreach (var span in parsedSpans)
                {
                    logEntry.Spans.Add(span);
                }
                _logEntries.Add(logEntry);
                while (_logEntries.Count > MAX_LOG_ENTRIES)
                {
                    _logEntries.RemoveAt(0);
                }
            }

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

        private static string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=.*?>|<\/color>", string.Empty);
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

            if (eventArgsDetails.Length > 0)
                eventArgsDetails.Length -= 2;

            return eventArgsDetails.ToString();
        }

        /// <summary>
        /// Create a IMGUI Debug Console
        /// </summary>
        public static void RenderDebugConsole(ref bool isOpen)
        {
            if (ImGui.Begin("Debug Console", ref isOpen, ImGuiWindowFlags.MenuBar))
            {
                bool shouldScrollToMatch = false;

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.Button("Options"))
                    {
                        ImGui.OpenPopup("MoreOptionsPopup");
                    }

                    ImGui.Separator();
                    ImGui.Text("Search");
                    if (ImGui.InputText("##Search", ref m_searchFilter, 1024))
                    {
                        shouldScrollToMatch = true;
                        if (ImGui.IsItemFocused())
                        {
                            Utils.Debug.Log("Input is focused");
                        }
                    }

                    if (ImGui.BeginPopup("MoreOptionsPopup"))
                    {
                        if (ImGui.Checkbox("Show Method Names", ref IsShowingMethodNames))
                        {
                            Log($"Debug Method Names: {Utils.Debug.IsShowingMethodNames}");
                        }
                        ImGui.Separator();
                        if (ImGui.Checkbox("InputDbg", ref IsDebugModePollEvents))
                        {
                            Log($"Debug Input Poll: {Utils.Debug.IsDebugModePollEvents}");
                        }
                        if (ImGui.Checkbox("EventDbg", ref IsDebugModeEventHub))
                        {
                            Log($"Debug EventHub: {Utils.Debug.IsDebugModeEventHub}");
                        }
                        ImGui.Separator();
                        if (ImGui.Button("Clear Logs"))
                        {
                            ClearLogs();
                        }
                        ImGui.EndPopup();
                    }
                    ImGui.EndMenuBar();
                }

                Vector2 availableSize = new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y);
                if (ImGui.BeginChild("ConsoleRegion", availableSize, ImGuiChildFlags.Borders))
                {
                    int scrollToIndex = -1;
                    lock (_logLock)
                    {
                        int index = 0;
                        foreach (var entry in _logEntries)
                        {
                            bool matchesSearch = string.IsNullOrWhiteSpace(m_searchFilter) || entry.Spans.Any(span => span.Text.Contains(m_searchFilter, StringComparison.OrdinalIgnoreCase));
                            if (matchesSearch)
                            {
                                if (scrollToIndex == -1 && shouldScrollToMatch)
                                {
                                    scrollToIndex = index;
                                }

                                bool firstSpan = true;
                                foreach (var span in entry.Spans)
                                {
                                    if (!firstSpan)
                                        ImGui.SameLine();
                                    firstSpan = false;

                                    Vector4 imguiColor = new Vector4(
                                       span.Color.R / 255f,
                                       span.Color.G / 255f,
                                       span.Color.B / 255f,
                                       1.0f
                                    );

                                    if (imguiColor.X == 0 && imguiColor.Y == 0 && imguiColor.Z == 0)
                                    {
                                        imguiColor = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
                                    }

                                    ImGui.PushStyleColor(ImGuiCol.Text, imguiColor);
                                    var text = RemoveColorTags(Regex.Replace(span.Text, @"\s+", " "));
                                    ImGui.TextUnformatted(text.Trim());
                                    ImGui.PopStyleColor();
                                }
                            }
                            index++;
                        }
                    }

                    if (scrollToIndex != -1)
                    {
                        ImGui.SetScrollHereY(1.0f);
                    }

                    ImGui.SetScrollHereY(1.0f);
                }
                    ImGui.EndChild();

            }
            ImGui.End();
        }

        public static void ClearLogs()
        {
            lock (_logLock)
            {
                _logEntries.Clear();
            }
        }
    }
}

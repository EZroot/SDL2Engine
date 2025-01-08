
using System.Drawing;

namespace SDL2Engine.Core.Utils
{
    public class LogEntry
    {
        public List<(string Text, Color Color)> Spans { get; set; } = new List<(string Text, Color Color)>();
    }
}

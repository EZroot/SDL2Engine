using System.Numerics;
using ImGuiNET;

namespace SDL2Engine.Core.GuiRenderer.Helpers
{
    public class ImGuiTableData
    {
        public ImGuiColumnData[] Columns { get; }
        public ImGuiTableFlags TableFlags {get;}
        public bool LableOnRight {get;}

        public ImGuiTableData(ImGuiTableFlags tableFlags = ImGuiTableFlags.None, bool labelOnRight = true, params ImGuiColumnData[] columns)
        {
            TableFlags = tableFlags;
            LableOnRight = labelOnRight;
            Columns = columns ?? Array.Empty<ImGuiColumnData>();
        }
    }

   public class ImGuiColumnData
    {
        public string Header { get; }
        public ImGuiInputData[] Values { get; set; }

        public ImGuiColumnData(string header, params ImGuiInputData[] values)
        {
            Header = header;
            Values = values;
        }
    }

       public class ImGuiInputData
    {
        public string Label { get; }
        public string Value { get; set; }
        public bool IsReadOnly { get; }
        public bool IsPassword { get; }
        public Vector4 Color { get; }

        public ImGuiInputData(string label, string value, Vector4 color, bool isReadOnly = false)
        {
            Label = label;
            Value = value;
            IsReadOnly = isReadOnly;
            Color = color;
        }

        public ImGuiInputData(string label, string value, bool isReadOnly = false)
        {
            Label = label;
            Value = value;
            IsReadOnly = isReadOnly;
            Color = Vector4.One;
        }
    }
}

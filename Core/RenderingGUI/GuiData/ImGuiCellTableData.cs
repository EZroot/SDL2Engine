namespace SDL2Engine.Core.GuiRenderer.Helpers
{
    public class ImGuiCellTableData
    {
        public ImGuiCellData[] ImGuiCell { get; }
        public ImGuiCellTableData(params ImGuiCellData[] values)
        {
            ImGuiCell = values;
        }
    }

    public class ImGuiCellData
    {
        public string Header { get; }
        public string[] Value { get; }
        public ImGuiCellData(string header, params string[] values)
        {
            Header = header;
            Value = values;
        }
    }
}
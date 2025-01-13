namespace SDL2Engine.Core.GuiRenderer.Helpers;

public class ImGuiDockData
{
    public bool IsDockInitialized { get; set; }
    public DockPanelData MainDock { get; set; }
    public DockPanelData LeftDock { get; set; }
    public DockPanelData TopDock { get; set; }
    public DockPanelData RightDock { get; set; }
    public DockPanelData BottomDock { get; set; }
    
    public ImGuiDockData(DockPanelData mainDockId, 
        DockPanelData leftDock, 
        DockPanelData topDockId, 
        DockPanelData rightDock, 
        DockPanelData bottomDock)
    {
        IsDockInitialized = false;
        MainDock = mainDockId;
        LeftDock = leftDock;
        TopDock = topDockId;
        RightDock = rightDock;
        BottomDock = bottomDock;
    }
}

public class DockPanelData
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public bool IsEnabled { get; set; }
    
    public DockPanelData(string name, bool isEnabled)
    {
        Name = name;
        Id = 0;
        IsEnabled = isEnabled;
    }
}

public enum DockPanelType
{
    Main,
    Left,
    Top,
    Right,
    Bottom
}
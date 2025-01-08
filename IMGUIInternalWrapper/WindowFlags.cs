using System;

[Flags]
public enum WindowFlags : uint
{
    None = 0,
    NoTitleBar = 1 << 0,
    NoResize = 1 << 1,
    NoMove = 1 << 2,
    NoScrollbar = 1 << 3,
    NoScrollWithMouse = 1 << 4,
    NoCollapse = 1 << 5,
    AlwaysAutoResize = 1 << 6,
    NoBackground = 1 << 7, // Note: This flag doesn't exist in ImGui by default
    NoSavedSettings = 1 << 8,
    NoMouseInputs = 1 << 9,
    MenuBar = 1 << 10,
    HorizontalScrollbar = 1 << 11,
    NoFocusOnAppearing = 1 << 12,
    NoBringToFrontOnFocus = 1 << 13,
    AlwaysVerticalScrollbar = 1 << 14,
    AlwaysHorizontalScrollbar = 1 << 15,
    NoDocking = 1 << 16,
    Popup = 1 << 17,
    Modal = 1 << 18,
    ChildWindow = 1 << 19,
    ChildMenu = 1 << 20,
    Tooltip = 1 << 21,
    PreferredFirstItem = 1 << 22,
    NoNavInputs = 1 << 23,
    NoNavFocus = 1 << 24,
    NavFlattened = 1 << 25,
    ChildWindowAutoFitX = 1 << 26,
    ChildWindowAutoFitY = 1 << 27,
    ComboBox = 1 << 28,
    DockNodeHost = 1 << 29,
    DockSpace = 1 << 30,
    // New flags added in recent ImGui versions
    // Add additional flags as needed
}

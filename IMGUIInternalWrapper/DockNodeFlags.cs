using System;

[Flags]
public enum DockNodeFlags : uint
{
    None = 0,
    KeepAliveOnly = 1 << 0,
    NoCentralNode = 1 << 1,
    NoDockingInCentralNode = 1 << 2,
    NoSplit = 1 << 3,
    NoResize = 1 << 4,
    NoDock = 1 << 5,
    PassthruCentralNode = 1 << 6,
    AutoHideTabBar = 1 << 7,
    NoTabBar = 1 << 8,
    NoTabListPopup = 1 << 9,
    NoWindowMenuButton = 1 << 10,
    NoCloseButton = 1 << 11,
    NoDockingOverEmptySpace = 1 << 12,
    NoDockingSplitEmptySpace = 1 << 13,
    NoResizeTabBar = 1 << 14,
    NoDockingSplitTop = 1 << 15,
    // New flags added in recent ImGui versions
    HideTabBar = 1 << 16, // Example of a new flag
    // Add additional flags as needed
}

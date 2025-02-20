using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace SDL2Engine.Core.Utils;

public static class PlatformInfo
{
    public const string RESOURCES_FOLDER = "/home/anon/Repos/SDL_Engine/SDL2Engine/resources";

    public static bool IsWindows { get; private set; }
    public static bool IsLinux { get; private set; }
    public static bool IsMacOS { get; private set; }
    public static bool IsAndroid { get; private set; }
    public static bool IsIOS { get; private set; }
    
    public static RendererType RendererType { get; private set; }
    public static PipelineType PipelineType { get; private set; }
    
    public static Vector2i WindowSize { get; set; }
    
    static PlatformInfo()
    {
        DetectPlatform();
    }

    public static void SetRenderType(RendererType rendererType)
    {
        RendererType = rendererType;
    }

    public static void SetPipelineType(PipelineType pipelineType)
    {
        PipelineType = pipelineType;
    }

    private static void DetectPlatform()
    {
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        IsAndroid = RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));
        IsIOS = RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"));
    }
}

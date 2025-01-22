namespace SDL2Engine.Core.Utils;

public static class FileHelper
{
    /// <summary>
    /// Opens a file, returns the content as string, closes a file 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ReadFileContents(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"No file exists: {filePath}");
        }
        return File.ReadAllText(filePath);
    }
}
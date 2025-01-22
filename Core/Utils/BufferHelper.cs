namespace SDL2Engine.Core.Utils;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;
public static class BufferHelper
{
    /// <summary>
    /// Creates and initializes a buffer object with the provided data.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the buffer (e.g., float, int).</typeparam>
    /// <param name="target">The target buffer type (e.g., ArrayBuffer, ElementArrayBuffer).</param>
    /// <param name="data">The data to store in the buffer.</param>
    /// <param name="usage">The usage hint for the buffer (e.g., StaticDraw).</param>
    /// <returns>The OpenGL buffer handle.</returns>
    public static int CreateBuffer<T>(BufferTarget target, T[] data, BufferUsageHint usage) where T : struct
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Buffer data cannot be null or empty.");

        int buffer = GL.GenBuffer();
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, data.Length * Marshal.SizeOf<T>(), data, usage);
        GL.BindBuffer(target, 0); // unbind
        return buffer;
    }

    /// <summary>
    /// Deletes a buffer object.
    /// </summary>
    /// <param name="buffer">The OpenGL buffer handle to delete.</param>
    public static void DeleteBuffer(int buffer)
    {
        if (buffer > 0)
            GL.DeleteBuffer(buffer);
    }
}

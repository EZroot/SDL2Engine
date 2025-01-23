using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SDL2Engine.Core.Addressables;

public class ImageService : IImageService
{
    private readonly IImageLoader m_imageLoader;
    private readonly Dictionary<int, TextureData> _idToTexture;
    private readonly Dictionary<string, int> _pathToId;
    private int _nextId;

    public ImageService()
    {
        _idToTexture = new Dictionary<int, TextureData>();
        _pathToId = new Dictionary<string, int>();
        _nextId = 1;

        m_imageLoader = new ImageLoader();
    }

    public IntPtr LoadImageRaw(string path)
    {
        return m_imageLoader.LoadImage(path);
    }

    /// <summary>
    /// Load and store a texture in hashmap
    /// </summary>
    /// <param name="renderer">SDL Renderer</param>
    /// <param name="path">Texture file path</param>
    /// <returns>TextureData object</returns>
    public TextureData LoadTexture(IntPtr renderer, string path)
    {
        if (_pathToId.ContainsKey(path))
        {
            int existingId = _pathToId[path];
            Debug.LogWarning($"Texture already loaded {path} with ID {existingId} </color>");
            return _idToTexture[existingId];
        }

        IntPtr surface = m_imageLoader.LoadImage(path);
        if (surface == IntPtr.Zero)
        {
            Debug.Throw<ArgumentNullException>(new ArgumentNullException(), $"Failed to load image: {path}");
            return null;
        }

        IntPtr texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
        SDL.SDL_FreeSurface(surface);

        if (texture == IntPtr.Zero)
            Debug.Throw<ArgumentNullException>(new ArgumentNullException(),
                $"Failed to create texture: {SDL.SDL_GetError()}");

        int id = _nextId++;

        SDL.SDL_QueryTexture(texture, out _, out _, out int width, out int height);
        SDL.SDL_Rect srcRect = new SDL.SDL_Rect { x = 0, y = 0, w = width, h = height };

        var textureData = new TextureData(id, texture, width, height, srcRect);
        _idToTexture[id] = textureData;
        _pathToId[path] = id;

        Debug.Log(
            $"<color=green>Texture Created:</color> Id:{textureData.Id} Size:{textureData.Width}x{textureData.Height} Path:{path}");
        return textureData;
    }

    public int LoadTextureOpenGL(string path)
    {
        // Load the image as an SDL surface
        IntPtr surface = SDL_image.IMG_Load(path);
        if (surface == IntPtr.Zero)
            throw new Exception($"Failed to load image: {SDL.SDL_GetError()}");

        IntPtr convertedSurface = SDL.SDL_ConvertSurfaceFormat(surface, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
        SDL.SDL_FreeSurface(surface); // Free original surface
        if (convertedSurface == IntPtr.Zero)
            throw new Exception($"Failed to convert surface format: {SDL.SDL_GetError()}");

        SDL.SDL_Surface converted = Marshal.PtrToStructure<SDL.SDL_Surface>(convertedSurface);
        int width = converted.w;
        int height = converted.h;
        IntPtr pixels = converted.pixels;

        int texId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texId);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.BindTexture(TextureTarget.Texture2D, 0);

        SDL.SDL_FreeSurface(convertedSurface);

        Console.WriteLine($"Texture Loaded: ID={texId}, Size={width}x{height}, Path={path}");
        return texId;
    }
    
    public void DrawTextureGL(OpenGLHandle glHandler, nint textureId, ICamera camera, Matrix4 modelMatrix)
    {
        var cameraGL = (CameraGL)camera;

        GL.UseProgram(glHandler.Handles.Shader);

        // Combine projection and view matrices
        Matrix4 projectionView = cameraGL.View * cameraGL.Projection;
        int projViewMatrixLocation = GL.GetUniformLocation(glHandler.Handles.Shader, "projViewMatrix");
        if (projViewMatrixLocation >= 0)
            GL.UniformMatrix4(projViewMatrixLocation, false, ref projectionView);

        // Pass the model matrix
        int modelMatrixLocation = GL.GetUniformLocation(glHandler.Handles.Shader, "modelMatrix");
        if (modelMatrixLocation >= 0)
            GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrix);

        // Bind VAO
        GL.BindVertexArray(glHandler.Handles.Vao);

        // Bind texture
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, (int)textureId);

        // Draw only the quad
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        // Cleanup
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }
    
    public void DrawTexturesGLBatched(OpenGLHandle glHandler, int[] textureIds, ICamera camera, Matrix4[] modelMatrices)
    {
        var cameraGL = (CameraGL)camera;

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.UseProgram(glHandler.Handles.Shader);

        // Combine projection and view matrices once if they are the same for all textures
        Matrix4 projectionView = cameraGL.View * cameraGL.Projection;
        int projViewMatrixLocation = GL.GetUniformLocation(glHandler.Handles.Shader, "projViewMatrix");
        if (projViewMatrixLocation >= 0)
            GL.UniformMatrix4(projViewMatrixLocation, false, ref projectionView);

        GL.BindVertexArray(glHandler.Handles.Vao);

        for (int i = 0; i < textureIds.Length; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureIds[i]);

            int modelMatrixLocation = GL.GetUniformLocation(glHandler.Handles.Shader, "modelMatrix");
            if (modelMatrixLocation >= 0)
                GL.UniformMatrix4(modelMatrixLocation, false, ref modelMatrices[i]);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        // Cleanup
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }
    
    public void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect, ICamera camera)
    {
        if (!_idToTexture.TryGetValue(textureId, out var textureData))
        {
            Debug.LogError($"Texture ID {textureId} not found.");
            return;
        }

        if (camera == null)
        {
            Debug.LogError("Camera provided is null.");
            return;
        }

        SDL.SDL_Rect transformedDstRect = ApplyCameraTransform(dstRect, camera);
        SDL.SDL_Rect srcRect = textureData.SrcRect;
        SDL.SDL_RenderCopy(renderer, textureData.Texture, ref srcRect, ref transformedDstRect);
    }

    public void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation,
        ref SDL.SDL_Point center, ICamera camera)
    {
        if (!_idToTexture.TryGetValue(textureId, out var textureData))
        {
            Debug.LogError($"Texture ID {textureId} not found.");
            return;
        }

        if (camera == null)
        {
            Debug.LogError("Camera provided is null.");
            return;
        }

        float angleInDegrees = rotation * (180f / (float)Math.PI);

        SDL.SDL_Rect transformedDstRect = ApplyCameraTransform(destRect, camera);
        var srcRec = textureData.SrcRect;
        SDL.SDL_RenderCopyEx(renderer, textureData.Texture, ref srcRec, ref transformedDstRect, angleInDegrees,
            ref center,
            SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }


    /// <summary>
    /// Unload a specific texture from asset manager
    /// </summary>
    /// <param name="id">Unique texture ID</param>
    public void UnloadTexture(int id)
    {
        if (_idToTexture.ContainsKey(id))
        {
            SDL.SDL_DestroyTexture(_idToTexture[id].Texture);
            _idToTexture.Remove(id);

            string path = _pathToId.FirstOrDefault(x => x.Value == id).Key;
            if (path != null)
                _pathToId.Remove(path);

            Debug.Log($"Texture Unloaded: ID={id}, Path={path}");
        }
        else
        {
            Debug.Log($"<color=orange>WARNING: Attempted to unload non-existent texture ID: {id}</color>");
        }
    }

    /// <summary>
    /// Unloads ALL textures and sounds from asset manager
    /// </summary>
    public void Cleanup()
    {
        foreach (var textureData in _idToTexture.Values)
        {
            SDL.SDL_DestroyTexture(textureData.Texture);
        }

        _idToTexture.Clear();
        _pathToId.Clear();

        SDL_image.IMG_Quit();

        Debug.Log("AssetManager Cleanup Completed.");
    }

    /// <summary>
    /// Applies camera transformation to the destination rectangle.
    /// </summary>
    /// <param name="dstRect">Original destination rectangle in world coordinates.</param>
    /// <param name="camera">Camera to apply transformation.</param>
    /// <returns>Transformed destination rectangle in screen coordinates.</returns>
    private SDL.SDL_Rect ApplyCameraTransform(SDL.SDL_Rect dstRect, ICamera camera)
    {
        // Calculate the offset based on camera position and zoom
        Vector2 cameraOffset = camera.GetOffset();

        // Apply zoom to the position and size
        float zoom = camera.Zoom;

        float transformedX = (dstRect.x - cameraOffset.X) * zoom;
        float transformedY = (dstRect.y - cameraOffset.Y) * zoom;
        float transformedW = dstRect.w * zoom;
        float transformedH = dstRect.h * zoom;

        // Create a new transformed SDL_Rect
        SDL.SDL_Rect transformedRect = new SDL.SDL_Rect
        {
            x = (int)transformedX,
            y = (int)transformedY,
            w = (int)transformedW,
            h = (int)transformedH
        };

        return transformedRect;
    }
}
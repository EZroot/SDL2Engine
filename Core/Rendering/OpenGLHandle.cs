/// <summary>
/// Represents the mandatory OpenGL handles.
/// </summary>
public struct OpenGLMandatoryHandles
{
    public int Vao { get; }
    public int Vbo { get; }
    public int Ebo { get; }
    public int Shader { get; }

    public OpenGLMandatoryHandles(int vao, int vbo, int ebo, int shader)
    {
        Vao = ValidateHandle(vao);
        Vbo = ValidateHandle(vbo);
        Ebo = ValidateHandle(ebo);
        Shader = ValidateHandle(shader);
    }

    private int ValidateHandle(int handle)
    {
        if (handle < 0)
            throw new ArgumentOutOfRangeException(nameof(handle), "Handle must be non-negative.");
        return handle;
    }
}

/// <summary>
/// Represents optional locations for attributes and uniforms in OpenGL.
/// </summary>
public struct OpenGLAttributeLocations
{
    public int? TexCoordLocation { get; }
    public int? ProjMatrixLocation { get; }
    public int? PositionLocation { get; }
    public int? UVLocation { get; }
    public int? ColorLocation { get; }
    public int? ProjectionLocation { get; }

    public OpenGLAttributeLocations(int? texCoordLocation = null, int? projMatrixLocation = null,
                                    int? positionLocation = null, int? uvLocation = null,
                                    int? colorLocation = null, int? projectionLocation = null)
    {
        TexCoordLocation = texCoordLocation.HasValue ? ValidateLocation(texCoordLocation.Value) : (int?)null;
        ProjMatrixLocation = projMatrixLocation.HasValue ? ValidateLocation(projMatrixLocation.Value) : (int?)null;
        PositionLocation = positionLocation.HasValue ? ValidateLocation(positionLocation.Value) : (int?)null;
        UVLocation = uvLocation.HasValue ? ValidateLocation(uvLocation.Value) : (int?)null;
        ColorLocation = colorLocation.HasValue ? ValidateLocation(colorLocation.Value) : (int?)null;
        ProjectionLocation = projectionLocation.HasValue ? ValidateLocation(projectionLocation.Value) : (int?)null;
    }

    private int ValidateLocation(int location)
    {
        if (location < 0)
            throw new ArgumentOutOfRangeException(nameof(location), "Location must be non-negative.");
        return location;
    }
}

/// <summary>
/// Represents a handle for OpenGL resources, including VAO, VBO, shader program, and optionally attribute/uniform locations.
/// </summary>
public class OpenGLHandle
{
    public OpenGLMandatoryHandles Handles { get; }
    public OpenGLAttributeLocations? Locations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLHandle"/> class with specified handles and optional locations.
    /// </summary>
    /// <param name="handles">Mandatory OpenGL handles.</param>
    /// <param name="locations">Optional attribute/uniform locations.</param>
    public OpenGLHandle(OpenGLMandatoryHandles handles, OpenGLAttributeLocations? locations = null)
    {
        Handles = handles;
        Locations = locations;
    }
}

public class OpenGLHandle
{
    public int VaoHandle { get; }
    public int VboHandle { get; }
    public int ElementsHandle { get; }
    public int ShaderHandle { get; }

    // Attribute/Uniform locations
    public int? AttribLocationTex { get; }
    public int? AttribLocationProjMtx { get; }
    public int? AttribLocationPosition { get; }
    public int? AttribLocationUV { get; }
    public int? AttribLocationColor { get; }
    public int? ProjLocation { get; }

    public OpenGLHandle(
        int vao, int vbo, int elements, int shader,
        int? attribTex = null, int? attribProjMtx = null, 
        int? attribPosition = null, int? attribUV = null, 
        int? attribColor = null, int? projLocation = null)
    {
        VaoHandle = vao;
        VboHandle = vbo;
        ElementsHandle = elements;
        ShaderHandle = shader;
        AttribLocationTex = attribTex;
        AttribLocationProjMtx = attribProjMtx;
        AttribLocationPosition = attribPosition;
        AttribLocationUV = attribUV;
        AttribLocationColor = attribColor;
        ProjLocation = projLocation;
    }
}
public class OpenGLHandle
{
    public int VaoHandle;
    public int VboHandle;
    public int ElementsHandle;
    public int ShaderHandle;
    public int AttribLocationTex;
    public int AttribLocationProjMtx;
    public int AttribLocationPosition;
    public int AttribLocationUV;
    public int AttribLocationColor;

    public OpenGLHandle(int vaoHandle, int vboHandle, int elementsHandle, int shaderHandle, int attribLocationTex,
        int attribLocationProjMtx, int attribLocationPosition, int attribLocationUv, int attribLocationColor)
    {
        VaoHandle = vaoHandle;
        VboHandle = vboHandle;
        ElementsHandle = elementsHandle;
        ShaderHandle = shaderHandle;
        AttribLocationTex = attribLocationTex;
        AttribLocationProjMtx = attribLocationProjMtx;
        AttribLocationPosition = attribLocationPosition;
        AttribLocationUV = attribLocationUv;
        AttribLocationColor = attribLocationColor;
    }
}
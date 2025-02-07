using System.Data;
using OpenTK.Mathematics;
using SDL2Engine.Core.Lighting.Interfaces;

namespace SDL2Engine.Core.Lighting;

public enum LightType
{
    Directional,
    Point
}

public class Light : ILight
{
    private LightType m_lightType;
    private Matrix4 m_lightProjection;
    private float m_nearPlane, m_farPlane;
    private int m_projectionSize;
    
    public LightType LightType => m_lightType;
    public Matrix4 LightView { get; private set; }
    public Matrix4 LightProjection => m_lightProjection;
    public Vector3 LightPosition { get; private set; }
    public Vector3 LightDirection { get; private set; }
    public Light(LightType lightType, int projSize, float nearPlane, float farPlane)
    {
        m_lightType = lightType;
        m_projectionSize = projSize;
        m_nearPlane = nearPlane;
        m_farPlane = farPlane;

        if (m_lightType == LightType.Directional)
        {
            m_lightProjection = Matrix4.CreateOrthographicOffCenter(
                -m_projectionSize, m_projectionSize,
                -m_projectionSize, m_projectionSize,
                m_nearPlane, m_farPlane
            );
        }

        if (m_lightType == LightType.Point)
        {
            m_lightProjection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(90f), 1f, m_nearPlane, m_farPlane);
        }
    }

    public Matrix4 Update(Vector3 pos, Quaternion rotation, float lightDistance)
    {
        LightDirection = Vector3.Transform(Vector3.UnitZ, rotation);
        LightPosition = pos - LightDirection * lightDistance;
        var up = Vector3.Transform(Vector3.UnitY, rotation);
        LightView = Matrix4.LookAt(LightPosition, pos, up);
        var lightSpaceMatrix = m_lightProjection * LightView;
        return lightSpaceMatrix;
    }
    
    public Matrix4 Update(Vector3 pos, float lightRotationX, float lightRotationY, float lightRotationZ, float lightDistance)
    {
        Quaternion lightRotation = Quaternion.FromEulerAngles(lightRotationX, lightRotationY, lightRotationZ);
        LightDirection = Vector3.Transform(Vector3.UnitZ, lightRotation);
        LightPosition = pos - LightDirection * lightDistance;
        var up = Vector3.Transform(Vector3.UnitY, lightRotation);
        LightView = Matrix4.LookAt(LightPosition, pos, up);
        var lightSpaceMatrix = m_lightProjection * LightView;
        return lightSpaceMatrix;
    }
}
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
        else if (m_lightType == LightType.Point)
        {
            m_lightProjection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(90f), 1f, m_nearPlane, m_farPlane);
        }
    }

    // For point lights, lightDistance is used. For directional lights, we ignore the parameter.
    public Matrix4 Update(Vector3 pos, Quaternion rotation, float lightDistance)
    {
        LightDirection = Vector3.Transform(Vector3.UnitZ, rotation);
        if (m_lightType == LightType.Directional)
            // Use m_farPlane (or another suitably large value) to simulate "infinite" distance.
            LightPosition = pos - LightDirection * m_farPlane;
        else
            LightPosition = pos - LightDirection * lightDistance;
        
        var up = Vector3.Transform(Vector3.UnitY, rotation);
        LightView = Matrix4.LookAt(LightPosition, pos, up);
        return m_lightProjection * LightView;
    }
    
    public Matrix4 Update(Vector3 pos, float lightRotationX, float lightRotationY, float lightRotationZ, float lightDistance)
    {
        Quaternion lightRotation = Quaternion.FromEulerAngles(lightRotationX, lightRotationY, lightRotationZ);
        LightDirection = Vector3.Transform(Vector3.UnitZ, lightRotation);
        if (m_lightType == LightType.Directional)
            LightPosition = pos - LightDirection * m_farPlane;
        else
            LightPosition = pos - LightDirection * lightDistance;
        
        var up = Vector3.Transform(Vector3.UnitY, lightRotation);
        LightView = Matrix4.LookAt(LightPosition, pos, up);
        return m_lightProjection * LightView;
    }
}

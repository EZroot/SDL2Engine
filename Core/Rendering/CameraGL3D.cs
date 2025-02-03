using OpenTK.Mathematics;
using SDL2Engine.Core.Rendering.Interfaces;
using Vector2 = System.Numerics.Vector2;

public class CameraGL3D : ICamera
{
    public Matrix4 Projection { get; private set; }
    public Matrix4 View { get; private set; }
    public Vector3 Position { get; private set; }
    public float Zoom { get; }
    public string Name { get; }

    public Vector3 Target;
    public Vector3 Up;

    private float fov;
    private float aspect;
    private float near;
    private float far;

    public CameraGL3D(Vector3 position, Vector3 target, Vector3 up, float fovDegrees, float aspect, float near, float far)
    {
        Position = position;
        Target = target;
        Up = up;
        fov = fovDegrees;
        this.aspect = aspect;
        this.near = near;
        this.far = far;

        UpdateProjection();
        UpdateView();
    }

    public void UpdateProjection()
    {
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(fov), aspect, near, far);
    }

    public void UpdateView()
    {
        View = Matrix4.LookAt(Position, Target, Up);
    }
    public void Move(OpenTK.Mathematics.Vector3 delta)
    {
        throw new NotImplementedException();
    }

    public void SetPosition(OpenTK.Mathematics.Vector3 newPosition)
    {
        throw new NotImplementedException();
    }

    public void SetZoom(float newZoom)
    {
        throw new NotImplementedException();
    }

    public OpenTK.Mathematics.Vector3 GetOffset()
    {
        throw new NotImplementedException();
    }

    public void SetName(string name)
    {
        throw new NotImplementedException();
    }
}
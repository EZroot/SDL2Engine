using OpenTK.Mathematics;
using SDL2Engine.Core.Cameras.Interfaces;
namespace SDL2Engine.Core.Cameras;
public class CameraGL3D : ICamera
{
    public Matrix4 Projection { get; private set; }
    public Matrix4 View { get; private set; }
    public Vector3 Position { get; private set; }
    public float Zoom { get; private set; }
    public string Name { get; private set; }

    // Free cam vectors
    public Vector3 Front { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }

    private float fov;
    private float aspect;
    private float near;
    private float far;

    // Euler angles for free look
    private float yaw;
    private float pitch;

    private float _viewportWidth, _viewportHeight;

    // Constructor: derive initial front from target.
    public CameraGL3D(Vector3 position, Vector3 target, Vector3 worldUp, float fovDegrees, float aspect, float near, float far)
    {
        Position = position;
        fov = fovDegrees;
        this.aspect = aspect;
        this.near = near;
        this.far = far;

        // Set default zoom equal to fov
        Zoom = fovDegrees;

        // Initialize front from given target
        Front = Vector3.Normalize(target - position);
        // Derive yaw and pitch from front vector
        yaw = MathF.Atan2(Front.Z, Front.X) * MathHelper.RadiansToDegrees;
        pitch = MathF.Asin(Front.Y) * MathHelper.RadiansToDegrees;


        // Set initial Up from worldUp
        Up = worldUp;
        UpdateCameraVectors();

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
        View = Matrix4.LookAt(Position, Position + Front, Up);
    }

    // Recalculate the camera's coordinate system based on yaw/pitch.
    private void UpdateCameraVectors()
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        Front = Vector3.Normalize(front);

        // Recalculate Right and Up using world up (assumed to be Y-up)
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    // Move camera based on a delta vector (X: right, Y: up, Z: forward)
    public void Move(Vector3 delta)
    {
        Position += Right * delta.X;
        Position += Up * delta.Y;
        Position += Front * delta.Z;
        UpdateView();
    }
    
    public void ResizeViewport(float width, float height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        UpdateProjection();
    }
    
    public void SetPosition(Vector3 newPosition)
    {
        Position = newPosition;
        UpdateView();
    }

    // Adjust fov and update projection. Here, zoom is directly tied to fov.
    public void SetZoom(float newZoom)
    {
        Zoom = newZoom;
        fov = newZoom;
        UpdateProjection();
    }

    public Vector3 GetOffset()
    {
        return Vector3.Zero;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    // Process mouse movement to adjust yaw and pitch for free look.
    public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
    {
        const float sensitivity = 0.1f;
        xOffset *= sensitivity;
        yOffset *= sensitivity;

        yaw += xOffset;
        pitch += yOffset;

        if (constrainPitch)
            pitch = Math.Clamp(pitch, -89f, 89f);

        UpdateCameraVectors();
        UpdateView();
    }
}

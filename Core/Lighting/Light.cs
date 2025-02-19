using OpenTK.Mathematics;
using SDL2Engine.Core.Lighting.Interfaces;

namespace SDL2Engine.Core.Lighting
{
    public enum LightType
    {
        Directional,
        Point
    }

    public class Light : ILight
    {
        private readonly LightType _lightType;
        private readonly float _nearPlane, _farPlane;
        private readonly int _projectionSize;
        private readonly Matrix4 _lightProjection;

        public LightType LightType => _lightType;
        public Matrix4 LightView { get; private set; }
        public Matrix4 LightProjection => _lightProjection;
        public Vector3 LightPosition { get; private set; }
        public Vector3 LightDirection { get; private set; }

        public Light(LightType lightType, int projectionSize, float nearPlane, float farPlane)
        {
            _lightType = lightType;
            _projectionSize = projectionSize;
            _nearPlane = nearPlane;
            _farPlane = farPlane;

            if (_lightType == LightType.Directional)
            {
                // Orthographic projection for directional lights.
                _lightProjection = Matrix4.CreateOrthographicOffCenter(
                    -_projectionSize, _projectionSize,
                    -_projectionSize, _projectionSize,
                    _nearPlane, _farPlane);
            }
            else // Point light.
            {
                _lightProjection = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(90f), 1920f / 1080f, _nearPlane, _farPlane);
            }
        }

        /// <summary>
        /// Updates the light's view matrix from a given position and rotation.
        /// For directional lights, the light is assumed to come from above by default.
        /// </summary>
        /// <param name="position">The target position the light is illuminating.</param>
        /// <param name="rotation">The rotation applied to the light direction.</param>
        /// <param name="lightDistance">For point lights, the effective light distance.</param>
        /// <returns>The combined light-space matrix (projection * view).</returns>
        public Matrix4 Update(Vector3 position, Quaternion rotation, float lightDistance)
        {
            // Compute light direction.
            // Using -UnitY so that with no rotation the light comes from above.
            LightDirection = Vector3.Transform(Vector3.UnitZ, rotation);

            // For directional lights, simulate an "infinite" light by using the far plane.
            float distance = (_lightType == LightType.Directional) ? _farPlane : lightDistance;
            LightPosition = position - LightDirection * distance;

            // Choose an up vector that isn't collinear with the light direction.
            // If the light is nearly vertical, use UnitZ instead of UnitY.
            Vector3 up = (MathF.Abs(Vector3.Dot(LightDirection, Vector3.UnitY)) > 0.99f)
                ? Vector3.UnitZ
                : Vector3.UnitY;

            LightView = Matrix4.LookAt(LightPosition, position, up);

            return _lightProjection * LightView;
        }

        /// <summary>
        /// Overload for Update that accepts Euler angles (in radians) for rotation.
        /// </summary>
        public Matrix4 Update(Vector3 position, float rotationX, float rotationY, float rotationZ, float lightDistance)
        {
            Quaternion rotation = Quaternion.FromEulerAngles(rotationX, rotationY, rotationZ);
            return Update(position, rotation, lightDistance);
        }
    }
}

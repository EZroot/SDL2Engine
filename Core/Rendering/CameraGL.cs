
using OpenTK.Mathematics;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering;

/// <summary>
/// OpenGL version of our SDL camera
/// </summary>
public class CameraGL : Camera
{
    private Matrix4 _projection;
        private Matrix4 _view;

        public Matrix4 Projection => _projection;
        public Matrix4 View => _view;

        private float _viewportWidth;
        private float _viewportHeight;

        public CameraGL(Vector3 initialPosition, float viewportWidth, float viewportHeight, float initialZoom = 1.0f, string name = null)
            : base(initialPosition, initialZoom, name)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;

            UpdateProjection();
            UpdateView();
        }

        /// <summary>
        /// Updates the projection matrix based on the viewport size and zoom level.
        /// </summary>
        public void UpdateProjection()
        {
            _projection = Matrix4.CreateOrthographicOffCenter(
                0, _viewportWidth / Zoom,
                _viewportHeight / Zoom, 0,
                -1, 1);
        }

        /// <summary>
        /// Updates the view matrix based on the camera's position.
        /// </summary>
        public void UpdateView()
        {
            _view = Matrix4.CreateTranslation(-Position.X, -Position.Y, 0);
        }

        /// <summary>
        /// Resizes the viewport and updates the projection matrix.
        /// </summary>
        public void ResizeViewport(float width, float height)
        {
            _viewportWidth = width;
            _viewportHeight = height;
            UpdateProjection();
        }

        /// <summary>
        /// Sets the camera's position and updates the view matrix.
        /// </summary>
        public override void SetPosition(Vector3 newPosition)
        {
            base.SetPosition(newPosition);
            UpdateView();
        }

        public override void Move(Vector3 delta)
        {
            SetPosition(Position + delta);
        }
        
        /// <summary>
        /// Sets the camera's zoom level and updates the projection matrix.
        /// </summary>
        public override void SetZoom(float newZoom)
        {
            base.SetZoom(newZoom);
            UpdateProjection();
        }
}
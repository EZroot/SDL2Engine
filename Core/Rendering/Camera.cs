using System;
using OpenTK.Mathematics;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Rendering
{
    public class Camera : ICamera
    {
        public Vector3 Position { get; private set; }
        public float Zoom { get; private set; }
        public string Name { get; private set; }

        public Camera(Vector3 initialPosition, float initialZoom = 1.0f, string name = null)
        {
            Position = initialPosition;
            Zoom = initialZoom;
            Name = name;
        }

        /// <summary>
        /// Moves the camera by the specified delta.
        /// </summary>
        /// <param name="delta">Change in position.</param>
        public virtual void Move(Vector3 delta)
        {
            Position += delta;
        }

        /// <summary>
        /// Sets the camera's position.
        /// </summary>
        /// <param name="newPosition">New position.</param>
        public virtual void SetPosition(Vector3 newPosition)
        {
            Position = newPosition;
        }

        /// <summary>
        /// Sets the camera's zoom level.
        /// </summary>
        /// <param name="newZoom">New zoom level.</param>
        public virtual void SetZoom(float newZoom)
        {
            Zoom = Math.Clamp(newZoom, 0.1f, 10.0f); // Clamp to reasonable zoom levels
        }

        /// <summary>
        /// Gets the camera transformation offset.
        /// </summary>
        /// <returns>Offset vector.</returns>
        public virtual Vector3 GetOffset()
        {
            return Position * Zoom;
        }

        /// <summary>
        /// Sets the camera's name.
        /// </summary>
        /// <param name="name">Name of the camera.</param>
        public virtual void SetName(string name)
        {
            Name = name;
        }
    }
}

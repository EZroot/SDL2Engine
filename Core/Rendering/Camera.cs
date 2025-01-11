using System;
using System.Numerics;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Rendering
{
    public class Camera : ICamera
    {
        public Vector2 Position { get; private set; }
        public float Zoom { get; private set; }
        public string Name { get; private set; }

        public Camera(Vector2 initialPosition, float initialZoom = 1.0f, string name = null)
        {
            Position = initialPosition;
            Zoom = initialZoom;
            Name = name;
        }

        /// <summary>
        /// Moves the camera by the specified delta.
        /// </summary>
        /// <param name="delta">Change in position.</param>
        public void Move(Vector2 delta)
        {
            Position += delta;
            Console.WriteLine($"Camera Moved: New Position={Position}");
        }

        /// <summary>
        /// Sets the camera's position.
        /// </summary>
        /// <param name="newPosition">New position.</param>
        public void SetPosition(Vector2 newPosition)
        {
            Position = newPosition;
            Console.WriteLine($"Camera Position Set: Position={Position}");
        }

        /// <summary>
        /// Sets the camera's zoom level.
        /// </summary>
        /// <param name="newZoom">New zoom level.</param>
        public void SetZoom(float newZoom)
        {
            Zoom = Math.Clamp(newZoom, 0.1f, 10.0f); // Clamp to reasonable zoom levels
            Console.WriteLine($"Camera Zoom Set: Zoom={Zoom}");
        }

        /// <summary>
        /// Gets the camera transformation offset.
        /// </summary>
        /// <returns>Offset vector.</returns>
        public Vector2 GetOffset()
        {
            return Position * Zoom;
        }

        /// <summary>
        /// Sets the camera's name.
        /// </summary>
        /// <param name="name">Name of the camera.</param>
        public void SetName(string name)
        {
            Name = name;
            Console.WriteLine($"Camera Name Set: Name={Name}");
        }
    }
}

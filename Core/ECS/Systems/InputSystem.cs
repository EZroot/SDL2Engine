using System.Numerics;
using SDL2;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.ECS.Systems
{
    public class InputSystem : ISystem
    {
        private readonly ComponentManager _componentManager;

        private const float Acceleration = 9f;
        private const float Deceleration = 9f;
        private const float MaxSpeed = 20f;
        private const float Damping = 0.8f;

        public InputSystem(ComponentManager componentManager)
        {
            _componentManager = componentManager;
        }

        public void Update(float deltaTime)
        {
            var positions = _componentManager.GetComponentDictionary<PositionComponent>();
            var velocities = _componentManager.GetComponentDictionary<VelocityComponent>();
            var player = _componentManager.GetComponentDictionary<PlayerTag>();

            foreach (var entityId in velocities.Keys)
            {
                if (positions.TryGetValue(entityId, out var position) &&
                    velocities.TryGetValue(entityId, out var velocity) &&
                    player.TryGetValue(entityId, out var playerTag))
                {
                    Vector2 inputDirection = GetInputDirection();
                    if (inputDirection != Vector2.Zero)
                    {
                        inputDirection = Vector2.Normalize(inputDirection);
                        velocity.Velocity += inputDirection * Acceleration * deltaTime;
                        if (velocity.Velocity.Length() > MaxSpeed)
                        {
                            velocity.Velocity = Vector2.Normalize(velocity.Velocity) * MaxSpeed;
                        }
                    }
                    else
                    {
                        ApplyDeceleration(ref velocity, deltaTime);
                    }

                    velocity.Velocity *= MathF.Pow(Damping, deltaTime);

                    // Save updated components in-place
                    _componentManager.AddComponent(new Entity(entityId), velocity);
                }
            }
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            // Rendering is handled elsewhere
        }

        /// <summary>
        /// Retrieves the current input direction based on key presses.
        /// </summary>
        /// <returns>A normalized direction vector.</returns>
        private Vector2 GetInputDirection()
        {
            Vector2 direction = Vector2.Zero;

            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
                direction.Y -= 1;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
                direction.Y += 1;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
                direction.X -= 1;
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
                direction.X += 1;

            return direction;
        }

        /// <summary>
        /// Applies deceleration to the velocity when no input is detected.
        /// </summary>
        /// <param name="velocity">Reference to the velocity component.</param>
        /// <param name="deltaTime">Time elapsed since the last frame.</param>
        private void ApplyDeceleration(ref VelocityComponent velocity, float deltaTime)
        {
            if (velocity.Velocity.Length() > 0)
            {
                Vector2 decelerationVector = Vector2.Normalize(velocity.Velocity) * Deceleration * deltaTime;

                if (decelerationVector.Length() > velocity.Velocity.Length())
                {
                    velocity.Velocity = Vector2.Zero;
                }
                else
                {
                    velocity.Velocity -= decelerationVector;
                }
            }
        }
    }
}
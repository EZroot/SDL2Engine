using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.ECS.Systems
{
    public class InputSystem : ISystem
    {
        private readonly ComponentManager _componentManager;
        private readonly SpatialPartitionerECS _partitioner;

        private const float Acceleration = 9f;
        private const float Deceleration = 9f;
        private const float MaxSpeed = 20f;
        private const float Damping = 0.8f;

        private const float SeparationRadius = 32f; 
        private const float SeparationWeight = 5.0f; 
        private const float SeparationForce = 10f;

        private const float StandingStillSeparationScale = 50f; 
        private const float MovementThreshold = 0.1f;

        private readonly List<int> _playerEntityIds;

        public InputSystem(ComponentManager componentManager, SpatialPartitionerECS partitioner, int maxSeparationBoids)
        {
            _componentManager = componentManager;
            _partitioner = partitioner;

            // Initialize reusable data structures
            _playerEntityIds = new List<int>();
        }

        public void Update(float deltaTime)
        {
            // Validate deltaTime
            if (!IsValidDeltaTime(deltaTime))
            {
                Debug.LogError(
                    $"InputSystem Update Error: Invalid deltaTime detected (Value: {deltaTime}). Update aborted.");
                return;
            }

            // Retrieve component dictionaries once
            var positions = _componentManager.GetComponentDictionary<PositionComponent>();
            var velocities = _componentManager.GetComponentDictionary<VelocityComponent>();
            var players = _componentManager.GetComponentDictionary<PlayerTag>();

            // Collect all player entity IDs with necessary components
            _playerEntityIds.Clear();
            foreach (var entityId in players.Keys)
            {
                if (positions.ContainsKey(entityId) && velocities.ContainsKey(entityId))
                {
                    _playerEntityIds.Add(entityId);
                }
            }

            // If there are no players, exit early
            if (_playerEntityIds.Count == 0)
                return;

            // Assuming a single player, iterate through players
            foreach (var entityId in _playerEntityIds)
            {
                if (!positions.TryGetValue(entityId, out var positionComponent) ||
                    !velocities.TryGetValue(entityId, out var velocityComponent))
                {
                    Debug.LogError($"InputSystem Update Error: Missing components for player {entityId}.");
                    continue;
                }

                // Handle player input for movement
                Vector2 inputDirection = GetInputDirection();
                if (inputDirection != Vector2.Zero)
                {
                    inputDirection = SafeNormalize(inputDirection);
                    velocityComponent.Velocity += inputDirection * Acceleration * deltaTime;

                    if (velocityComponent.Velocity.Length > MaxSpeed)
                    {
                        velocityComponent.Velocity = SafeNormalize(velocityComponent.Velocity) * MaxSpeed;
                    }
                }
                else
                {
                    ApplyDeceleration(ref velocityComponent, deltaTime);
                }

                velocityComponent.Velocity *= MathF.Pow(Damping, deltaTime);

                float speed = velocityComponent.Velocity.Length;
                float separationScale = speed > MovementThreshold ? 1.0f : StandingStillSeparationScale;

                ApplySeparation(ref velocityComponent, positionComponent.Position, deltaTime, positions, velocities, separationScale);

                _componentManager.AddComponent(new Entity(entityId), velocityComponent);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyDeceleration(ref VelocityComponent velocity, float deltaTime)
        {
            if (velocity.Velocity.Length > 0)
            {
                Vector2 decelerationVector = SafeNormalize(velocity.Velocity) * Deceleration * deltaTime;

                if (decelerationVector.Length > velocity.Velocity.Length)
                {
                    velocity.Velocity = Vector2.Zero;
                }
                else
                {
                    velocity.Velocity -= decelerationVector;
                }
            }
        }

        /// <summary>
        /// Applies separation behavior to the player's velocity based on nearby boids.
        /// </summary>
        /// <param name="velocity">Reference to the velocity component.</param>
        /// <param name="position">Current position of the player.</param>
        /// <param name="deltaTime">Time elapsed since the last frame.</param>
        /// <param name="positions">Dictionary of PositionComponents.</param>
        /// <param name="velocities">Dictionary of VelocityComponents.</param>
        /// <param name="separationScale">Scale factor for the separation force.</param>
        private void ApplySeparation(
            ref VelocityComponent velocity,
            Vector2 position,
            float deltaTime,
            Dictionary<int, PositionComponent> positions,
            Dictionary<int, VelocityComponent> velocities,
            float separationScale)
        {
            var nearbyBoids = _partitioner.GetNeighbors(position, SeparationRadius);

            Vector2 separationForce = Vector2.Zero;
            int separationCount = 0;

            foreach (var boid in nearbyBoids)
            {
                if (positions.TryGetValue(boid.Id, out var boidPositionComp) &&
                    velocities.TryGetValue(boid.Id, out var boidVelocityComp))
                {
                    Vector2 boidPosition = boidPositionComp.Position;
                    Vector2 boidVelocity = boidVelocityComp.Velocity;

                    Vector2 distanceVector = position - boidPosition;
                    float distanceSquared = distanceVector.LengthSquared;

                    if (distanceSquared < 0.0001f)
                        continue; 

                    float distance = MathF.Sqrt(distanceSquared);

                    if (distance < SeparationRadius && distance > 0)
                    {
                        Vector2 force = SafeNormalize(distanceVector) * (SeparationRadius - distance) / SeparationRadius;
                        separationForce += force;
                        separationCount++;
                    }
                }
            }

            if (separationCount > 0)
            {
                separationForce /= separationCount;
                separationForce *= SeparationWeight;

                velocity.Velocity += separationForce * SeparationForce * separationScale * deltaTime;
            }
        }

        /// <summary>
        /// Validates the deltaTime.
        /// </summary>
        /// <param name="deltaTime">The delta time to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidDeltaTime(float deltaTime)
        {
            return !(float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f);
        }

        /// <summary>
        /// Safely normalizes a vector. Returns Vector2.Zero if the vector has zero length.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        /// <returns>A normalized vector or Vector2.Zero if normalization is not possible.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2 SafeNormalize(Vector2 vector)
        {
            if (vector.LengthSquared > 0)
            {
                Vector2 normalized = Vector2.Normalize(vector);
                if (HasInvalidComponents(normalized))
                {
                    Debug.LogError(
                        $"InputSystem SafeNormalize Error: Normalized vector contains invalid components (Vector: {vector}, Normalized: {normalized}). Returning Vector2.Zero.");
                    return Vector2.Zero;
                }

                return normalized;
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Checks if any component of the vector is NaN or Infinity.
        /// </summary>
        /// <param name="vector">The vector to check.</param>
        /// <returns>True if any component is invalid; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasInvalidComponents(Vector2 vector)
        {
            return float.IsNaN(vector.X) || float.IsInfinity(vector.X) ||
                   float.IsNaN(vector.Y) || float.IsInfinity(vector.Y);
        }
    }
}

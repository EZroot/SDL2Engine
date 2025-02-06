using System;
using System.Collections.Generic;
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
    public class BoidMovementSystem : ISystem
    {
        private readonly ComponentManager _componentManager;
        private readonly SpatialPartitionerECS _partitioner;

        // Boid behavior constants
        private const float NeighborRadius = 28f;
        private const float NeighborRadiusSquared = NeighborRadius * NeighborRadius;
        private const float AlignmentWeight = 3.5f;
        private const float CohesionWeight = 2.0f;
        private const float SeparationWeight = 5.5f;
        private const float MinSeparationDistance = 1f;
        private const float MaxSpeed = 20f;
        private const float Damping = 0.1f;

        // Reusable data structures to minimize allocations
        private readonly List<int> _boidEntityIds;
        private Vector2[] _boidSteerings;
        private Vector2[] _boidPositions;
        private Vector2[] _boidVelocities;

        // Lock object for thread-safe component additions (if needed)
        private readonly object _componentLock = new object();

        public BoidMovementSystem(ComponentManager componentManager, SpatialPartitionerECS partitioner, int maxBoids)
        {
            _componentManager = componentManager;
            _partitioner = partitioner;

            // Initialize reusable data structures based on maxBoids
            _boidEntityIds = new List<int>(maxBoids);
            _boidSteerings = new Vector2[maxBoids];
            _boidPositions = new Vector2[maxBoids];
            _boidVelocities = new Vector2[maxBoids];
        }

        public void Update(float deltaTime)
        {
            // Validate deltaTime
            if (!IsValidDeltaTime(deltaTime))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Invalid deltaTime detected (Value: {deltaTime}). Update aborted.");
                return;
            }

            // Retrieve component dictionaries once
            var positions = _componentManager.GetComponentDictionary<PositionComponent>();
            var velocities = _componentManager.GetComponentDictionary<VelocityComponent>();
            var boids = _componentManager.GetComponentDictionary<BoidTag>();

            // Collect all boid entity IDs with necessary components
            _boidEntityIds.Clear();
            foreach (var entityId in boids.Keys)
            {
                if (positions.ContainsKey(entityId) && velocities.ContainsKey(entityId))
                {
                    _boidEntityIds.Add(entityId);
                }
            }

            int boidCount = _boidEntityIds.Count;

            // Ensure arrays are large enough
            if (boidCount > _boidSteerings.Length)
            {
                int newSize = Math.Max(boidCount, _boidSteerings.Length * 2);
                Array.Resize(ref _boidSteerings, newSize);
                Array.Resize(ref _boidPositions, newSize);
                Array.Resize(ref _boidVelocities, newSize);
            }

            // Populate position and velocity arrays
            for (int i = 0; i < boidCount; i++)
            {
                int entityId = _boidEntityIds[i];
                _boidPositions[i] = positions[entityId].Position;
                _boidVelocities[i] = velocities[entityId].Velocity;
            }

            Vector2 mousePosition = new Vector2(InputManager.MouseX, InputManager.MouseY);
            var mouseDown = InputManager.IsMouseButtonPressed(SDL.SDL_BUTTON_LEFT);
            // Parallel processing of boid movements
            Parallel.For(0, boidCount, i =>
            {
                _boidSteerings[i] = CalculateSteering(
                    _boidEntityIds[i],
                    _boidPositions[i],
                    _boidVelocities[i],
                    mousePosition,
                    mouseDown,
                    positions,
                    velocities);
            });

            // Parallel updating of entities
            Parallel.For(0, boidCount, i =>
            {
                UpdateEntity(_boidEntityIds[i], _boidSteerings[i], deltaTime, velocities, positions);
            });
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            // Rendering handled elsewhere
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
        /// Calculates the steering vector for a single boid.
        /// </summary>
        private Vector2 CalculateSteering(
            int entityId,
            Vector2 currentPosition,
            Vector2 currentVelocity,
            Vector2 mousePosition,
            bool isMouseDown,
            Dictionary<int, PositionComponent> positions,
            Dictionary<int, VelocityComponent> velocities)
        {
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            Vector2 separation = Vector2.Zero;

            Vector2 toMouse = mousePosition - currentPosition;
            Vector2 mouseAttraction = isMouseDown ? (toMouse.LengthSquared > 0 ? Vector2.Normalize(toMouse) * CohesionWeight / 2 : Vector2.Zero) : Vector2.Zero;

            // Validate currentPosition and currentVelocity
            if (HasInvalidComponents(currentPosition))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Invalid currentPosition for boid {entityId} (Value: {currentPosition}). Steering set to zero.");
                return Vector2.Zero;
            }

            if (HasInvalidComponents(currentVelocity))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Invalid currentVelocity for boid {entityId} (Value: {currentVelocity}). Steering set to zero.");
                return Vector2.Zero;
            }

            var neighbors = _partitioner.GetNeighbors(currentPosition, NeighborRadius);

            int neighborCount = 0;
            Vector2 averageVelocity = Vector2.Zero;
            Vector2 centerOfMass = Vector2.Zero;

            foreach (var neighbor in neighbors)
            {
                if (neighbor.Id == entityId)
                    continue; // Skip self

                if (positions.TryGetValue(neighbor.Id, out var positionComp) &&
                    velocities.TryGetValue(neighbor.Id, out var velocityComp))
                {
                    Vector2 neighborPosition = positionComp.Position;
                    Vector2 neighborVelocity = velocityComp.Velocity;

                    // Validate neighbor data
                    if (HasInvalidComponents(neighborPosition) || HasInvalidComponents(neighborVelocity))
                    {
                        Debug.LogError(
                            $"BoidMovementSystem Update Error: Invalid neighbor data for boid {neighbor.Id} (Position: {neighborPosition}, Velocity: {neighborVelocity}). Neighbor skipped.");
                        continue; // Skip invalid neighbors
                    }

                    Vector2 diff = neighborPosition - currentPosition;
                    float distanceSq = diff.LengthSquared;

                    if (distanceSq > NeighborRadiusSquared)
                        continue; // Outside neighbor radius

                    averageVelocity += neighborVelocity;
                    neighborCount++;
                    centerOfMass += neighborPosition;
                    float distance = MathF.Sqrt(distanceSq);

                    if (distance < MinSeparationDistance && distance > 0)
                    {
                        Vector2 normalizedDiff = SafeNormalize(diff);
                        if (normalizedDiff == Vector2.Zero)
                        {
                            Debug.LogError(
                                $"BoidMovementSystem Update Error: Normalized separation vector is zero for boid {entityId}.");
                            continue;
                        }

                        separation -= normalizedDiff * (MinSeparationDistance - distance) / MinSeparationDistance;
                    }
                    else if (distance > 0)
                    {
                        separation -= diff / distanceSq;
                    }
                }
            }

            if (neighborCount > 0)
            {
                averageVelocity /= neighborCount;
                alignment = SafeNormalize(averageVelocity) * AlignmentWeight;
                centerOfMass /= neighborCount;
                Vector2 directionToCenter = centerOfMass - currentPosition;
                if (directionToCenter.LengthSquared > 0)
                    cohesion = SafeNormalize(directionToCenter) * CohesionWeight;
            }

            if (separation.LengthSquared > 0)
                separation = SafeNormalize(separation) * SeparationWeight;

            Vector2 steering = alignment + cohesion + separation + mouseAttraction;

            // Validate steering before assignment
            if (HasInvalidComponents(steering))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Steering vector contains invalid components for boid {entityId} (Steering: {steering}). Steering set to zero.");
                steering = Vector2.Zero;
            }

            return steering;
        }

        /// <summary>
        /// Updates the entity's velocity and position based on the steering vector.
        /// </summary>
        private void UpdateEntity(
            int entityId,
            Vector2 steering,
            float deltaTime,
            Dictionary<int, VelocityComponent> velocities,
            Dictionary<int, PositionComponent> positions)
        {
            if (!velocities.TryGetValue(entityId, out var velocityComponent) ||
                !positions.TryGetValue(entityId, out var positionComponent))
            {
                Debug.LogError($"BoidMovementSystem Update Error: Missing components for boid {entityId}.");
                return;
            }

            velocityComponent.Velocity += steering * 20f * deltaTime;

            float dampingFactor = MathF.Pow(Damping, deltaTime);
            if (float.IsNaN(dampingFactor) || float.IsInfinity(dampingFactor))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Invalid damping factor for boid {entityId} (Value: {dampingFactor}). Damping skipped.");
            }
            else
            {
                velocityComponent.Velocity *= dampingFactor;
            }

            if (velocityComponent.Velocity.Length > MaxSpeed)
            {
                Vector2 clampedVelocity = SafeNormalize(velocityComponent.Velocity) * MaxSpeed;
                if (HasInvalidComponents(clampedVelocity))
                {
                    Debug.LogError(
                        $"BoidMovementSystem Update Error: Clamped velocity contains invalid components for boid {entityId} (Clamped Velocity: {clampedVelocity}). Velocity set to zero.");
                    velocityComponent.Velocity = Vector2.Zero;
                }
                else
                {
                    velocityComponent.Velocity = clampedVelocity;
                }
            }

            if (HasInvalidComponents(velocityComponent.Velocity))
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Updated velocity contains invalid components for boid {entityId} (Velocity: {velocityComponent.Velocity}). Velocity set to zero.");
                velocityComponent.Velocity = Vector2.Zero;
            }

            // Assuming ComponentManager.AddComponent is thread-safe
            _componentManager.AddComponent(new Entity(entityId), velocityComponent);
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
                        $"SafeNormalize Error: Normalized vector contains invalid components (Vector: {vector}, Normalized: {normalized}). Returning Vector2.Zero.");
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

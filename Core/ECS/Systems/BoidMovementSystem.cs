using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Rendering.Interfaces;
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
        private const float AlignmentWeight = 2.5f;
        private const float CohesionWeight = 2.0f;
        private const float SeparationWeight = 4.5f;
        private const float MinSeparationDistance = 1f;
        private const float MaxSpeed = 20f;
        private const float Damping = 0.1f;

        public BoidMovementSystem(ComponentManager componentManager, SpatialPartitionerECS partitioner)
        {
            _componentManager = componentManager;
            _partitioner = partitioner;
        }

        public void Update(float deltaTime)
        {
            // Validate deltaTime
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f)
            {
                Debug.LogError(
                    $"BoidMovementSystem Update Error: Invalid deltaTime detected (Value: {deltaTime}). Update aborted.");
                return;
            }

            var positions = _componentManager.GetComponentDictionary<PositionComponent>();
            var velocities = _componentManager.GetComponentDictionary<VelocityComponent>();
            var boids = _componentManager.GetComponentDictionary<BoidTag>();

            // Collect all boid entity IDs with necessary components
            var boidEntityIds = new List<int>();
            foreach (var entityId in boids.Keys)
            {
                if (positions.ContainsKey(entityId) && velocities.ContainsKey(entityId))
                {
                    boidEntityIds.Add(entityId);
                }
            }

            int boidCount = boidEntityIds.Count;
            Vector2[] boidSteerings = new Vector2[boidCount];
            Vector2[] boidPositions = new Vector2[boidCount];
            Vector2[] boidVelocities = new Vector2[boidCount];

            for (int i = 0; i < boidCount; i++)
            {
                int entityId = boidEntityIds[i];
                boidPositions[i] = positions[entityId].Position;
                boidVelocities[i] = velocities[entityId].Velocity;
            }

            Vector2 mousePosition = new Vector2(InputManager.MouseX, InputManager.MouseY);


            // Parallel processing of boid movements
            Parallel.For(0, boidCount, i =>
            {
                Vector2 alignment = Vector2.Zero;
                Vector2 cohesion = Vector2.Zero;
                Vector2 separation = Vector2.Zero;

                Vector2 currentPosition = boidPositions[i];
                Vector2 currentVelocity = boidVelocities[i];

                Vector2 toMouse = mousePosition - currentPosition;
                Vector2 mouseAttraction = Vector2.Zero;
                if (toMouse.LengthSquared() > 0)
                    mouseAttraction = Vector2.Normalize(toMouse) * CohesionWeight;

                // Validate currentPosition and currentVelocity
                if (HasInvalidComponents(currentPosition))
                {
                    Debug.LogError(
                        $"BoidMovementSystem Update Error: Invalid currentPosition for boid {boidEntityIds[i]} (Value: {currentPosition}). Steering set to zero.");
                    boidSteerings[i] = Vector2.Zero;
                    return;
                }

                if (HasInvalidComponents(currentVelocity))
                {
                    Debug.LogError(
                        $"BoidMovementSystem Update Error: Invalid currentVelocity for boid {boidEntityIds[i]} (Value: {currentVelocity}). Steering set to zero.");
                    boidSteerings[i] = Vector2.Zero;
                    return;
                }

                var neighbors = _partitioner.GetNeighbors(currentPosition, NeighborRadius);

                int neighborCount = 0;
                Vector2 averageVelocity = Vector2.Zero;
                Vector2 centerOfMass = Vector2.Zero;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor.Id == boidEntityIds[i])
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
                        float distanceSq = diff.LengthSquared();

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
                                    $"BoidMovementSystem Update Error: Normalized separation vector is zero for boid {boidEntityIds[i]}.");
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
                    if (directionToCenter.LengthSquared() > 0)
                        cohesion = SafeNormalize(directionToCenter) * CohesionWeight;
                }

                if (separation.LengthSquared() > 0)
                    separation = SafeNormalize(separation) * SeparationWeight;

                Vector2 steering = alignment + cohesion + separation + mouseAttraction;

                // Validate steering before assignment
                if (HasInvalidComponents(steering))
                {
                    Debug.LogError(
                        $"BoidMovementSystem Update Error: Steering vector contains invalid components for boid {boidEntityIds[i]} (Steering: {steering}). Steering set to zero.");
                    steering = Vector2.Zero;
                }

                boidSteerings[i] = steering;
            });

            // Update velocities and positions
            for (int i = 0; i < boidCount; i++)
            {
                int entityId = boidEntityIds[i];
                var velocityComponent = velocities[entityId];
                var positionComponent = positions[entityId];

                velocityComponent.Velocity += boidSteerings[i] * 20f * deltaTime;

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

                if (velocityComponent.Velocity.Length() > MaxSpeed)
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

                _componentManager.AddComponent(new Entity(entityId), velocityComponent);
            }
            
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            // Rendering handled elsewhere
        }

        /// <summary>
        /// Safely normalizes a vector. Returns Vector2.Zero if the vector has zero length.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        /// <returns>A normalized vector or Vector2.Zero if normalization is not possible.</returns>
        private Vector2 SafeNormalize(Vector2 vector)
        {
            if (vector.LengthSquared() > 0)
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
        private bool HasInvalidComponents(Vector2 vector)
        {
            return float.IsNaN(vector.X) || float.IsInfinity(vector.X) ||
                   float.IsNaN(vector.Y) || float.IsInfinity(vector.Y);
        }
    }
}
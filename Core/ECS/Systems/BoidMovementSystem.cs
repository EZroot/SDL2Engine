using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SDL2Engine.Core.ECS.Components;
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
        private const float NeighborRadius = 32f;
        private const float NeighborRadiusSquared = NeighborRadius * NeighborRadius;
        private const float AlignmentWeight = 2.5f;
        private const float CohesionWeight = 2.0f;
        private const float SeparationWeight = 4.5f;
        private const float MinSeparationDistance = 1f;
        private const float MaxSpeed = 20f;
        private const float Damping = 0.8f;

        public BoidMovementSystem(ComponentManager componentManager, SpatialPartitionerECS partitioner)
        {
            _componentManager = componentManager;
            _partitioner = partitioner;
        }

        public void Update(float deltaTime)
        {
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

            // TODO: ADD/REMOVE OVERFLOWS! Which causes the entities to 'dissapear' 
            // TODO: Look in to what the fk is causing overflow (Spatial paritioner)
            Parallel.For(0, boidCount, i =>
            {
                Vector2 alignment = Vector2.Zero;
                Vector2 cohesion = Vector2.Zero;
                Vector2 separation = Vector2.Zero;

                Vector2 currentPosition = boidPositions[i];
                Vector2 currentVelocity = boidVelocities[i];

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
                            separation -= Vector2.Normalize(diff) * (MinSeparationDistance - distance) / MinSeparationDistance;
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
                    alignment = Vector2.Normalize(averageVelocity) * AlignmentWeight;
                    centerOfMass /= neighborCount;
                    Vector2 directionToCenter = centerOfMass - currentPosition;
                    if (directionToCenter.LengthSquared() > 0)
                        cohesion = Vector2.Normalize(directionToCenter) * CohesionWeight;
                }
                if (separation.LengthSquared() > 0)
                    separation = Vector2.Normalize(separation) * SeparationWeight;

                boidSteerings[i] = alignment + cohesion + separation;
            });

            for (int i = 0; i < boidCount; i++)
            {
                int entityId = boidEntityIds[i];
                var velocityComponent = velocities[entityId];
                var positionComponent = positions[entityId];
                velocityComponent.Velocity += boidSteerings[i] * deltaTime;
                velocityComponent.Velocity *= MathF.Pow(Damping, deltaTime);
                if (velocityComponent.Velocity.Length() > MaxSpeed)
                {
                    velocityComponent.Velocity = Vector2.Normalize(velocityComponent.Velocity) * MaxSpeed;
                }
                _componentManager.AddComponent(new Entity(entityId), velocityComponent);
                // _partitioner.UpdateEntity(new Entity(entityId), positionComponent.Position - velocityComponent.Velocity * deltaTime);
            }
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            // rendering handled elsewhere
        }
    }
}

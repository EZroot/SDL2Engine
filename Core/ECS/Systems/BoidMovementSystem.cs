// namespace SDL2Engine.Core.ECS.Systems;
//
// // BoidMovementSystem.cs
// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using System.Threading.Tasks;
// using SDL2Engine.Core.ECS.Components;
// using SDL2Engine.Core.Partitions;
// using SDL2Engine.Core.Input;
// using SDL2Engine.Core.Rendering.Interfaces;
//
// public class BoidMovementSystem : ISystem
// {
//     private readonly ComponentManager _componentManager;
//     private readonly SpatialPartitioner<Entity> _partitioner;
//     private readonly float _worldSize;
//     private readonly float _boidSpeed;
//
//     private const float NeighborRadius = 32f;
//     private const float NeighborRadiusSquared = NeighborRadius * NeighborRadius;
//     private const float AlignmentWeight = 2.5f;
//     private const float CohesionWeight = 2.0f;
//     private const float SeparationWeight = 4.5f;
//     private const float DebugMousePullWeight = 1f;
//     private const float MinSeparationDistance = 1f;
//
//     public BoidMovementSystem(ComponentManager componentManager, float worldSize, float boidSpeed,
//         int spatialPartitionSize = 32)
//     {
//         _componentManager = componentManager;
//         _worldSize = worldSize;
//         _boidSpeed = boidSpeed;
//         // _partitioner = new SpatialPartitioner<Entity>(spatialPartitionSize);
//     }
//
//     public void Update(float deltaTime)
//     {
//         var boids = _componentManager.GetEntitiesWith<PositionComponent, VelocityComponent, BoidComponent>();
//         _partitioner.Clear();
//
//         // Insert all boids into the spatial partitioner
//         foreach (var boid in boids)
//         {
//             if (_componentManager.TryGetComponent(boid, out PositionComponent position))
//             {
//                 _partitioner.Insert(boid.Id, position.Position);
//             }
//         }
//
//         var boidSteerings = new Vector2[boids.Count];
//         Vector2 mousePosition = new Vector2(InputManager.MouseX, InputManager.MouseY);
//
//         // Parallel processing for performance
//         Parallel.For(0, boids.Count, i =>
//         {
//             var boid = boids[i];
//             if (!_componentManager.TryGetComponent(boid, out PositionComponent position) ||
//                 !_componentManager.TryGetComponent(boid, out VelocityComponent velocity))
//             {
//                 boidSteerings[i] = Vector2.Zero;
//                 return;
//             }
//
//             var neighbors = _partitioner.GetNeighbors(position.Position, NeighborRadius);
//             Vector2 alignment = CalculateAlignment(boid, neighbors) * AlignmentWeight;
//             Vector2 cohesion = CalculateCohesion(boid, neighbors) * CohesionWeight;
//             Vector2 separation = CalculateSeparation(boid, neighbors) * SeparationWeight;
//
//             Vector2 toMouse = mousePosition - position.Position;
//             Vector2 mouseAttraction = Vector2.Zero;
//             if (toMouse.LengthSquared() > 0)
//                 mouseAttraction = Vector2.Normalize(toMouse) * DebugMousePullWeight;
//
//             boidSteerings[i] = alignment + cohesion + separation + mouseAttraction;
//         });
//
//         // Apply the steering to each boid
//         for (int i = 0; i < boids.Count; i++)
//         {
//             var boid = boids[i];
//             if (_componentManager.TryGetComponent(boid, out VelocityComponent velocity) &&
//                 _componentManager.TryGetComponent(boid, out PositionComponent position))
//             {
//                 velocity.Velocity += boidSteerings[i] * deltaTime;
//
//                 float speedSq = velocity.Velocity.LengthSquared();
//                 if (speedSq > _boidSpeed * _boidSpeed)
//                 {
//                     velocity.Velocity = Vector2.Normalize(velocity.Velocity) * _boidSpeed;
//                 }
//
//                 // Update position
//                 position.Position += velocity.Velocity * deltaTime;
//
//                 // Optional: Handle world boundaries
//                 position.Position = Vector2.Clamp(position.Position, Vector2.Zero, new Vector2(_worldSize));
//
//                 _componentManager.AddComponent(boid, velocity);
//                 _componentManager.AddComponent(boid, position);
//             }
//         }
//     }
//
//     private Vector2 CalculateAlignment(Entity boid, IEnumerable<int> neighbors)
//     {
//         Vector2 averageVelocity = Vector2.Zero;
//         int count = 0;
//         foreach (var neighborId in neighbors)
//         {
//             if (_componentManager.TryGetComponent(neighborId, out VelocityComponent neighborVelocity))
//             {
//                 averageVelocity += neighborVelocity.Velocity;
//                 count++;
//             }
//         }
//
//         if (count == 0) return Vector2.Zero;
//         averageVelocity /= count;
//         if (_componentManager.TryGetComponent(boid, out VelocityComponent boidVelocity))
//         {
//             return averageVelocity - boidVelocity.Velocity;
//         }
//
//         return Vector2.Zero;
//     }
//
//     private Vector2 CalculateCohesion(Entity boid, IEnumerable<int> neighbors)
//     {
//         Vector2 centerOfMass = Vector2.Zero;
//         int count = 0;
//         foreach (var neighborId in neighbors)
//         {
//             if (_componentManager.TryGetComponent(neighborId, out PositionComponent neighborPosition))
//             {
//                 centerOfMass += neighborPosition.Position;
//                 count++;
//             }
//         }
//
//         if (count == 0) return Vector2.Zero;
//         centerOfMass /= count;
//
//         if (_componentManager.TryGetComponent(boid, out PositionComponent boidPosition))
//         {
//             Vector2 direction = centerOfMass - boidPosition.Position;
//             return (direction.LengthSquared() > 0) ? Vector2.Normalize(direction) : Vector2.Zero;
//         }
//
//         return Vector2.Zero;
//     }
//
//     private Vector2 CalculateSeparation(Entity boid, IEnumerable<int> neighbors)
//     {
//         Vector2 separation = Vector2.Zero;
//
//         if (!_componentManager.TryGetComponent(boid, out PositionComponent boidPosition))
//             return Vector2.Zero;
//
//         foreach (var neighborId in neighbors)
//         {
//             if (neighborId == boid.Id) continue;
//
//             if (_componentManager.TryGetComponent(neighborId, out PositionComponent neighborPosition))
//             {
//                 Vector2 diff = boidPosition.Position - neighborPosition.Position;
//                 float distanceSquared = diff.LengthSquared();
//                 if (distanceSquared > 0 && distanceSquared < NeighborRadiusSquared)
//                 {
//                     float distance = MathF.Sqrt(distanceSquared);
//                     if (distance < MinSeparationDistance)
//                     {
//                         float strength = (MinSeparationDistance - distance) / MinSeparationDistance;
//                         separation += Vector2.Normalize(diff) * strength * 10.0f;
//                     }
//                     else
//                     {
//                         float strength = 1.0f / distanceSquared;
//                         separation += diff * strength;
//                     }
//                 }
//             }
//         }
//
//         return (separation.LengthSquared() > 0) ? Vector2.Normalize(separation) : Vector2.Zero;
//     }
// }
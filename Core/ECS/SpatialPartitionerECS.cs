using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.ECS;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SDL2Engine.Core.Partitions
{
    public class SpatialPartitionerECS
    {
        private readonly float cellSize;
        private readonly Dictionary<(int, int), HashSet<Entity>> grid;
        private readonly ComponentManager componentManager;

        private sealed class CellComparer : IEqualityComparer<(int, int)>
        {
            public bool Equals((int, int) c1, (int, int) c2) => c1 == c2;
            public int GetHashCode((int, int) cell) => (cell.Item1 * 397) ^ cell.Item2;
        }

        public SpatialPartitionerECS(ComponentManager componentManager, float cellSize)
        {
            this.componentManager = componentManager;
            this.cellSize = cellSize;
            grid = new Dictionary<(int, int), HashSet<Entity>>(new CellComparer());

            // Initialize grid with existing entities
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            var entities = componentManager.GetEntitiesWith<PositionComponent>();
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        private (int, int) GetCell(Vector2 position)
        {
            int x = (int)MathF.Floor(position.X / cellSize);
            int y = (int)MathF.Floor(position.Y / cellSize);
            return (x, y);
        }

        public void Add(Entity entity)
        {
            if (!componentManager.TryGetComponent(entity, out PositionComponent positionComp))
                return;

            var cell = GetCell(positionComp.Position);

            if (componentManager.TryGetComponent(entity, out CurrentCellComponent currentCellComp))
            {
                if (currentCellComp.Cell == cell)
                    return;
                // Remove from old cell
                RemoveFromCell(entity, currentCellComp.Cell);
            }

            AddToCell(entity, cell);
            componentManager.AddComponent(entity, new CurrentCellComponent(cell));
        }

        public void Remove(Entity entity)
        {
            if (!componentManager.TryGetComponent(entity, out CurrentCellComponent currentCellComp))
                return;

            RemoveFromCell(entity, currentCellComp.Cell);
            componentManager.RemoveComponent<CurrentCellComponent>(entity);
        }

        private void AddToCell(Entity entity, (int, int) cell)
        {
            if (!grid.TryGetValue(cell, out var set))
            {
                set = new HashSet<Entity>();
                grid[cell] = set;
            }

            set.Add(entity);
        }

        private void RemoveFromCell(Entity entity, (int, int) cell)
        {
            if (grid.TryGetValue(cell, out var set))
            {
                set.Remove(entity);
                if (set.Count == 0)
                {
                    grid.Remove(cell);
                }
            }
            else
            {
                // Log error if necessary
                Debug.LogError($"Failed to remove entity {entity.Id} from cell {cell}");
            }
        }

        public void UpdateEntity(Entity entity, Vector2 oldPosition)
        {
            if (!componentManager.TryGetComponent(entity, out PositionComponent newPositionComp))
                return;

            var oldCell = GetCell(oldPosition);
            var newCell = GetCell(newPositionComp.Position);

            if (oldCell != newCell)
            {
                RemoveFromCell(entity, oldCell);
                AddToCell(entity, newCell);
                componentManager.AddComponent(entity, new CurrentCellComponent(newCell));
            }
        }

        public IEnumerable<Entity> GetNeighbors(Vector2 position, float radius)
        {
            float radiusSq = radius * radius;

            int minX = (int)MathF.Floor((position.X - radius) / cellSize);
            int maxX = (int)MathF.Floor((position.X + radius) / cellSize);
            int minY = (int)MathF.Floor((position.Y - radius) / cellSize);
            int maxY = (int)MathF.Floor((position.Y + radius) / cellSize);

            var neighbors = new List<Entity>();

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (grid.TryGetValue((x, y), out var cellEntities))
                    {
                        foreach (var entity in cellEntities)
                        {
                            if (componentManager.TryGetComponent(entity, out PositionComponent posComp))
                            {
                                if (Vector2.DistanceSquared(posComp.Position, position) <= radiusSq)
                                {
                                    neighbors.Add(entity);
                                }
                            }
                        }
                    }
                }
            }

            return neighbors;
        }

        public IEnumerable<Entity> GetEntitiesInCell(Vector2 position)
        {
            var cell = GetCell(position);
            if (grid.TryGetValue(cell, out var set))
            {
                return set;
            }

            return Enumerable.Empty<Entity>();
        }

        public void RenderDebug(IRenderService renderService, Matrix4 projection, ICameraService cameraService = null)
        {
            Color4 rectColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f); // Red
            Color4 lineColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f); // Green

            Vector2 cameraOffset = Vector2.Zero;

            if (cameraService != null)
            {
                cameraOffset = cameraService.GetActiveCamera().GetOffset();
            }

            foreach (var kvp in grid)
            {
                var cell = kvp.Key;
                var entities = kvp.Value;

                Vector2 topLeft = new Vector2(cell.Item1 * cellSize, cell.Item2 * cellSize) - cameraOffset;
                Vector2 bottomRight = topLeft + new Vector2(cellSize, cellSize);

                var tl = new OpenTK.Mathematics.Vector2(topLeft.X, topLeft.Y);
                var br = new OpenTK.Mathematics.Vector2(bottomRight.X, bottomRight.Y);
                renderService.DrawRect(tl, br, rectColor);

                foreach (var entity in entities)
                {
                    if (componentManager.TryGetComponent(entity, out PositionComponent posComp))
                    {
                        Vector2 objPosition = posComp.Position - cameraOffset;

                        foreach (var other in entities)
                        {
                            if (entity.Equals(other)) continue;

                            if (componentManager.TryGetComponent(other, out PositionComponent otherPosComp))
                            {
                                Vector2 otherPosition = otherPosComp.Position - cameraOffset;

                                var objPos = new OpenTK.Mathematics.Vector2(objPosition.X, objPosition.Y);
                                var otherPos = new OpenTK.Mathematics.Vector2(otherPosition.X, otherPosition.Y);
                                renderService.DrawLine(objPos, otherPos, lineColor);
                            }
                        }
                    }
                }
            }
        }
    }
}
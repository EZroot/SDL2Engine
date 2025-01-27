using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
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
        private readonly Dictionary<(int, int), List<Entity>> grid;
        private readonly ComponentManager componentManager;

        public SpatialPartitionerECS(ComponentManager componentManager, float cellSize)
        {
            this.componentManager = componentManager;
            this.cellSize = cellSize;
            grid = new Dictionary<(int, int), List<Entity>>();

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
            if (!grid.TryGetValue(cell, out var cellList))
            {
                cellList = new List<Entity>();
                grid[cell] = cellList;
            }

            cellList.Add(entity);
        }

        private void RemoveFromCell(Entity entity, (int, int) cell)
        {
            if (grid.TryGetValue(cell, out var cellList))
            {
                cellList.Remove(entity);
                if (cellList.Count == 0)
                {
                    grid.Remove(cell);
                }
            }
            else
            {
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
                    var cell = (x, y);
                    if (grid.TryGetValue(cell, out var cellEntities))
                    {
                        foreach (var entity in cellEntities)
                        {
                            if (componentManager.TryGetComponent(entity, out PositionComponent posComp))
                            {
                                Vector2 diff = posComp.Position - position;
                                if (diff.LengthSquared() <= radiusSq)
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
            if (grid.TryGetValue(cell, out var cellEntities))
            {
                return cellEntities;
            }

            return Enumerable.Empty<Entity>();
        }

        public void RenderDebug(IRenderService renderService, Matrix4 projection, ICameraService cameraService = null)
        {
            Color4 rectColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
            Color4 lineColor = new Color4(0.0f, 1.0f, 0.0f, 1.0f);

            Vector2 cameraOffset = Vector2.Zero;

            if (cameraService != null)
            {
                cameraOffset = cameraService.GetActiveCamera().GetOffset();
            }

            foreach (var cell in grid.Keys)
            {
                // Calculate the top-left and bottom-right coordinates of the cell
                Vector2 topLeft = new Vector2(cell.Item1 * cellSize, cell.Item2 * cellSize) - cameraOffset;
                Vector2 bottomRight = topLeft + new Vector2(cellSize, cellSize);

                // Convert to OpenTK Vector2 for rendering
                var tl = new OpenTK.Mathematics.Vector2(topLeft.X, topLeft.Y);
                var br = new OpenTK.Mathematics.Vector2(bottomRight.X, bottomRight.Y);

                renderService.DrawRect(tl, br, rectColor);
                var entitiesInCell = grid[cell];
                if (entitiesInCell.Count > 1)
                {
                    for (int i = 0; i < entitiesInCell.Count; i++)
                    {
                        var entityA = entitiesInCell[i];
                        if (!componentManager.TryGetComponent(entityA, out PositionComponent posCompA))
                            continue;
                        Vector2 posA = posCompA.Position - cameraOffset;
                        var vA = new OpenTK.Mathematics.Vector2(posA.X, posA.Y);

                        for (int j = i + 1; j < entitiesInCell.Count; j++)
                        {
                            var entityB = entitiesInCell[j];
                            if (!componentManager.TryGetComponent(entityB, out PositionComponent posCompB))
                                continue;
                            Vector2 posB = posCompB.Position - cameraOffset;
                            var vB = new OpenTK.Mathematics.Vector2(posB.X, posB.Y);

                            renderService.DrawLine(vA, vB, lineColor);
                        }
                    }
                }
            }
        }

    }
}
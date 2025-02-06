using SDL2;
using System.Collections.Generic;
using OpenTK.Mathematics;
using SDL2Engine.Core.Partitions.Interfaces;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Partitions;

public class SpatialPartitioner : IPartitioner
{
    private readonly float cellSize;
    private readonly Dictionary<(int, int), HashSet<GameObject>> grid;

    private sealed class CellComparer : IEqualityComparer<(int, int)>
    {
        public bool Equals((int, int) c1, (int, int) c2) => c1 == c2;
        public int GetHashCode((int, int) cell) => (cell.Item1 * 397) ^ cell.Item2;
    }
    
    public SpatialPartitioner(float cellSize)
    {
        this.cellSize = cellSize;
        grid = new Dictionary<(int, int), HashSet<GameObject>>(new CellComparer());
    }

    private (int, int) GetCell(Vector3 position)
    {
        int x = (int)Math.Floor(position.X / cellSize);
        int y = (int)Math.Floor(position.Y / cellSize);
        return (x, y);
    }

    public void Add(GameObject obj)
    {
        if (obj == null) return;
        var cell = GetCell(obj.Position);
        if (obj.CurrentCell == cell) return;

        if (!grid.TryGetValue(cell, out var set))
        {
            set = new HashSet<GameObject>();
            grid[cell] = set;
        }
        set.Add(obj);
        obj.CurrentCell = cell;
    }

    public void Remove(GameObject obj)
    {
        if (obj?.CurrentCell == null) return;
        var cell = obj.CurrentCell.Value;

        if (grid.TryGetValue(cell, out var set) && set.Remove(obj))
        {
            if (set.Count == 0) grid.Remove(cell);
            obj.CurrentCell = null;
        }
        else
        {
            Debug.LogError($"Failed to remove object from cell {cell}");
        }
    }

    public void Update(GameObject obj)
    {
        if (obj == null) return;
        var newCell = GetCell(obj.Position);
        if (obj.CurrentCell != newCell)
        {
            Remove(obj);
            Add(obj);
        }
    }

    public IEnumerable<GameObject> GetNeighbors(Vector3 position, float radius)
    {
        float radiusSq = radius * radius;

        int minX = (int)MathF.Floor((position.X - radius) / cellSize);
        int maxX = (int)MathF.Floor((position.X + radius) / cellSize);
        int minY = (int)MathF.Floor((position.Y - radius) / cellSize);
        int maxY = (int)MathF.Floor((position.Y + radius) / cellSize);

        var neighbors = new List<GameObject>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (grid.TryGetValue((x, y), out var cellObjects))
                {
                    foreach (var obj in cellObjects)
                    {
                        if (Vector2.DistanceSquared(new Vector2(obj.Position.X, obj.Position.Y), new Vector2(position.X, position.Y)) <= radiusSq)
                            neighbors.Add(obj);
                    }
                }
            }
        }

        return neighbors;
    }

    public IEnumerable<GameObject> GetObjectsInCell(Vector3 position)
    {
        var cell = GetCell(position);
        return grid.TryGetValue(cell, out var set) ? set : Enumerable.Empty<GameObject>();
    }

    public void RenderDebug(nint renderer, ICameraService cameraService = null)
    {
        var rectColor = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 };
        var lineColor = new SDL.SDL_Color { r = 0, g = 255, b = 0, a = 255 };


        OpenTK.Mathematics.Vector3 cameraOffset = OpenTK.Mathematics.Vector3.Zero;

        if (cameraService != null)
        {
            cameraOffset = cameraService.GetActiveCamera().GetOffset();
        }

        foreach (var kvp in grid)
        {
            var cell = kvp.Key;
            var objects = kvp.Value;

            var rect = new SDL.SDL_Rect
            {
                x = (int)(cell.Item1 * cellSize - cameraOffset.X),
                y = (int)(cell.Item2 * cellSize - cameraOffset.Y),
                w = (int)cellSize,
                h = (int)cellSize
            };
            SDL.SDL_SetRenderDrawColor(renderer, rectColor.r, rectColor.g, rectColor.b, rectColor.a);

            SDL.SDL_RenderDrawRect(renderer, ref rect);

            SDL.SDL_SetRenderDrawColor(renderer, lineColor.r, lineColor.g, lineColor.b, lineColor.a);

            foreach (var obj in objects)
            {
                Vector3 objPosition = obj.Position - cameraOffset;

                foreach (var other in objects)
                {
                    if (obj == other) continue;

                    Vector3 otherPosition = other.Position - cameraOffset;

                    SDL.SDL_RenderDrawLine(renderer,
                        (int)objPosition.X, (int)objPosition.Y,
                        (int)otherPosition.X, (int)otherPosition.Y);
                }
            }
        }
    }
}
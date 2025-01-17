using SDL2;
using System.Collections.Generic;
using System.Numerics;
using SDL2Engine.Core.Partitions.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Partitions;
public class SpatialPartitioner : IPartitioner
{
    private readonly float cellSize;
    private readonly Dictionary<(int, int), HashSet<GameObject>> grid = new();

    public SpatialPartitioner(float cellSize)
    {
        this.cellSize = cellSize;
    }

    private (int, int) GetCell(Vector2 position)
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

        if (!grid.ContainsKey(cell))
        {
            grid[cell] = new HashSet<GameObject>();
        }
        grid[cell].Add(obj);

        obj.CurrentCell = cell;
    }

    public void Remove(GameObject obj)
    {
        if (obj == null || obj.CurrentCell == null) return;

        var cell = obj.CurrentCell.Value;
        if (grid.TryGetValue(cell, out var objects) && objects.Remove(obj))
        {
            if (objects.Count == 0)
            {
                grid.Remove(cell);
            }
            obj.CurrentCell = null; 
        }
        else
        {
            Debug.LogError($"[SpatialPartitioner] Failed to remove object from cell {cell}, position {obj.Position}");
        }
    }

    public void Update(GameObject obj, Vector2 oldPosition)
    {
        if (obj == null) return;

        var newCell = GetCell(obj.Position);
        if (obj.CurrentCell != newCell)
        {
            Remove(obj); 
            Add(obj);    
        }
    }

    public IEnumerable<GameObject> GetObjectsInCell(Vector2 position)
    {
        var cell = GetCell(position);
        return grid.ContainsKey(cell) ? grid[cell] : new HashSet<GameObject>();
    }

    public void RenderDebug(nint renderer, ICameraService cameraService = null)
    {
        var rectColor = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 }; 
        var lineColor = new SDL.SDL_Color { r = 0, g = 255, b = 0, a = 255 }; 

        SDL.SDL_SetRenderDrawColor(renderer, rectColor.r, rectColor.g, rectColor.b, rectColor.a);

        Vector2 cameraOffset = Vector2.Zero;

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

            SDL.SDL_RenderDrawRect(renderer, ref rect);

            SDL.SDL_SetRenderDrawColor(renderer, lineColor.r, lineColor.g, lineColor.b, lineColor.a);

            foreach (var obj in objects)
            {
                Vector2 objPosition = obj.Position - cameraOffset;

                foreach (var other in objects)
                {
                    if (obj == other) continue;

                    Vector2 otherPosition = other.Position - cameraOffset;

                    SDL.SDL_RenderDrawLine(renderer,
                        (int)objPosition.X, (int)objPosition.Y,
                        (int)otherPosition.X, (int)otherPosition.Y);
                }
            }
        }
    }

}

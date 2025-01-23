namespace SDL2Engine.Core.ECS.Components
{
    public struct CurrentCellComponent : IComponent
    {
        public (int, int) Cell;

        public CurrentCellComponent((int, int) cell)
        {
            Cell = cell;
        }
    }
}
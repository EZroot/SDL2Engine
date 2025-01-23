namespace SDL2Engine.Core.ECS;

public class Entity
{
    public int Id { get; private set; }
        
    public Entity(int id)
    {
        Id = id;
    }
}
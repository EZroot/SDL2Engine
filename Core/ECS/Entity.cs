namespace SDL2Engine.Core.ECS
{
    public struct Entity
    {
        public int Id { get; }

        internal Entity(int id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            return obj is Entity entity && Id == entity.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
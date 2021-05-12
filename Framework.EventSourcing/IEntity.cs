// This file was originally taken from https://github.com/aneshas/tactical-ddd

namespace Framework.DDD
{
    public interface IEntity<out TIdentity> where TIdentity : IEntityId 
    {
        TIdentity Id { get; } 
    }
}
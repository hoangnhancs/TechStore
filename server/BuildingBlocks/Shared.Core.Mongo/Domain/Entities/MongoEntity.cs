using MongoDB.Entities;

namespace Shared.Core.Mongo.Domain.Entities
{
    /// <summary>
    /// Base entity for MongoDB using MongoDB.Entities library.
    /// Combines MongoDB.Entities.Entity with our custom IEntity interface.
    /// </summary>
    public abstract class MongoEntity : Entity, IEntity<string> 
    {
        // MongoDB.Entities.Entity already has ID property
        // We just need to satisfy IEntity<string> interface
        string IEntity<string>.Id => ID;
    }

    /// <summary>
    /// Base entity for MongoDB with custom ID type.
    /// Use this if you need to work with different ID types while still using MongoDB's string IDs internally.
    /// </summary>
    public abstract class MongoEntity<TId> : Entity, IEntity<TId>
    {
        // Custom typed ID property
        public abstract TId Id { get; }
    }
}

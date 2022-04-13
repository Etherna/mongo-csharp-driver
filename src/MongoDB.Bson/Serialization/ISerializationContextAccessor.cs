namespace Etherna.MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provide an access to current serialization context.
    /// Implemented as a workaround for driver's static global registries.
    /// </summary>
    public interface ISerializationContextAccessor
    {
        /// <summary>
        /// Try to get a BsonSerializerRegistry from current context
        /// </summary>
        /// <returns>BsonSerializerRegistry if available, null otherwise</returns>
        IBsonSerializerRegistry TryGetCurrentBsonSerializerRegistry();
    }
}

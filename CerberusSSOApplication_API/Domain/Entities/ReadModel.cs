using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class ReadModel
{
    [BsonElement("concurrencyIndex")]
    public ulong ConcurrencyIndex { get; set; }
}

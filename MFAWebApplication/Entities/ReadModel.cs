using MongoDB.Bson.Serialization.Attributes;

namespace MFAWebApplication.Entities;

public class ReadModel
{
    [BsonElement("concurrencyIndex")]
    public ulong ConcurrencyIndex { get; set; }
}

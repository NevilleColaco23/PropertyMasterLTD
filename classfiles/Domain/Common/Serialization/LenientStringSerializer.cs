using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MyWarehouse.Domain.Common.Serialization;

/// <summary>
/// Tolerant string serializer that gracefully handles legacy/corrupt documents where a
/// field expected to be a string (e.g. Users.LockoutEnd) was instead persisted as an
/// empty array, null, or other unexpected BsonType. Any non-string representation is
/// treated as null on read, preventing a hard FormatException during deserialization.
/// </summary>
public class LenientStringSerializer : SerializerBase<string?>
{
    public override string? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        switch (reader.GetCurrentBsonType())
        {
            case BsonType.String:
                return reader.ReadString();

            case BsonType.Null:
                reader.ReadNull();
                return null;

            case BsonType.Array:
                reader.ReadStartArray();
                while (reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    context.Reader.SkipValue();
                }
                reader.ReadEndArray();
                return null;

            case BsonType.Document:
                reader.ReadStartDocument();
                while (reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    reader.SkipName();
                    context.Reader.SkipValue();
                }
                reader.ReadEndDocument();
                return null;

            default:
                // Fall back to skipping the value rather than throwing, to keep reads resilient.
                reader.SkipValue();
                return null;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string? value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else
        {
            context.Writer.WriteString(value);
        }
    }
}

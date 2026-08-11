using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MyWarehouse.Domain.Common.Serialization;

namespace APIUnitProject.Tests.DomainTests.Common
{
    public class LenientStringSerializerTests
    {
        private readonly LenientStringSerializer _serializer = new();

        private string? DeserializeFromBson(BsonDocument wrapper)
        {
            using var reader = new BsonDocumentReader(wrapper);
            reader.ReadStartDocument();
            reader.ReadName("Value");

            var context = BsonDeserializationContext.CreateRoot(reader);
            var args = new BsonDeserializationArgs { NominalType = typeof(string) };

            var result = _serializer.Deserialize(context, args);

            reader.ReadEndDocument();
            return result;
        }

        [Fact]
        public void Deserialize_StringValue_ReturnsString()
        {
            var wrapper = new BsonDocument("Value", "2024-01-01T00:00:00Z");

            var result = DeserializeFromBson(wrapper);

            Assert.Equal("2024-01-01T00:00:00Z", result);
        }

        [Fact]
        public void Deserialize_NullValue_ReturnsNull()
        {
            var wrapper = new BsonDocument("Value", BsonNull.Value);

            var result = DeserializeFromBson(wrapper);

            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_EmptyArrayValue_ReturnsNullInsteadOfThrowing()
        {
            var wrapper = new BsonDocument("Value", new BsonArray());

            var result = DeserializeFromBson(wrapper);

            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_ArrayWithElementsValue_ReturnsNullInsteadOfThrowing()
        {
            var wrapper = new BsonDocument("Value", new BsonArray { "unexpected", "values" });

            var result = DeserializeFromBson(wrapper);

            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_DocumentValue_ReturnsNullInsteadOfThrowing()
        {
            var wrapper = new BsonDocument("Value", new BsonDocument("nested", "value"));

            var result = DeserializeFromBson(wrapper);

            Assert.Null(result);
        }

        [Fact]
        public void Serialize_NullValue_WritesBsonNull()
        {
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                writer.WriteStartDocument();
                writer.WriteName("Value");

                var context = BsonSerializationContext.CreateRoot(writer);
                _serializer.Serialize(context, new BsonSerializationArgs(), null);

                writer.WriteEndDocument();
            }

            Assert.Equal(BsonType.Null, document["Value"].BsonType);
        }

        [Fact]
        public void Serialize_StringValue_WritesString()
        {
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                writer.WriteStartDocument();
                writer.WriteName("Value");

                var context = BsonSerializationContext.CreateRoot(writer);
                _serializer.Serialize(context, new BsonSerializationArgs(), "hello");

                writer.WriteEndDocument();
            }

            Assert.Equal("hello", document["Value"].AsString);
        }
    }
}

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MqttBridge.Processors;

public class BsonDateOnlySerializer : SerializerBase<DateOnly>
{
    public static BsonDateOnlySerializer Instance { get; } = new();

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        DateTimeSerializer.DateOnlyInstance.Serialize(context, args, value.ToDateTime(TimeOnly.MinValue));
    }

    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        DateTime dateTime = DateTimeSerializer.DateOnlyInstance.Deserialize(context, args);
        return DateOnly.FromDateTime(dateTime);
    }
}
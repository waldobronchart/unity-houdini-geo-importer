using System;
using UnityEngine;

namespace Newtonsoft.Json
{
    public class JsonConverterBounds : JsonConverter<Bounds>
    {
        public override void WriteJson(JsonWriter writer, Bounds value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.min.x);
            writer.WriteValue(value.min.y);
            writer.WriteValue(value.min.z);
            writer.WriteValue(value.max.x);
            writer.WriteValue(value.max.y);
            writer.WriteValue(value.max.z);
            writer.WriteEndArray();
        }

        public override Bounds ReadJson(
            JsonReader reader, Type objectType, Bounds existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // TODO
            return new Bounds();
        }

        public override bool CanRead => false;
    }
}

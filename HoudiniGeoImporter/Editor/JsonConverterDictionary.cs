using System;
using System.Collections.Generic;

namespace Newtonsoft.Json
{
    public class JsonConverterDictionary : JsonConverter<Dictionary<string, object>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, object> value, JsonSerializer serializer)
        {
            JsonTextWriterAdvanced writerAdvanced = writer as JsonTextWriterAdvanced;
            
            writerAdvanced.WriteStartDictionary();
            foreach (KeyValuePair<string, object> kvp in value)
            {
                writerAdvanced.WriteDictionaryKeyValuePair(kvp.Key, kvp.Value);
            }
            writerAdvanced.WriteEndDictionary();
        }

        public override Dictionary<string, object> ReadJson(
            JsonReader reader, Type objectType, Dictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // We don't ever have to read one.
            return new Dictionary<string, object>();
        }

        public override bool CanRead => false;
    }
}

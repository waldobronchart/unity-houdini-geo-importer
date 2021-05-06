using System.Collections.Generic;
using System.IO;

namespace Newtonsoft.Json
{
    /// <summary>
    /// The basic json text writer with access to some of the protected members for advanced formatting.
    /// </summary>
    public class JsonTextWriterAdvanced : JsonTextWriter
    {
        private enum Hierarchies
        {
            Dictionary,
            Object,
            Array,
        }
        
        private enum ValueTypes
        {
            Value,
            DictionaryKey,
            DictionaryValue,
        }
        
        private static JsonSerializer cachedJsonSerializer;
        private static JsonSerializer JsonSerializer
        {
            get
            {
                if (cachedJsonSerializer == null)
                {
                    cachedJsonSerializer = JsonSerializer.Create();
                    cachedJsonSerializer.Converters.Add(new JsonConverterBounds());
                    cachedJsonSerializer.Converters.Add(new JsonConverterDictionary());
                }
                return cachedJsonSerializer;
            }
        }

        private bool isArrayDictionary;
        private ValueTypes valueType;

        private Stack<Hierarchies> hierarchyStack = new Stack<Hierarchies>();
        private Hierarchies CurrentHierarchy => hierarchyStack.Peek();

        public JsonTextWriterAdvanced(TextWriter textWriter) : base(textWriter)
        {
        }

        private void WriteNewLine()
        {
            base.WriteWhitespace("\n");
        }

        private void WriteIndent(bool withLineBreak)
        {
            if (withLineBreak)
                WriteNewLine();
            
            for (int i = 0; i < Top; i++)
            {
                base.WriteRaw(IndentChar.ToString());
            }
        }

        public void WriteStartDictionary()
        {
            isArrayDictionary = true;
            WriteStartArray();
            isArrayDictionary = false;
        }
        
        public void WriteDictionaryKeyValuePair(object key, object value)
        {
            valueType = ValueTypes.DictionaryKey;
            WriteValue(key);

            valueType = ValueTypes.DictionaryValue;
            JsonSerializer.Serialize(this, value);
            
            valueType = ValueTypes.Value;
        }

        protected override void WriteIndent()
        {
            // The value of a dictionary's key/value pair does not get indentation...
            if (CurrentHierarchy == Hierarchies.Dictionary && valueType == ValueTypes.DictionaryValue)
                return;
            
            WriteIndent(true);
        }

        public void WriteEndDictionary()
        {
            isArrayDictionary = true;
            WriteEndArray();
            isArrayDictionary = false;
        }

        public override void WriteValue(object value)
        {
            if (value is Dictionary<string, object>)
            {
                JsonSerializer.Serialize(this, value);
                return;
            }

            base.WriteValue(value);
        }

        public override void WriteStartArray()
        {
            base.WriteStartArray();
            
            hierarchyStack.Push(isArrayDictionary ? Hierarchies.Dictionary : Hierarchies.Array);
        }

        public override void WriteEndArray()
        {
            base.WriteEndArray();
            
            hierarchyStack.Pop();
        }

        public override void WriteStartObject()
        {
            base.WriteStartObject();
            
            hierarchyStack.Push(Hierarchies.Object);
        }

        public override void WriteEndObject()
        {
            base.WriteEndObject();

            hierarchyStack.Pop();
        }
    }
}

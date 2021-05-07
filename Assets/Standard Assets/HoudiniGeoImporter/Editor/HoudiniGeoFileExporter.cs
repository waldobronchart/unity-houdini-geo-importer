/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

namespace Houdini.GeoImporter
{
    public static class HoudiniGeoFileExporter
    {
        [Serializable]
        private class AttributeOptionsType
        {
            public string type;
            public string value;
        }
        
        [Serializable]
        private class AttributeOptions
        {
            public AttributeOptionsType type = new AttributeOptionsType();

            public AttributeOptions(string type, string value)
            {
                this.type.type = type;
                this.type.value = value;
            }
        }
        
        private const string DateFormat = "yyyy-MM-d HH:mm:ss";
        
        private static StringWriter stringWriter;
        private static JsonTextWriterAdvanced writer;

        private static HoudiniGeo data;

        public static void Export(HoudiniGeo data)
        {
            stringWriter = new StringWriter();
            stringWriter = new StringWriter();
            writer = new JsonTextWriterAdvanced(stringWriter);
            writer.DateFormatString = DateFormat;
            writer.Formatting = Formatting.Indented;
            writer.IndentChar = '\t';
            HoudiniGeoFileExporter.data = data;

            WriteData();

            SaveDataToFile();
        }

        private static void WriteData()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            
            AddFileInfoToDictionary(dictionary);

            AddTopologyToDictionary(dictionary);

            AddAttributesToDictionary(dictionary);

            AddPrimitivesToDictionary(dictionary);
            
            writer.WriteValue(dictionary);
        }

        private static void AddFileInfoToDictionary(Dictionary<string, object> dictionary)
        {
            dictionary.Add("fileversion", data.fileVersion);
            dictionary.Add("hasindex", data.hasIndex);
            dictionary.Add("pointcount", data.pointCount);
            dictionary.Add("vertexcount", data.vertexCount);
            dictionary.Add("primitivecount", data.primCount);
            dictionary.Add("info", data.fileInfo);
        }

        private static void AddTopologyToDictionary(Dictionary<string, object> dictionary)
        {
            dictionary.Add(
                "topology", new Dictionary<string, object>
                {
                    {
                        "pointref", new Dictionary<string, object>
                        {
                            {"indices", data.pointRefs}
                        }
                    }
                });
        }

        private static void AddAttributesToDictionary(Dictionary<string, object> dictionary)
        {
            // Add the attributes dictionary itself.
            Dictionary<string, object> attributesDictionary = new Dictionary<string, object>();
            dictionary.Add("attributes", attributesDictionary);
            
            // Now add an array to that dictionary for every type of attribute owner.
            foreach (KeyValuePair<string,HoudiniGeoAttributeOwner> kvp in HoudiniGeoFileParser.ATTRIBUTES_TO_PARSE)
            {
                List<object> ownerTypeAttributes = new List<object>(); 
                
                // Populate the attribute owner's array with all attributes of that owner (vertex/point/primitive/detail).
                foreach (HoudiniGeoAttribute attribute in data.attributes)
                {
                    if (attribute.owner != kvp.Value)
                        continue;

                    AddSingleAttributeToDictionary(ownerTypeAttributes, attribute);
                }

                // Only add it if there actually are attributes for this owner type.
                if (ownerTypeAttributes.Count > 0)
                    attributesDictionary.Add(kvp.Key, ownerTypeAttributes);
            }
        }

        private static void AddSingleAttributeToDictionary(List<object> attributes, HoudiniGeoAttribute attribute)
        {
            string typeString = HoudiniGeoFileParser.AttributeTypeEnumValueToCategoryString(attribute.type);

            // Each attribute has a list with two dictionaries: a header and a body.
            List<object> attributeDictionaries = new List<object>();
            attributes.Add(attributeDictionaries);

            // Header dictionary.
            Dictionary<string, object> header = new Dictionary<string, object>()
            {
                {"scope", "public"}, // TODO: Does this ever vary?
                {"type", typeString},
                {"name", attribute.name},
                {
                    "options", // TODO: What is this for?
                    attribute.type == HoudiniGeoAttributeType.String
                        ? new object()
                        : new AttributeOptions("string", "point")
                },
            };
            attributeDictionaries.Add(header);

            // Body dictionary.
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("size", attribute.tupleSize); // TODO: Is this a duplicate of values.size down below?
            string storageType;
            switch (attribute.type)
            {
                // Numeric types
                case HoudiniGeoAttributeType.Float:
                case HoudiniGeoAttributeType.Integer:
                    storageType = HoudiniGeoFileParser.AttributeEnumValueToTypeStr(attribute.type);
                    body.Add("storage", storageType);

                    // TODO: What are we supposed to fill in for the defaults?
                    Dictionary<string, object> defaultsDictionary = new Dictionary<string, object>();
                    body.Add("defaults", defaultsDictionary);
                    defaultsDictionary.Add("size", 1);
                    defaultsDictionary.Add(
                        "storage", storageType); // TODO: Is this duplicated from the storage type above?
                    defaultsDictionary.Add("values", new float[] {0});

                    // Actual values
                    Dictionary<string, object> valuesDictionary = new Dictionary<string, object>();
                    body.Add("values", valuesDictionary);
                    valuesDictionary.Add(
                        "size", attribute.tupleSize); // TODO: Is this a duplicate of the tuple size above?
                    valuesDictionary.Add("storage", storageType); // TODO: Also duplicated?

                    // Tuples.
                    if (attribute.type == HoudiniGeoAttributeType.Float)
                    {
                        string valuesKey = attribute.tupleSize == 1 ? "arrays" : "tuples";
                        valuesDictionary.Add(valuesKey, BreakIntoTuples(attribute.floatValues, attribute.tupleSize));
                    }
                    else if (attribute.type == HoudiniGeoAttributeType.Integer)
                    {
                        valuesDictionary.Add("arrays", BreakIntoTuples(attribute.intValues, attribute.tupleSize));
                    }

                    break;
                // String types
                case HoudiniGeoAttributeType.String:
                    // TODO: Why is storage type integer for strings and is it always like that?
                    storageType = HoudiniGeoFileParser.AttributeEnumValueToTypeStr(HoudiniGeoAttributeType.Integer);
                    body.Add("storage", storageType);

                    BreakIntoUniqueStringsAndIndices(
                        attribute.stringValues, out string[] uniqueStrings, out int[] indices);
                    body.Add("strings", uniqueStrings);

                    Dictionary<string, object> indicesDictionary = new Dictionary<string, object>();
                    body.Add("indices", indicesDictionary);
                    indicesDictionary.Add("size", attribute.tupleSize);
                    indicesDictionary.Add("storage", storageType);

                    indicesDictionary.Add("arrays", new int[][] {indices});
                    break;
                case HoudiniGeoAttributeType.Invalid:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            attributeDictionaries.Add(body);
        }

        private static void BreakIntoUniqueStringsAndIndices(string[] duplicated, out string[] unique, out int[] indices)
        {
            List<string> uniqueStrings = new List<string>();
            indices = new int[duplicated.Length];
            for (int i = 0; i < duplicated.Length; i++)
            {
                // New string found, add it to the list and set the index to it.
                if (!uniqueStrings.Contains(duplicated[i]))
                {
                    uniqueStrings.Add(duplicated[i]);
                    indices[i] = uniqueStrings.Count - 1;
                }

                // Existing string. Go and find the index in the list.
                indices[i] = uniqueStrings.IndexOf(duplicated[i]);
            }

            unique = uniqueStrings.ToArray();
        }

        /// <summary>
        /// Breaks a sequential series of values into tuples of a specified size.
        /// </summary>
        private static T[][] BreakIntoTuples<T>(T[] sequential, int tupleSize)
        {
            List<T[]> tuples = new List<T[]>();
            for (int i = 0; i <= sequential.Length - tupleSize; i += tupleSize)
            {
                T[] tuple = new T[tupleSize];
                for (int j = 0; j < tupleSize; j++)
                {
                    tuple[j] = sequential[i + j];
                }
                tuples.Add(tuple);
            }
            return tuples.ToArray();
        }
        
        private static void AddPrimitivesToDictionary(Dictionary<string, object> dictionary)
        {
            // TODO: Add primitives support. I'll be honest: I'm really only interested in point data myself.
            dictionary.Add("primitives", new object[] {});
        }

        private static void SaveDataToFile()
        {
            writer.Flush();

            string text = stringWriter.GetStringBuilder().ToString();
            writer.Close();

            string path = data.exportPath;

            // DEBUG: Change the extension to .txt so it doesn't get parsed as a Houdini file yet while working.
            //path = Path.ChangeExtension(path, "txt");

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);

            AssetDatabase.Refresh();
        }
    }
}

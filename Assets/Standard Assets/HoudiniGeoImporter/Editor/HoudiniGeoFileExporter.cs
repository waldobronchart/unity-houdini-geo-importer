/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

namespace Houdini.GeoImporter
{
    public static class HoudiniGeoFileExporter
    {
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
            
            // File info
            dictionary.Add("fileversion", data.fileVersion);
            dictionary.Add("hasindex", data.hasIndex);
            dictionary.Add("pointcount", data.pointCount);
            dictionary.Add("vertexcount", data.vertexCount);
            dictionary.Add("primitivecount", data.primCount);
            dictionary.Add("info", data.fileInfo);

            // Topology
            dictionary.Add("topology", new Dictionary<string, object>
            {
                {"pointref", new Dictionary<string, object>
                    {
                        {"indices", data.pointRefs}
                    }
                }
            });

            AddAttributesToDictionary(dictionary);
            
            writer.WriteValue(dictionary);
        }

        private static void AddAttributesToDictionary(Dictionary<string, object> dictionary)
        {
            // Add the attributes dictionary itself.
            Dictionary<string, object> attributesDictionary = new Dictionary<string, object>();
            dictionary.Add("attributes", attributesDictionary);
            
            // Now add a dictionary to that dictionary for every type of attribute owner.
            foreach (KeyValuePair<string,HoudiniGeoAttributeOwner> kvp in HoudiniGeoFileParser.ATTRIBUTES_TO_PARSE)
            {
                List<object> ownerTypeAttributes = new List<object>(); 
                
                foreach (HoudiniGeoAttribute attribute in data.attributes)
                {
                    if (attribute.owner != kvp.Value)
                        continue;
                    
                    // Each attribute has a list with two dictionaries: a header and a body.
                    List<object> attributeDictionaries = new List<object>();
                    ownerTypeAttributes.Add(attributeDictionaries);

                    // Header dictionary.
                    Dictionary<string, object> header = new Dictionary<string, object>()
                    {
                        {"scope", "public"}
                    };
                    attributeDictionaries.Add(header);
                    
                    // Body dictionary.
                    Dictionary<string, object> body = new Dictionary<string, object>()
                    {
                        {"size", 3}
                    };
                    attributeDictionaries.Add(body);
                }

                // Only add it if there actually are attributes for this owner type.
                if (ownerTypeAttributes.Count > 0)
                    attributesDictionary.Add(kvp.Key, ownerTypeAttributes);
            }
        }

        private static void SaveDataToFile()
        {
            writer.Flush();

            string text = stringWriter.GetStringBuilder().ToString();
            writer.Close();

            string path = data.exportPath;

            // NOTE: For now we change the extension to .txt because it won't be parsed properly yet anyway.
            path = Path.ChangeExtension(path, "txt");

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);

            AssetDatabase.Refresh();
        }
    }
}

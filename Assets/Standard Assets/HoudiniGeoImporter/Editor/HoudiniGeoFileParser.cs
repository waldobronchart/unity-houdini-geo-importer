﻿/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Houdini.GeoImporter
{
    public class HoudiniGeoParseException : System.Exception
    {
        public HoudiniGeoParseException(string message) : base(message)
        {

        }
    }

    public static class HoudiniGeoFileParser
    {
        public static HoudiniGeo Parse(string assetPath)
        {
            return ParseInternal(assetPath, null);
        }

        public static void ParseInto(string assetPath, HoudiniGeo existingGeo)
        {
            ParseInternal(assetPath, existingGeo);
        }

        private static HoudiniGeo ParseInternal(string assetPath, HoudiniGeo existingGeo=null)
        {
            if (!File.Exists(assetPath))
            {
                throw new FileNotFoundException("File not found: " + assetPath);
            }

            // Parse the json
            JToken mainToken = null;
            try
            {
                mainToken = JToken.Parse(File.ReadAllText(assetPath));
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("HoudiniGeoParseError: JSON in file '{0}' could not be parsed", assetPath));
                throw e;
            }
            
            // The houdini geo format expects the main element to be an array
            if (mainToken.Type != JTokenType.Array)
            {
                throw new HoudiniGeoParseException("Unexpected type in geo json.");
            }

            // The main element is an array that actually functions as a dictionary!
            Dictionary<string, JToken> geoDataDict = ArrayKeyValueToDictionary(mainToken.Children().ToArray());
            
            HoudiniGeo houdiniGeo = existingGeo;
            if (houdiniGeo == null)
            {
                houdiniGeo = ScriptableObject.CreateInstance<HoudiniGeo>();
            }
            houdiniGeo.sourceAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            
            houdiniGeo.fileVersion = geoDataDict["fileversion"].ValueSafe<string>();
            houdiniGeo.pointCount = geoDataDict["pointcount"].ValueSafe<int>();
            houdiniGeo.vertexCount = geoDataDict["vertexcount"].ValueSafe<int>();
            houdiniGeo.primCount = geoDataDict["primitivecount"].ValueSafe<int>();

            houdiniGeo.fileInfo = ParseFileInfo(geoDataDict["info"] as JObject);

            ParseTopology(houdiniGeo, geoDataDict["topology"]);
            ParseAttributes(houdiniGeo, geoDataDict["attributes"]);
            ParsePrimitives(houdiniGeo, geoDataDict["primitives"]);

            return houdiniGeo;
        }

        private static HoudiniGeoFileInfo ParseFileInfo(JObject infoValueToken)
        {
            //"info",{
            //	"software":"Houdini 13.0.665",
            //	"hostname":"waldos-mbp.home",
            //	"artist":"waldo",
            //	"timetocook":0.248844,
            //	"date":"2015-02-20 22:00:37",
            //	"time":0,
            //	"bounds":[-5.10615968704,5.16862106323,0,4.77225112915,-5.18210935593,5.08164167404],
            //	"primcount_summary":"     10,197 Polygons\n",
            //	"attribute_summary":"     1 point attributes:\tP\n"
            //},

            var fileInfo = new HoudiniGeoFileInfo();
            fileInfo.software = infoValueToken["software"].Value<string>();
            fileInfo.hostname = infoValueToken["hostname"].Value<string>();
            fileInfo.artist = infoValueToken["artist"].Value<string>();
            fileInfo.timeToCook = infoValueToken["timetocook"].Value<float>();
            fileInfo.date = infoValueToken["date"].Value<System.DateTime>();

            float[] bv = infoValueToken["bounds"].Values<float>().ToArray();
            Vector3 boundsMax = new Vector3(bv[0], bv[1], bv[2]);
            Vector3 boundsMin = new Vector3(bv[3], bv[4], bv[5]);
            fileInfo.bounds = new Bounds(boundsMax, Vector3.zero);
            fileInfo.bounds.Expand(boundsMin);

            fileInfo.primCountSummary = infoValueToken["primcount_summary"].Value<string>();
            fileInfo.attributeSummary = infoValueToken["attribute_summary"].Value<string>();

            return fileInfo;
        }

        private static void ParseTopology(HoudiniGeo geo, JToken topologyValueToken)
        {
            //"topology",[
            //   "pointref",[
            //   	"indices",[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18]
            //   ]
            //],

            Dictionary<string, JToken> topologyDict = ArrayKeyValueToDictionary(topologyValueToken.Children().ToArray());
            Dictionary<string, JToken> pointRefDict = ArrayKeyValueToDictionary(topologyDict["pointref"].Children().ToArray());

            geo.pointRefs = pointRefDict["indices"].Values<int>().ToArray();
        }

        private static Dictionary<string, HoudiniGeoAttributeOwner> ATTRIBUTES_TO_PARSE;

        private static void ParseAttributes(HoudiniGeo geo, JToken attributesValueToken)
        {
            // "attributes",[
            // 		"vertexattributes",[
            //			[(attribute obj)],
            //			[(attribute obj)],
            //			...
            // 		],
            // 		"pointattributes",[
            //			[(attribute obj)],
            //			[(attribute obj)],
            //			...
            // 		],
            //		...
            // ],

            if (ATTRIBUTES_TO_PARSE == null)
            {
                ATTRIBUTES_TO_PARSE = new Dictionary<string, HoudiniGeoAttributeOwner>();
                ATTRIBUTES_TO_PARSE.Add("vertexattributes", HoudiniGeoAttributeOwner.Vertex);
                ATTRIBUTES_TO_PARSE.Add("pointattributes", HoudiniGeoAttributeOwner.Point);
                ATTRIBUTES_TO_PARSE.Add("primitiveattributes", HoudiniGeoAttributeOwner.Primitive);
                ATTRIBUTES_TO_PARSE.Add("globalattributes", HoudiniGeoAttributeOwner.Detail);
            }

            Dictionary<string, JToken> attributeTokensDict = ArrayKeyValueToDictionary(attributesValueToken.Children().ToArray());

            // Parse each attribute group
            var geoAttributes = new List<HoudiniGeoAttribute>();
            foreach (var attrKeyVal in ATTRIBUTES_TO_PARSE)
            {
                string attrGroupKey = attrKeyVal.Key;
                HoudiniGeoAttributeOwner attrOwner = attrKeyVal.Value;

                JToken groupValueToken;
                if (attributeTokensDict.TryGetValue(attrGroupKey, out groupValueToken))
                {
                    // Parse each attribute in group
                    foreach (var attributeToken in groupValueToken.Children())
                    {
                        var attribute = ParseSingleAttribute(attributeToken, attrOwner);
                        if (attribute != null)
                        {
                            geoAttributes.Add(attribute);
                        }
                    }
                }
            }

            geo.attributes = geoAttributes.ToArray();
        }

        private static HoudiniGeoAttribute ParseSingleAttribute(JToken attrToken, HoudiniGeoAttributeOwner owner)
        {
            // NUMERIC
            // [
            // 		[
            // 			"scope","public",
            // 			"type","numeric",
            // 			"name","P",														<- Extract This
            // 			"options",{
            // 				"type":{
            // 					"type":"string",
            // 					"value":"hpoint"
            // 				}
            // 			}
            // 		],
            // 		[
            // 			"size",4,														<- Extract This
            // 			"storage","fpreal32",											<- Extract This
            // 			"defaults",[
            // 				"size",4,
            // 				"storage","fpreal64",
            // 				"values",[0,0,0,1]
            // 			],
            // 			"values",[
            // 				"size",4,
            // 				"storage","fpreal32",
            // 				"tuples",[[-0.5,-0.5,-0.5,1],[0.5,-0.5,-0.5,1],...]			<- Extract This
            // 			]
            // 		]
            // ]

            // STRING
            // [
            // 		[
            // 			"scope","public",
            // 			"type","string",
            // 			"name","varmap",
            // 			"options",{
            // 			}
            // 		],
            // 		[
            // 			"size",1,
            // 			"storage","int32",
            // 			"strings",["SHIT_INT -> SHIT_INT"],
            // 			"indices",[
            // 				"size",1,
            // 				"storage","int32",
            // 				"arrays",[[0]]
            // 			]
            // 		]
            // ]
            
            JToken[] childBlockTokens = attrToken.Children().ToArray();
            JToken headerToken = childBlockTokens[0];
            JToken bodyToken = childBlockTokens[1];
            
            var geoAttribute = new HoudiniGeoAttribute();
            geoAttribute.owner = owner;

            // Parse header block
            Dictionary<string, JToken> headerBlockDict = ArrayKeyValueToDictionary(headerToken.Children().ToArray());
            geoAttribute.name = headerBlockDict["name"].Value<string>();
            string valueType = headerBlockDict["type"].Value<string>();

            // Parse body block
            Dictionary<string, JToken> valuesBlockDict = ArrayKeyValueToDictionary(bodyToken.Children().ToArray());
            geoAttribute.tupleSize = valuesBlockDict["size"].Value<int>();

            // Parse Numeric types
            if (valueType == "numeric")
            {
                // Get storage type (float, int)
                string storageType = valuesBlockDict["storage"].Value<string>();
                geoAttribute.type = AttributeTypeStrToEnumValue(storageType);
                if (geoAttribute.type == HoudiniGeoAttributeType.Invalid)
                {
                    Debug.LogWarning("HoudiniGeoFileParser: unsuppored numeric storage type " + valueType);
                    return null;
                }

                // Get all values
                Dictionary<string, JToken> valuesDict = ArrayKeyValueToDictionary(valuesBlockDict["values"].Children().ToArray());
                if (geoAttribute.type == HoudiniGeoAttributeType.Float)
                {
                    int tupleSize = valuesDict["size"].Value<int>();
                    string valuesKey = (tupleSize == 1) ? "arrays" : "tuples";
                    geoAttribute.floatValues = valuesDict[valuesKey].Children().SelectMany(t => t.Values<float>()).ToArray();
                }
                else if (geoAttribute.type == HoudiniGeoAttributeType.Integer)
                {
                    geoAttribute.intValues = valuesDict["arrays"].Children().SelectMany(t => t.Values<int>()).ToArray();
                }
            }
            // Parse String types
            else if (valueType == "string")
            {
                geoAttribute.type = HoudiniGeoAttributeType.String;
                
                Dictionary<string, JToken> indicesDict = ArrayKeyValueToDictionary(valuesBlockDict["indices"].Children().ToArray());
                string[] stringValues = valuesBlockDict["strings"].Values<string>().ToArray();
                int[] indices = indicesDict["arrays"].Children().SelectMany(t => t.Values<int>()).ToArray();

                geoAttribute.stringValues = indices.Select(i => (i >= 0 && i < stringValues.Length) ? stringValues[i] : "").ToArray();
            }
            // Unexpected type?
            else
            {
                Debug.LogWarning("HoudiniGeoFileParser: unsuppored attribute valueType " + valueType);
                return null;
            }

            return geoAttribute;
        }

        
        
        private static void ParsePrimitives(HoudiniGeo geo, JToken primitivesValueToken)
        {
            // "primitives",[
            // 		[
            // 			[Header],
            // 			[Body]
            // 		],
            // 		[
            // 			[Header],
            // 			[Body]
            // 		],
            //		...
            // ],

            int primIdCounter = 0;

            var polyPrimitives = new List<PolyPrimitive>();
            var bezierCurvePrimitives = new List<BezierCurvePrimitive>();
            var nurbCurvePrimitives = new List<NURBCurvePrimitive>();

            foreach (var primitiveToken in primitivesValueToken.Children())
            {
                // Houdini 13
                // Primitive [[Header], [Body]]
                //[
                //	[
                //		"type","run",
                //		"runtype","Poly",           <- Options: Poly, Sphere, BezierCurve, NURBCurve
                //		"varyingfields",["vertex"],
                //		"uniformfields",{
                //		"closed":true
                //		}
                //	],
                //	[
                //		[[0,1,2,3]],
                //		[[4,5,6,7]],
                //		[[8,9,10,11]],
                //		[[12,13,14,15]],
                //		...
                //	]
                //]

                // Houdini 16
                // Primitive [[Header], [Body]]:
                // [
                // 	[
                // 		"type","Polygon_run"        <- Options: Polygon_run, PolygonCurve_run
                // 	],
                // 	[
                // 		"startvertex",0,
                // 		"nprimitives",12,
                // 		"nvertices_rle",[4,12]
                // 	]
                // ]
                
                JToken[] childBlockTokens = primitiveToken.Children().ToArray();
                JToken headerToken = childBlockTokens[0];
                JToken bodyToken = childBlockTokens[1];
                
                // Parse header
                Dictionary<string, JToken> headerDict = ArrayKeyValueToDictionary(headerToken.Children().ToArray());
                string type = headerDict["type"].Value<string>();

                switch (type)
                {
                    case "Polygon_run":
                    case "PolygonCurve_run":
                    {
                        polyPrimitives.AddRange(ParsePrimitivePolygonRun(bodyToken, ref primIdCounter));
                        break;
                    }

                    case "run":
                    {
                        string runType = headerDict["runtype"].Value<string>();
                        switch (runType)
                        {
                            case "Poly":
                                polyPrimitives.AddRange(ParsePolyPrimitiveGroup(headerDict, bodyToken, ref primIdCounter));
                                break;

                            case "Sphere":
                            case "BezierCurve":
                            case "NURBCurve":
                                break;
                        }
                        break;
                    }
                }
            }

            geo.polyPrimitives = polyPrimitives.ToArray();
            geo.bezierCurvePrimitives = bezierCurvePrimitives.ToArray();
            geo.nurbCurvePrimitives = nurbCurvePrimitives.ToArray();
        }

        private static PolyPrimitive[] ParsePrimitivePolygonRun(JToken bodyToken, ref int primIdCounter)
        {
            // RLE Compressed
            // bodyToken:
            // [
            // 	   "startvertex",0,
            // 	   "nprimitives",12,
            // 	   "nvertices_rle",[4,12,3,6]   <- this means: 12 prims with 4 verts each, then 6 prims with 3 verts each
            //                                    which translates to [[0, 1, 2, 3], [4, 5, 6, 7], [8, 9, 10, 11], ...]
            // 	]

            // Primitive Vert Count
            // bodyToken:
            // [
            //     "startvertex",0,
            //     "nprimitives",9,
            //     "nvertices",[4,8,26,18,24,20,26,22,26]      <- [prim with 4 verts, prim with 8 verts, ...]
            //                                                    which translates to [[0, 1, 2, 3], [4, 5, 6, 7, 8, 9, 10, 11], ...]
            // ]

            Dictionary<string, JToken> bodyDict = ArrayKeyValueToDictionary(bodyToken.Children().ToArray());

            int numPrimitives = bodyDict["nprimitives"].Value<int>();
            int vertexIdCounter = bodyDict["startvertex"].Value<int>();
            PolyPrimitive[] polyPrimitives = new PolyPrimitive[numPrimitives];

            if (bodyDict.ContainsKey("nvertices_rle"))
            {
                JEnumerable<JToken> numVerticesRLE = bodyDict["nvertices_rle"].Children();

                // Build array of primitives from RLE-compressed primitive description
                int primIndex = 0;
                var enumNumVerticesRLE = numVerticesRLE.GetEnumerator();
                while (enumNumVerticesRLE.MoveNext())
                {
                    int numVerticesInPrimitive = enumNumVerticesRLE.Current.Value<int>();
                    enumNumVerticesRLE.MoveNext();
                    int numRepeatPrimitive = enumNumVerticesRLE.Current.Value<int>();

                    for (int i = 0; i < numRepeatPrimitive; i++)
                    {
                        PolyPrimitive prim = new PolyPrimitive();
                        prim.id = primIdCounter++;
                        prim.indices = new int[numVerticesInPrimitive];
                        for (int v = 0; v < numVerticesInPrimitive; v++)
                            prim.indices[v] = vertexIdCounter++;
                        prim.triangles = TriangulateNGon(prim.indices);
                        polyPrimitives[primIndex++] = prim;
                    }
                }
            }
            else if (bodyDict.ContainsKey("nvertices"))
            {
                int primIndex = 0;
                JEnumerable<JToken> numVertices = bodyDict["nvertices"].Children();
                foreach (JToken vertCountToken in numVertices)
                {
                    int numVerticesInPrimitive = vertCountToken.Value<int>();

                    PolyPrimitive prim = new PolyPrimitive();
                    prim.id = primIdCounter++;
                    prim.indices = new int[numVerticesInPrimitive];
                    for (int v = 0; v < numVerticesInPrimitive; v++)
                        prim.indices[v] = vertexIdCounter++;
                    prim.triangles = TriangulateNGon(prim.indices);
                    polyPrimitives[primIndex++] = prim;
                }
            }

            return polyPrimitives;
        }

        private static PolyPrimitive[] ParsePolyPrimitiveGroup(Dictionary<string, JToken> headerDict, JToken bodyToken, ref int primIdCounter)
        {
            // bodyToken:
            // [
            //     [[0,1,2,3]],
            //     [[4,5,6,7]],
            //     [[8,9,10,11]],
            //     [[12,13,14,15]],
            //     ...
            //	]

            int primIdCounterInternal = primIdCounter;
            PolyPrimitive[] prims = bodyToken.Children().Select(primToken => {
                var primChildTokens = primToken.Children().ToArray();

                var prim = new PolyPrimitive();
                prim.id = primIdCounterInternal++;
                prim.indices = primChildTokens[0].Values<int>().ToArray();
                prim.triangles = TriangulateNGon(prim.indices);
                return prim;
            }).ToArray();

            primIdCounter = primIdCounterInternal;
            return prims;
        }

        private static int[] TriangulateNGon(int[] indices)
        {
            if (indices.Length <= 3)
            {
                return indices;
            }

            // Naive triangulation! Does not work for convex ngons
            int[] triangles = new int[(indices.Length - 2) * 3];
            for (int t=0, offset=1; offset<indices.Length-1; offset++)
            {
                triangles[t++] = indices[0];
                triangles[t++] = indices[offset];
                triangles[t++] = indices[offset+1];
            }
            return triangles;
        }







        private static T ValueSafe<T>(this JToken jToken)
        {
            try
            {
                return jToken.Value<T>();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                throw new HoudiniGeoParseException(string.Format("Expecting property value of type '{0}' but found '{1}' instead", 
                                                                 typeof(T).Name, jToken.Type));
            }
        }

        private static Dictionary<string, JToken> ArrayKeyValueToDictionary(JToken[] tokens)
        {
            var tokenDictionary = new Dictionary<string, JToken>();
            
            for (int i=0; i<tokens.Length; i+=2)
            {
                var keyToken = tokens[i];
                var valueToken = tokens[i+1];
                tokenDictionary.Add(keyToken.Value<string>(), valueToken);
            }
            
            return tokenDictionary;
        }

        private static HoudiniGeoAttributeType AttributeTypeStrToEnumValue(string typeStr)
        {
            switch (typeStr.ToLower())
            {
            case "int32":
                return HoudiniGeoAttributeType.Integer;
            case "fpreal32":
            case "fpreal64":
                return HoudiniGeoAttributeType.Float;
            case "string":
                return HoudiniGeoAttributeType.String;
            default:
                throw new HoudiniGeoParseException("Unexpected attribute type: " + typeStr);
            }
        }
    }
}

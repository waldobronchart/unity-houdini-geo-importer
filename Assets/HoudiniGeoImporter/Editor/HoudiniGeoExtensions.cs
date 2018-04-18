/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Houdini.GeoImporter
{
	public static class HoudiniGeoExtensions
	{
		internal static void ImportAllMeshes(this HoudiniGeo geo)
		{
			string geoAssetPath = AssetDatabase.GetAssetPath(geo);
			if (!File.Exists(geoAssetPath))
			{
				return;
			}

			// Convert to unity mesh and store mesh as sub asset
			if (geo.polyPrimitives.Length > 0)
			{
				var mesh = AssetDatabase.LoadAllAssetsAtPath(geoAssetPath).Where(a => a is Mesh).FirstOrDefault() as Mesh;
				if (mesh == null)
				{
					mesh = new Mesh();
					AssetDatabase.AddObjectToAsset(mesh, geoAssetPath);
				}
				
				geo.ToUnityMesh(mesh);
				EditorUtility.SetDirty(mesh);
			}
		}

		public static void ToUnityMesh(this HoudiniGeo geo, Mesh mesh)
		{
			if (geo.polyPrimitives.Length == 0)
			{
				Debug.LogError("Cannot convert HoudiniGeo to Mesh because geo has no PolyPrimitives");
				return;
			}

			mesh.name = geo.name;
			int[] indices = geo.polyPrimitives.SelectMany(p => p.indices).ToArray();
			int vertexCount = indices.Length;
			if (vertexCount > 65000)
			{
				throw new Exception(string.Format("Vertex count ({0}) exceeds limit of {1}!", geo.vertexCount, 65000));
			}

			// Check if position attribute P exists
			HoudiniGeoAttribute posAttr = null;
			if (!geo.TryGetAttribute(HoudiniGeo.POS_ATTR_NAME, HoudiniGeoAttributeType.Float, out posAttr))
			{
				Debug.LogWarning("HoudiniGEO has no Position attribute on points or vertices");
			}

			// Get Vertex/Point positions
			Vector3[] posAttrValues = null;
			posAttr.GetValues(out posAttrValues);
			
			// Get uv attribute values
			HoudiniGeoAttribute uvAttr = null;
			Vector2[] uvAttrValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.UV_ATTR_NAME, HoudiniGeoAttributeType.Float, out uvAttr))
			{
				uvAttr.GetValues(out uvAttrValues);
			}
			
			// Get uv2 attribute values
			HoudiniGeoAttribute uv2Attr = null;
			Vector2[] uv2AttrValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.UV2_ATTR_NAME, HoudiniGeoAttributeType.Float, out uv2Attr))
			{
				uv2Attr.GetValues(out uv2AttrValues);
			}
			
			// Get normal attribute values
			HoudiniGeoAttribute normalAttr = null;
			Vector3[] normalAttrValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.NORMAL_ATTR_NAME, HoudiniGeoAttributeType.Float, out normalAttr))
			{
				normalAttr.GetValues(out normalAttrValues);
			}
			
			// Get color attribute values
			HoudiniGeoAttribute colorAttr = null;
			Color[] colorAttrValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.COLOR_ATTR_NAME, HoudiniGeoAttributeType.Float, out colorAttr))
			{
				colorAttr.GetValues(out colorAttrValues);

				// Get alpha color values
				HoudiniGeoAttribute alphaAttr = null;
				float[] alphaAttrValues = null;
				if (geo.TryGetAttribute(HoudiniGeo.ALPHA_ATTR_NAME, HoudiniGeoAttributeType.Float, colorAttr.owner, out alphaAttr))
				{
					alphaAttr.GetValues(out alphaAttrValues);

					if (colorAttrValues.Length == alphaAttrValues.Length)
					{
						for (int i=0; i<colorAttrValues.Length; i++)
						{
							colorAttrValues[i].a = alphaAttrValues[i];
						}
					}
			    }
			}
			
			// Get tangent attribute values
			HoudiniGeoAttribute tangentAttr = null;
			Vector3[] tangentAttrValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.TANGENT_ATTR_NAME, HoudiniGeoAttributeType.Float, out tangentAttr))
			{
				tangentAttr.GetValues(out tangentAttrValues);
			}

			// Get material primitive attribute (Multiple materials result in multiple submeshes)
			HoudiniGeoAttribute materialAttr = null;
			string[] materialAttributeValues = null;
			if (geo.TryGetAttribute(HoudiniGeo.MATERIAL_ATTR_NAME, HoudiniGeoAttributeType.String, HoudiniGeoAttributeOwner.Primitive, out materialAttr))
			{
				materialAttr.GetValues(out materialAttributeValues);
			}

			// Create our mesh attribute buffers
			var submeshInfo = new Dictionary<string, List<int>>();
			var positions = new Vector3[vertexCount];
			var uvs = new Vector2[vertexCount]; // unity doesn't like it when meshes have no uvs
			var uvs2 = (uv2Attr != null) ? new Vector2[vertexCount] : null;
			var normals = (normalAttr != null) ? new Vector3[vertexCount] : null;
			var colors = (colorAttr != null) ? new Color[vertexCount] : null;
			var tangents = (tangentAttr != null) ? new Vector4[vertexCount] : null;

			// Fill the mesh buffers
			int[] vertToPoint = geo.pointRefs;
			Dictionary<int, int> vertIndexGlobalToLocal = new Dictionary<int, int>();
			for (int i=0; i<vertexCount; ++i)
			{
				int vertIndex = indices[i];
				int pointIndex = vertToPoint[vertIndex];
				vertIndexGlobalToLocal.Add(vertIndex, i);
				
				// Position
				switch (posAttr.owner)
				{
				case HoudiniGeoAttributeOwner.Vertex:
					positions[i] = posAttrValues[vertIndex];
					break;
				case HoudiniGeoAttributeOwner.Point:
					positions[i] = posAttrValues[pointIndex];
					break;
				}
				
				// UV1
				if (uvAttr != null)
				{
					switch (uvAttr.owner)
					{
					case HoudiniGeoAttributeOwner.Vertex:
						uvs[i] = uvAttrValues[vertIndex];
						break;
					case HoudiniGeoAttributeOwner.Point:
						uvs[i] = uvAttrValues[pointIndex];
						break;
					}
				}
				else
				{
					// Unity likes to complain when a mesh doesn't have any UVs so we'll just add a default
					uvs[i] = Vector2.zero;
				}
				
				// UV2
				if (uv2Attr != null)
				{
					switch (uv2Attr.owner)
					{
					case HoudiniGeoAttributeOwner.Vertex:
						uvs2[i] = uv2AttrValues[vertIndex];
						break;
					case HoudiniGeoAttributeOwner.Point:
						uvs2[i] = uv2AttrValues[pointIndex];
						break;
					}
				}
				
				// Normals
				if (normalAttr != null)
				{
					switch (normalAttr.owner)
					{
					case HoudiniGeoAttributeOwner.Vertex:
						normals[i] = normalAttrValues[vertIndex];
						break;
					case HoudiniGeoAttributeOwner.Point:
						normals[i] = normalAttrValues[pointIndex];
						break;
					}
				}
				
				// Colors
				if (colorAttr != null)
				{
					switch (colorAttr.owner)
					{
					case HoudiniGeoAttributeOwner.Vertex:
						colors[i] = colorAttrValues[vertIndex];
						break;
					case HoudiniGeoAttributeOwner.Point:
						colors[i] = colorAttrValues[pointIndex];
						break;
					}
				}
				
				// Fill tangents info
				if (tangentAttr != null)
				{
					switch (tangentAttr.owner)
					{
					case HoudiniGeoAttributeOwner.Vertex:
						tangents[i] = tangentAttrValues[vertIndex];
						break;
					case HoudiniGeoAttributeOwner.Point:
						tangents[i] = tangentAttrValues[pointIndex];
						break;
					}
				}
			}

			// Get primitive attribute values and created submeshes
			foreach (var polyPrim in geo.polyPrimitives)
			{
				// Normals
				if (normalAttr != null && normalAttr.owner == HoudiniGeoAttributeOwner.Primitive)
				{
					foreach (var vertIndex in polyPrim.indices)
					{
						int localVertIndex = vertIndexGlobalToLocal[vertIndex];
						normals[localVertIndex] = normalAttrValues[polyPrim.id];
					}
				}

				// Colors
				if (colorAttr != null && colorAttr.owner == HoudiniGeoAttributeOwner.Primitive)
				{
					foreach (var vertIndex in polyPrim.indices)
					{
						int localVertIndex = vertIndexGlobalToLocal[vertIndex];
						colors[localVertIndex] = colorAttrValues[polyPrim.id];
					}
				}

				// Add face to submesh based on material attribute
				var materialName = (materialAttr == null) ? HoudiniGeo.DEFAULT_MATERIAL_NAME : materialAttributeValues[polyPrim.id];
				if (!submeshInfo.ContainsKey(materialName))
				{
					submeshInfo.Add(materialName, new List<int>());
				}
				submeshInfo[materialName].AddRange(polyPrim.triangles);
			}

			// Assign buffers to mesh
			mesh.vertices = positions;
			mesh.subMeshCount = submeshInfo.Count;
			mesh.uv = uvs;
			mesh.uv2 = uvs2;
			mesh.normals = normals;
			mesh.colors = colors;
			mesh.tangents = tangents;
			
			// Set submesh indexbuffers
			int submeshIndex = 0;
			foreach (var item in submeshInfo)
			{
				// Skip empty submeshes
				if (item.Value.Count == 0)
					continue;
				
				// Set the indices for the submesh (Reversed by default because axis coordinates Z flipped)
				IEnumerable<int> submeshIndices = item.Value;
				if (!geo.importSettings.reverseWinding)
				{
					submeshIndices = submeshIndices.Reverse();
				}
				mesh.SetIndices(submeshIndices.ToArray(), MeshTopology.Triangles, submeshIndex);
				
				submeshIndex++;
			}

			// Calculate any missing buffers
			mesh.ConvertToUnityCoordinates();
			mesh.RecalculateBounds();
			if (normalAttr == null)
			{
				mesh.RecalculateNormals();
			}
		}

		private static void ConvertToUnityCoordinates(this Mesh mesh)
		{
			Vector3[] vertices = mesh.vertices;
			for (int i=0; i<vertices.Length; i++)
			{
				vertices[i].z *= -1;
			}
			mesh.vertices = vertices;

			Vector3[] normals = mesh.normals;
			for (int i=0; i<normals.Length; i++)
			{
				normals[i].z *= -1;
			}
			mesh.normals = normals;
			
			Vector4[] tangents = mesh.tangents;
			for (int i=0; i<tangents.Length; i++)
			{
				tangents[i].z *= -1;
			}
			mesh.tangents = tangents;
		}








		public static bool HasAttribute(this HoudiniGeo geo, string attrName, HoudiniGeoAttributeOwner owner)
		{
			if (owner == HoudiniGeoAttributeOwner.Any)
			{
				return geo.attributes.Any(a => a.name == attrName);
			}

			return geo.attributes.Any(a => a.owner == owner && a.name == attrName);
		}
		
		public static bool TryGetAttribute(this HoudiniGeo geo, string attrName, out HoudiniGeoAttribute attr)
		{
			attr = geo.attributes.FirstOrDefault(a => a.name == attrName);
			return (attr != null);
		}
		
		public static bool TryGetAttribute(this HoudiniGeo geo, string attrName, HoudiniGeoAttributeType type, out HoudiniGeoAttribute attr)
		{
			attr = geo.attributes.FirstOrDefault(a => a.type == type && a.name == attrName);
			return (attr != null);
		}

		public static bool TryGetAttribute(this HoudiniGeo geo, string attrName, HoudiniGeoAttributeOwner owner, out HoudiniGeoAttribute attr)
		{
			if (owner == HoudiniGeoAttributeOwner.Any)
			{
				attr = geo.attributes.FirstOrDefault(a => a.name == attrName);
			}
			else
			{
				attr = geo.attributes.FirstOrDefault(a => a.owner == owner && a.name == attrName);
			}

			return (attr != null);
		}
		
		public static bool TryGetAttribute(this HoudiniGeo geo, string attrName, HoudiniGeoAttributeType type,
		                                   HoudiniGeoAttributeOwner owner, out HoudiniGeoAttribute attr)
		{
			if (owner == HoudiniGeoAttributeOwner.Any)
			{
				attr = geo.attributes.FirstOrDefault(a => a.type == type && a.name == attrName);
			}
			else
			{
				attr = geo.attributes.FirstOrDefault(a => a.owner == owner && a.type == type && a.name == attrName);
			}
			return (attr != null);
		}







		private static void GetValues(this HoudiniGeoAttribute attr, out float[] values)
		{
			if (!attr.ValidateForGetValues<float>(HoudiniGeoAttributeType.Float, 1))
			{
				values = new float[0];
				return;
			}

			values = attr.floatValues;
		}
		
		private static void GetValues(this HoudiniGeoAttribute attr, out Vector2[] values)
		{
			if (!attr.ValidateForGetValues<Vector2>(HoudiniGeoAttributeType.Float, 2))
			{
				values = new Vector2[0];
				return;
			}
			
			// Convert to Vector2
			float[] rawValues = attr.floatValues;
			values = new Vector2[rawValues.Length / attr.tupleSize];
			for (int i=0; i<values.Length; i++)
			{
				values[i].x = rawValues[i * attr.tupleSize];
				values[i].y = rawValues[i * attr.tupleSize + 1];
			}
		}

		private static void GetValues(this HoudiniGeoAttribute attr, out Vector3[] values)
		{
			if (!attr.ValidateForGetValues<Vector3>(HoudiniGeoAttributeType.Float, 3))
			{
				values = new Vector3[0];
				return;
			}

			// Convert to Vector3
			float[] rawValues = attr.floatValues;
			values = new Vector3[rawValues.Length / attr.tupleSize];
			for (int i=0; i<values.Length; i++)
			{
				values[i].x = rawValues[i * attr.tupleSize];
				values[i].y = rawValues[i * attr.tupleSize + 1];
				values[i].z = rawValues[i * attr.tupleSize + 2];
			}
		}
		
		private static void GetValues(this HoudiniGeoAttribute attr, out Vector4[] values)
		{
			if (!attr.ValidateForGetValues<Vector4>(HoudiniGeoAttributeType.Float, 4))
			{
				values = new Vector4[0];
				return;
			}
			
			// Convert to Vector4
			float[] rawValues = attr.floatValues;
			values = new Vector4[rawValues.Length / attr.tupleSize];
			for (int i=0; i<values.Length; i++)
			{
				values[i].x = rawValues[i * attr.tupleSize];
				values[i].y = rawValues[i * attr.tupleSize + 1];
				values[i].z = rawValues[i * attr.tupleSize + 2];
				values[i].w = rawValues[i * attr.tupleSize + 3];
			}
		}
		
		private static void GetValues(this HoudiniGeoAttribute attr, out Color[] values)
		{
			if (!attr.ValidateForGetValues<Color>(HoudiniGeoAttributeType.Float, 3))
			{
				values = new Color[0];
				return;
			}
			
			// Convert to Color
			float[] rawValues = attr.floatValues;
			values = new Color[rawValues.Length / attr.tupleSize];
			for (int i=0; i<values.Length; i++)
			{
				values[i].r = rawValues[i * attr.tupleSize];
				values[i].g = rawValues[i * attr.tupleSize + 1];
				values[i].b = rawValues[i * attr.tupleSize + 2];
				values[i].a = 1;
				if (attr.tupleSize == 4)
				{
					values[i].a = rawValues[i * attr.tupleSize + 3];
				}
			}
		}

		private static void GetValues(this HoudiniGeoAttribute attr, out int[] values)
		{
			if (!attr.ValidateForGetValues<int>(HoudiniGeoAttributeType.Integer, 1))
			{
				values = new int[0];
				return;
			}
			
			values = attr.intValues;
		}
		
		private static void GetValues(this HoudiniGeoAttribute attr, out string[] values)
		{
			if (!attr.ValidateForGetValues<string>(HoudiniGeoAttributeType.String, 1))
			{
				values = new string[0];
				return;
			}
			
			values = attr.stringValues;
		}
		
		private static bool ValidateForGetValues<T>(this HoudiniGeoAttribute attr, HoudiniGeoAttributeType expectedType, 
		                                            int expectedMinTupleSize)
		{
			if (attr.type != expectedType)
			{
				Debug.LogError(string.Format("Cannot convert raw values of {0} attribute '{1}' to {2} (type: {3})", 
				                             attr.owner, attr.name, typeof(T).Name, attr.type));
				return false;
			}
			
			if (attr.tupleSize < expectedMinTupleSize)
			{
				Debug.LogError(string.Format("The tuple size of {0} attribute '{1}' too small for conversion to {2}",
				                             attr.owner, attr.name, typeof(T).Name));
				return false;
			}
			
			return true;
		}
	}
}
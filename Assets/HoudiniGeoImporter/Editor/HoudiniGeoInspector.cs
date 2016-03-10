using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Houdini.GeoImporter
{
	[CustomEditor(typeof(HoudiniGeo))]
	public class HoudiniGeoInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.DrawDefaultInspector();

			var houdiniGeo = target as HoudiniGeo;

			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Reimport Meshes"))
				{
					houdiniGeo.ImportAllMeshes();
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}
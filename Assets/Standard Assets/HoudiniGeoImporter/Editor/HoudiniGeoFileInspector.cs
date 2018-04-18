/**
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

namespace Houdini.GeoImporter
{
	[CustomEditor(typeof(UnityEngine.Object))]
	public class HoudiniGeoFileInspector : Editor
	{
		private bool isHoudiniGeoFile;
		private bool fileCheckPerformed;

		private HoudiniGeo houdiniGeo;
		private Editor houdiniGeoInspector;

		public override void OnInspectorGUI()
		{
			if (!isHoudiniGeoFile && !fileCheckPerformed)
			{
				string assetPath = AssetDatabase.GetAssetPath(target);
				isHoudiniGeoFile = (assetPath.EndsWith(".geo"));
				fileCheckPerformed = true;
			}
			else if (!isHoudiniGeoFile)
			{
				return;
			}

			if (houdiniGeo == null)
			{
				string assetPath = AssetDatabase.GetAssetPath(target);
				string outDir = Path.GetDirectoryName(assetPath);
				string assetName = Path.GetFileNameWithoutExtension(assetPath);

				// Parse geo
				string geoOutputPath = string.Format("{0}/{1}.asset", outDir, assetName);
				houdiniGeo = AssetDatabase.LoadAllAssetsAtPath(geoOutputPath).Where(a => a is HoudiniGeo).FirstOrDefault() as HoudiniGeo;
				houdiniGeoInspector = Editor.CreateEditor(houdiniGeo);
			}

			if (houdiniGeoInspector != null)
			{
				GUI.enabled = true;
				houdiniGeoInspector.DrawDefaultInspector();
			}
		}
	}
}
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
    public class HoudiniGeoAssetPostProcessor : AssetPostprocessor
    {
        private static bool IsHoudiniGeoFile(string path)
        {
            return path.ToLower().EndsWith(".geo");
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string[] houdiniGeosImported = importedAssets.Where(p => IsHoudiniGeoFile(p)).ToArray();

            foreach (var assetPath in houdiniGeosImported)
            {
                //Debug.Log("Importing: " + assetPath);

                string outDir = Path.GetDirectoryName(assetPath);
                string assetName = Path.GetFileNameWithoutExtension(assetPath);

                // Parse geo
                var geoOutputPath = string.Format("{0}/{1}.asset", outDir, assetName);
                var houdiniGeo = AssetDatabase.LoadAllAssetsAtPath(geoOutputPath).Where(a => a is HoudiniGeo).FirstOrDefault() as HoudiniGeo;
                if (houdiniGeo == null)
                {
                    houdiniGeo = ScriptableObject.CreateInstance<HoudiniGeo>();
                    AssetDatabase.CreateAsset(houdiniGeo, geoOutputPath);
                }

                HoudiniGeoFileParser.ParseInto(assetPath, houdiniGeo);

                houdiniGeo.ImportAllMeshes();

                EditorUtility.SetDirty(houdiniGeo);
            }

            if (houdiniGeosImported.Length > 0)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}

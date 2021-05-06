/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using UnityEngine;
using UnityEditor;

namespace Houdini.GeoImporter
{
    [CustomEditor(typeof(HoudiniGeo))]
    public class HoudiniGeoInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            HoudiniGeo houdiniGeo = target as HoudiniGeo;

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

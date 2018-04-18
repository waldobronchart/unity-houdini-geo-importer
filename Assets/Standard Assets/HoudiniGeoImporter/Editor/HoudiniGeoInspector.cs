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

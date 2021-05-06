/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System.IO;
using UnityEngine;
using UnityEditor;

namespace Houdini.GeoImporter
{
    [CustomEditor(typeof(HoudiniGeo))]
    public class HoudiniGeoInspector : Editor
    {
        private SerializedProperty exportPathProperty;

        private void OnEnable()
        {
            exportPathProperty = serializedObject.FindProperty("exportPath");
        }
        
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
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            DrawExportSection();
        }

        private void DrawExportSection()
        {
            serializedObject.Update();

            // Nicely format the export related field and buttons.
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(exportPathProperty.stringValue) &&
                          exportPathProperty.stringValue.EndsWith("." + HoudiniGeo.EXTENSION);
            bool pressedExport = GUILayout.Button("Export", GUILayout.Width(75));
            GUI.enabled = true;
                
            EditorGUILayout.PropertyField(exportPathProperty, GUIContent.none);
                
            bool pressedPick = GUILayout.Button("...", GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            
            if (pressedExport)
                Debug.Log("TODO: Actually export the .geo file");

            // Show a nice dialog for picking a directory.
            if (pressedPick)
            {
                string directory, fileName;
                if (string.IsNullOrEmpty(exportPathProperty.stringValue))
                {
                    directory = Application.dataPath;
                    fileName = "Geometry";
                }
                else
                {
                    directory = Path.GetDirectoryName(exportPathProperty.stringValue);
                    fileName = Path.GetFileName(exportPathProperty.stringValue);
                }

                exportPathProperty.stringValue = EditorUtility.SaveFilePanel(
                    "GEO File to Export", directory, fileName, HoudiniGeo.EXTENSION);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

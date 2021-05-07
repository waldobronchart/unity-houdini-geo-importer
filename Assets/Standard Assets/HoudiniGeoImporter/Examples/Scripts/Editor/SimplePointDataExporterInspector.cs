/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using UnityEditor;
using UnityEngine;

namespace Houdini.GeoImporter
{
    [CustomEditor(typeof(SimplePointDataExporter))]
    public class SimplePointDataExporterInspector : Editor
    {
        private SimplePointDataExporter simplePointDataExporter;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            simplePointDataExporter = (SimplePointDataExporter)target;
            
            EditorGUILayout.Space();

            bool export = GUILayout.Button("Export", GUILayout.Height(40));
            if (export)
                Export();
        }
        
        public void Export()
        {
            PointCollection<PointData> points = new PointCollection<PointData>();
            
            // Create a point for every child.
            Transform[] transforms = simplePointDataExporter.GetComponentsInChildren<Transform>();
            for (int i = 1; i < transforms.Length; i++)
            {
                points.Add(new PointData(transforms[i].position));
            }
            
            points.ExportToGeoFile("Simple Point Data");
        }
    }
}

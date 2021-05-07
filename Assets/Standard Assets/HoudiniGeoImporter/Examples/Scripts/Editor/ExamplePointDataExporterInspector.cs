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
    /// <summary>
    /// Draws an export button and does the actual exporting of the data (exporting is editor code).
    /// </summary>
    [CustomEditor(typeof(ExamplePointDataExporter))]
    public class ExamplePointDataExporterInspector : Editor
    {
        private ExamplePointDataExporter examplePointDataExporter;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            examplePointDataExporter = (ExamplePointDataExporter)target;
            
            EditorGUILayout.Space();

            bool export = GUILayout.Button("Export", GUILayout.Height(40));
            if (export)
                Export();
        }

        private void Export()
        {
            PointCollection<ExamplePointDataExporter.ExamplePointData> points = examplePointDataExporter.GetPoints();

            points.ExportToGeoFile("Example Point Data");
        }
    }
}

/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using UnityEngine;

namespace Houdini.GeoImportExport
{
    /// <summary>
    /// Example class for how to export basic point data. Has an editor class that does the actual exporting.
    /// </summary>
    public class ExamplePointDataExporter : MonoBehaviour
    {
        public class ExamplePointData : PointData
        {
            public string name;
            public int index;

            public ExamplePointData(Vector3 p, string name, int index) : base(p)
            {
                this.name = name;
                this.index = index;
            }

            public override string ToString()
            {
                return $"{nameof(P)}: {P}, {nameof(name)}: {name}, {nameof(index)}: {index}";
            }
        }

        public PointCollection<ExamplePointData> GetPoints()
        {
            PointCollection<ExamplePointData> points = new PointCollection<ExamplePointData>();
            
            // Create a point for every child.
            Transform[] transforms = GetComponentsInChildren<Transform>();
            for (int i = 1; i < transforms.Length; i++)
            {
                points.Add(
                    new ExamplePointData(transforms[i].position, transforms[i].name, transforms[i].GetSiblingIndex()));
            }

            return points;
        }
    }
}

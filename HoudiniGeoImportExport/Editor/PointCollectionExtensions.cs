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

namespace Houdini.GeoImportExport
{
    public static class PointCollectionExtensions
    {
        public static void ExportToGeoFile<PointType>(this PointCollection<PointType> pointCollection, string path)
            where PointType : PointData
        {
            // Check if the filename is valid.
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(Path.GetFileName(path)))
            {
                Debug.LogWarning($"Tried to export PointCollection<{typeof(PointType).Name}> to invalid path: '{path}'");
                return;
            }

            // If a relative path is specified, make it an absolute path in the Assets folder.
            if (string.IsNullOrEmpty(Path.GetDirectoryName(path)) || !Path.IsPathRooted(path))
                path = Path.Combine(Application.dataPath, path);

            // Make sure it ends with the Houdini extension.
            path = Path.ChangeExtension(path, HoudiniGeo.EXTENSION);

            // Clean up the path a little.
            path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            

            HoudiniGeo houdiniGeo = HoudiniGeo.Create();
            
            houdiniGeo.SetPoints(pointCollection);
            
            Debug.Log($"Export GEO file '{path}'");
            houdiniGeo.Export(path);
        }
    }
}

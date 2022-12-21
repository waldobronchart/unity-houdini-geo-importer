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
        public static void ExportToGeoFile<PointType>(this PointCollection<PointType> pointCollection,
            string path, bool translateCoordinateSystems = true)
            where PointType : PointData
        {
            HoudiniGeo houdiniGeo = HoudiniGeo.Create();
            ExportToGeoFile(pointCollection, houdiniGeo, path, translateCoordinateSystems);
        }
        
        public static void ExportToGeoFile<PointType>(this PointCollection<PointType> pointCollection,
            HoudiniGeo houdiniGeo, string path, bool translateCoordinateSystems = true)
            where PointType : PointData
        {
            houdiniGeo.AddPoints(pointCollection, translateCoordinateSystems);

            Debug.Log($"Exporting point collection to GEO file '{path}'");
            houdiniGeo.Export(path);
        }
    }
}

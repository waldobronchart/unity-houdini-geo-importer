/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
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
    public static class SplineCollectionExtensions
    {
        public static void ExportToGeoFile<SplineType, PointType>(
            this SplineCollection<SplineType> splineCollection, string path, bool translateCoordinateSystems = true)
            where SplineType : SplineData<PointType>
            where PointType : PointData
        {
            HoudiniGeo houdiniGeo = HoudiniGeo.Create();
            ExportToGeoFile<SplineType, PointType>(splineCollection, houdiniGeo, path, translateCoordinateSystems);
        }
        
        public static void ExportToGeoFile<SplineType, PointType>(
            this SplineCollection<SplineType> splineCollection, HoudiniGeo houdiniGeo, string path, bool translateCoordinateSystems = true)
            where SplineType : SplineData<PointType>
            where PointType : PointData
        {
            houdiniGeo.AddSplines<SplineType, PointType>(splineCollection, translateCoordinateSystems);

            Debug.Log($"Export spline collection to GEO file '{path}'");
            houdiniGeo.Export(path);
        }
    }
}

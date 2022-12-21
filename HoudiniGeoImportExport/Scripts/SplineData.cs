/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System;
using System.Collections.Generic;

namespace Houdini.GeoImportExport
{
    public abstract class SplineDataBase
    {
    }
    
    [Serializable]
    public class SplineData<PointType> : SplineDataBase
        where PointType : PointData
    {
        public List<PointType> points = new List<PointType>();

        public SplineData()
        {
        }
        
        public SplineData(List<PointType> points)
        {
            this.points.AddRange(points);
        }
    }
}

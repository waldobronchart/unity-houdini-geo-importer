/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System;
using UnityEngine;

namespace Houdini.GeoImportExport
{
    [Serializable]
    public class PointData
    {
        public Vector3 P;

        public PointData()
        {
        }

        public PointData(Vector3 p)
        {
            P = p;
        }
    }
}

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
    public static class Units
    {
        private const float UNITY_TO_HOUDINI_SCALE = 100;
        
        public static Vector3 ToUnityPosition(Vector3 houdiniPosition)
        {
            return new Vector3(-houdiniPosition.x, houdiniPosition.y, houdiniPosition.z) / UNITY_TO_HOUDINI_SCALE;
        }
        
        public static Vector3 ToHoudiniPosition(Vector3 unityPosition)
        {
            return new Vector3(-unityPosition.x, unityPosition.y, unityPosition.z) * UNITY_TO_HOUDINI_SCALE;
        }
        
        public static float ToUnityDistance(float houdiniDistance)
        {
            return houdiniDistance / UNITY_TO_HOUDINI_SCALE;
        }
        
        public static float ToHoudiniDistance(float unityDistance)
        {
            return unityDistance * UNITY_TO_HOUDINI_SCALE;
        }

        public static Vector3 ToUnityDirection(Vector3 houdiniDirection)
        {
            return new Vector3(-houdiniDirection.x, houdiniDirection.y, houdiniDirection.z).normalized;
        }
        
        public static Vector3 ToHoudiniDirection(Vector3 unityDirection)
        {
            return new Vector3(-unityDirection.x, unityDirection.y, unityDirection.z).normalized;
        }
    }
}

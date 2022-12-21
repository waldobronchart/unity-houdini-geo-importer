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
    public static class HoudiniGeoAttributeExtensions
    {
        public static void AddValueAsTuples(
            this HoudiniGeoAttribute attribute, object value, bool translateCoordinateSystems)
        {
            string name = attribute.name;

            // If specified, automatically translate the position to Houdini's format.
            if (translateCoordinateSystems && name == HoudiniGeoExtensions.PositionAttributeName)
            {
                Vector3 p = Units.ToHoudiniPosition((Vector3)value);
                value = p;
            }

            // If specified, automatically translate the direction to Houdini's format.
            else if (translateCoordinateSystems && (name == HoudiniGeoExtensions.NormalAttributeName ||
                                                    name == HoudiniGeoExtensions.UpAttributeName))
            {
                Vector3 n = Units.ToHoudiniDirection((Vector3)value);
                value = n;
            }

            // If specified, automatically translate the rotation to Houdini's format.
            else if (translateCoordinateSystems && name == HoudiniGeoExtensions.RotationAttributeName)
            {
                Quaternion orient = Units.ToHoudiniRotation((Quaternion)value);
                value = orient;
            }

            switch (value)
            {
                case bool b:
                    attribute.intValues.Add(b ? 1 : 0);
                    break;
                case float f:
                    attribute.floatValues.Add(f);
                    break;
                case int i:
                    attribute.intValues.Add(i);
                    break;
                case string s:
                    attribute.stringValues.Add(s);
                    break;
                case Vector2 vector2:
                    attribute.floatValues.Add(vector2.x);
                    attribute.floatValues.Add(vector2.y);
                    break;
                case Vector3 vector3:
                    attribute.floatValues.Add(vector3.x);
                    attribute.floatValues.Add(vector3.y);
                    attribute.floatValues.Add(vector3.z);
                    break;
                case Vector4 vector4:
                    attribute.floatValues.Add(vector4.x);
                    attribute.floatValues.Add(vector4.y);
                    attribute.floatValues.Add(vector4.z);
                    attribute.floatValues.Add(vector4.w);
                    break;
                case Vector2Int vector2Int:
                    attribute.floatValues.Add(vector2Int.x);
                    attribute.floatValues.Add(vector2Int.y);
                    break;
                case Vector3Int vector3Int:
                    attribute.floatValues.Add(vector3Int.x);
                    attribute.floatValues.Add(vector3Int.y);
                    break;
                case Quaternion quaternion:
                    attribute.floatValues.Add(quaternion.x);
                    attribute.floatValues.Add(quaternion.y);
                    attribute.floatValues.Add(quaternion.z);
                    attribute.floatValues.Add(quaternion.w);
                    break;
                case Color color:
                    attribute.floatValues.Add(color.r);
                    attribute.floatValues.Add(color.g);
                    attribute.floatValues.Add(color.b);
                    break;
            }
        }
    }
}

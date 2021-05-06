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

namespace Houdini.GeoImporter
{
    [Serializable]
    public class HoudiniGeoFileInfo
    {
        public string software;
        public string hostname;
        public string artist;
        public float timeToCook;
        public DateTime date;
        public Bounds bounds;
        public string primCountSummary;
        public string attributeSummary;
    }

    public enum HoudiniGeoAttributeType
    {
        Invalid = 0,
        Float,
        Integer,
        String,
    }

    public enum HoudiniGeoAttributeOwner
    {
        Invalid = 0,
        Vertex,
        Point,
        Primitive,
        Detail,
        Any,
    }

    [Serializable]
    public class HoudiniGeoAttribute
    {
        public string name;
        public HoudiniGeoAttributeType type;
        public HoudiniGeoAttributeOwner owner;
        public int tupleSize;

        public float[] floatValues;
        public int[] intValues;
        public string[] stringValues;
    }



    [Serializable]
    public abstract class HoudiniGeoPrimitive
    {
        public string type;
        public int id;
    }
    
    [Serializable]
    public class PolyPrimitive : HoudiniGeoPrimitive
    {
        public int[] indices;
        public int[] triangles;

        public PolyPrimitive()
        {
            type = "Poly";
        }
    }
    
    [Serializable]
    public class BezierCurvePrimitive : HoudiniGeoPrimitive
    {
        public int[] indices;
        public int order;
        public int[] knots;
        
        public BezierCurvePrimitive()
        {
            type = "BezierCurve";
        }
    }
    
    [Serializable]
    public class NURBCurvePrimitive : HoudiniGeoPrimitive
    {
        public int[] indices;
        public int order;
        public bool endInterpolation;
        public int[] knots;
        
        public NURBCurvePrimitive()
        {
            type = "NURBCurve";
        }
    }






    public enum HoudiniGeoGroupType
    {
        Invalid = 0,
        Points,
        Primitives,
        Edges,
    }

    public class HoudiniGeoGroup
    {
        public string name;
        public HoudiniGeoGroupType type;
    }

    public class PrimitiveGroup : HoudiniGeoGroup
    {
        public int[] ids;
    }
    
    public class PointGroup : HoudiniGeoGroup
    {
        public int[] ids;
        public int[] vertIds;
    }
    
    public class EdgeGroup : HoudiniGeoGroup
    {
        public int[][] pointPairs;
    }

    [Serializable]
    public class HoudiniGeoImportSettings
    {
        public bool reverseWinding;
    }

    public class HoudiniGeo : ScriptableObject
    {
        public const string EXTENSION = "geo";
        
        public const string POS_ATTR_NAME = "P";
        public const string NORMAL_ATTR_NAME = "N";
        public const string COLOR_ATTR_NAME = "Cd";
        public const string ALPHA_ATTR_NAME = "Alpha";
        public const string UV_ATTR_NAME = "uv";
        public const string UV2_ATTR_NAME = "uv2";
        public const string TANGENT_ATTR_NAME = "tangent";
        public const string MATERIAL_ATTR_NAME = "shop_materialpath";
        public const string DEFAULT_MATERIAL_NAME = "Default";

        public UnityEngine.Object sourceAsset;

        public HoudiniGeoImportSettings importSettings;

        public string fileVersion;
        public bool hasIndex;
        public int pointCount;
        public int vertexCount;
        public int primCount;
        public HoudiniGeoFileInfo fileInfo;

        public int[] pointRefs;
            
        public HoudiniGeoAttribute[] attributes;

        public PolyPrimitive[] polyPrimitives;
        public BezierCurvePrimitive[] bezierCurvePrimitives;
        public NURBCurvePrimitive[] nurbCurvePrimitives;

        public PrimitiveGroup[] primitiveGroups;
        public PointGroup[] pointGroups;
        public EdgeGroup[] edgeGroups;

        [HideInInspector] public string exportPath;
    }
}

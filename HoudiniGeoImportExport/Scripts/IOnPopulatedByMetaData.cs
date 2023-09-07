using Houdini.GeoImportExport;

namespace RoyTheunissen.UnityHoudiniGEOImportExport
{
    /// <summary>
    /// If implemented, the script will receive a callback when it's on a prefab and it was just instantiated on a point
    /// as specified by the metadata. This is useful for things like initializing components in a special way.
    /// For example: maybe the scale of a point should not be applied to the transform but to the Shape module of
    /// a particle system. This callback allows you to interpret such metadata in a custom way.
    /// </summary>
    public interface IOnPopulatedByMetaData
    {
        void OnPopulatedByMetaData(IPointDataPopulatable pointData);
    }
}

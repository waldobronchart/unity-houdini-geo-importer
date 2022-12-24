using UnityEngine;

namespace Houdini.GeoImportExport
{
    /// <summary>
    /// Interface to allow point data to expose the necessary fields to have prefabs populated on them.
    /// </summary>
    public interface IPointDataPopulatable
    {
        Vector3 P { get; }
        float pscale { get; }
        Vector3 scale { get; }
        Quaternion orient { get; }
        string name { get; }
    }
}

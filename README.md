# Unity Houdini Geo Importer/Exporter

Based on [Waldo Bronchart's](https://github.com/waldobronchart) [unity-houdini-geo-importer](https://github.com/waldobronchart/unity-houdini-geo-importer)

Imports and exports _.geo_ files. Quickly and conveniently passes points with attributes between Unity and Houdini.

Dependencies: `JsonDotNet`

## Overview

Editor Scripts
```
HoudiniGeoAssetPostProcessor.cs   listens for .geo file changes
HoudiniGeoExtensions.cs           editor-only HoudiniGeo helper methods
HoudiniGeoFileInspector.cs        custom inspector for .geo files
HoudiniGeoFileParser.cs           .geo file json parser
HoudiniGeoFileExporter.cs         .geo file json exporter
HoudiniGeoInspector.cs            HoudiniGeo asset inspector
```

Runtime Scripts
```
HoudiniGeo.cs                     custom unity asset to represent .geo parsed file contents
ExamplePointDataExporter.cs       Example of how to quickly export points with attributes to a .geo file
```

## Example

### Creating a Point Data Class

You can serialize any class inheriting from `PointData`. Define a field per attribute. Any public field or private field annotated with `[SerializeField]` will be saved. Supported field types are `float`, `int`, `string`, `Vector2`, `Vector3`, `Vector4`, `Vector2Int`, `Vector3Int` and `Color`.
```c#
public class ExamplePointData : PointData
{
    public string name;
    public int index;

    public ExamplePointData(Vector3 p, string name, int index) : base(p)
    {
        this.name = name;
        this.index = index;
    }
}
```

### Saving Point Data to a .GEO file

Add your `PointData` objects to a `PointDataCollection` (basically just a List) and call `ExportToGeoFile(string path)` on it.

### Reading Point Data from a .GEO file

If you have a reference to a `HoudiniGeo` object, you can call `GetPoints<PointType>()` on it and get a `PointCollection<PointType>` from it.

### Suggested Uses

- Lightweight solution for sharing custom scene metadata with Houdini.
  - e.g. telling it where hand-placed objects are so procedural effects like grass can avoid it.
- Lightweight solution for exporting point data to Unity and instantiating Unity prefabs there. Procedural placement without generating any new geometry assets!
  - e.g. generating points where you want to place trees, but placing them as prefabs so they have re-usable meshes that can be GPU instanced.

## To Do
- Exporting primitives is not yet implemented. Shouldn't be too hard to add but I'm not going to be using it!
- The importer doesn't distinguish between 32-bit and 64-bit yet so for now floating points are always exported as 64-bit.
- GEO files seem to have a lot of duplication and little quirks that I couldn't find any documentation on. I replicated the behaviour best I could and marked anything I found odd with a comment. If you have a non-standard use case and something isn't exporting quite right, have a look at `HoudiniGeoFileExporter.cs`!

## Contact
[Waldo Bronchart](https://waldobronchart.com) (importer)

[wbronchart@gmail.com](mailto:wbronchart@gmail.com)

---

[Roy Theunissen](https://roytheunissen.com) (exporter and `PointData`/`PointDataCollection` functionality)

[roy.theunissen@live.nl](mailto:roy.theunissen@live.nl)

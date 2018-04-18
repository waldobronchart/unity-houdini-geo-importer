# Unity Houdini Geo File Importer

This unity plugin imports the text based SideFX Houdini `.geo` file format and creates unity meshes based on its attributes.

Dependencies: `Newtonsoft.Json`

## Overview

Editor Scripts
```
HoudiniGeoAssetPostProcessor.cs   listens for .geo file changes
HoudiniGeoExtensions.cs           editor-only HoudiniGeo helper methods
HoudiniGeoFileInspector.cs        custom inspector for .geo files
HoudiniGeoFileParser.cs           .geo file json parser
HoudiniGeoInspector.cs            HoudiniGeo asset inspector
```

Runtime Scripts
```
HoudiniGeo.cs                     custom unity asset to represent .geo parsed file contents
```

## Limitations

- Only polygonal meshes are supported for now
- Triangulation: Naive triangulation is done on import for primitives with more than 3 vertices. Most likely this will not produce the desired result, so it is recommended to do triangulation in Houdini before exporting to the .geo file.

## Common issues

**My mesh is flipped**
Click on the generated .asset file, then check "Reverse Winding" in the inspector under "Import Settings"

## Example

Example of a complex mesh imported with this plugin

![Imgur](http://i.imgur.com/kQBBy6f.png)

# Unity Houdini Geo File Importer

This unity plugin imports text based Houdini .geo file format and creates unity meshes based on its attributes.

Dependencies: `JsonDotNet`

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

## Example

Example of a complex mesh imported with this plugin

![Imgur](http://i.imgur.com/kQBBy6f.png)

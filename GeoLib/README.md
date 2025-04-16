# GEOLib

GEOLib reads TRUMPF's .GEO files into a usable format for C#

The repository is available [here](https://github.com/sharptech-software/trumpf-geo-lib).
## What you're probably here for

```csharp
Drawing geoDrawing = Drawing.FromFile("path/to/file.geo"); // open geo file

Console.WriteLine( geoDrawing.ToSVG() ); // prints as SVG
```

## Additional features from 3.0+
You can now customize a few properties for SVG conversion.  
```csharp
Drawing geoDrawing = Drawing.FromFile("path/to/file.geo");

geoDrawing.StrokeColor = "#fff"; // use white where unspecified
geoDrawing.FillColor   = null;   // don't fill
geoDrawing.Responsive  = false;  // disable responsive SVG

Console.WriteLine( geoDrawing.ToSVG() );
```
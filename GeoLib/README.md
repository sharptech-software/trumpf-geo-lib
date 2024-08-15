# GEOLib

GEOLib reads TRUMPF's .GEO files into a usable format for C#
It can also convert them to SVGs!

The repository is available [here](https://github.com/sharptech-software/trumpf-geo-lib).
## What you're probably here for

```csharp
Drawing geoDrawing = Drawing.FromFile("path/to/file.geo"); // open geo file

Console.WriteLine( geoDrawing.ToSVG() ); // prints as SVG
```
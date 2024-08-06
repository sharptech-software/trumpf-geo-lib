using Fasteroid;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile(@"./Sleuthing/GeoTests.GEO");

var svg = test.ToSVG();
File.WriteAllText(Path.GetFullPath("../../../GeoTests.svg"), svg.ToString());

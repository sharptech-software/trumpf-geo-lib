using SharpTech;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile(@"./Sleuthing/large-arc-test.geo");


var svg = test.ToSVG();
File.WriteAllText(Path.GetFullPath("../../../large-arc-test.svg"), svg.ToString());

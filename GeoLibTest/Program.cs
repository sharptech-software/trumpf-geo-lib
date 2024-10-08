﻿using SharpTech;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile(@"./Sleuthing/GeoTestsTextBoundingBoxes.GEO");

var test_text = test.Entities.Where( (e => e is GEOLib.Text) )
                             .Cast<GEOLib.Text>()
                             .ToList();

var svg = test.ToSVG();
File.WriteAllText(Path.GetFullPath("../../../GeoTests.svg"), svg.ToString());

using Fasteroid;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile( @"./Example01.GEO" );

Debug.WriteLine(test);

Console.WriteLine("loaded the geo");
using Fasteroid;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile( @"./GeoTests.GEO" );

var experiment = test.Entities
    .Where(e => e is GEOLib.Line)
    .Select(e => (e as GEOLib.Line)! )
    .Where(e => Math.Floor(e.Start.X) == 1.0 )
    .Select(e => (e.Start.X, e.Color) )
    .ToList();

Console.WriteLine("experiment ready!");

foreach (var line in experiment) {
    Debug.WriteLine(line);
}

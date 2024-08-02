using Fasteroid;
using System.Diagnostics;

var test = await GEOLib.Drawing.FromFile( @"./Sleuthing/GeoTests.GEO" );

var experiment = test.Entities
    .Where(e => e.Att != null);


Console.WriteLine("experiment ready!");

foreach (var line in experiment) {
    Debug.WriteLine(line);
}

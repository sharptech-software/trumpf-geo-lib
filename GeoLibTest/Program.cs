using Fasteroid.GEOLib;

var test = await GEOLoader.Load( @"your geo here" );

Console.WriteLine("loaded the geo");

foreach (var block in test) {
    Console.WriteLine( block.Key );
    foreach (var entity in block.Value) {
        Console.WriteLine( string.Join( " ", entity ) );
    }
}

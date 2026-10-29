using HitMe.Server.Golden;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: GoldenVectorGenerator <rounds.json path>");
    return 2;
}

var output = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
var file = GoldenRoundsFactory.Create();
GoldenRoundsCodec.Write(output, file);
Console.WriteLine($"Wrote {file.Rounds.Length} golden rounds to {output}");
return 0;

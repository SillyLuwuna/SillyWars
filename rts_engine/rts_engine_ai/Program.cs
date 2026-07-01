using RtsEngine.Map;
using RtsEngine.Units;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;

using System.CommandLine;
using RtsEngine.AI;

namespace RtsEngine
{

public static class Program
{
	public static void Run(ParseResult result)
	{
		string? map = result.GetValue<string>("--map");
		string? network = result.GetValue<string>("--network");
		int? games = result.GetValue<int>("--games");
		if (map == null)
		{
			Console.WriteLine("Please provide a map with the flag --map.");
			return;
		}
		if (games == null || games == 0)
		{
			Console.WriteLine("Please provide the number of training games with the flag --games.");
			return;
		}

		WorldState state;
		try
		{
			state = WorldState.Load(map);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error opening map \"{map}\": {ex.Message}");
			return;
		}

		Trainer trainer;
		var oldOut = Console.Out;
		try
		{
			Console.SetOut(TextWriter.Null);
			if (network == null)
			{
				trainer = new Trainer(state);
			}
			else
			{
				trainer = new Trainer(state, network);
			}
		}
		finally
		{
			Console.SetOut(oldOut);
		}

		trainer.RunGames(games.Value);
	}

	public static async Task Main(string[] args)
	{
		RootCommand rootCommand = new RootCommand("RtsEngine runner");

		rootCommand.Add(new Option<string>("--map"));
		rootCommand.Add(new Option<string>("--network"));
		rootCommand.Add(new Option<int>("--games"));

		rootCommand.SetAction(Run);
		rootCommand.Parse(args).Invoke();
	}
}

}

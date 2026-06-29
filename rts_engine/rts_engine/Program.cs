using RtsEngine.Map;
using RtsEngine.Units;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Structures;

using System.CommandLine;

namespace RtsEngine
{

public static class Program
{
	public static Grid<Cell> GenerateMap()
	{
		Grid<Cell> grid = new Grid<Cell>(new Vec2(0, 0), 1, 50, 30);

		for (int x = 0; x < grid.Width; x++)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				if (x < 10 || x >= 40)
				{
					grid[x, y] = new Cell(CellType.Ground);
					continue;
				}

				if ((x >= 10 && x <= 14) || (x >= 35 && x <= 39))
				{
					if (y < 3 || y >= 27)
					{
						grid[x, y] = new Cell(CellType.Ground);
						continue;
					}
				}

				if ((x >= 15 && x <= 19) || (x >= 30 && x <= 34))
				{
					grid[x, y] = new Cell(CellType.Ground);
					continue;
				}

				if ((x >= 20 && x <= 24) || (x >= 25 && x <= 29))
				{
					if (y >= 14 && y <= 16)
					{
						grid[x, y] = new Cell(CellType.Ground);
						continue;
					}
				}

				if (x >= 23 && x < 27)
				{
					grid[x, y] = new Cell(CellType.Ground);
					continue;
				}

				if (x >= 21 && x <= 22)
				{
					if (y >= 27 || y < 3)
					{
						grid[x, y] = new Cell(CellType.Ground);
						continue;
					}
				}

				if (x >= 27 && x <= 28)
				{
					if (y >= 27 || y < 3)
					{
						grid[x, y] = new Cell(CellType.Ground);
						continue;
					}
				}

				// if ((x >= 10 && x <= 20) || (x >= 30 && x <= 40))
				// {
				// 	if (y >= 9 && y <= 11)
				// 	{
				// 		grid[x, y] = new Cell(CellType.Ground);
				// 		continue;
				// 	}
				// }
				//
				// int dx = x - 25;
				// int dy = y - 10;
				// int r = 8;
				// if (((dx * dx) + (dy * dy)) <= (r * r))
				// {
				// 	grid[x, y] = new Cell(CellType.Ground);
				// 	continue;
				// }

				grid[x, y] = new Cell(CellType.Empty);
			}
		}

		return grid;
	}

	public static void GenerateStructures(WorldState state)
	{
		Castle castle0 = Castle.CreateBuilt(0, new Vec2Int(0, 9));
		Castle castle1 = Castle.CreateBuilt(1, new Vec2Int(45, 9));

		state.AddEntity(castle0);
		state.AddEntity(castle1);
	}

	public static void GenerateResources(WorldState state)
	{
		Random rng = new Random();
		int numNodes = 20;
		for (int i = 0; i < numNodes; i++)
		{
			int x = (int)(rng.Next() % (state.Map.Width / 2 - 4));
			int y = (int)(rng.Next() % state.Map.Height);
			Vec2 pos0 = new Vec2(x + 0.5f, y + 0.5f);
			Vec2 pos1 = new Vec2((state.Map.Width - x - 1) + 0.5f, (y + 0.5f));
			if (state.IsTileOccupied(state.Map.CellPosFromWorldSpace(pos0)))
			{
				i--;
				continue;
			}
			state.AddEntity(new GoldNode(pos0, ~0u));
			state.AddEntity(new GoldNode(pos1, ~0u));
		}

		Grid<Cell> grid = state.Map;
		for (int x = 0; x < grid.Width; x++)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				if (x >= 21 && x <= 28)
				{
					if (y >= 27 || y < 3)
					{
						Vec2 pos = new Vec2(x + 0.5f, y + 0.5f);
						state.AddEntity(new GoldNode(pos, ~0u));
						continue;
					}
				}
			}
		}
	}

	public static void GenerateUnits(WorldState state)
	{
		Worker worker0 = new Worker(new Vec2(2.5f, 8.5f), 0);
		Worker worker1 = new Worker(new Vec2(47.5f, 8.5f), 1);

		state.AddEntity(worker0);
		state.AddEntity(worker1);
	}

	public static void GenerateInitialResources(WorldState state)
	{
		ResourceStack initialResources = new ResourceStack(Resource.Gold, 100);

		for (uint i = 0; i < state.NumPlayers; i++)
		{
			state.GiveResource(initialResources, i);
		}
	}

	public static void GenerateState()
	{
		WorldState state = new WorldState(GenerateMap(), 2);
		GenerateStructures(state);
		GenerateUnits(state);
		GenerateResources(state);
		GenerateInitialResources(state);
		state.Save("generated.sstate");
		Console.WriteLine($"Generated map to \"generated.sstate\".");
	}

	public static void Run(ParseResult result)
	{
		bool gen = result.GetValue<bool>("--gen");
		if (gen)
		{
			GenerateState();
			return;
		}

		string? map = result.GetValue<string>("--map");
		if (map == null)
		{
			Console.WriteLine("Please provide a map with the flag --map.");
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

		RtsEngine engine = RtsEngine.StartInstance(state);
		_ = engine.Start();

		Console.ReadKey();
		engine.Stop();
	}

	public static async Task Main(string[] args)
	{
		RootCommand rootCommand = new RootCommand("RtsEngine runner");

		rootCommand.Add(new Option<string>("--map"));
		rootCommand.Add(new Option<bool>("--gen"));

		rootCommand.SetAction(Run);
		rootCommand.Parse(args).Invoke();
	}
}

}

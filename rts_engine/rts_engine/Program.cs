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

	public static void GenerateStructures(WorldState world)
	{
		Castle castle0 = Castle.CreateBuilt(0, world, new Vec2Int(0, 9));
		Castle castle1 = Castle.CreateBuilt(1, world, new Vec2Int(45, 9));

		world.AddEntity(castle0);
		world.AddEntity(castle1);
	}

	public static void GenerateResources(WorldState world)
	{
		Random rng = new Random();
		int numNodes = 20;
		for (int i = 0; i < numNodes; i++)
		{
			int x = (int)(rng.Next() % (world.Map.Width / 2 - 4));
			int y = (int)(rng.Next() % world.Map.Height);
			Vec2 pos0 = new Vec2(x + 0.5f, y + 0.5f);
			Vec2 pos1 = new Vec2((world.Map.Width - x - 1) + 0.5f, (y + 0.5f));
			if (world.IsTileOccupied(world.Map.CellPosFromWorldSpace(pos0)))
			{
				i--;
				continue;
			}
			world.AddEntity(new GoldNode(pos0, world, ~0u));
			world.AddEntity(new GoldNode(pos1, world, ~0u));
		}

		Grid<Cell> grid = world.Map;
		for (int x = 0; x < grid.Width; x++)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				if (x >= 21 && x <= 28)
				{
					if (y >= 27 || y < 3)
					{
						Vec2 pos = new Vec2(x + 0.5f, y + 0.5f);
						world.AddEntity(new GoldNode(pos, world, ~0u));
						continue;
					}
				}
			}
		}
	}

	public static void GenerateUnits(WorldState world)
	{
		Worker worker0 = new Worker(new Vec2(2.5f, 8.5f), world, 0);
		Worker worker1 = new Worker(new Vec2(47.5f, 8.5f), world, 1);

		world.AddEntity(worker0);
		world.AddEntity(worker1);
	}

	public static void GenerateInitialResources(WorldState world)
	{
		ResourceStack initialResources = new ResourceStack(Resource.Gold, 100);

		for (uint i = 0; i < world.NumPlayers; i++)
		{
			world.GiveResource(initialResources, i);
		}
	}

	public static void GenerateState()
	{
		WorldState world = new WorldState(GenerateMap(), 2);
		GenerateStructures(world);
		GenerateUnits(world);
		GenerateResources(world);
		GenerateInitialResources(world);
		world.Save("generated.sworld");
		Console.WriteLine($"Generated map to \"generated.sworld\".");
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


		WorldState world;
		try
		{
			world = WorldState.Load(map);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error opening map \"{map}\": {ex.Message}");
			return;
		}

		// RtsEngine engine = RtsEngine.StartInstance(world);
		// RtsEngine engine = RtsEngine.StartInstance(world);
		RtsEngine engine = new RtsEngine(world);
		// _ = engine.Start();
		engine.Start();

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

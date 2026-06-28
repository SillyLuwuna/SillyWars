using RtsEngine.Map;
using RtsEngine.Units;
using RtsEngine.Networking;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using RtsEngine.Math;
using RtsEngine.Data;
using RtsEngine.EntityProperties;
using RtsEngine.Resources;
using RtsEngine.Structures;

namespace RtsEngine
{

public static class Program
{
	private static int NumPlayers = 2;

	public static async Task Main()
	{
		// Create map
		// Grid<Cell> grid = new Grid<Cell>(new Vec2(0, 0), 1, 200, 100);
		// int segmentationSize = 80;

		// Grid<Cell> grid = new Grid<Cell>(new Vec2(0, 0), 1, 30, 20);
		Grid<Cell> grid = new Grid<Cell>(new Vec2(0, 0), 1, 25, 15);

		int segmentationSize = 10;
		int segmentationGridWidth = (int)grid.Width / segmentationSize;
		for (int x = 0; x < grid.Width; x++)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				grid[x, y] = new Cell(CellType.Ground);
				// if (x < thirdGridWidth || x > thirdGridWidth * (segmentationSize - 1))
				// {
				// 	grid[x, y] = new Cell(CellType.Ground);
				// }
				// else
				// {
				// 	if (y == grid.Height / 2)
				// 	{
				// 		grid[x, y] = new Cell(CellType.Ground);
				// 	}
				// 	else
				// 	{
				// 		grid[x, y] = new Cell(CellType.Empty);
				// 	}
				// }
			}
		}

		WorldState? state = new WorldState(grid, NumPlayers);

		GoldNode node = new GoldNode(new Vec2(14.5f, 1.5f), ~0u);
		state.AddEntity(node);
		Entity entity = new Worker(new Vec2(4.5f, 2.5f), 2);
		state.AddEntity(entity);

		for (int x = 0; x < grid.Width; x++)
		{
			for (int y = 0; y < grid.Height; y++)
			{
				Vec2 pos = new Vec2(x + 0.5f, y + 0.4f);
				// if ((x + y) % 2 == 0)
				// {
				// 	continue;
				// }
				if (x < segmentationGridWidth)
				{
					uint owner = 1;
					Entity curr = (x + y) % 3 == 0 ? new Worker(pos, owner) : new Knight(pos, owner);
					state.AddEntity(curr);
				}
				else if (x > grid.Width - segmentationGridWidth - 1)
				{
					uint owner = 0;
					Entity curr = (x + y) % 3 == 0 ? new Worker(pos, owner) : new Knight(pos, owner);
					state.AddEntity(curr);
				}
			}
		}

		Barracks barracks1 = Barracks.CreateBuilt(1, new Vec2Int(20, 5));
		Barracks barracks2 = Barracks.CreateBuilt(1, new Vec2Int(20, 8));
		Barracks barracks3 = Barracks.CreateBuilt(0, new Vec2Int(20, 11));
		state.AddEntity(barracks1);
		state.AddEntity(barracks2);
		state.AddEntity(barracks3);

		ResourceStack initialResources = new ResourceStack(Resource.Gold, 10000);
		for (uint i = 0; i < NumPlayers; i++)
		{
			state.GiveResource(initialResources, i);
		}

		// Save map
		state.Save("test.smap");

		// Load

		// WorldState? state = WorldState.Load("test.smap");
		state = WorldState.Load("test.smap");

		// grid = state.Map;
		// for (int i = 0; i < grid.Size(); i++)
		// {
		// 	Console.Write(grid[i].IsWalkable + " ");
		// }

		// Start engine

		// Console.WriteLine(retrieved);

		RtsEngine engine = RtsEngine.StartInstance(state);
		_ = engine.Start();

		// Client client = new Client(1000);
		// client.MessageReceived += HandleMessage;
		// await client.ConnectAsync("localhost", 13774);

		Console.ReadKey();

		engine.Stop();
	}

	private static void HandleMessage(object? sender, byte[] data)
	{
		byte[] dd = DataCompressor.DecompressData(data);
		WorldState state = Serializer.FromBytes<WorldState>(dd);
		// SerializableGrid<Cell> map = state.Map;

		// Console.WriteLine();
		// Console.WriteLine();
		// for (int i = 0; i < state.Map.Size(); i++)
		// {
		// 	Console.Write(state.Map[i].IsWalkable + " ");
		// }
		// Console.WriteLine();
		//
		// for (uint i = 0; i < 10; i++)
		// {
		// 	Console.Write(state.GetEntity(i)!.Pos + " ");
		// 	Console.Write(state.GetEntity(i) is Worker);
		// 	Console.WriteLine();
		// }
	}

	//
	// private static async void HandleConnection(object? sender, TcpClient client)
	// {
	// 	Server server = (Server)sender!;
	//
	// 	WorldState state = WorldState.Load("test.smap");
	// 	await server.SendData(Serializer.ToBytes(state), 0);
	// }
}

}

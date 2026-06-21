using RtsEngine.Map;
using RtsEngine.Units;
using RtsEngine.Networking;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using RtsEngine.Math;
using RtsEngine.Data;
using RtsEngine.EntityProperties;

namespace RtsEngine
{

public static class Program
{
	public static async Task Main()
	{
		// Create map
		Grid<Cell> grid = new Grid<Cell>(new Vec2(0, 0), 1, 20, 10);

		for (int i = 0; i < grid.Size(); i++)
		{
			grid[i] = new Cell(CellType.Ground);
			// if (i % 2 == 0 || i % 3 == 1) grid[i] = new Cell(CellType.Ground);
			// else grid[i] = new Cell(CellType.Empty);
		}

		WorldState? state = new WorldState(grid);

		for (int i = 0; i < 10; i++)
		{
			Vec2 pos = new Vec2(i + 0.5f, (i*2) % 10 + 0.5f);
			uint owner = (((i % 4) == 0) || (((i + 3) % 4) == 0)) ? 0u : 1u;
			Entity curr = i % 2 == 0 ? new Worker(pos, owner) : new Knight(pos, owner);
			state.AddEntity(curr);
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

		RtsEngine engine = new RtsEngine(state);
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

namespace RtsEngine
{
using Map;
using Units;
using Networking;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
    using global::RtsEngine.Math;
    using global::RtsEngine.Data;

    public static class Program
{
	public static async Task Main()
	{
		SerializableGrid<Cell> grid = new SerializableGrid<Cell>(new Vec2(0, 0), 1, 20, 10);

		for (int i = 0; i < grid.Size(); i++)
		{
			if (i % 2 == 0 || i % 3 == 1) grid[i] = new Cell(CellType.Ground);
			else grid[i] = new Cell(CellType.Empty);
		}

		WorldState? state = new WorldState(grid);

		// for (int i = 0; i < state.Map.Size(); i++)
		// {
		// 	Console.Write(state.Map[i].IsWalkable + " ");
		// }
		// Console.WriteLine();

		for (int i = 0; i < 100; i++)
		{
			Vec2 pos = new Vec2(i, i*2);
			state.Entities.Add(i % 2 == 0 ? new Worker(pos) : new Knight(pos));
		}

		// for (int i = 0; i < 10; i++)
		// {
		// 	Console.Write(state.Entities[i].Pos + " ");
		// 	Console.Write("Worker? " + (state.Entities[i] is Worker));
		// 	Console.WriteLine();
		// }

		state.Save("test.smap");

		state = null;



		// Load

		// WorldState? state = WorldState.Load("test.smap");
		state = WorldState.Load("test.smap");

		// Console.WriteLine();
		// Console.WriteLine();
		// for (int i = 0; i < state.Map.Size(); i++)
		// {
		// 	Console.Write(state.Map[i].IsWalkable + " ");
		// }
		// Console.WriteLine();
		//
		// for (int i = 0; i < 10; i++)
		// {
		// 	Console.Write(state.Entities[i].Pos + " ");
		// 	Console.Write("Worker? " + (state.Entities[i] is Worker));
		// 	Console.WriteLine();
		// }




		// Send over network
		// Server server = new Server(13774, 1);
		// // server.MessageReceived += HandleMessage;
		// // server.ConnectionEstablished += HandleConnection;
		//
		// _ = server.StartAsync();
		//
		// Client client = new Client(1000);
		// client.MessageReceived += HandleMessage;
		// await Task.Delay(500);
		// await client.ConnectAsync("localhost", 13774);
		// await server.SendData(DataCompressor.CompressData(Serializer.ToBytes(state)), 0);
		// await client.SendAsync(DataCompressor.CompressData(Serializer.ToBytes(state)));

		// Console.ReadKey();
		// await Task.Delay(5000);

		// await server.SendData(Serializer.ToBytes(state), 0);

		// Console.ReadKey();
		// server.Stop();


		RtsEngine engine = new RtsEngine(state);
		_ = engine.Start();

		// Console.ReadKey();
		//
		// Client client = new Client(1000);
		// client.MessageReceived += HandleMessage;
		// await client.ConnectAsync("localhost", 13774);

		Console.ReadKey();

		engine.Stop();
	}

	// private static void HandleMessage(object? sender, byte[] data)
	// {
	// 	byte[] dd = DataCompressor.DecompressData(data);
	// 	WorldState state = Serializer.FromBytes<WorldState>(dd);
	// 	SerializableGrid<Cell> map = state.Map;
	//
	// 	Console.WriteLine();
	// 	Console.WriteLine();
	// 	for (int i = 0; i < map.Size(); i++)
	// 	{
	// 		Console.Write(map[i].IsWalkable + " ");
	// 	}
	// 	Console.WriteLine();
	//
	// 	for (int i = 0; i < 10; i++)
	// 	{
	// 		Console.Write(state.Entities[i].Pos + " ");
	// 		Console.Write("Worker? " + (state.Entities[i] is Worker));
	// 		Console.WriteLine();
	// 	}
	// }

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

namespace RtsEngine;
using Map;
using Units;
using Networking;

public static class Program
{
	public static async Task Main()
	{
		SerializableGrid<Cell> grid = new SerializableGrid<Cell>(new Vec2(0, 0), 1, 0, 0);

		for (int i = 0; i < grid.Size(); i++)
		{
			if (i % 2 == 0) grid[i] = new Cell(CellType.Ground);
			else grid[i] = new Cell(CellType.Empty);
		}

		WorldState? state = new WorldState(grid);
		SerializableGrid<Cell>? map = state.Map;

		for (int i = 0; i < map.Size(); i++)
		{
			Console.Write(map[i].IsWalkable + " ");
		}
		Console.WriteLine();

		for (int i = 0; i < 100; i++)
		{
			Vec2 pos = new Vec2(i, i*2);
			state.Entities.Add(i % 2 == 0 ? new Worker(pos) : new Knight(pos));
		}

		for (int i = 0; i < 10; i++)
		{
			Console.Write(state.Entities[i].Pos + " ");
			Console.Write("Worker? " + (state.Entities[i] is Worker));
			Console.WriteLine();
		}

		state.Save("test.smap");

		state = null;



		// Load

		state = WorldState.Load("test.smap");
		map = state.Map;

		Console.WriteLine();
		Console.WriteLine();
		for (int i = 0; i < map.Size(); i++)
		{
			Console.Write(map[i].IsWalkable + " ");
		}
		Console.WriteLine();

		for (int i = 0; i < 10; i++)
		{
			Console.Write(state.Entities[i].Pos + " ");
			Console.Write("Worker? " + (state.Entities[i] is Worker));
			Console.WriteLine();
		}




		// Send over network
		Server server = new Server(13774, 1);
		server.MessageReceived += HandleMessage;
		Client client = new Client();

		_ = server.StartAsync();

		await Task.Delay(500);

		await client.ConnectAsync("localhost", 13774);
		await client.SendAsync(Serializer.ToBytes(state));

		Console.ReadKey();
		server.Stop();
	}

	private static void HandleMessage(object? sender, byte[] data)
	{
		WorldState state = Serializer.FromBytes<WorldState>(data);
		SerializableGrid<Cell> map = state.Map;

		Console.WriteLine();
		Console.WriteLine();
		for (int i = 0; i < map.Size(); i++)
		{
			Console.Write(map[i].IsWalkable + " ");
		}
		Console.WriteLine();

		for (int i = 0; i < 10; i++)
		{
			Console.Write(state.Entities[i].Pos + " ");
			Console.Write("Worker? " + (state.Entities[i] is Worker));
			Console.WriteLine();
		}
	}
}

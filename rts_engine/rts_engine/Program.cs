namespace RtsEngine;

public static class Program
{
	public static void Main()
	{
		// SerializableGrid<Cell> grid = new SerializableGrid<Cell>(new Vec2(0, 0), 1, 10, 10);
		//
		// for (int i = 0; i < grid.Size(); i++)
		// {
		// 	if (i % 2 == 0) grid[i] = new Cell(true);
		// 	else grid[i] = new Cell(false);
		// }
		//
		// WorldState? state = new WorldState(grid);
		// SerializableGrid<Cell>? map = state.GetMapView();
		//
		// for (int i = 0; i < map.Size(); i++)
		// {
		// 	Console.Write(map[i].IsWalkable + " ");
		// }
		//
		// state.Save("test.smap");
		//
		// state = null;

		WorldState state = WorldState.Load("test.smap");
		SerializableGrid<Cell> map = state.GetMapView();

		Console.WriteLine();
		Console.WriteLine();
		for (int i = 0; i < map.Size(); i++)
		{
			Console.Write(map[i].IsWalkable + " ");
		}

		// Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		// GridRaycast<int> raycast = new GridRaycast<int>(grid);
		//
		// Vec2Int first = new Vec2Int(0, 0);
		// Vec2Int second = new Vec2Int(10, 3);
		//
		// Vec2 firstWorldPos = grid.WorldSpaceFromCellPos(first);
		// Vec2 secondWorldPos = grid.WorldSpaceFromCellPos(second);
		//
		// List<Vec2Int> collisions = raycast.CastRay(firstWorldPos, secondWorldPos);
		//
		// for (int i = 0; i < collisions.Count; i++)
		// {
		// 	Console.WriteLine("vec" + i + ":\t" + collisions[i]);
		// }
	}
}

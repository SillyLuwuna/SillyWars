Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
GridRaycast<int> raycast = new GridRaycast<int>(grid);

// Vec2 first = new Vec2(0.0f, 0.1f);
// Vec2 second = new Vec2(9.0f, 3.2f);

Vec2 first = new Vec2(0.0f, 0.0f);
Vec2 second = new Vec2(10.0f, 5.0f);

Console.WriteLine(first);

List<Vec2Int> collisions = raycast.CastRay(first, second);

for (int i = 0; i < collisions.Count; i++)
{
	Console.WriteLine("vec" + i + ":\t" + collisions[i]);
}

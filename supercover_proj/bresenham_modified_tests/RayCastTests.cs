namespace tests;

[TestClass]
public sealed class Test1
{
	[TestMethod]
	public void TestEqualPoints()
	{
		// Arrange
		Vec2 first = new Vec2(0.0f, 0.0f);
		Vec2 second = new Vec2(0.0f, 0.0f);

		Vec2Int expected = new Vec2Int(0, 0);

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);

		// Assert
		Assert.HasCount(1, collisions);
		Assert.Contains(expected, collisions);
	}

    [TestMethod]
    public void TestFirstOctant()
    {

    }

	[TestMethod]
	public void TestWorksWhenNegativeStart()
	{

	}
}

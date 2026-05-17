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
		// Arrange
		Vec2 first = new Vec2(0.0f, 0.1f);
		Vec2 second = new Vec2(10.0f, 3.2746031746031745f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(0, 0));
		expectedVals.Add(new Vec2Int(1, 0));
		expectedVals.Add(new Vec2Int(2, 0));
		expectedVals.Add(new Vec2Int(2, 1));
		expectedVals.Add(new Vec2Int(3, 1));
		expectedVals.Add(new Vec2Int(4, 1));
		expectedVals.Add(new Vec2Int(5, 1));
		expectedVals.Add(new Vec2Int(5, 2));
		expectedVals.Add(new Vec2Int(6, 2));
		expectedVals.Add(new Vec2Int(7, 2));
		expectedVals.Add(new Vec2Int(8, 2));
		expectedVals.Add(new Vec2Int(9, 2));
		expectedVals.Add(new Vec2Int(9, 3));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

	[TestMethod]
	public void TestLineOverlap()
	{
		// Arrange
		Vec2 first = new Vec2(0.0f, 0.0f);
		Vec2 second = new Vec2(10.0f, 5.0f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(0, 0));
		expectedVals.Add(new Vec2Int(1, 0));
		expectedVals.Add(new Vec2Int(1, 1));
		expectedVals.Add(new Vec2Int(2, 0));
		expectedVals.Add(new Vec2Int(2, 1));
		expectedVals.Add(new Vec2Int(3, 1));
		expectedVals.Add(new Vec2Int(3, 2));
		expectedVals.Add(new Vec2Int(4, 1));
		expectedVals.Add(new Vec2Int(4, 2));
		expectedVals.Add(new Vec2Int(5, 2));
		expectedVals.Add(new Vec2Int(5, 3));
		expectedVals.Add(new Vec2Int(6, 2));
		expectedVals.Add(new Vec2Int(6, 3));
		expectedVals.Add(new Vec2Int(7, 3));
		expectedVals.Add(new Vec2Int(7, 4));
		expectedVals.Add(new Vec2Int(8, 3));
		expectedVals.Add(new Vec2Int(8, 4));
		expectedVals.Add(new Vec2Int(9, 4));
		expectedVals.Add(new Vec2Int(9, 5));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
	}

	[TestMethod]
	public void TestWorksWhenNegativeStart()
	{
		// Arrange
		Vec2 first = new Vec2(-5.0f, -1.48730159f);
		Vec2 second = new Vec2(5.0f, 1.68730159f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(-5, -2));
		expectedVals.Add(new Vec2Int(-4, -2));
		expectedVals.Add(new Vec2Int(-4, -1));
		expectedVals.Add(new Vec2Int(-3, -1));
		expectedVals.Add(new Vec2Int(-2, -1));
		expectedVals.Add(new Vec2Int(-1, -1));
		expectedVals.Add(new Vec2Int(-1, 0));
		expectedVals.Add(new Vec2Int(0, 0));
		expectedVals.Add(new Vec2Int(1, 0));
		expectedVals.Add(new Vec2Int(2, 0));
		expectedVals.Add(new Vec2Int(2, 1));
		expectedVals.Add(new Vec2Int(3, 1));
		expectedVals.Add(new Vec2Int(4, 1));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
	}

    [TestMethod]
    public void TestSecondOctant()
    {
		// Arrange
		Vec2 first = new Vec2(0.1f, 0.0f);
		Vec2 second = new Vec2(3.2746031746031745f, 10.0f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(0, 0));
		expectedVals.Add(new Vec2Int(0, 1));
		expectedVals.Add(new Vec2Int(0, 2));
		expectedVals.Add(new Vec2Int(1, 2));
		expectedVals.Add(new Vec2Int(1, 3));
		expectedVals.Add(new Vec2Int(1, 4));
		expectedVals.Add(new Vec2Int(1, 5));
		expectedVals.Add(new Vec2Int(2, 5));
		expectedVals.Add(new Vec2Int(2, 6));
		expectedVals.Add(new Vec2Int(2, 7));
		expectedVals.Add(new Vec2Int(2, 8));
		expectedVals.Add(new Vec2Int(2, 9));
		expectedVals.Add(new Vec2Int(3, 10));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestThirdOctant()
    {
		// Arrange
		Vec2 first = new Vec2(-0.1f, 0.0f);
		Vec2 second = new Vec2(-3.2746031746031745f, 10.0f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(-1, 0));
		expectedVals.Add(new Vec2Int(-1, 1));
		expectedVals.Add(new Vec2Int(-1, 2));
		expectedVals.Add(new Vec2Int(-2, 2));
		expectedVals.Add(new Vec2Int(-2, 3));
		expectedVals.Add(new Vec2Int(-2, 4));
		expectedVals.Add(new Vec2Int(-2, 5));
		expectedVals.Add(new Vec2Int(-3, 5));
		expectedVals.Add(new Vec2Int(-3, 6));
		expectedVals.Add(new Vec2Int(-3, 7));
		expectedVals.Add(new Vec2Int(-3, 8));
		expectedVals.Add(new Vec2Int(-3, 9));
		expectedVals.Add(new Vec2Int(-4, 9));
		expectedVals.Add(new Vec2Int(-4, 10));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestFourthOctant()
    {
		// Arrange
		Vec2 first = new Vec2(-0.0f, 0.1f);
		Vec2 second = new Vec2(-10.0f, 3.2746031746031745f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(-1, 0));
		expectedVals.Add(new Vec2Int(-2, 0));
		expectedVals.Add(new Vec2Int(-3, 0));
		expectedVals.Add(new Vec2Int(-3, 1));
		expectedVals.Add(new Vec2Int(-4, 1));
		expectedVals.Add(new Vec2Int(-5, 1));
		expectedVals.Add(new Vec2Int(-6, 1));
		expectedVals.Add(new Vec2Int(-6, 2));
		expectedVals.Add(new Vec2Int(-7, 2));
		expectedVals.Add(new Vec2Int(-8, 2));
		expectedVals.Add(new Vec2Int(-9, 2));
		expectedVals.Add(new Vec2Int(-10, 2));
		expectedVals.Add(new Vec2Int(-10, 3));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestFifthOctant()
    {
		// Arrange
		Vec2 first = new Vec2(-0.0f, -0.1f);
		Vec2 second = new Vec2(-10.0f, -3.2746031746031745f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(-1, -1));
		expectedVals.Add(new Vec2Int(-2, -1));
		expectedVals.Add(new Vec2Int(-3, -1));
		expectedVals.Add(new Vec2Int(-3, -2));
		expectedVals.Add(new Vec2Int(-4, -2));
		expectedVals.Add(new Vec2Int(-5, -2));
		expectedVals.Add(new Vec2Int(-6, -2));
		expectedVals.Add(new Vec2Int(-6, -3));
		expectedVals.Add(new Vec2Int(-7, -3));
		expectedVals.Add(new Vec2Int(-8, -3));
		expectedVals.Add(new Vec2Int(-9, -3));
		expectedVals.Add(new Vec2Int(-10, -3));
		expectedVals.Add(new Vec2Int(-10, -4));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestSixthOctant()
    {
		// Arrange
		Vec2 first = new Vec2(-0.1f, -0.0f);
		Vec2 second = new Vec2(-3.2746031746031745f, -10.0f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(-1, -1));
		expectedVals.Add(new Vec2Int(-1, -2));
		expectedVals.Add(new Vec2Int(-1, -3));
		expectedVals.Add(new Vec2Int(-2, -3));
		expectedVals.Add(new Vec2Int(-2, -4));
		expectedVals.Add(new Vec2Int(-2, -5));
		expectedVals.Add(new Vec2Int(-2, -6));
		expectedVals.Add(new Vec2Int(-3, -6));
		expectedVals.Add(new Vec2Int(-3, -7));
		expectedVals.Add(new Vec2Int(-3, -8));
		expectedVals.Add(new Vec2Int(-3, -9));
		expectedVals.Add(new Vec2Int(-3, -10));
		expectedVals.Add(new Vec2Int(-4, -10));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestSeventhOctant()
    {
		// Arrange
		Vec2 first = new Vec2(0.1f, -0.0f);
		Vec2 second = new Vec2(3.2746031746031745f, -10.0f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(0, -1));
		expectedVals.Add(new Vec2Int(0, -2));
		expectedVals.Add(new Vec2Int(0, -3));
		expectedVals.Add(new Vec2Int(1, -3));
		expectedVals.Add(new Vec2Int(1, -4));
		expectedVals.Add(new Vec2Int(1, -5));
		expectedVals.Add(new Vec2Int(1, -6));
		expectedVals.Add(new Vec2Int(2, -6));
		expectedVals.Add(new Vec2Int(2, -7));
		expectedVals.Add(new Vec2Int(2, -8));
		expectedVals.Add(new Vec2Int(2, -9));
		expectedVals.Add(new Vec2Int(2, -10));
		expectedVals.Add(new Vec2Int(3, -10));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

    [TestMethod]
    public void TestEightOctant()
    {
		// Arrange
		Vec2 first = new Vec2(0.0f, -0.1f);
		Vec2 second = new Vec2(10.0f, -3.2746031746031745f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(0, -1));
		expectedVals.Add(new Vec2Int(1, -1));
		expectedVals.Add(new Vec2Int(2, -1));
		expectedVals.Add(new Vec2Int(2, -2));
		expectedVals.Add(new Vec2Int(3, -2));
		expectedVals.Add(new Vec2Int(4, -2));
		expectedVals.Add(new Vec2Int(5, -2));
		expectedVals.Add(new Vec2Int(5, -3));
		expectedVals.Add(new Vec2Int(6, -3));
		expectedVals.Add(new Vec2Int(7, -3));
		expectedVals.Add(new Vec2Int(8, -3));
		expectedVals.Add(new Vec2Int(9, -3));
		expectedVals.Add(new Vec2Int(9, -4));
		expectedVals.Add(new Vec2Int(10, -4));

		Grid<int> grid = new Grid<int>(new Vec2(0, 0), 1, 10, 10);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
    }

	[TestMethod]
	public void TestOnNonStandard0()
	{
		// Arrange
		Vec2 first = new Vec2(-28.71f, 3.28f);
		Vec2 second = new Vec2(-27.50f, 2.50f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(2, 10));
		expectedVals.Add(new Vec2Int(2, 9));
		expectedVals.Add(new Vec2Int(3, 9));

		Grid<int> grid = new Grid<int>(new Vec2(-31.00f, -7.00f), 1.0f, 40, 24);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
	}

	[TestMethod]
	public void TestOnNonStandard1()
	{
		// Arrange
		Vec2 first = new Vec2(-27.50f, 2.50f);
		Vec2 second = new Vec2(-26.50f, 0.50f);

		List<Vec2Int> expectedVals = new List<Vec2Int>();

		expectedVals.Add(new Vec2Int(3, 9));
		expectedVals.Add(new Vec2Int(3, 8));
		expectedVals.Add(new Vec2Int(4, 8));
		expectedVals.Add(new Vec2Int(4, 7));

		Grid<int> grid = new Grid<int>(new Vec2(-31.00f, -7.00f), 1.0f, 40, 24);
		GridRaycast<int> raycast = new GridRaycast<int>(grid);

		// Act
		List<Vec2Int> collisions = raycast.CastRay(first, second);
		foreach (Vec2Int collision in collisions)
		{
			Console.WriteLine(collision);
		}

		// Assert
		foreach (Vec2Int expected in expectedVals)
		{
			Assert.Contains(expected, collisions);
		}
	}
}

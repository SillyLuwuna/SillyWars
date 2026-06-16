namespace RtsEngine;

public class Line
{
	private float _a;
	private float _b;
	private float _c;

	// does not check if first == second
	public Line(Vec2 first, Vec2 second)
	{
		_a = first.y - second.y;
		_b = second.x - first.x;
		_c = first.x * second.y - second.x * first.y;
	}

	public float Fx(float y)
	{
		if (F.Zero(_a))
		{
			throw new DivideByZeroException("cannot calculate f(y) when a = 0");
		}

		return - (_b * y + _c) / _a;
	}

	public float Fy(float x)
	{
		if (F.Zero(_b))
		{
			throw new DivideByZeroException("cannot calculate f(x) when b = 0");
		}

		return - (_a * x + _c) / _b;
	}
	
	public bool IsHorizontal => F.Zero(_a);
	public bool IsVertical => F.Zero(_b);

	public override string ToString()
	{
		return "(" + _a + ")x + (" + _b + ")y + (" + _c + ")";
	}
}

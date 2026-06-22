using RtsEngine.Math;

namespace RtsEngine.Structures
{

public class Castle : BaseStructure
{
	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints { get; set; }

	public override int BuildEffort { get; set; }

	public Castle(uint ownerId, Vec2Int start) : base(ownerId, start)
	{
		Height = 2;
		Width = 3;

		MaxHitPoints = 40;

		BuildEffort = 20;
	}

	public override void Tick()
	{
	}
}

}

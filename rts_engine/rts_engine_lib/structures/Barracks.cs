using RtsEngine.Math;

namespace RtsEngine.Structures
{

public class Barracks : BaseStructure
{
	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints { get; set; }

	public override int BuildEffort { get; set; }

	public Barracks(uint ownerId, Vec2Int start) : base(ownerId, start)
	{
		Height = 2;
		Width = 2;

		MaxHitPoints = 20;

		BuildEffort = 10;
	}

	public override void Tick()
	{
	}
}

}

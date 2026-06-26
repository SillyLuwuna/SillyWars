using System.Collections.Generic;
using RtsEngine.Math;
using RtsEngine.Resources;
using RtsEngine.Units;

namespace RtsEngine.Structures
{

public class Castle : UnitProducer
{
	public const int BaseHeight = 3;
	public const int BaseWidth = 5;
	public const int BaseMaxHitpoints = 40;
	public const int BaseBuildEffort = 20;

	public const int BaseMaxUnitProduction = 4;
	public static readonly List<UnitType> BaseAllowedUnitTypes = new List<UnitType> { UnitType.Worker };

	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints { get; set; }

	public override int BuildEffort { get; set; }

	public override int MaxUnitProduction { get; set; }
	public override List<UnitType> AllowedUnitTypes { get; set; }

	public override ResourceStack Cost { get => new ResourceStack(Resource.Gold, 100); }

	public Castle(uint ownerId, Vec2Int start) : base(ownerId, start, BaseHeight, BaseWidth)
	{
		MaxHitPoints = BaseMaxHitpoints;

		BuildEffort = BaseBuildEffort;

		MaxUnitProduction = BaseMaxUnitProduction;
		AllowedUnitTypes = BaseAllowedUnitTypes;
	}

	public void DeliverResource(ResourceStack resourceStack)
	{
		if (this.IsDestroyed || !this.IsBuilt) return;

		RtsEngine.Instance.State.GiveResource(resourceStack, OwnerId);
	}

	public static Castle CreateBuilt(uint ownerId, Vec2Int start)
	{
		Castle castle = new Castle(ownerId, start);

		castle.HasBuildingStarted = true;
		castle.IsBuilt = true;

		return castle;
	}
}

}

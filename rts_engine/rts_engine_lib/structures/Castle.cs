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
	public const int BaseBuildEffort = 20;

	public const int BaseMaxUnitProduction = 4;
	public static readonly List<UnitType> BaseAllowedUnitTypes = new List<UnitType> { UnitType.Worker };

	public override int Height { get; set; }
	public override int Width { get; set; }

	public override int MaxHitPoints => 40;

	public override int BuildEffort { get; set; }

	public override int MaxUnitProduction { get; set; }
	public override List<UnitType> AllowedUnitTypes { get; set; }

	public override ResourceStack Cost => new ResourceStack(Resource.Gold, 90);

	public Castle(uint ownerId, WorldState world, Vec2Int start) : base(ownerId, world, start, BaseHeight, BaseWidth)
	{
		BuildEffort = BaseBuildEffort;

		MaxUnitProduction = BaseMaxUnitProduction;
		AllowedUnitTypes = BaseAllowedUnitTypes;
	}

	public void DeliverResource(ResourceStack resourceStack)
	{
		if (this.IsDestroyed || !this.IsBuilt) return;

		World.GiveResource(resourceStack, OwnerId);
	}

	public static Castle CreateBuilt(uint ownerId, WorldState world, Vec2Int start)
	{
		Castle castle = new Castle(ownerId, world, start);

		castle.HasBuildingStarted = true;
		castle.IsBuilt = true;
		castle.HitPoints = castle.MaxHitPoints;

		return castle;
	}

	public override StructureType StructureType => StructureType.Castle;
}

}

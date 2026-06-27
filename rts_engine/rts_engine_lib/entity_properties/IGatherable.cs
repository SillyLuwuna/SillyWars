using System.Collections.Generic;
using RtsEngine.Resources;

namespace RtsEngine.EntityProperties
{

public interface IGatherable : IEntity, IPositionable
{
	public int Remaining { get; }
	public int MaxAmount { get; }
	public int RequiredWork { get; }
	public int MaxGatherers { get; }
	public int GatherAmount { get; }
	public int CurrGathererCount { get; }

	public bool IsDepleted { get; }

	// returns a negative value if the gatherer cannot gather
	public ResourceStack TryGather(IGatherer gatherer);

	public bool IsInGatheringRange(IGatherer gatherer);
}

}


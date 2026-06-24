namespace RtsEngine.EntityProperties
{

public interface IGatherer : IEntity, IPositionable
{
	public int WorkPerGather { get; }
	public float GatherRange { get; }

	public void Gather(IGatherable gatherable);
}

}


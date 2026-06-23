using RtsEngine.Structures;

namespace RtsEngine.EntityProperties
{

public interface IBuilder : IEntity
{
	public int BuildSpeed { get; set; }
	public void Build(BaseStructure structure);
}

}


using RtsEngine.Structures;

namespace RtsEngine.EntityProperties
{

public interface IBuilder
{
	public int BuildSpeed { get; set; }
	public void Build(BaseStructure structure);
}

}


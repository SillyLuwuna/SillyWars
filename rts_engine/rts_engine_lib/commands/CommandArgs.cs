using RtsEngine.Data;

namespace RtsEngine.Commands
{

public abstract class CommandArgs : ISerializable
{
	public CommandArgs()
	{
	}

	public abstract void SerializeFields(SerializerWriter writer);
	public abstract void DeserializeFields(SerializerReader reader);
}

}

using System.IO;

namespace RtsEngine.Data
{

public interface ISerializable
{
	public void SerializeFields(SerializerWriter writer);
	public void DeserializeFields(SerializerReader reader);
}
}

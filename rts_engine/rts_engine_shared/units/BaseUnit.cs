using System.IO;

namespace RtsEngine.Units
{

public abstract class BaseUnit : Entity, ISerializable
{
	public int HP { get; protected set; }
	public float Range { get; protected set; }
	public int AttackDamage { get; protected set; }
	public float AttackSpeed { get; protected set; }
	public float MoveSpeed { get; protected set; }
	public int Sight { get; protected set; }
	public int TrainCost { get; protected set; }
	public float TrainTime { get; protected set; }
	public UnitType Type { get; protected set; }

	public UnitState State { get; protected set; }
	// Actions!

	public float Size { get; private set; } // for unit-unit collision

	public BaseUnit(Vec2 pos)
	{
		Pos = pos;

		Size = 0.1f;
	}

	public override void Tick()
	{
		// abstract logic
	}

	public override void SerializeFields(BinaryWriter writer)
	{
		base.SerializeFields(writer);
		writer.Write(HP);
		writer.Write(Range);
		writer.Write(AttackDamage);
		writer.Write(AttackSpeed);
		writer.Write(MoveSpeed);
		writer.Write(Sight);
		writer.Write(TrainCost);
		writer.Write(TrainTime);
		writer.Write((byte)Type);
		Serializer.Serialize(writer, State);
		// State.SerializeFields(writer);
		writer.Write(Size);
	}

	public override void DeserializeFields(BinaryReader reader)
	{
		base.DeserializeFields(reader);
		HP = reader.ReadInt32();
		Range = reader.ReadSingle();
		AttackDamage = reader.ReadInt32();
		AttackSpeed = reader.ReadSingle();
		MoveSpeed = reader.ReadSingle();
		Sight = reader.ReadInt32();
		TrainCost = reader.ReadInt32();
		TrainTime = reader.ReadSingle();
		Type = (UnitType)reader.ReadByte();
		State = Serializer.Deserialize<UnitState>(reader);
		Size = reader.ReadSingle();
	}

	// public void Move(Grid<Cell> grid, Vec2 goal)
	// {
	// 	TODO
	// }
}
}

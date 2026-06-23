namespace RtsEngine.EntityProperties
{

public interface IDestroyable : IEntity
{
	public int HitPoints { get; set; }
	public bool IsDestroyed { get; set; }

	public int TargetedByNum { get; set; }

	public void Damage(int damage)
	{
		HitPoints -= damage;
		if (HitPoints < 0)
		{
			Destroy();
		}
	}

	public void Destroy()
	{
		IsDestroyed = true;
	}
}

}


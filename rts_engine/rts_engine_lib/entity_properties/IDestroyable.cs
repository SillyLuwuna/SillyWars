using RtsEngine.Math;

namespace RtsEngine.EntityProperties
{

public interface IDestroyable
{
	public int HitPoints { get; set; }
	public bool IsDestroyed { get; set; }

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


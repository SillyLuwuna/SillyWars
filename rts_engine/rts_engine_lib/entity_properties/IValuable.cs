using RtsEngine.Math;
using RtsEngine.Resources;

namespace RtsEngine.EntityProperties
{

public interface IValuable : IEntity
{
	public ResourceStack Cost { get; }
	public bool IsPaid { get; set; }
	public bool TryPay()
	{
		if (IsPaid) return true;
		IsPaid = RtsEngine.Instance.State.TryTakeResource(Cost, this.OwnerId);
		return IsPaid;
	}
}

}


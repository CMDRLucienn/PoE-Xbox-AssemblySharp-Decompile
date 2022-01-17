using UnityEngine;

public class Wildstrike : GenericAbility
{
	public override void Activate(GameObject target)
	{
		if (m_ownerStats.IsEquipmentLocked)
		{
			base.Activate(target);
		}
	}

	public override void Activate(Vector3 target)
	{
		if (m_ownerStats.IsEquipmentLocked)
		{
			base.Activate(target);
		}
	}
}

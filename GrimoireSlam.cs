public class GrimoireSlam : GenericAbility
{
	protected override void CalculateWhyNotReady()
	{
		base.CalculateWhyNotReady();
		if (Owner != null)
		{
			Equipment component = Owner.GetComponent<Equipment>();
			if (component == null || component.CurrentItems == null || component.CurrentItems.Grimoire == null)
			{
				base.WhyNotReady = NotReadyValue.NoGrimoire;
			}
		}
	}
}

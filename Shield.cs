using UnityEngine;

public class Shield : MonoBehaviour
{
	public enum Category
	{
		Small,
		Medium,
		Large
	}

	public enum AttachType
	{
		Arm,
		Hand
	}

	public Category ShieldCategory;

	public AttachType ShieldAttachType;

	[Range(0f, 100f)]
	public int BaseDeflectBonus = 10;

	[Range(0f, 25f)]
	public int BaseReflexBonus = 2;

	[Range(-100f, 0f)]
	public int BaseAccuracyBonus;

	private const float m_damaged_shield_defense_multiplier = 0.5f;

	public int DeflectBonus => (int)((float)BaseDeflectBonus * DefenseBonusMultiplier());

	public int ReflexBonus => (int)((float)BaseReflexBonus * DefenseBonusMultiplier());

	public int AccuracyBonus => BaseAccuracyBonus;

	private float DefenseBonusMultiplier()
	{
		Equippable component = base.gameObject.GetComponent<Equippable>();
		if (component != null && component.DurabilityState == Equippable.DurabilityStateType.Damaged)
		{
			return 0.5f;
		}
		return 1f;
	}
}

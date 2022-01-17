using UnityEngine;

public class SpellList : ScriptableObject
{
	public enum InstructionSelectionType
	{
		Random,
		TopToBottom
	}

	public enum WeaponPreferenceType
	{
		UsePrimary,
		PrefersMelee,
		PrefersRanged,
		PrefersRangedIfWeaponLoaded
	}

	public WeaponPreferenceType WeaponPreference;

	public InstructionSelectionType SelectionType;

	public SpellCastData[] Spells;

	public TargetPreference DefaultTargetPreference;

	private void OnEnable()
	{
		if (Spells != null)
		{
			SpellCastData[] spells = Spells;
			for (int i = 0; i < spells.Length; i++)
			{
				spells[i].SetName();
			}
		}
	}
}

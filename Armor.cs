using System;
using UnityEngine;

public class Armor : MonoBehaviour
{
	public enum Category
	{
		Light,
		Medium,
		Heavy
	}

	public enum ArmorMaterial
	{
		Unknown,
		Brigadine,
		Cloth,
		Leather,
		Hide,
		Mail,
		Padded,
		Plate,
		Scale
	}

	public static string[] ArmorMatrialStrings = new string[9] { "unknown", "brigadine", "cloth", "leather", "hide", "mail", "padded", "plate", "scale" };

	public float DamageThreshhold = 1f;

	public float DamageReduction;

	public float SpeedFactor = 1f;

	public Category ArmorCategory;

	public ArmorMaterial Material;

	[ArmorDtRange(DamagePacket.DamageType.Pierce)]
	public int DtPercPiercing = 100;

	[ArmorDtRange(DamagePacket.DamageType.Slash)]
	public int DtPercSlashing = 100;

	[ArmorDtRange(DamagePacket.DamageType.Crush)]
	public int DtPercCrushing = 100;

	[ArmorDtRange(DamagePacket.DamageType.Burn)]
	public int DtPercBurning = 100;

	[ArmorDtRange(DamagePacket.DamageType.Freeze)]
	public int DtPercFreezing = 100;

	[ArmorDtRange(DamagePacket.DamageType.Shock)]
	public int DtPercShocking = 100;

	[ArmorDtRange(DamagePacket.DamageType.Corrode)]
	public int DtPercCorroding = 100;

	[SerializeField]
	[HideInInspector]
	private bool[] DamageImmunities;

	public ArmorLevelScaling LevelScaling = new ArmorLevelScaling();

	public float GetDamageThreshold(GameObject wearer)
	{
		float num = DamageThreshhold;
		CharacterStats component = ComponentUtils.GetComponent<CharacterStats>(wearer);
		if ((bool)component)
		{
			num = LevelScaling.AdjustDT(num, component.ScaledLevel);
		}
		return num;
	}

	public float CalculateDT(DamagePacket.DamageType dmgType, float bonusDT, GameObject wearer)
	{
		float num = DamageThreshhold + bonusDT;
		if (wearer != null)
		{
			CharacterStats component = wearer.GetComponent<CharacterStats>();
			if (component != null)
			{
				num = LevelScaling.AdjustDT(num, component.ScaledLevel);
				float num2 = 1f;
				for (int i = 0; i < component.ActiveStatusEffects.Count; i++)
				{
					StatusEffect statusEffect = component.ActiveStatusEffects[i];
					if (!statusEffect.Applied)
					{
						continue;
					}
					if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.BonusArmorDtMult)
					{
						num2 += statusEffect.CurrentAppliedValue - 1f;
					}
					else if (statusEffect.Params.AffectsStat == StatusEffect.ModifiedStat.BonusArmorDtMultAtLowHealth)
					{
						Health component2 = wearer.GetComponent<Health>();
						if (component2 != null && component2.HealthPercentage < 0.5f)
						{
							num2 += statusEffect.CurrentAppliedValue - 1f;
						}
					}
				}
				num *= num2;
			}
		}
		return AdjustForDamageType(num, dmgType);
	}

	public float CalculateDR(DamagePacket.DamageType dmgType)
	{
		float damageReduction = DamageReduction;
		return AdjustForDamageType(damageReduction, dmgType);
	}

	public bool IsImmune(DamagePacket.DamageType type)
	{
		VerifyDamageImmunitiesSize();
		if (type < DamagePacket.DamageType.Count)
		{
			return DamageImmunities[(int)type];
		}
		if (type == DamagePacket.DamageType.All)
		{
			for (int i = 0; i < 7; i++)
			{
				if (!DamageImmunities[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void SetIsImmune(DamagePacket.DamageType type, bool state)
	{
		VerifyDamageImmunitiesSize();
		if (type < DamagePacket.DamageType.Count)
		{
			DamageImmunities[(int)type] = state;
			return;
		}
		if (type == DamagePacket.DamageType.All)
		{
			for (int i = 0; i < 7; i++)
			{
				DamageImmunities[i] = state;
			}
			return;
		}
		throw new ArgumentException(string.Concat("Unsupported damage type '", type, "'."), "type");
	}

	private void VerifyDamageImmunitiesSize()
	{
		if (DamageImmunities == null)
		{
			DamageImmunities = new bool[7];
		}
		else if (DamageImmunities.Length < 7)
		{
			bool[] array = new bool[7];
			DamageImmunities.CopyTo(array, 0);
			DamageImmunities = array;
		}
	}

	public float AdjustForDamageType(float incoming, DamagePacket.DamageType dmgType)
	{
		if (IsImmune(dmgType))
		{
			return float.PositiveInfinity;
		}
		float num = incoming;
		switch (dmgType)
		{
		case DamagePacket.DamageType.Pierce:
			num *= (float)DtPercPiercing / 100f;
			break;
		case DamagePacket.DamageType.Slash:
			num *= (float)DtPercSlashing / 100f;
			break;
		case DamagePacket.DamageType.Crush:
			num *= (float)DtPercCrushing / 100f;
			break;
		case DamagePacket.DamageType.Burn:
			num *= (float)DtPercBurning / 100f;
			break;
		case DamagePacket.DamageType.Freeze:
			num *= (float)DtPercFreezing / 100f;
			break;
		case DamagePacket.DamageType.Shock:
			num *= (float)DtPercShocking / 100f;
			break;
		case DamagePacket.DamageType.Corrode:
			num *= (float)DtPercCorroding / 100f;
			break;
		case DamagePacket.DamageType.Raw:
			num = 0f;
			break;
		}
		return num;
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class DamagePacket
{
	public enum DamageType
	{
		Slash,
		Crush,
		Pierce,
		Burn,
		Freeze,
		Shock,
		Corrode,
		Count,
		None,
		All,
		Raw
	}

	[Serializable]
	public class DamageProcType
	{
		public DamageType Type = DamageType.Burn;

		public float PercentOfBaseDamage = 10f;

		public DamageProcType(DamageProcType other)
		{
			Type = other.Type;
			PercentOfBaseDamage = other.PercentOfBaseDamage;
		}

		public DamageProcType(DamageType dmgType, float pctOfBaseDamage)
		{
			Type = dmgType;
			PercentOfBaseDamage = pctOfBaseDamage;
		}

		public override string ToString()
		{
			return GUIUtils.Format(1277, TextUtils.NumberBonus(PercentOfBaseDamage)) + " " + GUIUtils.Format(1329, GUIUtils.GetDamageTypeString(Type));
		}
	}

	public DamageType Type;

	public float Minimum;

	public float Maximum;

	public DamageDataScaling LevelScaling = new DamageDataScaling();

	public List<DamageProcType> DamageProc = new List<DamageProcType>();

	public DamageType BestOfType = DamageType.None;

	[Tooltip("If set, the attack cannot kill or KO anyone.")]
	public bool NonLethal;

	public bool DoesDamage => Maximum > 0f;

	public float GetMinimum(GameObject owner)
	{
		return GetMinimum(ComponentUtils.GetComponent<CharacterStats>(owner));
	}

	public float GetMinimum(CharacterStats stats)
	{
		if ((bool)stats)
		{
			return LevelScaling.AdjustDamage(Minimum, stats.ScaledLevel);
		}
		return Minimum;
	}

	public float GetMaximum(GameObject owner)
	{
		return GetMaximum(ComponentUtils.GetComponent<CharacterStats>(owner));
	}

	public float GetMaximum(CharacterStats stats)
	{
		if ((bool)stats)
		{
			return LevelScaling.AdjustDamage(Maximum, stats.ScaledLevel);
		}
		return Maximum;
	}

	public float AverageBaseDamage(CharacterStats attacker)
	{
		return (GetMaximum(attacker) + GetMinimum(attacker)) / 2f;
	}

	public DamagePacket()
	{
	}

	public DamagePacket(DamagePacket other)
	{
		Copy(other);
	}

	public void Copy(DamagePacket d)
	{
		Type = d.Type;
		Minimum = d.Minimum;
		Maximum = d.Maximum;
		LevelScaling.CopyFrom(d.LevelScaling);
		foreach (DamageProcType item in d.DamageProc)
		{
			DamageProc.Add(item);
		}
		BestOfType = d.BestOfType;
		NonLethal = d.NonLethal;
	}

	public float RollDamage(CharacterStats roller)
	{
		return RollDamage(roller, 0f);
	}

	public float RollDamage(CharacterStats roller, float MinPercentAdjust)
	{
		float minimum = GetMinimum(roller);
		float maximum = GetMaximum(roller);
		float num = MinPercentAdjust / 100f * (maximum - minimum);
		return OEIRandom.RangeInclusive(minimum + num, maximum);
	}

	public string GetString(CharacterStats attackerStats, AttackBase attack, float multiplier, IEnumerable<StatusEffectParams> statusEffects, bool showBase)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string damageTypeString = GetDamageTypeString();
		float num = 0f;
		if (statusEffects != null)
		{
			foreach (StatusEffectParams statusEffect in statusEffects)
			{
				if (statusEffect.AffectsStat == StatusEffect.ModifiedStat.BonusDamage)
				{
					num += statusEffect.GetValue(attackerStats);
				}
				else if (statusEffect.AffectsStat == StatusEffect.ModifiedStat.BonusDamageMult)
				{
					multiplier += statusEffect.GetValue(attackerStats) - 1f;
				}
			}
		}
		float num2 = multiplier;
		if ((bool)attackerStats && (!attack || !attack.IgnoreCharacterStats))
		{
			num2 += attackerStats.StatDamageHealMultiplier - 1f;
		}
		if ((bool)attack)
		{
			num2 += attack.DamageMultiplier - 1f;
		}
		float minimum = GetMinimum(attackerStats);
		float maximum = GetMaximum(attackerStats);
		float num3 = (minimum + num) * num2;
		float num4 = (maximum + num) * num2;
		string text = num3.ToString("#0");
		string text2 = num4.ToString("#0");
		string text3 = ((!(text != text2)) ? text : GUIUtils.Format(445, text, text2));
		string text4 = (minimum * multiplier).ToString("#0");
		string text5 = (maximum * multiplier).ToString("#0");
		if (showBase && attackerStats != null && (text4 != text || text5 != text2))
		{
			bool isBuff = (num4 > Maximum || num3 > Minimum) && !(num4 < Maximum) && !(num3 < Minimum);
			if (text4 != text5)
			{
				stringBuilder.Append(AttackBase.FormatBase(text3, GUIUtils.Format(1555, GUIUtils.Format(445, text4, text5)), isBuff));
			}
			else
			{
				stringBuilder.Append(AttackBase.FormatBase(text3, GUIUtils.Format(1555, text4), isBuff));
			}
		}
		else
		{
			stringBuilder.Append(text3);
		}
		stringBuilder.Append(" ");
		stringBuilder.Append(damageTypeString);
		return stringBuilder.ToString();
	}

	public string GetBaseRangeString(CharacterStats attackerStats)
	{
		float num = 1f;
		float minimum = GetMinimum(attackerStats);
		float maximum = GetMaximum(attackerStats);
		string text = (minimum * num).ToString("#0");
		string text2 = (maximum * num).ToString("#0");
		return GUIUtils.Format(445, text, text2);
	}

	public string GetDamageTypeString()
	{
		if (BestOfType != DamageType.None)
		{
			return GUIUtils.GetDamageTypeString(Type) + "/" + GUIUtils.GetDamageTypeString(BestOfType);
		}
		return GUIUtils.GetDamageTypeString(Type);
	}
}

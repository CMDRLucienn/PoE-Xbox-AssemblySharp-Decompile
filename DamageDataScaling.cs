using System;
using UnityEngine;

[Serializable]
public class DamageDataScaling : LevelScaling
{
	[Tooltip("Base damage is multiplied by this ratio for each increment.")]
	public float BaseDamageRatioAdjustment = 1f;

	public override bool Empty => BaseDamageRatioAdjustment == 0f;

	public float AdjustDamage(float baseValue, int currentLevel)
	{
		return AdjustPercentValue(baseValue, BaseDamageRatioAdjustment, currentLevel);
	}

	public DamageDataScaling()
	{
	}

	public DamageDataScaling(DamageDataScaling other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(DamageDataScaling other)
	{
		BaseDamageRatioAdjustment = other.BaseDamageRatioAdjustment;
		CopyFrom((LevelScaling)other);
	}
}

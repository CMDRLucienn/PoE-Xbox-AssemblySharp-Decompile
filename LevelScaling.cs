using System;
using UnityEngine;

[Serializable]
public abstract class LevelScaling
{
	[Tooltip("The baseline level (increase is not applied on this level).")]
	[Range(0f, 21f)]
	public int BaseLevel = 1;

	[Tooltip("The increase is applied every [this many] levels after the Base Level.")]
	[Range(1f, 21f)]
	public int LevelIncrement = 1;

	[Tooltip("The level at which the effect stops tracking increases. If 0, it's infinite.")]
	[Range(0f, 21f)]
	public int MaxLevel;

	public abstract bool Empty { get; }

	protected LevelScaling()
	{
	}

	protected LevelScaling(LevelScaling other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(LevelScaling other)
	{
		BaseLevel = other.BaseLevel;
		LevelIncrement = other.LevelIncrement;
		MaxLevel = other.MaxLevel;
	}

	protected float AdjustValue(float baseValue, float adjustmentValue, int currentLevel)
	{
		if (LevelIncrement < 1)
		{
			LevelIncrement = 1;
		}
		if (adjustmentValue == 0f)
		{
			return baseValue;
		}
		if (MaxLevel == 0)
		{
			MaxLevel = int.MaxValue;
		}
		int num = (Mathf.Clamp(currentLevel, 0, MaxLevel) - BaseLevel) / LevelIncrement;
		return baseValue + (float)num * adjustmentValue;
	}

	protected float AdjustPercentValue(float baseValue, float adjustmentValue, int currentLevel)
	{
		if (LevelIncrement < 1)
		{
			LevelIncrement = 1;
		}
		if (adjustmentValue == 1f)
		{
			return baseValue;
		}
		if (MaxLevel == 0)
		{
			MaxLevel = int.MaxValue;
		}
		int num = (Mathf.Clamp(currentLevel, 0, MaxLevel) - BaseLevel) / LevelIncrement;
		if (num < 1)
		{
			return baseValue;
		}
		return baseValue * Mathf.Pow(adjustmentValue, num);
	}
}

using System;

[Serializable]
public class StatusEffectLevelScaling : LevelScaling
{
	public float ValueAdjustment;

	public float ExtraValueAdjustment;

	public float DurationAdjustment;

	public override bool Empty
	{
		get
		{
			if (ValueAdjustment == 0f && ExtraValueAdjustment == 0f)
			{
				return DurationAdjustment == 0f;
			}
			return false;
		}
	}

	public float AdjustValue(float baseValue, int currentLevel)
	{
		return AdjustValue(baseValue, ValueAdjustment, currentLevel);
	}

	public float AdjustExtraValue(float baseValue, int currentLevel)
	{
		return AdjustValue(baseValue, ExtraValueAdjustment, currentLevel);
	}

	public float AdjustDuration(float baseValue, int currentLevel)
	{
		return AdjustValue(baseValue, DurationAdjustment, currentLevel);
	}

	public StatusEffectLevelScaling()
	{
	}

	public StatusEffectLevelScaling(StatusEffectLevelScaling other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(StatusEffectLevelScaling other)
	{
		ValueAdjustment = other.ValueAdjustment;
		ExtraValueAdjustment = other.ExtraValueAdjustment;
		DurationAdjustment = other.DurationAdjustment;
		CopyFrom((LevelScaling)other);
	}

	public string GetValueString()
	{
		if (MaxLevel < int.MaxValue && MaxLevel > 0)
		{
			return GUIUtils.Format(2370, TextUtils.NumberBonus(ValueAdjustment, "#0"), LevelIncrement, BaseLevel, MaxLevel);
		}
		return GUIUtils.Format(2371, TextUtils.NumberBonus(ValueAdjustment, "#0"), LevelIncrement, BaseLevel);
	}

	public string GetDurationString()
	{
		if (MaxLevel < int.MaxValue && MaxLevel > 0)
		{
			return GUIUtils.Format(2370, TextUtils.NumberBonus(DurationAdjustment, "#0.#"), LevelIncrement, BaseLevel, MaxLevel);
		}
		return GUIUtils.Format(2371, TextUtils.NumberBonus(DurationAdjustment, "#0.#"), LevelIncrement, BaseLevel);
	}
}

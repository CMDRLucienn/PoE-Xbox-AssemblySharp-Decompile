using System;

[Serializable]
public class ArmorLevelScaling : LevelScaling
{
	public float DtAdjustment;

	public override bool Empty => DtAdjustment == 0f;

	public float AdjustDT(float baseValue, int currentLevel)
	{
		return AdjustValue(baseValue, DtAdjustment, currentLevel);
	}

	public ArmorLevelScaling()
	{
	}

	public ArmorLevelScaling(ArmorLevelScaling other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(ArmorLevelScaling other)
	{
		DtAdjustment = other.DtAdjustment;
		CopyFrom((LevelScaling)other);
	}
}

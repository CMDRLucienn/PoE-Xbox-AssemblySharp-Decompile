using System;

[Serializable]
public class AttackBaseScaling : LevelScaling
{
	public int AccuracyAdjustment;

	public override bool Empty => AccuracyAdjustment == 0;

	public int AdjustAccuracy(float baseValue, int currentLevel)
	{
		return (int)AdjustValue(baseValue, AccuracyAdjustment, currentLevel);
	}

	public AttackBaseScaling()
	{
	}

	public AttackBaseScaling(AttackBaseScaling other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(AttackBaseScaling other)
	{
		AccuracyAdjustment = other.AccuracyAdjustment;
		CopyFrom((LevelScaling)other);
	}
}

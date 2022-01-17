using System;

[Serializable]
public class SharedData
{
	public SharedStats.StatType SharedStat;

	public SharedStats.ShareType ShareRule;

	public float Value;

	public SharedData(SharedStats.StatType statType, SharedStats.ShareType shareType, float val)
	{
		SharedStat = statType;
		ShareRule = shareType;
		Value = val;
	}
}

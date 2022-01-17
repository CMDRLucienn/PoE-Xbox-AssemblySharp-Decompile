using System;

[Serializable]
public class ConditionalData
{
	public SpellCastData.ConditionalTargetType Target;

	public SpellCastData.ConditionType Condition;

	public SpellCastData.Operator Comparison;

	public int Value;
}

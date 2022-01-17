using System;

[AttributeUsage(AttributeTargets.Method)]
public class AbilityRequirementAttribute : Attribute
{
	public string ParamAbilityIdName { get; private set; }

	public AbilityRequirementAttribute(string paramAbilityIdName)
	{
		ParamAbilityIdName = paramAbilityIdName;
	}
}

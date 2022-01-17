using System;

[AttributeUsage(AttributeTargets.Method)]
public class StatRequirementAttribute : Attribute
{
	public string ParamTypeName { get; private set; }

	public string ParamValueName { get; private set; }

	public bool IsPersonalityReputation { get; private set; }

	public StatRequirementAttribute(string paramTypeName, string paramValueName, bool isPersonalityRep = false)
	{
		ParamTypeName = paramTypeName;
		ParamValueName = paramValueName;
		IsPersonalityReputation = isPersonalityRep;
	}
}

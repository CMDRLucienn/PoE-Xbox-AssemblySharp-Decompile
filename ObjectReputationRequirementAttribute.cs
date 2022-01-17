using System;

[AttributeUsage(AttributeTargets.Method)]
public class ObjectReputationRequirementAttribute : StatRequirementAttribute
{
	public string ParamObjectName { get; private set; }

	public ObjectReputationRequirementAttribute(string paramAxisName, string paramValueName, string paramObjectName)
		: base(paramAxisName, paramValueName, isPersonalityRep: true)
	{
		ParamObjectName = paramObjectName;
	}
}

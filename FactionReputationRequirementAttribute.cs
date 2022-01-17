using System;

[AttributeUsage(AttributeTargets.Method)]
public class FactionReputationRequirementAttribute : StatRequirementAttribute
{
	public string ParamFactionName { get; private set; }

	public FactionReputationRequirementAttribute(string paramAxisName, string paramValueName, string paramFactionName)
		: base(paramAxisName, paramValueName, isPersonalityRep: true)
	{
		ParamFactionName = paramFactionName;
	}
}

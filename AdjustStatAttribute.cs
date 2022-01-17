using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AdjustStatAttribute : Attribute
{
	public string ParamTypeName { get; private set; }

	public string ParamValueName { get; private set; }

	public string ParamLabelName { get; private set; }

	public AdjustStatAttribute(string paramTypeName, string paramValueName)
	{
		ParamTypeName = paramTypeName;
		ParamValueName = paramValueName;
		ParamLabelName = string.Empty;
	}

	public AdjustStatAttribute(string paramTypeName, string paramValueName, string paramLabelName)
	{
		ParamTypeName = paramTypeName;
		ParamValueName = paramValueName;
		ParamLabelName = paramLabelName;
	}
}

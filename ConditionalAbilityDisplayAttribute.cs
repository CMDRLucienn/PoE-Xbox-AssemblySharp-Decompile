using System;
using System.ComponentModel;
using System.Reflection;
using OEIFormats.FlowCharts;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Obsidian Tools can't call methods into the Unity DLL, so this method of handling conditional display will not work.")]
public class ConditionalAbilityDisplayAttribute : ConditionalDisplayAttribute
{
	private readonly string ParamIdName;

	public ConditionalAbilityDisplayAttribute(string paramIdName)
	{
		ParamIdName = paramIdName;
	}

	public override string GetDisplayString(ConditionalCall call)
	{
		ParameterInfo[] parameters = ScriptManager.GetMethodInfo(call).GetParameters();
		int parameterIndexByName = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamIdName);
		object obj = null;
		if (parameterIndexByName >= 0)
		{
			obj = TypeDescriptor.GetConverter(parameters[parameterIndexByName].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName]);
		}
		else
		{
			ConditionalDisplayAttribute.ReportBadParam(call, ParamIdName);
		}
		return "[" + StringTableManager.GetText(DatabaseString.StringTableType.Abilities, (int)obj) + "]";
	}
}

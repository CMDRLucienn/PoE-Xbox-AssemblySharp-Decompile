using System;
using System.ComponentModel;
using System.Reflection;
using OEIFormats.FlowCharts;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Obsidian Tools can't call methods into the Unity DLL, so this method of handling conditional display will not work.")]
public class ConditionalStatDisplayAttribute : ConditionalDisplayAttribute
{
	protected readonly string ParamTypeName;

	protected readonly string ParamValueName;

	public ConditionalStatDisplayAttribute(string paramTypeName, string paramValueName)
	{
		ParamTypeName = paramTypeName;
		ParamValueName = paramValueName;
	}

	public override string GetDisplayString(ConditionalCall call)
	{
		ParameterInfo[] parameters = ScriptManager.GetMethodInfo(call).GetParameters();
		int parameterIndexByName = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamTypeName);
		int parameterIndexByName2 = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamValueName);
		object obj = null;
		if (parameterIndexByName2 >= 0)
		{
			obj = TypeDescriptor.GetConverter(parameters[parameterIndexByName2].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName2]);
		}
		object statObject = null;
		if (parameterIndexByName >= 0)
		{
			statObject = TypeDescriptor.GetConverter(parameters[parameterIndexByName].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName]);
		}
		else
		{
			ConditionalDisplayAttribute.ReportBadParam(call, ParamTypeName);
		}
		if (parameterIndexByName2 >= 0)
		{
			return "[" + GUIUtils.GetPlayerStatString(statObject) + " " + (int)obj + "] ";
		}
		return "[" + GUIUtils.GetPlayerStatString(statObject) + "] ";
	}
}

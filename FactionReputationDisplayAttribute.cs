using System;
using System.ComponentModel;
using System.Reflection;
using OEIFormats.FlowCharts;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Obsidian Tools can't call methods into the Unity DLL, so this method of handling conditional display will not work.")]
public class FactionReputationDisplayAttribute : ConditionalStatDisplayAttribute
{
	private readonly string ParamFactionName;

	public FactionReputationDisplayAttribute(string paramAxisName, string paramValueName, string paramFactionName)
		: base(paramAxisName, paramValueName)
	{
		ParamFactionName = paramFactionName;
	}

	public override bool ShouldShowDisplayString(ConditionalCall call, bool isPlayerResponse)
	{
		if (!isPlayerResponse)
		{
			return GameState.Option.DisplayPersonalityReputationIndicators;
		}
		return true;
	}

	public override string GetDisplayString(ConditionalCall call)
	{
		ParameterInfo[] parameters = ScriptManager.GetMethodInfo(call).GetParameters();
		int parameterIndexByName = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamTypeName);
		int parameterIndexByName2 = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamValueName);
		int parameterIndexByName3 = ConditionalDisplayAttribute.GetParameterIndexByName(parameters, ParamFactionName);
		object statObject = null;
		if (parameterIndexByName2 >= 0)
		{
			statObject = TypeDescriptor.GetConverter(parameters[parameterIndexByName2].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName2]);
		}
		else
		{
			ConditionalDisplayAttribute.ReportBadParam(call, ParamValueName);
		}
		object statObject2 = null;
		if (parameterIndexByName >= 0)
		{
			statObject2 = TypeDescriptor.GetConverter(parameters[parameterIndexByName].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName]);
		}
		else
		{
			ConditionalDisplayAttribute.ReportBadParam(call, ParamTypeName);
		}
		object statObject3 = null;
		if (parameterIndexByName3 >= 0)
		{
			statObject3 = TypeDescriptor.GetConverter(parameters[parameterIndexByName3].ParameterType).ConvertFromString(call.Data.Parameters[parameterIndexByName3]);
		}
		else
		{
			ConditionalDisplayAttribute.ReportBadParam(call, ParamFactionName);
		}
		return StringUtility.Format("[{2}: {0} {1}] ", GUIUtils.GetPlayerStatString(statObject2), GUIUtils.GetPlayerStatString(statObject), GUIUtils.GetPlayerStatString(statObject3));
	}
}

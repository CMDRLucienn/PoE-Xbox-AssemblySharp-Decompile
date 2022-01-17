using System;
using System.Reflection;
using OEIFormats.FlowCharts;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
[Obsolete("Obsidian Tools can't call methods into the Unity DLL, so this method of handling conditional display will not work.")]
public abstract class ConditionalDisplayAttribute : Attribute
{
	public virtual bool ShouldShowDisplayString(ConditionalCall call, bool isPlayerResponse)
	{
		return true;
	}

	public abstract string GetDisplayString(ConditionalCall call);

	protected static int GetParameterIndexByName(ParameterInfo[] infos, string name)
	{
		for (int i = 0; i < infos.Length; i++)
		{
			if (name == infos[i].Name)
			{
				return i;
			}
		}
		return -1;
	}

	protected static void ReportBadParam(ConditionalCall call, string parameter)
	{
		string text = "Conditional '" + call.Data.FullName + "': parameter '" + parameter + "' specified in an attribute wasn't found.";
		if ((bool)UIDebug.Instance)
		{
			UIDebug.Instance.LogOnScreenWarning(text, UIDebug.Department.Programming, 10f);
		}
		else
		{
			Debug.LogError(text);
		}
	}
}

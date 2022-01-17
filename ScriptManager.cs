using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OEIFormats.FlowCharts;

public static class ScriptManager
{
	public static Dictionary<string, MethodInfo> ConditionalMethods;

	public static Dictionary<string, MethodInfo> ScriptMethods;

	static ScriptManager()
	{
		ConditionalMethods = new Dictionary<string, MethodInfo>();
		ScriptMethods = new Dictionary<string, MethodInfo>();
		LoadScripts();
	}

	public static void LoadScripts()
	{
		if (ConditionalMethods.Count == 0 && ScriptMethods.Count == 0)
		{
			MethodInfo[] methods = typeof(Conditionals).GetMethods();
			MethodInfo[] methods2 = typeof(Scripts).GetMethods();
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				ConditionalMethods.Add(ConvertMethodInfoToString(methodInfo), methodInfo);
			}
			array = methods2;
			foreach (MethodInfo methodInfo2 in array)
			{
				ScriptMethods.Add(ConvertMethodInfoToString(methodInfo2), methodInfo2);
			}
		}
	}

	public static string ConvertMethodInfoToString(MethodInfo methodInfo)
	{
		StringBuilder stringBuilder = new StringBuilder(methodInfo.ReturnType.Name);
		stringBuilder.Append(StringUtility.Format(" {0}(", methodInfo.Name));
		ParameterInfo[] parameters = methodInfo.GetParameters();
		for (int i = 0; i < parameters.Length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(parameters[i].ParameterType.Name);
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	public static MethodInfo GetMethodInfo(ConditionalCall conditionalCall)
	{
		MethodInfo value = null;
		ConditionalMethods.TryGetValue(conditionalCall.Data.FullName, out value);
		return value;
	}

	public static MethodInfo GetMethodInfo(ScriptCall scriptCall)
	{
		MethodInfo value = null;
		ScriptMethods.TryGetValue(scriptCall.Data.FullName, out value);
		return value;
	}

	public static MethodInfo GetMethodInfo(string callName)
	{
		MethodInfo value = null;
		ScriptMethods.TryGetValue(callName, out value);
		return value;
	}
}

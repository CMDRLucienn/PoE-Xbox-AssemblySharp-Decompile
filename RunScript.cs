using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

public class RunScript : MonoBehaviour
{
	public string FunctionName = "";

	public string Parameter1 = "";

	public string Parameter2 = "";

	public string Parameter3 = "";

	public string Parameter4 = "";

	private MethodInfo m_methodInfo;

	private List<object> parameters = new List<object>();

	private void Start()
	{
		MethodInfo[] methods = typeof(Scripts).GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.ToString().Contains(FunctionName))
			{
				m_methodInfo = methodInfo;
				break;
			}
		}
		if (m_methodInfo == null)
		{
			return;
		}
		ParameterInfo[] array = m_methodInfo.GetParameters();
		parameters = new List<object>();
		string[] array2 = new string[4] { Parameter1, Parameter2, Parameter3, Parameter4 };
		for (int j = 0; j < array.Length; j++)
		{
			ParameterInfo parameterInfo = array[j];
			if (parameterInfo.ParameterType.IsEnum)
			{
				object obj = null;
				try
				{
					obj = Enum.Parse(parameterInfo.ParameterType, array2[j]);
				}
				catch
				{
					obj = Enum.GetValues(parameterInfo.ParameterType).GetValue(0);
				}
				parameters.Add(obj);
			}
			else if (parameterInfo.ParameterType.IsValueType)
			{
				if (parameterInfo.ParameterType == typeof(Guid))
				{
					Guid empty = Guid.Empty;
					try
					{
						empty = new Guid(array2[j]);
					}
					catch
					{
						empty = Guid.Empty;
					}
					parameters.Add(empty);
					continue;
				}
				try
				{
					parameters.Add(Convert.ChangeType(array2[j], parameterInfo.ParameterType, CultureInfo.InvariantCulture));
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to execute script " + FunctionName + ". Could not convert " + array2[j] + " into " + parameterInfo.ParameterType.Name + ". " + ex.Message);
					break;
				}
			}
			else
			{
				parameters.Add(array2[j]);
			}
		}
	}

	public void OnDefaultAction()
	{
		if (!(m_methodInfo == null))
		{
			m_methodInfo.Invoke(null, parameters.ToArray());
		}
	}
}

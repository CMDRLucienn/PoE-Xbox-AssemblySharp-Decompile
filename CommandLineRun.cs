using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class CommandLineRun
{
	private static MethodInfo[] s_ScriptsMethods = typeof(Scripts).GetMethods();

	private static MethodInfo[] s_CommandLineMethods = typeof(CommandLine).GetMethods();

	public static void RunCommand(string command)
	{
		if (string.IsNullOrEmpty(command) || command.ToLower() == "runcommand")
		{
			return;
		}
		IList<string> list = StringUtility.CommandLineStyleSplit(command);
		bool flag = false;
		bool flag2 = false;
		string error = "";
		foreach (MethodInfo allMethod in GetAllMethods())
		{
			if (string.Compare(allMethod.Name, list[0], ignoreCase: true) != 0)
			{
				continue;
			}
			if (!MethodIsAvailable(allMethod))
			{
				flag = true;
				continue;
			}
			if (FillMethodParams(allMethod, list, out var param, out error))
			{
				allMethod.Invoke(null, param);
				return;
			}
			flag2 = true;
		}
		if (flag2)
		{
			Console.AddMessage("Command or script '" + list[0] + "' parameter error: " + error, Color.yellow);
		}
		else if (flag)
		{
			Console.AddMessage("The command or script '" + list[0] + "' is not available at this time.", Color.yellow);
		}
		else
		{
			Console.AddMessage("No command or script named '" + list[0] + "' exists.", Color.yellow);
		}
	}

	public static bool FillMethodParams(MethodInfo methodInfo, IList<string> stringparams, out object[] param, out string error)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters == null)
		{
			param = null;
			error = "failed to GetParameters";
			return false;
		}
		if (parameters.Length != stringparams.Count - 1)
		{
			param = null;
			error = "doesn't accept " + (stringparams.Count - 1) + " parameters";
			return false;
		}
		param = new object[parameters.Length];
		int i;
		for (i = 0; i < parameters.Length; i++)
		{
			Scripts.BrowserType browserType = Scripts.BrowserType.None;
			object[] customAttributes = methodInfo.GetCustomAttributes(BaseScriptParamAttribute.ScriptAttributeTypes[i], inherit: false);
			if (customAttributes.Length != 0)
			{
				browserType = ((BaseScriptParamAttribute)customAttributes[0]).Browser;
			}
			Type parameterType = parameters[i].ParameterType;
			if (parameterType.IsEnum)
			{
				try
				{
					param[i] = Enum.Parse(parameterType, stringparams[i + 1], ignoreCase: true);
				}
				catch (ArgumentException)
				{
					param[i] = Enum.GetValues(parameterType).GetValue(0);
				}
				continue;
			}
			if (browserType == Scripts.BrowserType.ObjectGuid || parameterType == typeof(Guid))
			{
				try
				{
					param[i] = new Guid(stringparams[i + 1]);
				}
				catch (FormatException)
				{
					SpecialCharacterInstanceID.SpecialCharacterInstance specialCharacterInstance = SpecialCharacterInstanceID.Parse(stringparams[i + 1]);
					CompanionNames.Companions companions = CompanionNames.Parse(stringparams[i + 1]);
					if (specialCharacterInstance != 0)
					{
						param[i] = SpecialCharacterInstanceID.GetSpecialGUID(specialCharacterInstance);
						continue;
					}
					if (companions != 0)
					{
						param[i] = CompanionInstanceID.GetSpecialGuid(companions);
						continue;
					}
					GameObject gameObject = GameObject.Find(stringparams[i + 1]);
					if ((bool)gameObject)
					{
						InstanceID component = gameObject.GetComponent<InstanceID>();
						if ((bool)component)
						{
							param[i] = component.Guid;
						}
						else
						{
							param[i] = Guid.Empty;
						}
						continue;
					}
					IEnumerable<InstanceID> enumerable = from iid in UnityEngine.Object.FindObjectsOfType<InstanceID>()
						where iid.name.IndexOf(stringparams[i + 1], StringComparison.OrdinalIgnoreCase) >= 0
						select iid;
					if (enumerable.Any())
					{
						if (enumerable.AnyX(2))
						{
							Console.AddMessage("Multiple objects matched '" + stringparams[i + 1] + "'.", Color.yellow);
							foreach (InstanceID item in enumerable)
							{
								Console.AddMessage(item.name, Color.yellow);
							}
						}
						else
						{
							Console.AddMessage("Interpreting '" + stringparams[i + 1] + "' as '" + enumerable.First().name + "'.");
							param[i] = enumerable.First().Guid;
						}
					}
					else
					{
						param[i] = Guid.Empty;
						Console.AddMessage("Couldn't find an object matching '" + stringparams[i + 1] + "'.", Color.yellow);
					}
				}
				continue;
			}
			switch (browserType)
			{
			case Scripts.BrowserType.Quest:
			{
				List<string> list2 = QuestManager.Instance.FindLoadedQuests(stringparams[i + 1]);
				if (list2.Count == 0)
				{
					param[i] = stringparams[i + 1];
					continue;
				}
				if (list2.Count == 1)
				{
					param[i] = list2[0];
					continue;
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("Multiple quests matched '" + stringparams[i + 1] + "':");
				foreach (string item2 in list2)
				{
					stringBuilder2.Append("  ");
					stringBuilder2.AppendLine(Path.GetFileNameWithoutExtension(item2));
				}
				error = stringBuilder2.ToString().Trim();
				return false;
			}
			case Scripts.BrowserType.Conversation:
			{
				List<string> list = ConversationManager.Instance.FindConversations(stringparams[i + 1]);
				if (list.Count == 0)
				{
					param[i] = stringparams[i + 1];
					continue;
				}
				if (list.Count == 1)
				{
					param[i] = list[0];
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Multiple conversations matched '" + stringparams[i + 1] + "':");
				foreach (string item3 in list)
				{
					stringBuilder.Append("  ");
					stringBuilder.AppendLine(Path.GetFileNameWithoutExtension(item3));
				}
				error = stringBuilder.ToString().Trim();
				return false;
			}
			}
			if (parameterType == typeof(bool))
			{
				if (string.Compare(stringparams[i + 1], bool.TrueString, ignoreCase: true) == 0 || string.Compare(stringparams[i + 1], "1", ignoreCase: true) == 0 || string.Compare(stringparams[i + 1], "yes", ignoreCase: true) == 0 || string.Compare(stringparams[i + 1], "on", ignoreCase: true) == 0)
				{
					param[i] = true;
					continue;
				}
				if (string.Compare(stringparams[i + 1], bool.FalseString, ignoreCase: true) != 0 && string.Compare(stringparams[i + 1], "0", ignoreCase: true) != 0 && string.Compare(stringparams[i + 1], "no", ignoreCase: true) != 0 && string.Compare(stringparams[i + 1], "off", ignoreCase: true) != 0)
				{
					error = "could not convert parameter " + (i + 1) + " ('" + stringparams[i + 1] + "') into type " + parameterType.Name;
					return false;
				}
				param[i] = false;
			}
			else if (parameterType.IsValueType)
			{
				try
				{
					param[i] = Convert.ChangeType(stringparams[i + 1], parameterType, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					error = "could not convert parameter " + (i + 1) + " ('" + stringparams[i + 1] + "') into type " + parameterType.Name;
					return false;
				}
			}
			else
			{
				param[i] = stringparams[i + 1];
			}
		}
		error = "";
		return true;
	}

	public static IEnumerable<string> GetPossibleCompletions(string command)
	{
		IList<string> stringparams = StringUtility.CommandLineStyleSplit(command);
		if (stringparams.Count == 0)
		{
			return null;
		}
		return (from s in GetPossibleValues(command)?.Where((string s) => s.StartsWith(stringparams.LastOrDefault(), StringComparison.InvariantCultureIgnoreCase))
			orderby s
			select s);
	}

	public static IEnumerable<string> GetPossibleValues(string command)
	{
		IList<string> list = StringUtility.CommandLineStyleSplit(command);
		int num = list.Count - 2;
		if (num < 0)
		{
			return from info in GetAllMethods()
				select info.Name;
		}
		foreach (MethodInfo allMethod in GetAllMethods())
		{
			if (string.Compare(allMethod.Name, list[0], ignoreCase: true) == 0)
			{
				ParameterInfo[] parameters = allMethod.GetParameters();
				if (parameters.Length < num)
				{
					return null;
				}
				BaseScriptParamAttribute scriptAttribute = allMethod.GetCustomAttributes(BaseScriptParamAttribute.ScriptAttributeTypes[num], inherit: false).FirstOrDefault() as BaseScriptParamAttribute;
				return GetPossibleValues(parameters[num], scriptAttribute);
			}
		}
		return null;
	}

	public static IEnumerable<string> GetPossibleValues(ParameterInfo info, BaseScriptParamAttribute scriptAttribute)
	{
		Type parameterType = info.ParameterType;
		Scripts.BrowserType browserType = Scripts.BrowserType.None;
		if (scriptAttribute != null)
		{
			browserType = scriptAttribute.Browser;
		}
		if (parameterType.IsEnum)
		{
			return Enum.GetNames(parameterType);
		}
		if (parameterType == typeof(bool))
		{
			return new string[2] { "true", "false" };
		}
		if (browserType == Scripts.BrowserType.ObjectGuid || parameterType == typeof(Guid))
		{
			return (from iid in UnityEngine.Object.FindObjectsOfType<InstanceID>()
				select iid.name).Concat(SpecialCharacterInstanceID.s_specialNames).Concat(CompanionNames.s_companionNames);
		}
		return browserType switch
		{
			Scripts.BrowserType.Quest => from path in QuestManager.Instance.GetLoadedQuestPaths()
				select Path.GetFileNameWithoutExtension(path), 
			Scripts.BrowserType.Conversation => from path in ConversationManager.Instance.GetAllConversationPaths()
				select Path.GetFileNameWithoutExtension(path), 
			Scripts.BrowserType.GlobalVariable => GlobalVariables.Instance.GetAllVariableNames(), 
			_ => null, 
		};
	}

	public static IEnumerable<MethodInfo> GetAllMethods()
	{
		MethodInfo[] array = s_ScriptsMethods;
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
		array = s_CommandLineMethods;
		foreach (MethodInfo methodInfo in array)
		{
			if (!methodInfo.IsPrivate)
			{
				yield return methodInfo;
			}
		}
	}

	public static bool MethodIsAvailable(MethodInfo method)
	{
		if (!GameState.Instance.CheatsEnabled)
		{
			MethodInfo[] array = s_CommandLineMethods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo == method && methodInfo.IsPublic && methodInfo.GetCustomAttributes(typeof(CheatAttribute), inherit: true).Length == 0)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static IEnumerable<MethodInfo> GetCheatMethods()
	{
		MethodInfo[] array = s_ScriptsMethods;
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
		array = s_CommandLineMethods;
		foreach (MethodInfo methodInfo in array)
		{
			if (!methodInfo.IsPrivate && methodInfo.GetCustomAttributes(typeof(CheatAttribute), inherit: true).Length != 0)
			{
				yield return methodInfo;
			}
		}
	}

	public static IEnumerable<MethodInfo> GetNonCheatMethods()
	{
		MethodInfo[] array = s_CommandLineMethods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.IsPublic && methodInfo.GetCustomAttributes(typeof(CheatAttribute), inherit: true).Length == 0)
			{
				yield return methodInfo;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using OEIFormats.FlowCharts;
using UnityEngine;

public class ScriptEvent : MonoBehaviour
{
	public enum ScriptEvents
	{
		Invalid = -1,
		OnEncounterAlert,
		OnCutsceneStart,
		OnCutsceneEnd,
		OnExamineStart,
		OnExamineEnd,
		OnItemCollected,
		OnTimerFinished,
		OnEnable,
		OnDisable,
		OnOpen,
		OnClose,
		OnLocked,
		OnUnlocked,
		OnUsed,
		OnLockUsed,
		OnEncounterStart,
		OnEncounterEnd,
		OnPartyMemberEnter,
		OnPartyMemberExit,
		OnHostileEnter,
		OnHostileExit,
		OnDeath,
		OnLevelLoaded,
		OnArrive,
		OnDetected,
		OnPerceptionPerLoad,
		OnTriggered,
		OnDisarmed,
		OnFadeFromBlackFinished,
		OnFadeToBlackFinished,
		OnHealthPercent75,
		OnHealthPercent50,
		OnHealthPercent25,
		OnNeutralEnter,
		OnNeutralExit,
		OnCharacterCreationClosed,
		OnRestFinished,
		OnInterstitialClosed,
		OnAttacked,
		OnAttackedByParty,
		OnContainerEmpty,
		OnUnconscious,
		OnItemInspected,
		OnContainerItemTaken,
		OnPerception,
		OnLevelMusicLoaded,
		OnPartyMemberEnterWhileNonStealthed,
		OnPartyMemberEnterWhileStealthed,
		OnCaughtItemStealing,
		OnItemDroppedAsLoot
	}

	private class ScriptCallHistoryEvent
	{
		public ScriptCall Call;

		public string Error;

		private DateTime m_Timestamp;

		public ScriptCallHistoryEvent(ScriptCall call)
		{
			m_Timestamp = DateTime.Now;
			Call = call;
		}

		private static string GetFunctionName(ScriptCall call)
		{
			if (call == null)
			{
				return "*null*";
			}
			int num = 0;
			for (int i = 0; i < call.Data.FullName.Length; i++)
			{
				if (call.Data.FullName[i] == ' ')
				{
					num = i + 1;
				}
				else if (call.Data.FullName[i] == '(' || i == call.Data.FullName.Length - 1)
				{
					return call.Data.FullName.Substring(num, i - num);
				}
			}
			return call.Data.FullName;
		}

		public override string ToString()
		{
			string text = string.Concat(m_Timestamp.ToLocalTime(), ": ", GetFunctionName(Call), "(", TextUtils.Join(Call.Data.Parameters, ", ", removeEmpty: false), ")");
			if (!string.IsNullOrEmpty(Error))
			{
				return "[FF0000]" + text + " (" + Error + ")[-]";
			}
			return text;
		}
	}

	private static List<ScriptEvent> ActiveScriptEventObjects = new List<ScriptEvent>();

	public List<ActionScript> Scripts = new List<ActionScript>();

	public static bool DisplayRecentScripts = false;

	private static List<ScriptCallHistoryEvent> s_ScriptDebugHistory = new List<ScriptCallHistoryEvent>();

	private const int ScriptDebugHistoryCapacity = 16;

	private void OnEnable()
	{
		ExecuteScript(ScriptEvents.OnEnable);
		ActiveScriptEventObjects.Add(this);
	}

	private void OnDisable()
	{
		ExecuteScript(ScriptEvents.OnDisable);
		ActiveScriptEventObjects.Remove(this);
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public bool HasScriptType(ScriptEvents scriptType)
	{
		foreach (ActionScript script in Scripts)
		{
			if (script != null && script.eventType == scriptType)
			{
				return true;
			}
		}
		return false;
	}

	public void ExecuteScript(ScriptEvents eventType)
	{
		foreach (ActionScript script in Scripts)
		{
			if (script == null || script.eventType != eventType || (script.Conditionals.Count > 0 && !EvaluateConditional(script.Conditionals)) || script.ScriptCalls == null)
			{
				continue;
			}
			foreach (OEIScriptCallData scriptCall2 in script.ScriptCalls)
			{
				ScriptCall scriptCall = new ScriptCall(scriptCall2.FullName, scriptCall2.Parameters);
				Execute(scriptCall);
			}
		}
	}

	private bool EvaluateConditional(List<ConditionalScript> Conditionals)
	{
		bool flag = true;
		int num = 0;
		LogicalOperator logicalOperator = LogicalOperator.Or;
		foreach (ConditionalScript Conditional in Conditionals)
		{
			if (Conditional != null)
			{
				bool flag2 = ConditionalToggle.EvaluateConditional(new ConditionalCall
				{
					Operator = Conditional.Op,
					Not = Conditional.Not,
					Data = 
					{
						FullName = Conditional.Function,
						Parameters = Conditional.Parameters
					}
				}, base.gameObject);
				flag = ((num == 0) ? flag2 : ((logicalOperator != LogicalOperator.Or) ? (flag2 && flag) : (flag2 || flag)));
				logicalOperator = Conditional.Op;
				num++;
			}
		}
		return flag;
	}

	protected void Execute(ScriptCall scriptCall)
	{
		bool flag = false;
		MethodInfo methodInfo = ScriptManager.GetMethodInfo(scriptCall);
		if (methodInfo == null)
		{
			return;
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (scriptCall.Data.Parameters.Count != parameters.Length)
		{
			Debug.LogError("Failed to execute script " + scriptCall.Data.FullName + " because of mismatched parameter count.", this);
			return;
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType == typeof(Guid) && SpecialCharacterInstanceID.GetSpecialTypeFromGuid(GetGuidFromParam(scriptCall.Data.Parameters[i])) == SpecialCharacterInstanceID.SpecialCharacterInstance.Party)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			RunScript(scriptCall);
			return;
		}
		for (int j = 0; j < PartyMemberAI.PartyMembers.Length; j++)
		{
			if (!(PartyMemberAI.PartyMembers[j] == null))
			{
				SpecialCharacterInstanceID.Add(PartyMemberAI.PartyMembers[j].gameObject, SpecialCharacterInstanceID.SpecialCharacterInstance.Party);
				RunScript(scriptCall);
			}
		}
	}

	protected void RunScript(ScriptCall scriptCall)
	{
		SpecialCharacterInstanceID.Add(base.gameObject, SpecialCharacterInstanceID.SpecialCharacterInstance.This);
		RunScriptHelper(scriptCall, this);
	}

	public static void RunScriptHelper(ScriptCall scriptCall, UnityEngine.Object context)
	{
		ScriptCallHistoryEvent scriptCallHistoryEvent = new ScriptCallHistoryEvent(scriptCall);
		if (s_ScriptDebugHistory.Count >= 16)
		{
			s_ScriptDebugHistory.RemoveAt(0);
		}
		s_ScriptDebugHistory.Add(scriptCallHistoryEvent);
		MethodInfo methodInfo = ScriptManager.GetMethodInfo(scriptCall);
		if (methodInfo == null)
		{
			Debug.LogError("Failed to find script " + scriptCall.Data.FullName + ".", context);
			scriptCallHistoryEvent.Error = "Failed to find script";
			return;
		}
		List<object> list = new List<object>();
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (scriptCall.Data.Parameters.Count != parameters.Length)
		{
			Debug.LogError("Failed to execute script " + scriptCall.Data.FullName + " because of mismatched parameter count.", context);
			scriptCallHistoryEvent.Error = "Mismatched parameter count";
			return;
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			if (parameterType.IsEnum)
			{
				object obj = null;
				try
				{
					obj = Enum.Parse(parameterType, scriptCall.Data.Parameters[i]);
				}
				catch
				{
					obj = Enum.GetValues(parameterType).GetValue(0);
				}
				list.Add(obj);
			}
			else if (parameterType.IsValueType)
			{
				if (parameterType == typeof(Guid))
				{
					list.Add(GetGuidFromParam(scriptCall.Data.Parameters[i]));
					continue;
				}
				try
				{
					list.Add(Convert.ChangeType(scriptCall.Data.Parameters[i], parameterType, CultureInfo.InvariantCulture));
				}
				catch (Exception exception)
				{
					string text = "Could not convert " + scriptCall.Data.Parameters[i] + " into " + parameterType.Name;
					Debug.LogException(exception, context);
					Debug.LogError("Failed to execute script " + scriptCall.Data.FullName + ". " + text + ".", context);
					scriptCallHistoryEvent.Error = text;
					return;
				}
			}
			else
			{
				list.Add(scriptCall.Data.Parameters[i]);
			}
		}
		try
		{
			methodInfo.Invoke(null, list.ToArray());
		}
		catch (Exception exception2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (list != null)
			{
				foreach (object item in list)
				{
					if (item != null)
					{
						stringBuilder.Append(" " + item.ToString());
					}
				}
			}
			Debug.LogException(exception2, context);
			Debug.LogError(string.Concat("Failed to execute script event script ", scriptCall.Data.FullName, "(", stringBuilder, ")."), context);
			scriptCallHistoryEvent.Error = "Uncaught exception, see log";
		}
	}

	public static Guid GetGuidFromParam(string param)
	{
		Guid empty = Guid.Empty;
		try
		{
			return new Guid(param);
		}
		catch
		{
			return Guid.Empty;
		}
	}

	public static void BroadcastEvent(ScriptEvents eventType)
	{
		for (int i = 0; i < ActiveScriptEventObjects.Count; i++)
		{
			if (ActiveScriptEventObjects[i] != null)
			{
				ActiveScriptEventObjects[i].ExecuteScript(eventType);
			}
		}
	}

	public static void DrawScriptHistory()
	{
		UIDebug.Instance.SetText("Script History Debug", TextUtils.FuncJoin((ScriptCallHistoryEvent s) => s.ToString(), s_ScriptDebugHistory, "\n"), Color.cyan);
		UIDebug.Instance.SetTextPosition("Script History Debug", 0.05f, 0.95f, UIWidget.Pivot.TopLeft);
	}
}

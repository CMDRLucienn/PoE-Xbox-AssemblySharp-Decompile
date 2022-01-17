using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using OEIFormats.FlowCharts;
using UnityEngine;

public class ConditionalToggle : MonoBehaviour
{
	private enum QueuedConditionalState
	{
		None,
		ScriptInactive,
		Active,
		Inactive
	}

	public bool CheckOnce = true;

	public bool ActivateOnlyThroughScript;

	[Persistent]
	public bool StartActivated;

	private bool Checked;

	private QueuedConditionalState mQueuedConditionalState;

	public List<ConditionalScript> Scripts = new List<ConditionalScript>();

	private List<ConditionalCall> InternalScripts;

	private void Awake()
	{
		InternalScripts = new List<ConditionalCall>();
		foreach (ConditionalScript script in Scripts)
		{
			ConditionalCall conditionalCall = new ConditionalCall();
			conditionalCall.Operator = script.Op;
			conditionalCall.Not = script.Not;
			conditionalCall.Data.FullName = script.Function;
			conditionalCall.Data.Parameters = script.Parameters;
			InternalScripts.Add(conditionalCall);
		}
	}

	private void OnEnable()
	{
		if (GetComponent<Persistence>() != null && !GameState.LoadedGame && !GameState.IsRestoredLevel)
		{
			RunEnabledCheck();
		}
	}

	public void Restored()
	{
		if (GameState.LoadedGame || GameState.IsRestoredLevel)
		{
			RunEnabledCheck();
		}
	}

	private void Update()
	{
		if (ConditionalToggleManager.Instance != null)
		{
			switch (mQueuedConditionalState)
			{
			case QueuedConditionalState.Active:
				ConditionalToggleManager.Instance.AddToActiveList(this);
				break;
			case QueuedConditionalState.Inactive:
				ConditionalToggleManager.Instance.AddToInactiveList(this);
				break;
			case QueuedConditionalState.ScriptInactive:
				ConditionalToggleManager.Instance.AddToScriptInactiveList(this);
				break;
			}
			mQueuedConditionalState = QueuedConditionalState.None;
		}
	}

	private void RunEnabledCheck()
	{
		if (Checked)
		{
			return;
		}
		if (ActivateOnlyThroughScript)
		{
			if (!StartActivated || !base.gameObject.activeInHierarchy)
			{
				InstanceID component = base.gameObject.GetComponent<InstanceID>();
				base.gameObject.SetActive(value: false);
				Encounter component2 = base.gameObject.GetComponent<Encounter>();
				if (component2 != null)
				{
					component2.DisableInstances();
				}
				else
				{
					component2 = base.gameObject.GetComponentInParent<Encounter>();
					if (component2 != null)
					{
						component2.DisableInstance(base.gameObject);
					}
				}
				if (component != null)
				{
					component.Load();
					if (ConditionalToggleManager.Instance != null)
					{
						ConditionalToggleManager.Instance.AddToScriptInactiveList(this);
					}
					else
					{
						mQueuedConditionalState = QueuedConditionalState.ScriptInactive;
					}
				}
			}
		}
		else if (Evaluate())
		{
			if (!CheckOnce)
			{
				if (ConditionalToggleManager.Instance != null)
				{
					ConditionalToggleManager.Instance.AddToActiveList(this);
				}
				else
				{
					mQueuedConditionalState = QueuedConditionalState.Active;
				}
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
			InstanceID component3 = base.gameObject.GetComponent<InstanceID>();
			Encounter component4 = base.gameObject.GetComponent<Encounter>();
			if (component4 != null)
			{
				component4.DisableInstances();
			}
			else
			{
				component4 = base.gameObject.GetComponentInParent<Encounter>();
				if (component4 != null)
				{
					component4.DisableInstance(base.gameObject);
				}
			}
			if (component3 != null)
			{
				component3.Load();
			}
			if (!CheckOnce)
			{
				if (ConditionalToggleManager.Instance != null)
				{
					ConditionalToggleManager.Instance.AddToInactiveList(this);
				}
				else
				{
					mQueuedConditionalState = QueuedConditionalState.Inactive;
				}
			}
		}
		Checked = true;
	}

	public void ForceActivate()
	{
		StartActivated = true;
	}

	public bool Evaluate()
	{
		return Evaluate(InternalScripts, base.gameObject);
	}

	public static bool Evaluate(List<ConditionalCall> scripts, GameObject owner)
	{
		bool flag = true;
		int num = 0;
		LogicalOperator logicalOperator = LogicalOperator.Or;
		foreach (ConditionalCall script in scripts)
		{
			if (script != null)
			{
				bool flag2 = EvaluateConditional(script, owner);
				flag = ((num == 0) ? flag2 : ((logicalOperator != LogicalOperator.Or) ? (flag2 && flag) : (flag2 || flag)));
				logicalOperator = script.Operator;
				num++;
			}
		}
		return flag;
	}

	public static bool EvaluateConditional(ConditionalCall conditional, GameObject gameObject)
	{
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		MethodInfo methodInfo = ScriptManager.GetMethodInfo(conditional);
		if (methodInfo == null)
		{
			return false;
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].ParameterType == typeof(Guid))
			{
				switch (SpecialCharacterInstanceID.GetSpecialTypeFromGuid(ScriptEvent.GetGuidFromParam(conditional.Data.Parameters[i])))
				{
				case SpecialCharacterInstanceID.SpecialCharacterInstance.PartyAll:
					flag2 = true;
					break;
				case SpecialCharacterInstanceID.SpecialCharacterInstance.PartyAny:
					flag2 = true;
					flag3 = true;
					break;
				default:
					continue;
				}
				break;
			}
		}
		if (!flag2)
		{
			flag = RunConditional(conditional, gameObject);
		}
		else
		{
			for (int j = 0; j < PartyMemberAI.PartyMembers.Length; j++)
			{
				if (PartyMemberAI.PartyMembers[j] == null || PartyMemberAI.PartyMembers[j].Secondary)
				{
					continue;
				}
				SpecialCharacterInstanceID.Add(PartyMemberAI.PartyMembers[j].gameObject, flag3 ? SpecialCharacterInstanceID.SpecialCharacterInstance.PartyAny : SpecialCharacterInstanceID.SpecialCharacterInstance.PartyAll);
				bool flag4 = RunConditional(conditional, gameObject);
				if (flag3)
				{
					if (flag4)
					{
						flag = true;
						break;
					}
					flag = false;
				}
				else if (!flag4)
				{
					flag = false;
					break;
				}
			}
		}
		if (conditional.Not)
		{
			return !flag;
		}
		return flag;
	}

	private static bool RunConditional(ConditionalCall conditional, GameObject gameObject)
	{
		bool flag = false;
		MethodInfo methodInfo = ScriptManager.GetMethodInfo(conditional);
		if (methodInfo == null)
		{
			Debug.LogWarning("Failed to find conditional " + conditional.Data.FullName + ".");
			return false;
		}
		List<object> list = new List<object>();
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (conditional.Data.Parameters.Count != parameters.Length)
		{
			Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + " because of mismatched parameter count.");
			return false;
		}
		SpecialCharacterInstanceID.Add(gameObject, SpecialCharacterInstanceID.SpecialCharacterInstance.This);
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			if (parameterInfo.ParameterType.IsEnum)
			{
				object obj = null;
				try
				{
					obj = Enum.Parse(parameterInfo.ParameterType, conditional.Data.Parameters[i]);
				}
				catch
				{
					obj = Enum.GetValues(parameterInfo.ParameterType).GetValue(0);
				}
				list.Add(obj);
			}
			else if (parameterInfo.ParameterType.IsValueType)
			{
				if (parameterInfo.ParameterType == typeof(Guid))
				{
					list.Add(ScriptEvent.GetGuidFromParam(conditional.Data.Parameters[i]));
					continue;
				}
				try
				{
					list.Add(Convert.ChangeType(conditional.Data.Parameters[i], parameterInfo.ParameterType, CultureInfo.InvariantCulture));
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + ". Could not convert " + conditional.Data.Parameters[i] + " into " + parameterInfo.ParameterType.Name + ".");
					return false;
				}
			}
			else
			{
				list.Add(conditional.Data.Parameters[i]);
			}
		}
		try
		{
			return (bool)methodInfo.Invoke(null, list.ToArray());
		}
		catch (Exception innerException)
		{
			while (innerException != null)
			{
				Debug.LogException(innerException);
				innerException = innerException.InnerException;
			}
			Debug.LogError("Failed to execute conditional " + conditional.Data.FullName + ".");
			return false;
		}
	}
}

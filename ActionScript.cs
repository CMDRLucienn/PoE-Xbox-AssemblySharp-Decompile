using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionScript : ISerializationCallbackReceiver
{
	public const int MAX_SCRIPT_CALLS = 10;

	[Obsolete("See ScriptCalls for the new data.")]
	public string Function;

	[Obsolete("See ScriptCalls for the new data.")]
	public List<string> Parameters = new List<string>();

	public ScriptEvent.ScriptEvents eventType;

	public List<ConditionalScript> Conditionals = new List<ConditionalScript>();

	public List<OEIScriptCallData> ScriptCalls = new List<OEIScriptCallData>();

	public ActionScript()
	{
		eventType = ScriptEvent.ScriptEvents.Invalid;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (!string.IsNullOrEmpty(Function))
		{
			OEIScriptCallData oEIScriptCallData = new OEIScriptCallData();
			oEIScriptCallData.FullName = Function;
			oEIScriptCallData.Parameters = new List<string>(Parameters);
			ScriptCalls.Add(oEIScriptCallData);
			Function = null;
			Parameters.Clear();
		}
	}
}

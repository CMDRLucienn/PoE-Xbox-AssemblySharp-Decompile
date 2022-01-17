using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam7Attribute : BaseScriptParamAttribute
{
	public ScriptParam7Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam7Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

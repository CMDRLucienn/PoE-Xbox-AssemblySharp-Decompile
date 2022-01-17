using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam1Attribute : BaseScriptParamAttribute
{
	public ScriptParam1Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam1Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

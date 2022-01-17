using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam2Attribute : BaseScriptParamAttribute
{
	public ScriptParam2Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam2Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam6Attribute : BaseScriptParamAttribute
{
	public ScriptParam6Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam6Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

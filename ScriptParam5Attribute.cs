using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam5Attribute : BaseScriptParamAttribute
{
	public ScriptParam5Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam5Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ScriptParam4Attribute : BaseScriptParamAttribute
{
	public ScriptParam4Attribute(string displayName, string description, string defaultValue)
		: base(displayName, description, defaultValue)
	{
	}

	public ScriptParam4Attribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
		: base(displayName, description, defaultValue, browser)
	{
	}
}

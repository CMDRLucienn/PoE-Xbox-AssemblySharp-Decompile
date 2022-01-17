using System;

[AttributeUsage(AttributeTargets.Method)]
public abstract class BaseScriptParamAttribute : Attribute
{
	public static Type[] ScriptAttributeTypes = new Type[8]
	{
		typeof(ScriptParam0Attribute),
		typeof(ScriptParam1Attribute),
		typeof(ScriptParam2Attribute),
		typeof(ScriptParam3Attribute),
		typeof(ScriptParam4Attribute),
		typeof(ScriptParam5Attribute),
		typeof(ScriptParam6Attribute),
		typeof(ScriptParam7Attribute)
	};

	public string DisplayName { get; private set; }

	public string Description { get; private set; }

	public string DefaultValue { get; private set; }

	public Scripts.BrowserType Browser { get; private set; }

	protected BaseScriptParamAttribute(string displayName, string description, string defaultValue)
	{
		DisplayName = displayName;
		Description = description;
		DefaultValue = defaultValue;
		Browser = Scripts.BrowserType.None;
	}

	protected BaseScriptParamAttribute(string displayName, string description, string defaultValue, Scripts.BrowserType browser)
	{
		DisplayName = displayName;
		Description = description;
		DefaultValue = defaultValue;
		Browser = browser;
	}
}

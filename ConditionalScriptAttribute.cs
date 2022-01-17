using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ConditionalScriptAttribute : ScriptAttribute
{
	public ConditionalScriptAttribute(string name, string path)
		: base(name, path)
	{
	}
}

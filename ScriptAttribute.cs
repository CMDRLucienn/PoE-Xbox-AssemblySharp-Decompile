using System;

[AttributeUsage(AttributeTargets.Method)]
public class ScriptAttribute : Attribute
{
	public string Name { get; private set; }

	public string Path { get; private set; }

	public ScriptAttribute(string name, string path)
	{
		Name = name;
		Path = path;
	}
}

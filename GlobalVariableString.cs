using System;

[Serializable]
public class GlobalVariableString
{
	public string Name;

	public GlobalVariableString()
	{
		Name = string.Empty;
	}

	public GlobalVariableString(string globalVariableName)
	{
		Name = globalVariableName;
	}
}

using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public class DesignerObsoleteAttribute : Attribute
{
	public readonly string Message;

	public DesignerObsoleteAttribute(string message)
	{
		Message = message;
	}
}

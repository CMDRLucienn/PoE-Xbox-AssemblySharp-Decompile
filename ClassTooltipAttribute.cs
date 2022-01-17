using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ClassTooltipAttribute : Attribute
{
	public readonly string tooltip = "";

	public ClassTooltipAttribute(string tooltip)
	{
		this.tooltip = tooltip;
	}
}

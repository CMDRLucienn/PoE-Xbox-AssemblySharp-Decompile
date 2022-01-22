using System;

public class LabelAttribute : Attribute
{
	public string Label;

	public LabelAttribute(string label)
	{
		Label = label;
	}
}
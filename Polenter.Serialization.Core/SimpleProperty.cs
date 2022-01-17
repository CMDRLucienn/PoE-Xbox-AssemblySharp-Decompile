using System;

namespace Polenter.Serialization.Core;

public sealed class SimpleProperty : Property
{
	public object Value { get; set; }

	public SimpleProperty(string name, Type type)
		: base(name, type)
	{
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.Simple;
	}

	public override string ToString()
	{
		string arg = base.ToString();
		if (Value == null)
		{
			return $"{arg}, (null)";
		}
		return $"{arg}, ({Value})";
	}
}

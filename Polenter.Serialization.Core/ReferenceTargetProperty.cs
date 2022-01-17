using System;

namespace Polenter.Serialization.Core;

public abstract class ReferenceTargetProperty : Property
{
	public ReferenceInfo Reference { get; set; }

	protected ReferenceTargetProperty(string name, Type type)
		: base(name, type)
	{
	}

	public virtual void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		Reference = source.Reference;
	}

	public override string ToString()
	{
		string arg = base.ToString();
		string arg2 = ((Reference != null) ? Reference.ToString() : "null");
		return $"{arg}, Reference={arg2}";
	}
}

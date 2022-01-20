// IEMod.Helpers.LabelAttribute
using System;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.Helpers.LabelAttribute")]
public class LabelAttribute : Attribute
{
	public string Label;

	[PatchedByMember("System.Void IEMod.Helpers.LabelAttribute::.ctor(System.String)")]
	public LabelAttribute(string label)
	{
		Label = label;
	}
}
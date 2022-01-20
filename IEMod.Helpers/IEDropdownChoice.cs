// IEMod.Helpers.IEDropdownChoice
using Patchwork.Attributes;

[PatchedByType("IEMod.Helpers.IEDropdownChoice")]
[NewType(null, null)]
public class IEDropdownChoice
{
	public string Label;

	public object Value;

	[PatchedByMember("System.Void IEMod.Helpers.IEDropdownChoice::.ctor(System.Object,System.String)")]
	public IEDropdownChoice(object value, string label)
	{
		Label = label;
		Value = value;
	}

	[PatchedByMember("System.String IEMod.Helpers.IEDropdownChoice::ToString()")]
	public override string ToString()
	{
		return Label;
	}
}

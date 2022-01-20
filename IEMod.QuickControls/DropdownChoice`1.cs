// IEMod.QuickControls.DropdownChoice<T>
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.DropdownChoice`1")]
public class DropdownChoice<T>
{
	public readonly T Value;

	public readonly string Label;

	[PatchedByMember("System.Void IEMod.QuickControls.DropdownChoice`1::.ctor(System.String,T)")]
	public DropdownChoice(string label, T value)
	{
		Label = label;
		Value = value;
	}

	[PatchedByMember("System.String IEMod.QuickControls.DropdownChoice`1::ToString()")]
	public override string ToString()
	{
		return Label;
	}
}

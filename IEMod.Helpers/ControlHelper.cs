// IEMod.Helpers.ControlHelper
using System.Linq;
using Patchwork.Attributes;

[PatchedByType("IEMod.Helpers.ControlHelper")]
[NewType(null, null)]
public static class ControlHelper
{
	[PatchedByMember("System.Void IEMod.Helpers.ControlHelper::SetSelectedValue(UIDropdownMenu,System.Object)")]
	public static void SetSelectedValue(this UIDropdownMenu dropdown, object value)
	{
		dropdown.SelectedItem = dropdown.Options.Cast<IEDropdownChoice>().SingleOrDefault((IEDropdownChoice x) => object.Equals(x.Value, value)) ?? dropdown.Options[0];
	}
}

// IEMod.QuickControls.BindingMode
using System;
using Patchwork.Attributes;

[NewType(null, null)]
[Flags]
[PatchedByType("IEMod.QuickControls.BindingMode")]
public enum BindingMode
{
	Disabled = 0,
	FromSource = 1,
	ToSource = 2,
	TwoWay = 3
}

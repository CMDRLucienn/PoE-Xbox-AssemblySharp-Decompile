// IEMod.QuickControls.Binding<T>
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.Binding`1")]
public class Binding<T>
{
	public IBindingValue<T> Source
	{
		[PatchedByMember("IEMod.QuickControls.IBindingValue`1<T> IEMod.QuickControls.Binding`1::get_Source()")]
		get;
	}

	public BindingMode Mode
	{
		[PatchedByMember("IEMod.QuickControls.BindingMode IEMod.QuickControls.Binding`1::get_Mode()")]
		get;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Binding`1::.ctor(IEMod.QuickControls.IBindingValue`1<T>,IEMod.QuickControls.BindingMode)")]
	public Binding(IBindingValue<T> source, BindingMode mode = BindingMode.TwoWay)
	{
		Source = source;
		Mode = mode;
	}
}

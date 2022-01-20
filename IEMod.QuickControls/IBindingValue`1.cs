// IEMod.QuickControls.IBindingValue<T>
using System;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.IBindingValue`1")]
public interface IBindingValue<T> : IDisposable
{
	T Value
	{
		[PatchedByMember("T IEMod.QuickControls.IBindingValue`1::get_Value()")]
		get;
		[PatchedByMember("System.Void IEMod.QuickControls.IBindingValue`1::set_Value(T)")]
		set;
	}

	string Name
	{
		[PatchedByMember("System.String IEMod.QuickControls.IBindingValue`1::get_Name()")]
		get;
	}

	bool IsDisposed
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.IBindingValue`1::get_IsDisposed()")]
		get;
	}

	event Action<IBindingValue<T>> HasChanged;

	[PatchedByMember("System.Void IEMod.QuickControls.IBindingValue`1::NotifyChange()")]
	void NotifyChange();
}

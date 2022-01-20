// IEMod.QuickControls.Binding
using System;
using System.Linq.Expressions;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.Binding")]
public static class Binding
{
	[PatchedByMember("IEMod.QuickControls.IBindingValue`1<T> IEMod.QuickControls.Binding::Bind(IEMod.QuickControls.Bindable`1<T>,IEMod.QuickControls.IBindingValue`1<T>,IEMod.QuickControls.BindingMode)")]
	public static IBindingValue<T> Bind<T>(this Bindable<T> bindable, IBindingValue<T> source, BindingMode mode = BindingMode.TwoWay)
	{
		bindable.Binding = source.ToBinding(mode);
		return source;
	}

	[PatchedByMember("IEMod.QuickControls.IBindingValue`1<T> IEMod.QuickControls.Binding::Bind(IEMod.QuickControls.Bindable`1<T>,System.Linq.Expressions.Expression`1<System.Func`1<T>>,IEMod.QuickControls.BindingMode)")]
	public static IBindingValue<T> Bind<T>(this Bindable<T> bindable, Expression<Func<T>> memberExpr, BindingMode mode = BindingMode.TwoWay)
	{
		return bindable.Bind(BindingValue.Member(memberExpr));
	}
}

// IEMod.QuickControls.BindingValue
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Patchwork.Attributes;

[PatchedByType("IEMod.QuickControls.BindingValue")]
[NewType(null, null)]
public static class BindingValue
{
	[PatchedByMember("IEMod.QuickControls.BindingValue`1<T> IEMod.QuickControls.BindingValue::Member(System.Linq.Expressions.Expression`1<System.Func`1<T>>,System.ComponentModel.INotifyPropertyChanged)")]
	public static BindingValue<T> Member<T>(Expression<Func<T>> expr, INotifyPropertyChanged notifier = null)
	{
		MemberAccess<T> accessor = ReflectHelper.AnalyzeMember(expr);
		object o = accessor.InstanceGetter();
		string text = ReflectHelper.TryGetName(o);
		notifier = notifier ?? (accessor.InstanceGetter() as INotifyPropertyChanged);
		BindingValue<T> bindingValue = new BindingValue<T>(accessor.Setter, accessor.Getter, (text ?? "?") + "." + accessor.TopmostMember.Name);
		if (notifier != null)
		{
			notifier.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == accessor.TopmostMember.Name)
				{
					bindingValue.NotifyChange();
				}
			};
		}
		return bindingValue;
	}

	[PatchedByMember("IEMod.QuickControls.IBindingValue`1<T> IEMod.QuickControls.BindingValue::OnChange(IEMod.QuickControls.IBindingValue`1<T>,System.Action`1<IEMod.QuickControls.IBindingValue`1<T>>)")]
	public static IBindingValue<T> OnChange<T>(this IBindingValue<T> bv, Action<IBindingValue<T>> action)
	{
		bv.HasChanged += action;
		return bv;
	}

	[PatchedByMember("IEMod.QuickControls.BindingValue`1<T> IEMod.QuickControls.BindingValue::Variable(T,System.String)")]
	public static BindingValue<T> Variable<T>(T initialValue, string name = "?")
	{
		BindingValue<T> bindingValue = null;
		Action<T> setter = delegate (T v)
		{
			initialValue = v;
			bindingValue.NotifyChange();
		};
		Func<T> getter = () => initialValue;
		bindingValue = new BindingValue<T>(setter, getter, name);
		return bindingValue;
	}

	[PatchedByMember("IEMod.QuickControls.BindingValue`1<T> IEMod.QuickControls.BindingValue::Const(T)")]
	public static BindingValue<T> Const<T>(T constant)
	{
		return new BindingValue<T>(null, () => constant);
	}

	[PatchedByMember("IEMod.QuickControls.Bindable`1<T> IEMod.QuickControls.BindingValue::ToBindable(IEMod.QuickControls.IBindingValue`1<T>)")]
	public static Bindable<T> ToBindable<T>(this IBindingValue<T> bv)
	{
		return new Bindable<T>(bv);
	}

	[PatchedByMember("IEMod.QuickControls.Binding`1<T> IEMod.QuickControls.BindingValue::ToBinding(IEMod.QuickControls.IBindingValue`1<T>,IEMod.QuickControls.BindingMode)")]
	public static Binding<T> ToBinding<T>(this IBindingValue<T> bv, BindingMode mode = BindingMode.TwoWay)
	{
		return new Binding<T>(bv, mode);
	}
}

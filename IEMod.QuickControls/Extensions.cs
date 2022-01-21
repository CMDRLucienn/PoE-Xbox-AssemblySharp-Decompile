using System;
using System.ComponentModel;
using System.Linq.Expressions;

public static class BindingValue
{
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

	public static IBindingValue<T> OnChange<T>(this IBindingValue<T> bv, Action<IBindingValue<T>> action)
	{
		bv.HasChanged += action;
		return bv;
	}

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

	public static BindingValue<T> Const<T>(T constant)
	{
		return new BindingValue<T>(null, () => constant);
	}

	public static Bindable<T> ToBindable<T>(this IBindingValue<T> bv)
	{
		return new Bindable<T>(bv);
	}

	public static Binding<T> ToBinding<T>(this IBindingValue<T> bv, BindingMode mode = BindingMode.TwoWay)
	{
		return new Binding<T>(bv, mode);
	}
}

public static class Binding
{
	public static IBindingValue<T> Bind<T>(this Bindable<T> bindable, IBindingValue<T> source, BindingMode mode = BindingMode.TwoWay)
	{
		bindable.Binding = source.ToBinding(mode);
		return source;
	}

	public static IBindingValue<T> Bind<T>(this Bindable<T> bindable, Expression<Func<T>> memberExpr, BindingMode mode = BindingMode.TwoWay)
	{
		return bindable.Bind(BindingValue.Member(memberExpr));
	}
}
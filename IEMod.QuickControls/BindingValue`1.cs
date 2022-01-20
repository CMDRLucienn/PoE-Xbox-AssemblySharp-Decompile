// IEMod.QuickControls.BindingValue<T>
using System;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.QuickControls.BindingValue`1")]
public class BindingValue<T> : IBindingValue<T>, IDisposable
{
	private readonly Action<T> _setter;

	private readonly Func<T> _getter;

	private Action<IBindingValue<T>> _hasChanged;

	public bool IsDisposed
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.BindingValue`1::get_IsDisposed()")]
		get;
		[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::set_IsDisposed(System.Boolean)")]
		private set;
	}

	public T Value
	{
		[PatchedByMember("T IEMod.QuickControls.BindingValue`1::get_Value()")]
		get
		{
			if (_getter == null)
			{
				throw IEDebug.Exception(null, "No getter!");
			}
			return (_getter == null) ? default(T) : _getter();
		}
		[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::set_Value(T)")]
		set
		{
			if (_setter != null)
			{
				_setter(value);
				NotifyChange();
			}
		}
	}

	public string Name
	{
		[PatchedByMember("System.String IEMod.QuickControls.BindingValue`1::get_Name()")]
		get;
		[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::set_Name(System.String)")]
		set;
	}

	public event Action<IBindingValue<T>> HasChanged
	{
		[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::add_HasChanged(System.Action`1<IEMod.QuickControls.IBindingValue`1<T>>)")]
		add
		{
			if (!IsDisposed)
			{
				_hasChanged = (Action<IBindingValue<T>>)Delegate.Combine(_hasChanged, value);
			}
		}
		[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::remove_HasChanged(System.Action`1<IEMod.QuickControls.IBindingValue`1<T>>)")]
		remove
		{
			if (!IsDisposed)
			{
				_hasChanged = (Action<IBindingValue<T>>)Delegate.Remove(_hasChanged, value);
			}
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::.ctor(System.Action`1<T>,System.Func`1<T>,System.String)")]
	public BindingValue(Action<T> setter, Func<T> getter, string name = null)
	{
		_setter = setter;
		_getter = getter;
		Name = name;
	}

	[PatchedByMember("T IEMod.QuickControls.BindingValue`1::op_Implicit(IEMod.QuickControls.BindingValue`1<T>)")]
	public static implicit operator T(BindingValue<T> bindingValue)
	{
		return (bindingValue == null) ? ((T)(object)null) : bindingValue.Value;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::NotifyChange()")]
	public virtual void NotifyChange()
	{
		if (!IsDisposed)
		{
			_hasChanged?.Invoke(this);
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.BindingValue`1::Dispose()")]
	public void Dispose()
	{
		_hasChanged = null;
		IsDisposed = true;
	}
}

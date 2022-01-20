// IEMod.QuickControls.Bindable<T>
using System;
using Patchwork.Attributes;

[PatchedByType("IEMod.QuickControls.Bindable`1")]
[NewType(null, null)]
public class Bindable<T> : IBindingValue<T>, IDisposable
{
	private readonly IBindingValue<T> _target;

	private bool _isUpdating;

	private readonly object _lock = new object();

	private Binding<T> _binding;

	public T Value
	{
		[PatchedByMember("T IEMod.QuickControls.Bindable`1::get_Value()")]
		get
		{
			return _target.Value;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::set_Value(T)")]
		set
		{
			_target.Value = value;
		}
	}

	public Binding<T> Binding
	{
		[PatchedByMember("IEMod.QuickControls.Binding`1<T> IEMod.QuickControls.Bindable`1::get_Binding()")]
		get
		{
			return _binding;
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::set_Binding(IEMod.QuickControls.Binding`1<T>)")]
		set
		{
			SetBindingDirect(value);
			if (_binding != null)
			{
				OnChanged(_binding.Source);
			}
		}
	}

	public string BindingString
	{
		[PatchedByMember("System.String IEMod.QuickControls.Bindable`1::get_BindingString()")]
		get
		{
			string text = "";
			switch (Binding?.Mode)
			{
				case BindingMode.FromSource:
					text = "<=";
					break;
				case BindingMode.ToSource:
					text = "=>";
					break;
				case BindingMode.TwoWay:
					text = "<=>";
					break;
				case BindingMode.Disabled:
					text = "<=/=>";
					break;
				case null:
					return _target.Name;
			}
			string name = typeof(T).Name;
			return "[" + name + "] '" + _target.Name + "' " + text + " '" + Binding.Source.Name + "'";
		}
	}

	public string Name
	{
		[PatchedByMember("System.String IEMod.QuickControls.Bindable`1::get_Name()")]
		get
		{
			return _target.Name;
		}
	}

	public bool IsDisposed
	{
		[PatchedByMember("System.Boolean IEMod.QuickControls.Bindable`1::get_IsDisposed()")]
		get
		{
			return _target.IsDisposed;
		}
	}

	public event Action<IBindingValue<T>> HasChanged
	{
		[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::add_HasChanged(System.Action`1<IEMod.QuickControls.IBindingValue`1<T>>)")]
		add
		{
			if (!IsDisposed)
			{
				_target.HasChanged += value;
			}
		}
		[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::remove_HasChanged(System.Action`1<IEMod.QuickControls.IBindingValue`1<T>>)")]
		remove
		{
			if (!IsDisposed)
			{
				_target.HasChanged -= value;
			}
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::.ctor(IEMod.QuickControls.IBindingValue`1<T>)")]
	public Bindable(IBindingValue<T> target)
	{
		_target = target;
		_target.HasChanged += OnChanged;
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::OnChanged(IEMod.QuickControls.IBindingValue`1<T>)")]
	private void OnChanged(IBindingValue<T> bindingValue)
	{
		if (_isUpdating || Binding == null)
		{
			return;
		}
		bool? flag = null;
		_isUpdating = true;
		if ((bindingValue == _target && (Binding.Mode & BindingMode.ToSource) == 0) || (bindingValue == Binding.Source && (Binding.Mode & BindingMode.FromSource) == 0))
		{
			return;
		}
		string text = ((bindingValue == _target) ? "target" : "source");
		string text2 = ((bindingValue != _target) ? "target" : "source");
		IBindingValue<T> bindingValue2 = ((bindingValue == _target) ? Binding.Source : _target);
		T value;
		try
		{
			value = bindingValue.Value;
		}
		catch (Exception ex)
		{
			if (ex is ObjectDisposedException || ex.InnerException is ObjectDisposedException)
			{
				IEDebug.Log("In binding (" + BindingString + "), when the " + text + " changed, tried to get the value but the object was disposed.");
				flag = bindingValue != _target;
				goto IL_01ed;
			}
			throw;
		}
		try
		{
			bindingValue2.Value = value;
		}
		catch (Exception ex2)
		{
			if (!(ex2 is ObjectDisposedException) && !(ex2.InnerException is ObjectDisposedException))
			{
				throw;
			}
			IEDebug.Log("In binding (" + BindingString + "), when the " + text + " changed, tried to update the " + text2 + "'s value, but the object was disposed.");
			flag = bindingValue != _target;
		}
		goto IL_01ed;
	IL_01ed:
		_isUpdating = false;
		bool? flag2 = flag;
		bool? flag3 = flag2;
		if (flag3.HasValue)
		{
			if (flag3.GetValueOrDefault())
			{
				IEDebug.Log("In binding (" + BindingString + "), the source (thing bound to this) was disposed, so the binding will be scrapped.");
				SetBindingDirect(null);
			}
			else
			{
				IEDebug.Log("In binding (" + BindingString + "), the target (the thing backing this) was disposed, so the binding will be scrapped.");
				Dispose();
			}
		}
	}

	[PatchedByMember("IEMod.QuickControls.Bindable`1<T> IEMod.QuickControls.Bindable`1::op_Implicit(IEMod.QuickControls.BindingValue`1<T>)")]
	public static implicit operator Bindable<T>(BindingValue<T> bv)
	{
		return bv.ToBindable();
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::SetBindingDirect(IEMod.QuickControls.Binding`1<T>)")]
	private void SetBindingDirect(Binding<T> newBinding)
	{
		if (_binding != null)
		{
			_binding.Source.HasChanged -= OnChanged;
		}
		_binding = newBinding;
		if (_binding != null)
		{
			_binding.Source.HasChanged += OnChanged;
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::NotifyChange()")]
	public void NotifyChange()
	{
		if (!IsDisposed)
		{
			_target.NotifyChange();
		}
	}

	[PatchedByMember("System.Void IEMod.QuickControls.Bindable`1::Dispose()")]
	public void Dispose()
	{
		Binding = null;
		_target.Dispose();
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;

public class OEIBindingList<T> : BindingList<T>
{
	private bool m_TypeNotifiesPropertyChanged;

	public OEIBindingList(IList<T> list)
		: base(list)
	{
		CheckType();
	}

	public OEIBindingList()
	{
		CheckType();
	}

	private void CheckType()
	{
		m_TypeNotifiesPropertyChanged = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
	}

	protected override void ClearItems()
	{
		if (m_TypeNotifiesPropertyChanged)
		{
			for (int num = base.Count - 1; num >= 0; num--)
			{
				if ((object)base[num] is INotifyPropertyChanged notifyPropertyChanged)
				{
					notifyPropertyChanged.PropertyChanged -= OnItemNotifiesChange;
				}
			}
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, T item)
	{
		if (m_TypeNotifiesPropertyChanged && item != null)
		{
			INotifyPropertyChanged obj = (INotifyPropertyChanged)(object)item;
			obj.PropertyChanged -= OnItemNotifiesChange;
			obj.PropertyChanged += OnItemNotifiesChange;
		}
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		if (!base.AllowRemove)
		{
			throw new NotSupportedException();
		}
		T val = base[index];
		base.RemoveItem(index);
		if (!Contains(val))
		{
			((INotifyPropertyChanged)(object)val).PropertyChanged -= OnItemNotifiesChange;
		}
	}

	protected override void SetItem(int index, T item)
	{
		if (m_TypeNotifiesPropertyChanged && item != null)
		{
			INotifyPropertyChanged obj = (INotifyPropertyChanged)(object)item;
			obj.PropertyChanged -= OnItemNotifiesChange;
			obj.PropertyChanged += OnItemNotifiesChange;
		}
		base.SetItem(index, item);
	}

	private void OnItemNotifiesChange(object sender, PropertyChangedEventArgs args)
	{
		OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, null));
	}
}

using System.Collections.Generic;

namespace Polenter.Serialization.Core.Binary;

internal sealed class IndexGenerator<T>
{
	private readonly List<T> _items = new List<T>();

	public IList<T> Items => _items;

	public int GetIndexOfItem(T item)
	{
		int num = _items.IndexOf(item);
		if (num > -1)
		{
			return num;
		}
		_items.Add(item);
		return _items.Count - 1;
	}
}

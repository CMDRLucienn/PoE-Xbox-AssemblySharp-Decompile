using System.Collections.ObjectModel;

namespace Polenter.Serialization.Core;

public sealed class PropertyCollection : Collection<Property>
{
	public Property Parent { get; set; }

	protected override void ClearItems()
	{
		foreach (Property item in base.Items)
		{
			item.Parent = null;
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, Property item)
	{
		base.InsertItem(index, item);
		item.Parent = Parent;
	}

	protected override void RemoveItem(int index)
	{
		base.Items[index].Parent = null;
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, Property item)
	{
		base.Items[index].Parent = null;
		base.SetItem(index, item);
		item.Parent = Parent;
	}
}

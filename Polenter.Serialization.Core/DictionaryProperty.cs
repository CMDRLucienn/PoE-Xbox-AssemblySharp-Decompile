using System;
using System.Collections.Generic;

namespace Polenter.Serialization.Core;

public sealed class DictionaryProperty : ComplexProperty
{
	private IList<KeyValueItem> _items;

	public IList<KeyValueItem> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<KeyValueItem>();
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public Type KeyType { get; set; }

	public Type ValueType { get; set; }

	public DictionaryProperty(string name, Type type)
		: base(name, type)
	{
	}

	public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		if (!(source is DictionaryProperty dictionaryProperty))
		{
			throw new InvalidCastException($"Invalid property type to make a flat copy. Expected {typeof(DictionaryProperty)}, current {source.GetType()}");
		}
		base.MakeFlatCopyFrom(source);
		KeyType = dictionaryProperty.KeyType;
		ValueType = dictionaryProperty.ValueType;
		Items = dictionaryProperty.Items;
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.Dictionary;
	}
}

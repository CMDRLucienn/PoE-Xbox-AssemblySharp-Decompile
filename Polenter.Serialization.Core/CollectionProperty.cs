using System;
using System.Collections.Generic;

namespace Polenter.Serialization.Core;

public sealed class CollectionProperty : ComplexProperty
{
	private IList<Property> _items;

	public IList<Property> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<Property>();
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public Type ElementType { get; set; }

	public CollectionProperty(string name, Type type)
		: base(name, type)
	{
	}

	public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		if (!(source is CollectionProperty collectionProperty))
		{
			throw new InvalidCastException($"Invalid property type to make a flat copy. Expected {typeof(CollectionProperty)}, current {source.GetType()}");
		}
		base.MakeFlatCopyFrom(source);
		ElementType = collectionProperty.ElementType;
		Items = collectionProperty.Items;
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.Collection;
	}
}

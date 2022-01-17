using System;

namespace Polenter.Serialization.Core;

public sealed class SingleDimensionalArrayProperty : ReferenceTargetProperty
{
	private PropertyCollection _items;

	public PropertyCollection Items
	{
		get
		{
			if (_items == null)
			{
				_items = new PropertyCollection
				{
					Parent = this
				};
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public int LowerBound { get; set; }

	public Type ElementType { get; set; }

	public SingleDimensionalArrayProperty(string name, Type type)
		: base(name, type)
	{
	}

	public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		if (!(source is SingleDimensionalArrayProperty singleDimensionalArrayProperty))
		{
			throw new InvalidCastException($"Invalid property type to make a flat copy. Expected {typeof(SingleDimensionalArrayProperty)}, current {source.GetType()}");
		}
		base.MakeFlatCopyFrom(source);
		LowerBound = singleDimensionalArrayProperty.LowerBound;
		ElementType = singleDimensionalArrayProperty.ElementType;
		Items = singleDimensionalArrayProperty.Items;
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.SingleDimensionalArray;
	}
}

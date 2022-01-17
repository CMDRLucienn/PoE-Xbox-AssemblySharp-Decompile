using System;
using System.Collections.Generic;

namespace Polenter.Serialization.Core;

public sealed class MultiDimensionalArrayProperty : ReferenceTargetProperty
{
	private IList<DimensionInfo> _dimensionInfos;

	private IList<MultiDimensionalArrayItem> _items;

	public IList<MultiDimensionalArrayItem> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new List<MultiDimensionalArrayItem>();
			}
			return _items;
		}
		set
		{
			_items = value;
		}
	}

	public IList<DimensionInfo> DimensionInfos
	{
		get
		{
			if (_dimensionInfos == null)
			{
				_dimensionInfos = new List<DimensionInfo>();
			}
			return _dimensionInfos;
		}
		set
		{
			_dimensionInfos = value;
		}
	}

	public Type ElementType { get; set; }

	public MultiDimensionalArrayProperty(string name, Type type)
		: base(name, type)
	{
	}

	public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		if (!(source is MultiDimensionalArrayProperty multiDimensionalArrayProperty))
		{
			throw new InvalidCastException($"Invalid property type to make a flat copy. Expected {typeof(SingleDimensionalArrayProperty)}, current {source.GetType()}");
		}
		base.MakeFlatCopyFrom(source);
		ElementType = multiDimensionalArrayProperty.ElementType;
		DimensionInfos = multiDimensionalArrayProperty.DimensionInfos;
		Items = multiDimensionalArrayProperty.Items;
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.MultiDimensionalArray;
	}
}

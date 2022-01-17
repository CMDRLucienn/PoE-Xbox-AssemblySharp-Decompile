using System;

namespace Polenter.Serialization.Core;

public class ComplexProperty : ReferenceTargetProperty
{
	private PropertyCollection _properties;

	public PropertyCollection Properties
	{
		get
		{
			if (_properties == null)
			{
				_properties = new PropertyCollection
				{
					Parent = this
				};
			}
			return _properties;
		}
		set
		{
			_properties = value;
		}
	}

	public ComplexProperty(string name, Type type)
		: base(name, type)
	{
	}

	public override void MakeFlatCopyFrom(ReferenceTargetProperty source)
	{
		if (!(source is ComplexProperty complexProperty))
		{
			throw new InvalidCastException($"Invalid property type to make a flat copy. Expected {typeof(ComplexProperty)}, current {source.GetType()}");
		}
		base.MakeFlatCopyFrom(source);
		Properties = complexProperty.Properties;
	}

	protected override PropertyArt GetPropertyArt()
	{
		return PropertyArt.Complex;
	}
}

using System;
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Serializing;

public sealed class PropertyTypeInfo<TProperty> where TProperty : Property
{
	public Type ExpectedPropertyType { get; set; }

	public Type ValueType { get; set; }

	public string Name { get; set; }

	public TProperty Property { get; set; }

	public PropertyTypeInfo(TProperty property, Type valueType)
	{
		Property = property;
		ExpectedPropertyType = valueType;
		ValueType = property.Type;
		Name = property.Name;
	}

	public PropertyTypeInfo(TProperty property, Type expectedPropertyType, Type valueType)
	{
		Property = property;
		ExpectedPropertyType = expectedPropertyType;
		ValueType = valueType;
		Name = property.Name;
	}
}

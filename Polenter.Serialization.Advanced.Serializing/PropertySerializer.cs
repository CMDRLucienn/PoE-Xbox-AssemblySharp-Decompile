using System;
using System.IO;
using Polenter.Serialization.Core;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced.Serializing;

public abstract class PropertySerializer : IPropertySerializer
{
	public void Serialize(Property property)
	{
		SerializeCore(new PropertyTypeInfo<Property>(property, null));
	}

	public abstract void Open(Stream stream);

	public abstract void Close();

	protected void SerializeCore(PropertyTypeInfo<Property> property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		if (property.Property is NullProperty property2)
		{
			SerializeNullProperty(new PropertyTypeInfo<NullProperty>(property2, property.ExpectedPropertyType, property.ValueType));
			return;
		}
		if (property.ExpectedPropertyType != null && property.ExpectedPropertyType == property.ValueType)
		{
			property.ValueType = null;
		}
		if (property.Property is SimpleProperty property3)
		{
			SerializeSimpleProperty(new PropertyTypeInfo<SimpleProperty>(property3, property.ExpectedPropertyType, property.ValueType));
		}
		else if (!(property.Property is ReferenceTargetProperty property4) || (!serializeReference(property4) && !serializeReferenceTarget(new PropertyTypeInfo<ReferenceTargetProperty>(property4, property.ExpectedPropertyType, property.ValueType))))
		{
			throw new InvalidOperationException($"Unknown Property: {property.Property.GetType()}");
		}
	}

	private bool serializeReferenceTarget(PropertyTypeInfo<ReferenceTargetProperty> property)
	{
		if (property.Property is MultiDimensionalArrayProperty multiDimensionalArrayProperty)
		{
			multiDimensionalArrayProperty.Reference.IsProcessed = true;
			SerializeMultiDimensionalArrayProperty(new PropertyTypeInfo<MultiDimensionalArrayProperty>(multiDimensionalArrayProperty, property.ExpectedPropertyType, property.ValueType));
			return true;
		}
		if (property.Property is SingleDimensionalArrayProperty singleDimensionalArrayProperty)
		{
			singleDimensionalArrayProperty.Reference.IsProcessed = true;
			SerializeSingleDimensionalArrayProperty(new PropertyTypeInfo<SingleDimensionalArrayProperty>(singleDimensionalArrayProperty, property.ExpectedPropertyType, property.ValueType));
			return true;
		}
		if (property.Property is DictionaryProperty dictionaryProperty)
		{
			dictionaryProperty.Reference.IsProcessed = true;
			SerializeDictionaryProperty(new PropertyTypeInfo<DictionaryProperty>(dictionaryProperty, property.ExpectedPropertyType, property.ValueType));
			return true;
		}
		if (property.Property is CollectionProperty collectionProperty)
		{
			collectionProperty.Reference.IsProcessed = true;
			SerializeCollectionProperty(new PropertyTypeInfo<CollectionProperty>(collectionProperty, property.ExpectedPropertyType, property.ValueType));
			return true;
		}
		if (property.Property is ComplexProperty complexProperty)
		{
			complexProperty.Reference.IsProcessed = true;
			SerializeComplexProperty(new PropertyTypeInfo<ComplexProperty>(complexProperty, property.ExpectedPropertyType, property.ValueType));
			return true;
		}
		return false;
	}

	private bool serializeReference(ReferenceTargetProperty property)
	{
		if (property.Reference.Count > 1 && property.Reference.IsProcessed)
		{
			SerializeReference(property);
			return true;
		}
		return false;
	}

	protected abstract void SerializeNullProperty(PropertyTypeInfo<NullProperty> property);

	protected abstract void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property);

	protected abstract void SerializeMultiDimensionalArrayProperty(PropertyTypeInfo<MultiDimensionalArrayProperty> property);

	protected abstract void SerializeSingleDimensionalArrayProperty(PropertyTypeInfo<SingleDimensionalArrayProperty> property);

	protected abstract void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property);

	protected abstract void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property);

	protected abstract void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property);

	protected abstract void SerializeReference(ReferenceTargetProperty referenceTarget);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced;

public sealed class BinaryPropertySerializer : PropertySerializer
{
	private readonly IBinaryWriter _writer;

	public BinaryPropertySerializer(IBinaryWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		_writer = writer;
	}

	public override void Open(Stream stream)
	{
		_writer.Open(stream);
	}

	public override void Close()
	{
		_writer.Close();
	}

	private void writePropertyHeader(byte elementId, string name, Type valueType)
	{
		_writer.WriteElementId(elementId);
		_writer.WriteName(name);
		_writer.WriteType(valueType);
	}

	private bool writePropertyHeaderWithReferenceId(byte elementId, ReferenceInfo info, string name, Type valueType)
	{
		if (info.Count < 2)
		{
			return false;
		}
		writePropertyHeader(elementId, name, valueType);
		_writer.WriteNumber(info.Id);
		return true;
	}

	protected override void SerializeNullProperty(PropertyTypeInfo<NullProperty> property)
	{
		writePropertyHeader(5, property.Name, property.ValueType);
	}

	protected override void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property)
	{
		writePropertyHeader(6, property.Name, property.ValueType);
		_writer.WriteValue(property.Property.Value);
	}

	protected override void SerializeMultiDimensionalArrayProperty(PropertyTypeInfo<MultiDimensionalArrayProperty> property)
	{
		if (!writePropertyHeaderWithReferenceId(13, property.Property.Reference, property.Name, property.ValueType))
		{
			writePropertyHeader(4, property.Name, property.ValueType);
		}
		_writer.WriteType(property.Property.ElementType);
		writeDimensionInfos(property.Property.DimensionInfos);
		writeMultiDimensionalArrayItems(property.Property.Items, property.Property.ElementType);
	}

	private void writeMultiDimensionalArrayItems(IList<MultiDimensionalArrayItem> items, Type defaultItemType)
	{
		_writer.WriteNumber(items.Count);
		foreach (MultiDimensionalArrayItem item in items)
		{
			writeMultiDimensionalArrayItem(item, defaultItemType);
		}
	}

	private void writeMultiDimensionalArrayItem(MultiDimensionalArrayItem item, Type defaultItemType)
	{
		_writer.WriteNumbers(item.Indexes);
		SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultItemType));
	}

	private void writeDimensionInfos(IList<DimensionInfo> dimensionInfos)
	{
		_writer.WriteNumber(dimensionInfos.Count);
		foreach (DimensionInfo dimensionInfo in dimensionInfos)
		{
			writeDimensionInfo(dimensionInfo);
		}
	}

	private void writeDimensionInfo(DimensionInfo info)
	{
		_writer.WriteNumber(info.Length);
		_writer.WriteNumber(info.LowerBound);
	}

	protected override void SerializeSingleDimensionalArrayProperty(PropertyTypeInfo<SingleDimensionalArrayProperty> property)
	{
		if (!writePropertyHeaderWithReferenceId(12, property.Property.Reference, property.Name, property.ValueType))
		{
			writePropertyHeader(7, property.Name, property.ValueType);
		}
		_writer.WriteType(property.Property.ElementType);
		_writer.WriteNumber(property.Property.LowerBound);
		writeItems(property.Property.Items, property.Property.ElementType);
	}

	private void writeItems(ICollection<Property> items, Type defaultItemType)
	{
		_writer.WriteNumber(items.Count);
		foreach (Property item in items)
		{
			SerializeCore(new PropertyTypeInfo<Property>(item, defaultItemType));
		}
	}

	protected override void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property)
	{
		if (!writePropertyHeaderWithReferenceId(11, property.Property.Reference, property.Name, property.ValueType))
		{
			writePropertyHeader(3, property.Name, property.ValueType);
		}
		_writer.WriteType(property.Property.KeyType);
		_writer.WriteType(property.Property.ValueType);
		writeProperties(property.Property.Properties, property.Property.Type);
		writeDictionaryItems(property.Property.Items, property.Property.KeyType, property.Property.ValueType);
	}

	private void writeDictionaryItems(IList<KeyValueItem> items, Type defaultKeyType, Type defaultValueType)
	{
		_writer.WriteNumber(items.Count);
		foreach (KeyValueItem item in items)
		{
			writeDictionaryItem(item, defaultKeyType, defaultValueType);
		}
	}

	private void writeDictionaryItem(KeyValueItem item, Type defaultKeyType, Type defaultValueType)
	{
		SerializeCore(new PropertyTypeInfo<Property>(item.Key, defaultKeyType));
		SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultValueType));
	}

	protected override void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property)
	{
		if (!writePropertyHeaderWithReferenceId(10, property.Property.Reference, property.Name, property.ValueType))
		{
			writePropertyHeader(1, property.Name, property.ValueType);
		}
		_writer.WriteType(property.Property.ElementType);
		writeProperties(property.Property.Properties, property.Property.Type);
		writeItems(property.Property.Items, property.Property.ElementType);
	}

	protected override void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property)
	{
		if (!writePropertyHeaderWithReferenceId(8, property.Property.Reference, property.Name, property.ValueType))
		{
			writePropertyHeader(2, property.Name, property.ValueType);
		}
		writeProperties(property.Property.Properties, property.Property.Type);
	}

	protected override void SerializeReference(ReferenceTargetProperty referenceTarget)
	{
		writePropertyHeader(9, referenceTarget.Name, null);
		_writer.WriteNumber(referenceTarget.Reference.Id);
	}

	private void writeProperties(PropertyCollection properties, Type ownerType)
	{
		_writer.WriteNumber(Convert.ToInt16(properties.Count));
		foreach (Property property2 in properties)
		{
			if (property2 == null)
			{
				continue;
			}
			PropertyInfo property = ownerType.GetProperty(property2.Name);
			if (property != null)
			{
				SerializeCore(new PropertyTypeInfo<Property>(property2, property.PropertyType));
				continue;
			}
			FieldInfo field = ownerType.GetField(property2.Name);
			if (field != null)
			{
				SerializeCore(new PropertyTypeInfo<Property>(property2, field.FieldType));
			}
		}
	}
}

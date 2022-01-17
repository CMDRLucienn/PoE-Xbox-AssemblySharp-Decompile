using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;
using Polenter.Serialization.Core;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced;

public sealed class XmlPropertySerializer : PropertySerializer
{
	private readonly IXmlWriter _writer;

	public XmlPropertySerializer(IXmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		_writer = writer;
	}

	protected override void SerializeNullProperty(PropertyTypeInfo<NullProperty> property)
	{
		writeStartProperty("Null", property.Name, property.ValueType);
		writeEndProperty();
	}

	protected override void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property)
	{
		if (property.Property.Value != null)
		{
			writeStartProperty("Simple", property.Name, property.ValueType);
			_writer.WriteAttribute("value", property.Property.Value);
			writeEndProperty();
		}
	}

	private void writeEndProperty()
	{
		_writer.WriteEndElement();
	}

	private void writeStartProperty(string elementId, string propertyName, Type propertyType)
	{
		_writer.WriteStartElement(elementId);
		if (!string.IsNullOrEmpty(propertyName))
		{
			_writer.WriteAttribute("name", propertyName);
		}
		if (propertyType != null)
		{
			_writer.WriteAttribute("type", propertyType);
		}
	}

	protected override void SerializeMultiDimensionalArrayProperty(PropertyTypeInfo<MultiDimensionalArrayProperty> property)
	{
		writeStartProperty("MultiArray", property.Name, property.ValueType);
		if (property.Property.Reference.Count > 1)
		{
			_writer.WriteAttribute("id", property.Property.Reference.Id);
		}
		writeDimensionInfos(property.Property.DimensionInfos);
		writeMultiDimensionalArrayItems(property.Property.Items, property.Property.ElementType);
		writeEndProperty();
	}

	private void writeMultiDimensionalArrayItems(IEnumerable<MultiDimensionalArrayItem> items, Type defaultItemType)
	{
		_writer.WriteStartElement("Items");
		foreach (MultiDimensionalArrayItem item in items)
		{
			writeMultiDimensionalArrayItem(item, defaultItemType);
		}
		_writer.WriteEndElement();
	}

	private void writeMultiDimensionalArrayItem(MultiDimensionalArrayItem item, Type defaultTypeOfItemValue)
	{
		_writer.WriteStartElement("Item");
		_writer.WriteAttribute("indexes", item.Indexes);
		SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultTypeOfItemValue));
		_writer.WriteEndElement();
	}

	private void writeDimensionInfos(IEnumerable<DimensionInfo> infos)
	{
		_writer.WriteStartElement("Dimensions");
		foreach (DimensionInfo info in infos)
		{
			writeDimensionInfo(info);
		}
		_writer.WriteEndElement();
	}

	protected override void SerializeSingleDimensionalArrayProperty(PropertyTypeInfo<SingleDimensionalArrayProperty> property)
	{
		writeStartProperty("SingleArray", property.Name, property.ValueType);
		if (property.Property.Reference.Count > 1)
		{
			_writer.WriteAttribute("id", property.Property.Reference.Id);
		}
		if (property.Property.LowerBound != 0)
		{
			_writer.WriteAttribute("lowerBound", property.Property.LowerBound);
		}
		writeItems(property.Property.Items, property.Property.ElementType);
		writeEndProperty();
	}

	private void writeDimensionInfo(DimensionInfo info)
	{
		_writer.WriteStartElement("Dimension");
		if (info.Length != 0)
		{
			_writer.WriteAttribute("length", info.Length);
		}
		if (info.LowerBound != 0)
		{
			_writer.WriteAttribute("lowerBound", info.LowerBound);
		}
		_writer.WriteEndElement();
	}

	protected override void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property)
	{
		writeStartProperty("Dictionary", property.Name, property.ValueType);
		if (property.Property.Reference.Count > 1)
		{
			_writer.WriteAttribute("id", property.Property.Reference.Id);
		}
		writeProperties(property.Property.Properties, property.Property.Type);
		writeDictionaryItems(property.Property.Items, property.Property.KeyType, property.Property.ValueType);
		writeEndProperty();
	}

	private void writeDictionaryItems(IEnumerable<KeyValueItem> items, Type defaultKeyType, Type defaultValueType)
	{
		_writer.WriteStartElement("Items");
		foreach (KeyValueItem item in items)
		{
			writeDictionaryItem(item, defaultKeyType, defaultValueType);
		}
		_writer.WriteEndElement();
	}

	private void writeDictionaryItem(KeyValueItem item, Type defaultKeyType, Type defaultValueType)
	{
		_writer.WriteStartElement("Item");
		SerializeCore(new PropertyTypeInfo<Property>(item.Key, defaultKeyType));
		SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultValueType));
		_writer.WriteEndElement();
	}

	private void writeValueType(Type type)
	{
		if (type != null)
		{
			_writer.WriteAttribute("valueType", type);
		}
	}

	private void writeKeyType(Type type)
	{
		if (type != null)
		{
			_writer.WriteAttribute("keyType", type);
		}
	}

	protected override void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property)
	{
		writeStartProperty("Collection", property.Name, property.ValueType);
		if (property.Property.Reference.Count > 1)
		{
			_writer.WriteAttribute("id", property.Property.Reference.Id);
		}
		writeProperties(property.Property.Properties, property.Property.Type);
		writeItems(property.Property.Items, property.Property.ElementType);
		writeEndProperty();
	}

	private void writeItems(IEnumerable<Property> properties, Type defaultItemType)
	{
		_writer.WriteStartElement("Items");
		foreach (Property property in properties)
		{
			SerializeCore(new PropertyTypeInfo<Property>(property, defaultItemType));
		}
		_writer.WriteEndElement();
	}

	private void writeProperties(ICollection<Property> properties, Type ownerType)
	{
		if (properties.Count == 0)
		{
			return;
		}
		_writer.WriteStartElement("Properties");
		foreach (Property property2 in properties)
		{
			PropertyInfo property = ownerType.GetProperty(property2.Name);
			if (property != null)
			{
				SerializeCore(new PropertyTypeInfo<Property>(property2, property.PropertyType));
			}
			else
			{
				SerializeCore(new PropertyTypeInfo<Property>(property2, null));
			}
		}
		_writer.WriteEndElement();
	}

	protected override void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property)
	{
		writeStartProperty("Complex", property.Name, property.ValueType);
		if (property.Property.Reference.Count > 1)
		{
			_writer.WriteAttribute("id", property.Property.Reference.Id);
		}
		writeProperties(property.Property.Properties, property.Property.Type);
		writeEndProperty();
	}

	protected override void SerializeReference(ReferenceTargetProperty referenceTarget)
	{
		writeStartProperty("Reference", referenceTarget.Name, null);
		_writer.WriteAttribute("id", referenceTarget.Reference.Id);
		writeEndProperty();
	}

	public override void Open(Stream stream)
	{
		_writer.Open(stream);
	}

	public override void Close()
	{
		_writer.Close();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Deserializing;
using Polenter.Serialization.Core;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced;

public sealed class BinaryPropertyDeserializer : IPropertyDeserializer
{
	private readonly IBinaryReader _reader;

	private readonly Dictionary<int, ReferenceTargetProperty> _propertyCache = new Dictionary<int, ReferenceTargetProperty>();

	public BinaryPropertyDeserializer(IBinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		_reader = reader;
	}

	public void Open(Stream stream)
	{
		_reader.Open(stream);
	}

	public Property Deserialize()
	{
		byte elementId = _reader.ReadElementId();
		return deserialize(elementId, null);
	}

	public void Close()
	{
		_reader.Close();
	}

	private Property deserialize(byte elementId, Type expectedType)
	{
		string propertyName = _reader.ReadName();
		return deserialize(elementId, propertyName, expectedType);
	}

	private Property deserialize(byte elementId, string propertyName, Type expectedType)
	{
		Type type = _reader.ReadType();
		if (type == null)
		{
			type = expectedType;
		}
		int num = 0;
		if (elementId == 9 || Elements.IsElementWithId(elementId))
		{
			num = _reader.ReadNumber();
			if (elementId == 9)
			{
				return createProperty(num, propertyName, type);
			}
		}
		Property property = createProperty(elementId, propertyName, type);
		if (property == null)
		{
			return null;
		}
		if (property is NullProperty result)
		{
			return result;
		}
		if (property is SimpleProperty simpleProperty)
		{
			parseSimpleProperty(simpleProperty);
			return simpleProperty;
		}
		if (property is ReferenceTargetProperty referenceTargetProperty && num > 0)
		{
			referenceTargetProperty.Reference = new ReferenceInfo();
			referenceTargetProperty.Reference.Id = num;
			referenceTargetProperty.Reference.IsProcessed = true;
			if (_propertyCache.ContainsKey(num))
			{
				_propertyCache[num] = referenceTargetProperty;
			}
			else
			{
				_propertyCache.Add(num, referenceTargetProperty);
			}
		}
		if (property is MultiDimensionalArrayProperty multiDimensionalArrayProperty)
		{
			parseMultiDimensionalArrayProperty(multiDimensionalArrayProperty);
			return multiDimensionalArrayProperty;
		}
		if (property is SingleDimensionalArrayProperty singleDimensionalArrayProperty)
		{
			parseSingleDimensionalArrayProperty(singleDimensionalArrayProperty);
			return singleDimensionalArrayProperty;
		}
		if (property is DictionaryProperty dictionaryProperty)
		{
			parseDictionaryProperty(dictionaryProperty);
			return dictionaryProperty;
		}
		if (property is CollectionProperty collectionProperty)
		{
			parseCollectionProperty(collectionProperty);
			return collectionProperty;
		}
		if (property is ComplexProperty complexProperty)
		{
			parseComplexProperty(complexProperty);
			return complexProperty;
		}
		return property;
	}

	private void parseComplexProperty(ComplexProperty property)
	{
		readProperties(property.Properties, property.Type);
	}

	private void readProperties(PropertyCollection properties, Type ownerType)
	{
		int num = _reader.ReadNumber();
		for (int i = 0; i < num; i++)
		{
			byte elementId = _reader.ReadElementId();
			string text = _reader.ReadName();
			PropertyInfo property = ownerType.GetProperty(text);
			if (property != null)
			{
				Property property2;
				if (Tools.WasConvertedToString(property.PropertyType))
				{
					property2 = deserialize(elementId, text, typeof(string));
					property2.ConvertedType = property.PropertyType;
				}
				else
				{
					property2 = deserialize(elementId, text, property.PropertyType);
				}
				properties.Add(property2);
				continue;
			}
			FieldInfo field = ownerType.GetField(text);
			if (field != null)
			{
				Property property3;
				if (Tools.WasConvertedToString(field.FieldType))
				{
					property3 = deserialize(elementId, text, typeof(string));
					property3.ConvertedType = field.FieldType;
				}
				else
				{
					property3 = deserialize(elementId, text, field.FieldType);
				}
				properties.Add(property3);
			}
		}
	}

	private void parseCollectionProperty(CollectionProperty property)
	{
		property.ElementType = _reader.ReadType();
		readProperties(property.Properties, property.Type);
		readItems(property.Items, property.ElementType);
	}

	private void parseDictionaryProperty(DictionaryProperty property)
	{
		property.KeyType = _reader.ReadType();
		property.ValueType = _reader.ReadType();
		readProperties(property.Properties, property.Type);
		readDictionaryItems(property.Items, property.KeyType, property.ValueType);
	}

	private void readDictionaryItems(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType)
	{
		int num = _reader.ReadNumber();
		for (int i = 0; i < num; i++)
		{
			readDictionaryItem(items, expectedKeyType, expectedValueType);
		}
	}

	private void readDictionaryItem(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType)
	{
		byte elementId = _reader.ReadElementId();
		Property key = deserialize(elementId, expectedKeyType);
		elementId = _reader.ReadElementId();
		Property value = deserialize(elementId, expectedValueType);
		KeyValueItem item = new KeyValueItem(key, value);
		items.Add(item);
	}

	private void parseSingleDimensionalArrayProperty(SingleDimensionalArrayProperty property)
	{
		property.ElementType = _reader.ReadType();
		property.LowerBound = _reader.ReadNumber();
		readItems(property.Items, property.ElementType);
	}

	private void readItems(ICollection<Property> items, Type expectedElementType)
	{
		int num = _reader.ReadNumber();
		for (int i = 0; i < num; i++)
		{
			byte elementId = _reader.ReadElementId();
			Property item = deserialize(elementId, expectedElementType);
			items.Add(item);
		}
	}

	private void parseMultiDimensionalArrayProperty(MultiDimensionalArrayProperty property)
	{
		property.ElementType = _reader.ReadType();
		readDimensionInfos(property.DimensionInfos);
		readMultiDimensionalArrayItems(property.Items, property.ElementType);
	}

	private void readMultiDimensionalArrayItems(IList<MultiDimensionalArrayItem> items, Type expectedElementType)
	{
		int num = _reader.ReadNumber();
		for (int i = 0; i < num; i++)
		{
			readMultiDimensionalArrayItem(items, expectedElementType);
		}
	}

	private void readMultiDimensionalArrayItem(IList<MultiDimensionalArrayItem> items, Type expectedElementType)
	{
		int[] indexes = _reader.ReadNumbers();
		byte elementId = _reader.ReadElementId();
		Property value = deserialize(elementId, expectedElementType);
		MultiDimensionalArrayItem item = new MultiDimensionalArrayItem(indexes, value);
		items.Add(item);
	}

	private void readDimensionInfos(IList<DimensionInfo> dimensionInfos)
	{
		int num = _reader.ReadNumber();
		for (int i = 0; i < num; i++)
		{
			readDimensionInfo(dimensionInfos);
		}
	}

	private void readDimensionInfo(IList<DimensionInfo> dimensionInfos)
	{
		DimensionInfo dimensionInfo = new DimensionInfo();
		dimensionInfo.Length = _reader.ReadNumber();
		dimensionInfo.LowerBound = _reader.ReadNumber();
		dimensionInfos.Add(dimensionInfo);
	}

	private void parseSimpleProperty(SimpleProperty property)
	{
		property.Value = _reader.ReadValue(property.Type);
	}

	private static Property createProperty(byte elementId, string propertyName, Type propertyType)
	{
		switch (elementId)
		{
		case 6:
			return new SimpleProperty(propertyName, propertyType);
		case 2:
		case 8:
			return new ComplexProperty(propertyName, propertyType);
		case 1:
		case 10:
			return new CollectionProperty(propertyName, propertyType);
		case 3:
		case 11:
			return new DictionaryProperty(propertyName, propertyType);
		case 7:
		case 12:
			return new SingleDimensionalArrayProperty(propertyName, propertyType);
		case 4:
		case 13:
			return new MultiDimensionalArrayProperty(propertyName, propertyType);
		case 5:
			return new NullProperty(propertyName);
		default:
			return null;
		}
	}

	private Property createProperty(int referenceId, string propertyName, Type propertyType)
	{
		ReferenceTargetProperty referenceTargetProperty = _propertyCache[referenceId];
		ReferenceTargetProperty obj = (ReferenceTargetProperty)Property.CreateInstance(referenceTargetProperty.Art, propertyName, propertyType);
		referenceTargetProperty.Reference.Count++;
		obj.MakeFlatCopyFrom(referenceTargetProperty);
		obj.Reference = new ReferenceInfo
		{
			Id = referenceId
		};
		return obj;
	}
}

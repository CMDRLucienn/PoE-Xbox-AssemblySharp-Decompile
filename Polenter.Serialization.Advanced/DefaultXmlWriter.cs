using System;
using System.IO;
using System.Text;
using System.Xml;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;

namespace Polenter.Serialization.Advanced;

public sealed class DefaultXmlWriter : IXmlWriter
{
	private readonly XmlWriterSettings _settings;

	private readonly ISimpleValueConverter _simpleValueConverter;

	private readonly ITypeNameConverter _typeNameProvider;

	private XmlWriter _writer;

	public DefaultXmlWriter(ITypeNameConverter typeNameProvider, ISimpleValueConverter simpleValueConverter, XmlWriterSettings settings)
	{
		if (typeNameProvider == null)
		{
			throw new ArgumentNullException("typeNameProvider");
		}
		if (simpleValueConverter == null)
		{
			throw new ArgumentNullException("simpleValueConverter");
		}
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		_simpleValueConverter = simpleValueConverter;
		_settings = settings;
		_typeNameProvider = typeNameProvider;
	}

	public void WriteStartElement(string elementId)
	{
		_writer.WriteStartElement(elementId);
	}

	public void WriteEndElement()
	{
		_writer.WriteEndElement();
	}

	public void WriteAttribute(string attributeId, string text)
	{
		if (text != null)
		{
			_writer.WriteAttributeString(attributeId, text);
		}
	}

	public void WriteAttribute(string attributeId, Type type)
	{
		if (!(type == null))
		{
			string text = _typeNameProvider.ConvertToTypeName(type);
			WriteAttribute(attributeId, text);
		}
	}

	public void WriteAttribute(string attributeId, int number)
	{
		_writer.WriteAttributeString(attributeId, number.ToString());
	}

	public void WriteAttribute(string attributeId, int[] numbers)
	{
		string arrayOfIntAsText = getArrayOfIntAsText(numbers);
		_writer.WriteAttributeString(attributeId, arrayOfIntAsText);
	}

	public void WriteAttribute(string attributeId, object value)
	{
		if (value != null)
		{
			string value2 = _simpleValueConverter.ConvertToString(value);
			_writer.WriteAttributeString(attributeId, value2);
		}
	}

	public void Open(Stream stream)
	{
		_writer = XmlWriter.Create(stream, _settings);
		_writer.WriteStartDocument(standalone: true);
	}

	public void Close()
	{
		_writer.WriteEndDocument();
		_writer.Close();
	}

	private static string getArrayOfIntAsText(int[] values)
	{
		if (values.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int num in values)
		{
			stringBuilder.Append(num.ToString());
			stringBuilder.Append(",");
		}
		return stringBuilder.ToString().TrimEnd(',');
	}
}

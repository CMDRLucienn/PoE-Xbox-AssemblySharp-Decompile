using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;

namespace Polenter.Serialization.Advanced;

public sealed class DefaultXmlReader : IXmlReader
{
	private readonly XmlReaderSettings _settings;

	private readonly ITypeNameConverter _typeNameConverter;

	private readonly ISimpleValueConverter _valueConverter;

	private XmlReader _currentReader;

	private Stack<XmlReader> _readerStack;

	public DefaultXmlReader(ITypeNameConverter typeNameConverter, ISimpleValueConverter valueConverter, XmlReaderSettings settings)
	{
		if (typeNameConverter == null)
		{
			throw new ArgumentNullException("typeNameConverter");
		}
		if (valueConverter == null)
		{
			throw new ArgumentNullException("valueConverter");
		}
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		_typeNameConverter = typeNameConverter;
		_valueConverter = valueConverter;
		_settings = settings;
	}

	public string ReadElement()
	{
		while (_currentReader.Read())
		{
			if (_currentReader.NodeType == XmlNodeType.Element)
			{
				return _currentReader.Name;
			}
		}
		return null;
	}

	public IEnumerable<string> ReadSubElements()
	{
		_currentReader.MoveToElement();
		XmlReader subReader = _currentReader.ReadSubtree();
		subReader.Read();
		pushCurrentReader(subReader);
		try
		{
			string text = ReadElement();
			while (!string.IsNullOrEmpty(text))
			{
				yield return text;
				text = ReadElement();
			}
		}
		finally
		{
			DefaultXmlReader defaultXmlReader = this;
			subReader.Close();
			defaultXmlReader.popCurrentReader();
		}
	}

	public string GetAttributeAsString(string attributeName)
	{
		if (!_currentReader.MoveToAttribute(attributeName))
		{
			return null;
		}
		return _currentReader.Value;
	}

	public Type GetAttributeAsType(string attributeName)
	{
		string attributeAsString = GetAttributeAsString(attributeName);
		return _typeNameConverter.ConvertToType(attributeAsString);
	}

	public int GetAttributeAsInt(string attributeName)
	{
		if (!_currentReader.MoveToAttribute(attributeName))
		{
			return 0;
		}
		return _currentReader.ReadContentAsInt();
	}

	public int[] GetAttributeAsArrayOfInt(string attributeName)
	{
		if (!_currentReader.MoveToAttribute(attributeName))
		{
			return null;
		}
		return getArrayOfIntFromText(_currentReader.Value);
	}

	public object GetAttributeAsObject(string attributeName, Type expectedType)
	{
		string attributeAsString = GetAttributeAsString(attributeName);
		return _valueConverter.ConvertFromString(attributeAsString, expectedType);
	}

	public void Open(Stream stream)
	{
		_readerStack = new Stack<XmlReader>();
		XmlReader reader = XmlReader.Create(stream, _settings);
		pushCurrentReader(reader);
	}

	public void Close()
	{
		_currentReader.Close();
	}

	private void popCurrentReader()
	{
		if (_readerStack.Count > 0)
		{
			_readerStack.Pop();
		}
		if (_readerStack.Count > 0)
		{
			_currentReader = _readerStack.Peek();
		}
		else
		{
			_currentReader = null;
		}
	}

	private void pushCurrentReader(XmlReader reader)
	{
		_readerStack.Push(reader);
		_currentReader = reader;
	}

	private static int[] getArrayOfIntFromText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string[] array = text.Split(',');
		if (array.Length == 0)
		{
			return null;
		}
		List<int> list = new List<int>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			int item = int.Parse(array2[i]);
			list.Add(item);
		}
		return list.ToArray();
	}
}

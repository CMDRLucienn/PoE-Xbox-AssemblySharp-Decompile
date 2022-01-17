using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced;

public sealed class SizeOptimizedBinaryReader : IBinaryReader
{
	private delegate T HeaderCallback<T>(string text);

	private readonly Encoding _encoding;

	private readonly IList<string> _names = new List<string>();

	private readonly ITypeNameConverter _typeNameConverter;

	private readonly IList<Type> _types = new List<Type>();

	private BinaryReader _reader;

	public SizeOptimizedBinaryReader(ITypeNameConverter typeNameConverter, Encoding encoding)
	{
		if (typeNameConverter == null)
		{
			throw new ArgumentNullException("typeNameConverter");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		_typeNameConverter = typeNameConverter;
		_encoding = encoding;
	}

	public byte ReadElementId()
	{
		return _reader.ReadByte();
	}

	public Type ReadType()
	{
		int index = BinaryReaderTools.ReadNumber(_reader);
		return _types[index];
	}

	public int ReadNumber()
	{
		return BinaryReaderTools.ReadNumber(_reader);
	}

	public int[] ReadNumbers()
	{
		return BinaryReaderTools.ReadNumbers(_reader);
	}

	public string ReadName()
	{
		int index = BinaryReaderTools.ReadNumber(_reader);
		return _names[index];
	}

	public object ReadValue(Type expectedType)
	{
		return BinaryReaderTools.ReadValue(expectedType, _reader);
	}

	public void Open(Stream stream)
	{
		_reader = new BinaryReader(stream, _encoding);
		_names.Clear();
		readHeader(_reader, _names, (string text) => text);
		_types.Clear();
		readHeader(_reader, _types, _typeNameConverter.ConvertToType);
	}

	public void Close()
	{
	}

	private static void readHeader<T>(BinaryReader reader, IList<T> items, HeaderCallback<T> readCallback)
	{
		int num = BinaryReaderTools.ReadNumber(reader);
		for (int i = 0; i < num; i++)
		{
			string text = BinaryReaderTools.ReadString(reader);
			T item = readCallback(text);
			items.Add(item);
		}
	}
}

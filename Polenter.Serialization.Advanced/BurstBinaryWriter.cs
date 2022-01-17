using System;
using System.IO;
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced;

public sealed class BurstBinaryWriter : IBinaryWriter
{
	private readonly Encoding _encoding;

	private readonly ITypeNameConverter _typeNameConverter;

	private BinaryWriter _writer;

	public BurstBinaryWriter(ITypeNameConverter typeNameConverter, Encoding encoding)
	{
		if (typeNameConverter == null)
		{
			throw new ArgumentNullException("typeNameConverter");
		}
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		_encoding = encoding;
		_typeNameConverter = typeNameConverter;
	}

	public void WriteElementId(byte id)
	{
		_writer.Write(id);
	}

	public void WriteNumber(int number)
	{
		BinaryWriterTools.WriteNumber(number, _writer);
	}

	public void WriteNumbers(int[] numbers)
	{
		BinaryWriterTools.WriteNumbers(numbers, _writer);
	}

	public void WriteType(Type type)
	{
		if (type == null)
		{
			_writer.Write(value: false);
			return;
		}
		_writer.Write(value: true);
		_writer.Write(_typeNameConverter.ConvertToTypeName(type));
	}

	public void WriteName(string name)
	{
		BinaryWriterTools.WriteString(name, _writer);
	}

	public void WriteValue(object value)
	{
		BinaryWriterTools.WriteValue(value, _writer);
	}

	public void Open(Stream stream)
	{
		_writer = new BinaryWriter(stream, _encoding);
	}

	public void Close()
	{
		_writer.Flush();
	}
}

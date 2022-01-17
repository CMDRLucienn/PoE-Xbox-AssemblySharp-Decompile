using System;
using System.IO;
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced;

public sealed class BurstBinaryReader : IBinaryReader
{
	private readonly Encoding _encoding;

	private readonly ITypeNameConverter _typeNameConverter;

	private BinaryReader _reader;

	public BurstBinaryReader(ITypeNameConverter typeNameConverter, Encoding encoding)
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

	public string ReadName()
	{
		return BinaryReaderTools.ReadString(_reader);
	}

	public byte ReadElementId()
	{
		return _reader.ReadByte();
	}

	public Type ReadType()
	{
		if (!_reader.ReadBoolean())
		{
			return null;
		}
		string typeName = _reader.ReadString();
		return _typeNameConverter.ConvertToType(typeName);
	}

	public int ReadNumber()
	{
		return BinaryReaderTools.ReadNumber(_reader);
	}

	public int[] ReadNumbers()
	{
		return BinaryReaderTools.ReadNumbers(_reader);
	}

	public object ReadValue(Type expectedType)
	{
		return BinaryReaderTools.ReadValue(expectedType, _reader);
	}

	public void Open(Stream stream)
	{
		_reader = new BinaryReader(stream, _encoding);
	}

	public void Close()
	{
	}
}

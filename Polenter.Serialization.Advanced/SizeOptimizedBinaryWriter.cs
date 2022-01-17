using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Core.Binary;

namespace Polenter.Serialization.Advanced;

public sealed class SizeOptimizedBinaryWriter : IBinaryWriter
{
	private sealed class ByteWriteCommand : WriteCommand
	{
		public byte Data { get; set; }

		public ByteWriteCommand(byte data)
		{
			Data = data;
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(Data);
		}
	}

	private sealed class NumberWriteCommand : WriteCommand
	{
		public int Data { get; set; }

		public NumberWriteCommand(int data)
		{
			Data = data;
		}

		public override void Write(BinaryWriter writer)
		{
			BinaryWriterTools.WriteNumber(Data, writer);
		}
	}

	private sealed class NumbersWriteCommand : WriteCommand
	{
		public int[] Data { get; set; }

		public NumbersWriteCommand(int[] data)
		{
			Data = data;
		}

		public override void Write(BinaryWriter writer)
		{
			BinaryWriterTools.WriteNumbers(Data, writer);
		}
	}

	private sealed class ValueWriteCommand : WriteCommand
	{
		public object Data { get; set; }

		public ValueWriteCommand(object data)
		{
			Data = data;
		}

		public override void Write(BinaryWriter writer)
		{
			BinaryWriterTools.WriteValue(Data, writer);
		}
	}

	private abstract class WriteCommand
	{
		public abstract void Write(BinaryWriter writer);
	}

	private readonly Encoding _encoding;

	private readonly ITypeNameConverter _typeNameConverter;

	private List<WriteCommand> _cache;

	private IndexGenerator<string> _names;

	private Stream _stream;

	private IndexGenerator<Type> _types;

	public SizeOptimizedBinaryWriter(ITypeNameConverter typeNameConverter, Encoding encoding)
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
		_cache.Add(new ByteWriteCommand(id));
	}

	public void WriteType(Type type)
	{
		int indexOfItem = _types.GetIndexOfItem(type);
		_cache.Add(new NumberWriteCommand(indexOfItem));
	}

	public void WriteName(string name)
	{
		int indexOfItem = _names.GetIndexOfItem(name);
		_cache.Add(new NumberWriteCommand(indexOfItem));
	}

	public void WriteValue(object value)
	{
		_cache.Add(new ValueWriteCommand(value));
	}

	public void WriteNumber(int number)
	{
		_cache.Add(new NumberWriteCommand(number));
	}

	public void WriteNumbers(int[] numbers)
	{
		_cache.Add(new NumbersWriteCommand(numbers));
	}

	public void Open(Stream stream)
	{
		_stream = stream;
		_cache = new List<WriteCommand>();
		_types = new IndexGenerator<Type>();
		_names = new IndexGenerator<string>();
	}

	public void Close()
	{
		BinaryWriter binaryWriter = new BinaryWriter(_stream, _encoding);
		writeNamesHeader(binaryWriter);
		writeTypesHeader(binaryWriter);
		writeCache(_cache, binaryWriter);
		binaryWriter.Flush();
	}

	private static void writeCache(List<WriteCommand> cache, BinaryWriter writer)
	{
		foreach (WriteCommand item in cache)
		{
			item.Write(writer);
		}
	}

	private void writeNamesHeader(BinaryWriter writer)
	{
		BinaryWriterTools.WriteNumber(_names.Items.Count, writer);
		foreach (string item in _names.Items)
		{
			BinaryWriterTools.WriteString(item, writer);
		}
	}

	private void writeTypesHeader(BinaryWriter writer)
	{
		BinaryWriterTools.WriteNumber(_types.Items.Count, writer);
		foreach (Type item in _types.Items)
		{
			BinaryWriterTools.WriteString(_typeNameConverter.ConvertToTypeName(item), writer);
		}
	}
}

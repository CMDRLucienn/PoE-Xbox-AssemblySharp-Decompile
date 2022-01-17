using System;
using System.IO;

namespace Polenter.Serialization.Core.Binary;

public static class BinaryWriterTools
{
	public static void WriteNumber(int number, BinaryWriter writer)
	{
		byte numberSize = NumberSize.GetNumberSize(number);
		writer.Write(numberSize);
		if (numberSize > 0)
		{
			switch (numberSize)
			{
			case 1:
				writer.Write((byte)number);
				break;
			case 2:
				writer.Write((short)number);
				break;
			default:
				writer.Write(number);
				break;
			}
		}
	}

	public static void WriteNumbers(int[] numbers, BinaryWriter writer)
	{
		WriteNumber(numbers.Length, writer);
		for (int i = 0; i < numbers.Length; i++)
		{
			WriteNumber(numbers[i], writer);
		}
	}

	public static void WriteValue(object value, BinaryWriter writer)
	{
		if (value == null)
		{
			writer.Write(value: false);
			return;
		}
		writer.Write(value: true);
		writeValueCore(value, writer);
	}

	public static void WriteString(string text, BinaryWriter writer)
	{
		if (string.IsNullOrEmpty(text))
		{
			writer.Write(value: false);
			return;
		}
		writer.Write(value: true);
		writer.Write(text);
	}

	private static void writeValueCore(object value, BinaryWriter writer)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value", "Written data can not be null.");
		}
		Type type = value.GetType();
		if (type == typeof(byte[]))
		{
			writeArrayOfByte((byte[])value, writer);
			return;
		}
		if (type == typeof(string))
		{
			writer.Write((string)value);
			return;
		}
		if (type == typeof(bool))
		{
			writer.Write((bool)value);
			return;
		}
		if (type == typeof(byte))
		{
			writer.Write((byte)value);
			return;
		}
		if (type == typeof(char))
		{
			writer.Write((char)value);
			return;
		}
		if (type == typeof(DateTime))
		{
			writer.Write(((DateTime)value).Ticks);
			return;
		}
		if (type == typeof(Guid))
		{
			writer.Write(((Guid)value).ToByteArray());
			return;
		}
		if (type == typeof(decimal))
		{
			writer.Write((decimal)value);
			return;
		}
		if (type == typeof(double))
		{
			writer.Write((double)value);
			return;
		}
		if (type == typeof(short))
		{
			writer.Write((short)value);
			return;
		}
		if (type == typeof(int))
		{
			writer.Write((int)value);
			return;
		}
		if (type == typeof(long))
		{
			writer.Write((long)value);
			return;
		}
		if (type == typeof(sbyte))
		{
			writer.Write((sbyte)value);
			return;
		}
		if (type == typeof(float))
		{
			writer.Write((float)value);
			return;
		}
		if (type == typeof(ushort))
		{
			writer.Write((ushort)value);
			return;
		}
		if (type == typeof(uint))
		{
			writer.Write((uint)value);
			return;
		}
		if (type == typeof(ulong))
		{
			writer.Write((ulong)value);
			return;
		}
		if (type == typeof(TimeSpan))
		{
			writer.Write(((TimeSpan)value).Ticks);
			return;
		}
		if (type.IsEnum)
		{
			writer.Write(Convert.ToInt32(value));
			return;
		}
		if (isType(type))
		{
			writer.Write(((Type)value).AssemblyQualifiedName);
			return;
		}
		throw new InvalidOperationException($"Unknown simple type: {type.FullName}");
	}

	private static void writeDecimal(decimal value, BinaryWriter writer)
	{
		int[] bits = decimal.GetBits(value);
		writer.Write(bits[0]);
		writer.Write(bits[1]);
		writer.Write(bits[2]);
		writer.Write(bits[3]);
	}

	private static bool isType(Type type)
	{
		if (!(type == typeof(Type)))
		{
			return type.IsSubclassOf(typeof(Type));
		}
		return true;
	}

	private static void writeArrayOfByte(byte[] data, BinaryWriter writer)
	{
		WriteNumber(data.Length, writer);
		writer.Write(data);
	}
}

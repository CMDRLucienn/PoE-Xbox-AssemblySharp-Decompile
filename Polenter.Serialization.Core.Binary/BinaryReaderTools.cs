using System;
using System.Collections.Generic;
using System.IO;

namespace Polenter.Serialization.Core.Binary;

public static class BinaryReaderTools
{
	public static string ReadString(BinaryReader reader)
	{
		if (!reader.ReadBoolean())
		{
			return null;
		}
		return reader.ReadString();
	}

	public static int ReadNumber(BinaryReader reader)
	{
		return reader.ReadByte() switch
		{
			0 => 0, 
			1 => reader.ReadByte(), 
			2 => reader.ReadInt16(), 
			_ => reader.ReadInt32(), 
		};
	}

	public static int[] ReadNumbers(BinaryReader reader)
	{
		int num = ReadNumber(reader);
		if (num == 0)
		{
			return new int[0];
		}
		List<int> list = new List<int>();
		for (int i = 0; i < num; i++)
		{
			list.Add(ReadNumber(reader));
		}
		return list.ToArray();
	}

	public static object ReadValue(Type expectedType, BinaryReader reader)
	{
		if (!reader.ReadBoolean())
		{
			return null;
		}
		return readValueCore(expectedType, reader);
	}

	private static object readValueCore(Type type, BinaryReader reader)
	{
		try
		{
			if (type == typeof(byte[]))
			{
				return readArrayOfByte(reader);
			}
			if (type == typeof(string))
			{
				return reader.ReadString();
			}
			if (type == typeof(bool))
			{
				return reader.ReadBoolean();
			}
			if (type == typeof(byte))
			{
				return reader.ReadByte();
			}
			if (type == typeof(char))
			{
				return reader.ReadChar();
			}
			if (type == typeof(DateTime))
			{
				return new DateTime(reader.ReadInt64());
			}
			if (type == typeof(Guid))
			{
				return new Guid(reader.ReadBytes(16));
			}
			if (type == typeof(decimal))
			{
				return reader.ReadDecimal();
			}
			if (type == typeof(double))
			{
				return reader.ReadDouble();
			}
			if (type == typeof(short))
			{
				return reader.ReadInt16();
			}
			if (type == typeof(int))
			{
				return reader.ReadInt32();
			}
			if (type == typeof(long))
			{
				return reader.ReadInt64();
			}
			if (type == typeof(sbyte))
			{
				return reader.ReadSByte();
			}
			if (type == typeof(float))
			{
				return reader.ReadSingle();
			}
			if (type == typeof(ushort))
			{
				return reader.ReadUInt16();
			}
			if (type == typeof(uint))
			{
				return reader.ReadUInt32();
			}
			if (type == typeof(ulong))
			{
				return reader.ReadUInt64();
			}
			if (type == typeof(TimeSpan))
			{
				return new TimeSpan(reader.ReadInt64());
			}
			if (type.IsEnum)
			{
				return readEnumeration(type, reader);
			}
			if (isType(type))
			{
				return Type.GetType(reader.ReadString(), throwOnError: true);
			}
			throw new InvalidOperationException($"Unknown simple type: {type.FullName}");
		}
		catch (Exception innerException)
		{
			throw new SimpleValueParsingException($"Invalid type: {type}. See details in the inner exception.", innerException);
		}
	}

	private static object readDecimal(BinaryReader reader)
	{
		return new decimal(new int[4]
		{
			reader.ReadInt32(),
			reader.ReadInt32(),
			reader.ReadInt32(),
			reader.ReadInt32()
		});
	}

	private static bool isType(Type type)
	{
		if (!(type == typeof(Type)))
		{
			return type.IsSubclassOf(typeof(Type));
		}
		return true;
	}

	private static object readEnumeration(Type expectedType, BinaryReader reader)
	{
		int value = reader.ReadInt32();
		return Enum.ToObject(expectedType, value);
	}

	private static byte[] readArrayOfByte(BinaryReader reader)
	{
		int num = ReadNumber(reader);
		if (num == 0)
		{
			return null;
		}
		return reader.ReadBytes(num);
	}
}

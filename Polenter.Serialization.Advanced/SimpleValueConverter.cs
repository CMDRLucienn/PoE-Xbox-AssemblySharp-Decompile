using System;
using System.Globalization;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Advanced;

public sealed class SimpleValueConverter : ISimpleValueConverter
{
	private readonly CultureInfo _cultureInfo;

	private readonly ITypeNameConverter _typeNameConverter;

	private const char NullChar = '\0';

	private const string NullCharAsString = "&#x0;";

	public SimpleValueConverter()
	{
		_cultureInfo = CultureInfo.InvariantCulture;
		_typeNameConverter = new TypeNameConverter();
	}

	public SimpleValueConverter(CultureInfo cultureInfo, ITypeNameConverter typeNameConverter)
	{
		_cultureInfo = cultureInfo;
		_typeNameConverter = typeNameConverter;
	}

	public string ConvertToString(object value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		if (value.GetType() == typeof(byte[]))
		{
			return Convert.ToBase64String((byte[])value);
		}
		if (isType(value))
		{
			return _typeNameConverter.ConvertToTypeName((Type)value);
		}
		if (value.Equals('\0'))
		{
			return "&#x0;";
		}
		return Convert.ToString(value, _cultureInfo);
	}

	public object ConvertFromString(string text, Type type)
	{
		try
		{
			if (type == typeof(string))
			{
				return text;
			}
			if (type == typeof(bool))
			{
				return Convert.ToBoolean(text, _cultureInfo);
			}
			if (type == typeof(byte))
			{
				return Convert.ToByte(text, _cultureInfo);
			}
			if (type == typeof(char))
			{
				if (text == "&#x0;")
				{
					return '\0';
				}
				return Convert.ToChar(text, _cultureInfo);
			}
			if (type == typeof(DateTime))
			{
				return Convert.ToDateTime(text, _cultureInfo);
			}
			if (type == typeof(decimal))
			{
				return Convert.ToDecimal(text, _cultureInfo);
			}
			if (type == typeof(double))
			{
				return Convert.ToDouble(text, _cultureInfo);
			}
			if (type == typeof(short))
			{
				return Convert.ToInt16(text, _cultureInfo);
			}
			if (type == typeof(int))
			{
				return Convert.ToInt32(text, _cultureInfo);
			}
			if (type == typeof(long))
			{
				return Convert.ToInt64(text, _cultureInfo);
			}
			if (type == typeof(sbyte))
			{
				return Convert.ToSByte(text, _cultureInfo);
			}
			if (type == typeof(float))
			{
				return Convert.ToSingle(text, _cultureInfo);
			}
			if (type == typeof(ushort))
			{
				return Convert.ToUInt16(text, _cultureInfo);
			}
			if (type == typeof(uint))
			{
				return Convert.ToUInt32(text, _cultureInfo);
			}
			if (type == typeof(ulong))
			{
				return Convert.ToUInt64(text, _cultureInfo);
			}
			if (type == typeof(TimeSpan))
			{
				return TimeSpan.Parse(text);
			}
			if (type == typeof(Guid))
			{
				return new Guid(text);
			}
			if (type.IsEnum)
			{
				return Enum.Parse(type, text, ignoreCase: true);
			}
			if (type == typeof(byte[]))
			{
				return Convert.FromBase64String(text);
			}
			if (isType(type))
			{
				return _typeNameConverter.ConvertToType(text);
			}
			throw new InvalidOperationException($"Unknown simple type: {type.FullName}");
		}
		catch (Exception innerException)
		{
			throw new SimpleValueParsingException($"Invalid value: {text}. See details in the inner exception.", innerException);
		}
	}

	private static bool isType(object value)
	{
		return value as Type != null;
	}
}

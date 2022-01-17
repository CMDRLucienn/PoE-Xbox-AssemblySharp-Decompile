using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Polenter.Serialization.Advanced.Serializing;

namespace Polenter.Serialization.Advanced;

public sealed class TypeNameConverter : ITypeNameConverter
{
	private readonly Dictionary<Type, string> _cache = new Dictionary<Type, string>();

	public bool IncludeAssemblyVersion { get; private set; }

	public bool IncludeCulture { get; private set; }

	public bool IncludePublicKeyToken { get; private set; }

	public TypeNameConverter()
	{
	}

	public TypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken)
	{
		IncludeAssemblyVersion = includeAssemblyVersion;
		IncludeCulture = includeCulture;
		IncludePublicKeyToken = includePublicKeyToken;
	}

	public string ConvertToTypeName(Type type)
	{
		if (type == null)
		{
			return string.Empty;
		}
		if (_cache.ContainsKey(type))
		{
			return _cache[type];
		}
		string text = type.AssemblyQualifiedName;
		if (!IncludeAssemblyVersion)
		{
			text = removeAssemblyVersion(text);
		}
		if (!IncludeCulture)
		{
			text = removeCulture(text);
		}
		if (!IncludePublicKeyToken)
		{
			text = removePublicKeyToken(text);
		}
		_cache.Add(type, text);
		return text;
	}

	public Type ConvertToType(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		return Type.GetType(typeName, throwOnError: true);
	}

	private static string removePublicKeyToken(string typename)
	{
		return Regex.Replace(typename, ", PublicKeyToken=\\w+", string.Empty);
	}

	private static string removeCulture(string typename)
	{
		return Regex.Replace(typename, ", Culture=\\w+", string.Empty);
	}

	private static string removeAssemblyVersion(string typename)
	{
		return Regex.Replace(typename, ", Version=\\d+.\\d+.\\d+.\\d+", string.Empty);
	}
}

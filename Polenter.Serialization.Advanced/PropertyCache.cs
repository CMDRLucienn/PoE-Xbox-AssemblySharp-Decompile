using System;
using System.Collections.Generic;
using System.Reflection;

namespace Polenter.Serialization.Advanced;

internal class PropertyCache
{
	private readonly Dictionary<Type, IList<PropertyInfo>> _cache = new Dictionary<Type, IList<PropertyInfo>>();

	public IList<PropertyInfo> TryGetPropertyInfos(Type type)
	{
		if (!_cache.ContainsKey(type))
		{
			return null;
		}
		return _cache[type];
	}

	public void Add(Type key, IList<PropertyInfo> value)
	{
		_cache.Add(key, value);
	}
}

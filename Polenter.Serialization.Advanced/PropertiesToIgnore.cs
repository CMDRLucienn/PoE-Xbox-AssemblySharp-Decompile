using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Polenter.Serialization.Advanced;

public sealed class PropertiesToIgnore
{
	private sealed class TypePropertiesToIgnore
	{
		private IList<string> _propertyNames;

		public Type Type { get; set; }

		public IList<string> PropertyNames
		{
			get
			{
				if (_propertyNames == null)
				{
					_propertyNames = new List<string>();
				}
				return _propertyNames;
			}
			set
			{
				_propertyNames = value;
			}
		}

		public TypePropertiesToIgnore(Type type)
		{
			Type = type;
		}
	}

	private sealed class TypePropertiesToIgnoreCollection : KeyedCollection<Type, TypePropertiesToIgnore>
	{
		protected override Type GetKeyForItem(TypePropertiesToIgnore item)
		{
			return item.Type;
		}

		public int IndexOf(Type type)
		{
			for (int i = 0; i < base.Count; i++)
			{
				if (base[i].Type == type)
				{
					return i;
				}
			}
			return -1;
		}

		public TypePropertiesToIgnore TryFind(Type type)
		{
			foreach (TypePropertiesToIgnore item in base.Items)
			{
				if (item.Type == type)
				{
					return item;
				}
			}
			return null;
		}

		public bool ContainsProperty(Type type, string propertyName)
		{
			return TryFind(type)?.PropertyNames.Contains(propertyName) ?? false;
		}
	}

	private readonly TypePropertiesToIgnoreCollection _propertiesToIgnore = new TypePropertiesToIgnoreCollection();

	public void Add(Type type, string propertyName)
	{
		TypePropertiesToIgnore propertiesToIgnore = getPropertiesToIgnore(type);
		if (!propertiesToIgnore.PropertyNames.Contains(propertyName))
		{
			propertiesToIgnore.PropertyNames.Add(propertyName);
		}
	}

	private TypePropertiesToIgnore getPropertiesToIgnore(Type type)
	{
		TypePropertiesToIgnore typePropertiesToIgnore = _propertiesToIgnore.TryFind(type);
		if (typePropertiesToIgnore == null)
		{
			typePropertiesToIgnore = new TypePropertiesToIgnore(type);
			_propertiesToIgnore.Add(typePropertiesToIgnore);
		}
		return typePropertiesToIgnore;
	}

	public bool Contains(Type type, string propertyName)
	{
		return _propertiesToIgnore.ContainsProperty(type, propertyName);
	}
}

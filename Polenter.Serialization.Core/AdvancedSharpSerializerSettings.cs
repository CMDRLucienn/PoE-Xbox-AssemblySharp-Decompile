using System;
using System.Collections.Generic;
using Polenter.Serialization.Advanced;
using Polenter.Serialization.Advanced.Serializing;

namespace Polenter.Serialization.Core;

public class AdvancedSharpSerializerSettings
{
	private PropertiesToIgnore _propertiesToIgnore;

	private IList<Type> _attributesToIgnore;

	public PropertiesToIgnore PropertiesToIgnore
	{
		get
		{
			if (_propertiesToIgnore == null)
			{
				_propertiesToIgnore = new PropertiesToIgnore();
			}
			return _propertiesToIgnore;
		}
		set
		{
			_propertiesToIgnore = value;
		}
	}

	public IList<Type> AttributesToIgnore
	{
		get
		{
			if (_attributesToIgnore == null)
			{
				_attributesToIgnore = new List<Type>();
			}
			return _attributesToIgnore;
		}
		set
		{
			_attributesToIgnore = value;
		}
	}

	public string RootName { get; set; }

	public ITypeNameConverter TypeNameConverter { get; set; }

	public AdvancedSharpSerializerSettings()
	{
		AttributesToIgnore.Add(typeof(ExcludeFromSerializationAttribute));
		RootName = "Root";
	}
}

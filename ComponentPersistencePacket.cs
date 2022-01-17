using System;
using System.Collections.Generic;
using Polenter.Serialization;

[Serializable]
public class ComponentPersistencePacket
{
	private Type m_componentType;

	private Dictionary<string, object> m_variables = new Dictionary<string, object>(32);

	public string TypeString
	{
		get
		{
			if (!(m_componentType == null))
			{
				return m_componentType.ToString();
			}
			return string.Empty;
		}
		set
		{
			m_componentType = Type.GetType(value);
		}
	}

	[ExcludeFromSerialization]
	public Type ComponentType
	{
		get
		{
			return m_componentType;
		}
		set
		{
			m_componentType = value;
		}
	}

	public Dictionary<string, object> Variables
	{
		get
		{
			return m_variables;
		}
		set
		{
			foreach (string key in value.Keys)
			{
				m_variables.Add(key, value[key]);
			}
		}
	}

	public void AddVariable(string name, object val)
	{
		if (!Variables.ContainsKey(name))
		{
			Variables.Add(name, val);
		}
		else
		{
			Variables[name] = val;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is string)
		{
			return ComponentType.ToString() == obj as string;
		}
		return ComponentType == obj.GetType();
	}

	public override int GetHashCode()
	{
		return ComponentType.GetHashCode();
	}
}

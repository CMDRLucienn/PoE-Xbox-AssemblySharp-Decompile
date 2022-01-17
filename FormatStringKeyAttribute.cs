using System;
using System.Text;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class FormatStringKeyAttribute : PropertyAttribute
{
	private string[] m_Keys;

	public FormatStringKeyAttribute(params object[] keys)
	{
		m_Keys = new string[keys.Length];
		for (int i = 0; i < keys.Length; i++)
		{
			if (keys[i] is string)
			{
				m_Keys[i] = (string)keys[i];
				continue;
			}
			throw new ArgumentException("Keys must be strings.", "keys");
		}
	}

	public int GetKeyCount()
	{
		return m_Keys.Length;
	}

	public string GetKey(int index)
	{
		return m_Keys[index];
	}

	public string GetTooltip()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_Keys.Length; i++)
		{
			stringBuilder.Append('{');
			stringBuilder.Append(i);
			stringBuilder.Append("}: ");
			stringBuilder.Append(m_Keys[i]);
			if (i < m_Keys.Length - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}
}

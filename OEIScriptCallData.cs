using System;
using System.Collections.Generic;

[Serializable]
public class OEIScriptCallData
{
	public string m_fullName;

	public List<string> m_parameters = new List<string>();

	public string FullName
	{
		get
		{
			return m_fullName;
		}
		set
		{
			m_fullName = value;
		}
	}

	public List<string> Parameters
	{
		get
		{
			return m_parameters;
		}
		set
		{
			m_parameters = value;
		}
	}
}

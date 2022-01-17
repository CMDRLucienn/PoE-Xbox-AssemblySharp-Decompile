using UnityEngine;

internal class StringTooltip : ITooltipContent
{
	private string m_String = "";

	public StringTooltip(string text)
	{
		m_String = text;
	}

	public string GetTooltipName(GameObject owner)
	{
		return "";
	}

	public string GetTooltipContent(GameObject owner)
	{
		return m_String;
	}

	public Texture GetTooltipIcon()
	{
		return null;
	}
}

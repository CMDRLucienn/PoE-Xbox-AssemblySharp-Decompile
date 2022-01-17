using System;
using System.Text;
using UnityEngine;

public class UIVersionNumber : MonoBehaviour
{
	private StringBuilder m_stringBuilder = new StringBuilder();

	private static string[] s_CommandLineArgs;

	private UILabel m_Label;

	static UIVersionNumber()
	{
		s_CommandLineArgs = Environment.GetCommandLineArgs();
	}

	private void Start()
	{
		m_Label = GetComponent<UILabel>();
		UpdateText();
	}

	private void Update()
	{
		if (m_Label.isVisible)
		{
			UpdateText();
		}
	}

	private void UpdateText()
	{
		if ((bool)m_Label)
		{
			m_Label.text = ProductConfiguration.GetVersion();
		}
	}
}

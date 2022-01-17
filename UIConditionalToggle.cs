using System.Collections.Generic;
using OEIFormats.FlowCharts;
using UnityEngine;

public class UIConditionalToggle : MonoBehaviour
{
	public List<ConditionalScript> Scripts = new List<ConditionalScript>();

	private List<ConditionalCall> m_InternalScripts;

	private UIWidget m_Widget;

	private UIPanel m_Panel;

	private void OnEnable()
	{
		if (m_InternalScripts == null)
		{
			m_InternalScripts = new List<ConditionalCall>();
			foreach (ConditionalScript script in Scripts)
			{
				ConditionalCall conditionalCall = new ConditionalCall();
				conditionalCall.Operator = script.Op;
				conditionalCall.Not = script.Not;
				conditionalCall.Data.FullName = script.Function;
				conditionalCall.Data.Parameters = script.Parameters;
				m_InternalScripts.Add(conditionalCall);
			}
		}
		if (m_Widget == null)
		{
			m_Widget = GetComponent<UIWidget>();
		}
		if (m_Panel == null)
		{
			m_Panel = GetComponent<UIPanel>();
		}
		bool flag = ConditionalToggle.Evaluate(m_InternalScripts, base.gameObject);
		if ((bool)m_Widget)
		{
			m_Widget.alpha = (flag ? 1f : 0f);
		}
		if ((bool)m_Panel)
		{
			m_Panel.alpha = (flag ? 1f : 0f);
		}
	}
}

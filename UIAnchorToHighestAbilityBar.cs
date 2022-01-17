using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
public class UIAnchorToHighestAbilityBar : MonoBehaviour
{
	private UIAnchor m_Anchor;

	private void Start()
	{
		m_Anchor = GetComponent<UIAnchor>();
	}

	private void Update()
	{
		UIAbilityBarRowBg uIAbilityBarRowBg = UIAbilityBar.Instance.HighestRowBackground();
		UIWidget widgetContainer = m_Anchor.widgetContainer;
		m_Anchor.widgetContainer = (uIAbilityBarRowBg ? uIAbilityBarRowBg.Sizer : null);
		if (widgetContainer != m_Anchor.widgetContainer)
		{
			m_Anchor.Update();
		}
	}
}

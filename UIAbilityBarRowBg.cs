using UnityEngine;

public class UIAbilityBarRowBg : MonoBehaviour
{
	public UIWidget Sizer;

	public UIWidget Pointer;

	private UIAnchor m_Anchor;

	private void Awake()
	{
		m_Anchor = GetComponent<UIAnchor>();
	}

	public void Show(UIWidget anchorPoint, int level)
	{
		base.gameObject.SetActive(value: true);
		Pointer.gameObject.SetActive(level > 0);
		m_Anchor.widgetContainer = anchorPoint;
		m_Anchor.enabled = m_Anchor.widgetContainer;
		if (!m_Anchor.enabled)
		{
			base.transform.localPosition = new Vector3(0f, level * UIAbilityBar.Instance.VertSpacing, 0f);
		}
	}
}

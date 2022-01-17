using UnityEngine;

public class UIWindowSwitcher : MonoBehaviour
{
	public UIAnchor Anchor;

	private UIPanel m_Panel;

	private bool m_Visible;

	public static UIWindowSwitcher Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		m_Panel = GetComponent<UIPanel>();
		Update();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if ((bool)m_Panel)
		{
			bool flag = (bool)Anchor.widgetContainer && (bool)Anchor.widgetContainer.panel && Anchor.widgetContainer.panel.alpha > 0f;
			m_Panel.alpha = ((m_Visible && flag) ? 1f : 0f);
		}
	}

	public void Show(UIWidget anchor)
	{
		Anchor.widgetContainer = anchor;
		m_Visible = true;
		Update();
	}

	public void Hide()
	{
		m_Visible = false;
	}
}

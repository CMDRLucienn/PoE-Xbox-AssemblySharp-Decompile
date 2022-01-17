using UnityEngine;

public class UIHideAtMinResolution : MonoBehaviour
{
	public Vector2 MinResolution = new Vector2(1280f, 720f);

	private UIWidget m_Widget;

	private UIPanel m_Panel;

	public bool ChildWidgets;

	private UIWidget[] m_ChildWidgets;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Panel = GetComponent<UIPanel>();
		if (ChildWidgets)
		{
			m_ChildWidgets = GetComponentsInChildren<UIWidget>();
		}
		Update();
	}

	private void Update()
	{
		int num = (((float)Screen.width > MinResolution.x || (float)Screen.height > MinResolution.y) ? 1 : 0);
		if ((bool)m_Widget)
		{
			m_Widget.alpha = num;
		}
		if ((bool)m_Panel)
		{
			m_Panel.alpha = num;
		}
		if (m_ChildWidgets != null)
		{
			UIWidget[] childWidgets = m_ChildWidgets;
			for (int i = 0; i < childWidgets.Length; i++)
			{
				childWidgets[i].alpha = num;
			}
		}
	}
}

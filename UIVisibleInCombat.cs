using UnityEngine;

public class UIVisibleInCombat : MonoBehaviour
{
	private UIWidget m_Widget;

	private UIPanel m_Panel;

	public bool Invert;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Panel = GetComponent<UIPanel>();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		float alpha = ((GameState.InCombat != Invert) ? 1f : 0f);
		if ((bool)m_Panel)
		{
			m_Panel.alpha = alpha;
		}
		if ((bool)m_Widget)
		{
			m_Widget.alpha = alpha;
		}
	}
}

using UnityEngine;

public abstract class UIBaseTooltip : MonoBehaviour
{
	protected UIAnchor m_Anchor;

	public UIPanel Panel;

	private int m_FramesTillShowTooltip = -1;

	protected virtual void Start()
	{
		Hide();
	}

	protected virtual void Update()
	{
		if (m_FramesTillShowTooltip > 0)
		{
			m_FramesTillShowTooltip--;
		}
		else if (m_FramesTillShowTooltip == 0)
		{
			m_FramesTillShowTooltip = -1;
			Panel.alpha = 1f;
		}
	}

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected abstract void SetText(string text);

	public virtual void Show(UIWidget button, string text)
	{
		m_FramesTillShowTooltip = 1;
		if (m_Anchor == null)
		{
			m_Anchor = GetComponent<UIAnchor>();
		}
		if ((bool)m_Anchor)
		{
			m_Anchor.widgetContainer = button;
		}
		SetText(text);
	}

	public virtual void Hide()
	{
		if (Panel != null)
		{
			Panel.alpha = 0f;
		}
		m_FramesTillShowTooltip = -1;
	}
}

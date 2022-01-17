using UnityEngine;

public class UIActionBarPanelStretch : MonoBehaviour
{
	private UIPanel m_Panel;

	public UIAnchor SolidHudAnchor;

	public UIAnchor NonSolidAnchor;

	public Transform ResolutionScaler;

	private void Start()
	{
		m_Panel = GetComponent<UIPanel>();
	}

	private void Update()
	{
		if (SolidHudAnchor != null)
		{
			SolidHudAnchor.enabled = GameState.Option.GetOption(GameOption.BoolOption.SOLID_HUD);
		}
		if (NonSolidAnchor != null)
		{
			NonSolidAnchor.enabled = !SolidHudAnchor.enabled;
		}
		if (m_Panel != null)
		{
			m_Panel.clipRange = new Vector4(m_Panel.clipRange.x, m_Panel.clipRange.y, (float)Screen.width / ResolutionScaler.transform.localScale.x, m_Panel.clipRange.w);
		}
	}
}

using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class UISolidHudPanel : MonoBehaviour
{
	public UIPanel CopyAlphaFrom;

	private UIPanel m_Panel;

	public bool Invert;

	private void Awake()
	{
		m_Panel = GetComponent<UIPanel>();
	}

	private void Update()
	{
		float num = (CopyAlphaFrom ? CopyAlphaFrom.alpha : 1f);
		m_Panel.alpha = num * ((GameState.Option.GetOption(GameOption.BoolOption.SOLID_HUD) ^ Invert) ? 1f : 0f);
	}
}

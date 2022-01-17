using UnityEngine;

public class GamePausedVisibility : MonoBehaviour
{
	private bool m_Paused = true;

	private bool m_Slowmo;

	private bool m_Fastmo;

	private bool m_Hidden;

	private UIWidget[] m_Widgets;

	public UILabel Label;

	public bool OnPaused = true;

	public bool OnSlowmo;

	public bool OnFastmo;

	private bool m_Visible
	{
		get
		{
			if ((m_Paused && OnPaused) || (m_Slowmo && OnSlowmo) || (m_Fastmo && OnFastmo))
			{
				if (InGameHUD.Instance != null)
				{
					return !InGameHUD.Instance.HidePause;
				}
				return false;
			}
			return false;
		}
	}

	private void Start()
	{
		m_Widgets = GetComponentsInChildren<UIWidget>();
		if ((bool)InGameHUD.Instance)
		{
			InGameHUD.Instance.OnHudVisibilityChanged += HudVisChanged;
		}
	}

	private void OnDestroy()
	{
		if ((bool)InGameHUD.Instance)
		{
			InGameHUD.Instance.OnHudVisibilityChanged -= HudVisChanged;
		}
	}

	private void Update()
	{
		bool flag = TimeController.Instance == null || TimeController.Instance.Paused;
		bool flag2 = !flag && TimeController.Instance != null && Time.timeScale < 1f;
		bool flag3 = !flag && TimeController.Instance != null && Time.timeScale > 1f;
		if (m_Paused != flag || m_Slowmo != flag2 || m_Fastmo != flag3 || (InGameHUD.Instance != null && m_Hidden != InGameHUD.Instance.HidePause))
		{
			m_Paused = flag;
			m_Slowmo = flag2;
			m_Fastmo = flag3;
			m_Hidden = InGameHUD.Instance == null || InGameHUD.Instance.HidePause;
			UpdateVis();
		}
	}

	private void HudVisChanged(bool vis)
	{
		UpdateVis();
	}

	private void UpdateVis()
	{
		UIWidget[] widgets = m_Widgets;
		for (int i = 0; i < widgets.Length; i++)
		{
			widgets[i].gameObject.SetActive(m_Visible && InGameHUD.Instance.HudUserMode != 2);
		}
		if ((bool)Label)
		{
			if (OnPaused && m_Paused)
			{
				Label.text = GUIUtils.GetText(1403).ToUpper();
			}
			else if (OnSlowmo && m_Slowmo)
			{
				Label.text = GUIUtils.GetText(1496) + " [" + GameState.Controls.GetControlString(MappedControl.SLOW_TOGGLE) + "]";
			}
			else if (OnFastmo && m_Fastmo)
			{
				Label.text = GUIUtils.GetText(1497) + " [" + GameState.Controls.GetControlString(MappedControl.FAST_TOGGLE) + "]";
			}
		}
	}
}

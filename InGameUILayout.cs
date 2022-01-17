using AnimationOrTween;
using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class InGameUILayout : MonoBehaviour
{
	private static UIRoot s_root;

	private static Camera s_NGUICamera;

	private UIPanel m_HudPanel;

	public UIWidget Fade;

	public UITweener[] TweenOnHUDShowHide = new UITweener[0];

	public int HudHeight;

	public static UIRoot Root
	{
		get
		{
			if (s_root == null)
			{
				s_root = UIRoot.GetFirstUIRoot();
			}
			return s_root;
		}
	}

	public static Camera NGUICamera
	{
		get
		{
			if (s_NGUICamera == null)
			{
				s_NGUICamera = NGUITools.FindCameraForLayer(LayerUtility.FindLayerValue("NGUI"));
			}
			return s_NGUICamera;
		}
	}

	public static InGameUILayout Instance { get; private set; }

	public static float toNguiScale => (float)Root.activeHeight / (float)Screen.height;

	private void Awake()
	{
		Instance = this;
		if (InGameHUD.Instance.ShowHUD)
		{
			InGameHUD.Instance.ShowHUD = true;
		}
	}

	private void Start()
	{
		Fade.gameObject.SetActive(value: true);
		FadeManager.Instance.SetFadeTarget(Fade);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		s_root = null;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static Vector3 ScreenToNgui(Vector3 screen)
	{
		return new Vector3((screen.x - (float)Screen.width / 2f) * toNguiScale, (screen.y - (float)Screen.height / 2f) * toNguiScale, screen.z);
	}

	public static Vector3 NguiToScreen(Vector3 ngui)
	{
		return new Vector3(ngui.x / toNguiScale + (float)Screen.width / 2f, (float)Screen.height / 2f - ngui.y / toNguiScale, ngui.z);
	}

	public static Rect NguiToScreen(Rect ngui)
	{
		return new Rect(ngui.x / toNguiScale + (float)Screen.width / 2f, (float)Screen.height / 2f - ngui.y / toNguiScale, ngui.width / toNguiScale, ngui.height / toNguiScale);
	}

	public static Vector3 NguiToScreenScale(Vector3 ngui)
	{
		return new Vector3(ngui.x / toNguiScale, ngui.y / toNguiScale, ngui.z);
	}

	public void ShowHud()
	{
		if (m_HudPanel == null)
		{
			m_HudPanel = GetComponent<UIPanel>();
		}
		if (m_HudPanel != null)
		{
			m_HudPanel.alpha = 1f;
		}
		UITweener[] tweenOnHUDShowHide = TweenOnHUDShowHide;
		foreach (UITweener uITweener in tweenOnHUDShowHide)
		{
			if ((bool)uITweener)
			{
				uITweener.Play(forward: false);
			}
		}
	}

	public void HideHud()
	{
		UITweener[] tweenOnHUDShowHide = TweenOnHUDShowHide;
		for (int i = 0; i < tweenOnHUDShowHide.Length; i++)
		{
			tweenOnHUDShowHide[i].Play(forward: true);
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				partyMemberAI.HideDestinationTarget();
			}
		}
	}

	private void HudTweenEnded(UITweener tween)
	{
		if (tween.direction != Direction.Reverse)
		{
			if (m_HudPanel == null)
			{
				m_HudPanel = GetComponent<UIPanel>();
			}
			if (m_HudPanel != null)
			{
				m_HudPanel.alpha = 0f;
			}
		}
	}
}

using System;
using UnityEngine;

public class UIOffscreenObjectPointer : UIScreenEdgeAvoider
{
	public UIPanel VisualsContainer;

	public GameObject PointerArrow;

	public UITexture IconTexture;

	public GameObject Collider;

	public int ScreenMargin = 20;

	private GameObject m_Target;

	private Transform m_TargetTransform;

	private InGameUILayout m_Layout;

	private UIResolutionScaler m_LayoutScaler;

	public override Bounds Bounds => new Bounds(base.transform.position, new Vector3(base.transform.lossyScale.x * ((float)ScreenMargin + IconTexture.transform.localScale.x), base.transform.lossyScale.y * ((float)ScreenMargin + IconTexture.transform.localScale.y), 1f));

	public GameObject Target
	{
		get
		{
			return m_Target;
		}
		set
		{
			Portrait component;
			if (m_Target != null)
			{
				component = m_Target.GetComponent<Portrait>();
				if ((bool)component)
				{
					component.OnPortraitChanged -= PortraitChanged;
				}
			}
			m_Target = value;
			m_TargetTransform = m_Target.transform;
			component = m_Target.GetComponent<Portrait>();
			if (component != null)
			{
				Shader shader = Shader.Find("Unlit/Transparent Clip Colored");
				if (shader != null)
				{
					IconTexture.material = new Material(shader);
				}
				IconTexture.mainTexture = component.TextureSmall;
				component.OnPortraitChanged += PortraitChanged;
				UIClippedTexture component2 = IconTexture.GetComponent<UIClippedTexture>();
				if ((bool)component2)
				{
					component2.OnTextureChanged();
				}
			}
			else
			{
				IconTexture.gameObject.SetActive(value: false);
			}
		}
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Layout == null)
		{
			m_Layout = InGameUILayout.Instance;
			m_LayoutScaler = m_Layout.GetComponentInChildren<UIResolutionScaler>();
			if ((bool)Collider)
			{
				UIEventListener uIEventListener = UIEventListener.Get(Collider);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
			}
		}
	}

	private void OnChildClick(GameObject sender)
	{
		if ((bool)Target)
		{
			CameraControl.Instance.FocusOnPoint(m_TargetTransform.position, 0.4f);
		}
	}

	private void PortraitChanged()
	{
		Target = Target;
	}

	private void Update()
	{
		if (!InGameHUD.Instance.ShowHUD || GameState.Option.GetOption(GameOption.BoolOption.HIDE_OFFSCREEN_POINTERS))
		{
			VisualsContainer.alpha = 0f;
		}
		else if (Target != null && Camera.main != null)
		{
			Init();
			Vector3 vector = Camera.main.WorldToScreenPoint(m_TargetTransform.position);
			if (vector.x >= 0f && vector.y >= 0f && vector.x < (float)Screen.width && vector.y < (float)Screen.height)
			{
				VisualsContainer.alpha = 0f;
				return;
			}
			VisualsContainer.alpha = 1f;
			int num = (int)((float)m_Layout.HudHeight * m_LayoutScaler.GetScaleY());
			int screenMargin = ScreenMargin;
			int num2 = ScreenMargin + num;
			int num3 = Screen.height - ScreenMargin;
			int num4 = Screen.width - ScreenMargin;
			int num5 = num4 - screenMargin;
			int num6 = num3 - num2;
			Vector3 screen = vector;
			Vector3 vector2 = new Vector3(num5 / 2, num6 / 2, 0f) - vector;
			float num7 = (float)num6 / (float)num5;
			float num8 = vector2.y / vector2.x;
			if (Mathf.Abs(num8 / num7) < 1f)
			{
				if (vector2.x < 0f)
				{
					screen.x = num4;
				}
				else
				{
					screen.x = screenMargin;
				}
				screen.y = (float)(num6 / 2) - Mathf.Sign(vector2.x) * num8 * (float)num5 / 2f;
			}
			else
			{
				if (vector2.y < 0f)
				{
					screen.y = num3;
				}
				else
				{
					screen.y = num2;
				}
				screen.x = (float)(num5 / 2) - Mathf.Sign(vector2.y) * (float)num6 / (2f * num8);
			}
			screen.z = base.transform.localPosition.z;
			base.transform.localPosition = InGameUILayout.ScreenToNgui(screen);
			UIScreenEdgeBlocker.Avoid(this);
			PointerArrow.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(screen.x - vector.x, vector.y - screen.y) * 57.29578f);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}

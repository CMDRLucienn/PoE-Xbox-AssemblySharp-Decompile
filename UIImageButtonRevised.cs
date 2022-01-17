using System;
using UnityEngine;

public class UIImageButtonRevised : UIIsButton
{
	public delegate void StateChangedDelegate();

	private UIWidget m_Widget;

	public bool OverrideNeutral;

	public Color OverrideNeutralColor;

	public bool OverrideMoused;

	public Color OverrideMousedColor;

	public Color DisabledColor = Color.black;

	[Tooltip("If set, the button will not touch the alpha of the widget.")]
	public bool LeaveAlpha;

	public UIAudioList.UIAudioType ButtonDownSound = UIAudioList.UIAudioType.ButtonDown;

	public UIAudioList.UIAudioType ButtonUpSound = UIAudioList.UIAudioType.ButtonUp;

	public UIAudioList.UIAudioType HoverSound;

	private const float m_MouseoffFadeDuration = 0.14f;

	private TweenColorIndependent m_MouseoffTweenOut;

	[Tooltip("If set, the user cannot change the state of this button via any interaction.")]
	public bool StateLocked;

	[Tooltip("Make the object pixel-perfect when a new sprite is set?")]
	public bool ResizeSprite;

	public bool RepositionSprite = true;

	public bool ColorSprite = true;

	public bool ChangeSprite = true;

	public string normalSprite;

	public string hoverSprite;

	public string pressedSprite;

	public string disabledSprite;

	private bool hovered;

	private bool pressed;

	private bool forceDown;

	private bool forceHover;

	private bool m_WasHovered;

	private bool overrideHovered;

	private bool overridePressed;

	private bool iAmDown;

	public UILabel Label;

	public Vector2 LabelOffsetWhenDown;

	public Vector2 GraphicOffsetWhenDown;

	public int DepthChangeWhenDown;

	private Vector3 m_LabelInitial;

	private Vector3 m_WidgetInitial;

	public StateChangedDelegate StateChanged;

	public bool IsInspectable;

	public Color NeutralColor
	{
		get
		{
			if (OverrideNeutral)
			{
				return OverrideNeutralColor;
			}
			return new Color(0f, 0f, 0f);
		}
	}

	public Color MousedColor
	{
		get
		{
			if (OverrideMoused)
			{
				return OverrideMousedColor;
			}
			return new Color(1f, 1f, 1f);
		}
	}

	public bool Pressed
	{
		get
		{
			if (!pressed)
			{
				return forceDown;
			}
			return true;
		}
	}

	public bool Hovered
	{
		get
		{
			if (!hovered)
			{
				return forceHover;
			}
			return true;
		}
	}

	public void SetNeutralColor(Color c)
	{
		OverrideNeutral = true;
		OverrideNeutralColor = c;
		if ((bool)m_MouseoffTweenOut)
		{
			m_MouseoffTweenOut.to = NeutralColor;
		}
		if (Application.isPlaying)
		{
			UpdateImage();
		}
	}

	public void SetMousedColor(Color c)
	{
		OverrideMoused = true;
		OverrideMousedColor = c;
		if ((bool)m_MouseoffTweenOut)
		{
			m_MouseoffTweenOut.from = MousedColor;
		}
		if (Application.isPlaying)
		{
			UpdateImage();
		}
	}

	private void OnEnable()
	{
		UpdateImage();
	}

	private void OnDisable()
	{
		OnHover(isOver: false);
		if (m_Widget is UISprite)
		{
			(m_Widget as UISprite).spriteName = disabledSprite;
		}
		if (ColorSprite)
		{
			m_MouseoffTweenOut.Reset();
			m_MouseoffTweenOut.enabled = false;
			SetWidgetColor(DisabledColor);
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void SetWidgetColor(Color color)
	{
		if (LeaveAlpha)
		{
			color.a = m_Widget.color.a;
		}
		m_Widget.color = color;
	}

	private void Init()
	{
		if (m_Widget == null)
		{
			UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				m_Widget = componentsInChildren[0];
			}
		}
		if (!(m_MouseoffTweenOut != null))
		{
			m_MouseoffTweenOut = GetComponent<TweenColorIndependent>();
			if (m_MouseoffTweenOut == null)
			{
				m_MouseoffTweenOut = base.gameObject.AddComponent<TweenColorIndependent>();
				m_MouseoffTweenOut.from = MousedColor;
				m_MouseoffTweenOut.to = NeutralColor;
				m_MouseoffTweenOut.color = NeutralColor;
				m_MouseoffTweenOut.duration = 0.14f;
			}
			m_MouseoffTweenOut.ResetTo(1f);
			if (m_Widget != null)
			{
				m_WidgetInitial = m_Widget.transform.localPosition;
			}
			if (string.IsNullOrEmpty(normalSprite) && m_Widget is UISprite)
			{
				normalSprite = (m_Widget as UISprite).spriteName;
			}
			if (string.IsNullOrEmpty(hoverSprite))
			{
				hoverSprite = normalSprite;
			}
			if (string.IsNullOrEmpty(pressedSprite))
			{
				pressedSprite = normalSprite;
			}
			if (string.IsNullOrEmpty(disabledSprite))
			{
				disabledSprite = normalSprite;
			}
			if (Label != null)
			{
				m_LabelInitial = Label.transform.localPosition;
			}
		}
	}

	public void SetWidgetRootPosition(Vector3 local)
	{
		m_WidgetInitial = local;
	}

	private void Update()
	{
		UpdateImage();
	}

	public void ChangeNormalSprite(string sprite)
	{
		if (!(sprite == normalSprite))
		{
			if (hoverSprite == normalSprite)
			{
				hoverSprite = sprite;
			}
			if (pressedSprite == normalSprite)
			{
				pressedSprite = sprite;
			}
			if (disabledSprite == normalSprite)
			{
				disabledSprite = sprite;
			}
			normalSprite = sprite;
		}
	}

	public void resetSprites()
	{
		normalSprite = null;
		hoverSprite = null;
		pressedSprite = null;
		disabledSprite = null;
	}

	public void SetOverridePressed(bool state)
	{
		if (overridePressed != state)
		{
			overridePressed = state;
			UpdateImage();
		}
	}

	public void SetOverrideHighlighted(bool state)
	{
		if (overrideHovered != state)
		{
			overrideHovered = state;
			UpdateImage();
		}
	}

	public void AddAdditionalCollider(GameObject go)
	{
		UIEventListener uIEventListener = UIEventListener.Get(go);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnOtherHover));
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnOtherPress));
	}

	private void OnOtherHover(GameObject go, bool isOver)
	{
		OnHover(isOver);
	}

	private void OnOtherPress(GameObject go, bool pressed)
	{
		OnPress(pressed);
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled && !StateLocked)
		{
			hovered = isOver;
			UpdateImage();
			if (isOver && (bool)GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(HoverSound, preventOverlap: false);
			}
		}
	}

	private void OnPress(bool pressed)
	{
		if (!base.enabled || StateLocked)
		{
			return;
		}
		if (GlobalAudioPlayer.Instance != null)
		{
			if (pressed)
			{
				GlobalAudioPlayer.Instance.Play(ButtonDownSound, preventOverlap: false);
			}
			else
			{
				GlobalAudioPlayer.Instance.Play(ButtonUpSound, preventOverlap: false);
			}
		}
		this.pressed = pressed;
		UpdateImage();
	}

	public void ForceHover(bool val)
	{
		if (val != forceHover)
		{
			forceHover = val;
			UpdateImage();
		}
	}

	public void ForceDown(bool val)
	{
		if (val != forceDown)
		{
			forceDown = val;
			UpdateImage();
		}
	}

	private void UpdateTweenColor()
	{
		if (base.enabled && (bool)m_Widget && m_MouseoffTweenOut.enabled && ColorSprite)
		{
			SetWidgetColor(m_MouseoffTweenOut.color);
		}
	}

	public void UpdateImage()
	{
		if (StateChanged != null)
		{
			StateChanged();
		}
		Init();
		UISprite uISprite = m_Widget as UISprite;
		string text = (uISprite ? uISprite.spriteName : null);
		if (Label != null && RepositionSprite)
		{
			if (Pressed || overridePressed)
			{
				Label.transform.localPosition = m_LabelInitial + (Vector3)LabelOffsetWhenDown;
			}
			else
			{
				Label.transform.localPosition = m_LabelInitial;
			}
		}
		if (!(m_Widget != null))
		{
			return;
		}
		if (base.enabled)
		{
			if (Pressed || overridePressed)
			{
				if ((bool)uISprite && ChangeSprite)
				{
					uISprite.spriteName = pressedSprite;
				}
				if (RepositionSprite)
				{
					m_Widget.transform.localPosition = m_WidgetInitial + (Vector3)GraphicOffsetWhenDown;
				}
				if (!iAmDown)
				{
					iAmDown = true;
					m_Widget.depth += DepthChangeWhenDown;
				}
			}
			else
			{
				if ((bool)uISprite && ChangeSprite)
				{
					uISprite.spriteName = ((Hovered || overrideHovered) ? hoverSprite : normalSprite);
				}
				if (RepositionSprite)
				{
					m_Widget.transform.localPosition = m_WidgetInitial;
				}
				if (iAmDown)
				{
					iAmDown = false;
					m_Widget.depth -= DepthChangeWhenDown;
				}
			}
			if (Hovered || overrideHovered || Pressed || overridePressed)
			{
				if (ColorSprite && (InGameHUD.Instance == null || InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.None || IsInspectable))
				{
					if ((bool)m_MouseoffTweenOut && !m_WasHovered)
					{
						m_MouseoffTweenOut.Reset();
						m_MouseoffTweenOut.enabled = false;
					}
					SetWidgetColor(MousedColor);
				}
				m_WasHovered = true;
			}
			else
			{
				if (ColorSprite)
				{
					if ((bool)m_MouseoffTweenOut && m_WasHovered)
					{
						m_MouseoffTweenOut.Play(forward: true);
					}
					else
					{
						SetWidgetColor(NeutralColor);
					}
				}
				m_WasHovered = false;
			}
		}
		else
		{
			if ((bool)uISprite && ChangeSprite)
			{
				uISprite.spriteName = disabledSprite;
			}
			if (ColorSprite)
			{
				m_MouseoffTweenOut.Reset();
				m_MouseoffTweenOut.enabled = false;
				SetWidgetColor(DisabledColor);
			}
		}
		UpdateTweenColor();
		if (ResizeSprite && (!uISprite || uISprite.spriteName != text))
		{
			m_Widget.MakePixelPerfect();
		}
	}
}

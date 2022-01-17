using UnityEngine;

public class UITriggerIcon : UIScreenEdgeAvoider
{
	public UITexture Icon;

	public UILabel Label;

	public UIEventListener.VoidDelegate ClickCallback;

	public UIEventListener.BoolDelegate HoverCallback;

	private Vector3 m_World;

	private bool m_AvoidHud;

	private UIAnchor m_LabelAnchor;

	private const float screenMargin = 20f;

	public bool Hiding { get; private set; }

	public override Bounds Bounds => new Bounds(base.transform.position, new Vector3(base.transform.parent.lossyScale.x * (Icon.transform.localScale.x + 20f), base.transform.parent.lossyScale.y * (Icon.transform.localScale.y + 20f), 1f));

	private void Awake()
	{
		if ((bool)Label)
		{
			m_LabelAnchor = Label.GetComponent<UIAnchor>();
		}
	}

	public void Set(Vector3 world, bool avoidHud)
	{
		m_World = world;
		m_AvoidHud = avoidHud;
		if ((bool)Label)
		{
			Label.text = "";
		}
		Update();
	}

	public void SetString(string str)
	{
		if ((bool)Label)
		{
			Label.text = str;
		}
	}

	private void Update()
	{
		Vector3 screen = Camera.main.WorldToScreenPoint(m_World);
		base.transform.localPosition = InGameUILayout.ScreenToNgui(screen);
		if (Anchor.enabled)
		{
			Anchor.Update();
		}
		if ((bool)m_LabelAnchor && m_LabelAnchor.enabled)
		{
			m_LabelAnchor.Update();
		}
		if (!Hiding && m_AvoidHud)
		{
			UIWidget widgetContainer = Anchor.widgetContainer;
			UIScreenEdgeBlocker.AvoidAnchor(this);
			if ((bool)widgetContainer && widgetContainer != Anchor.widgetContainer)
			{
				Anchor.widgetContainer = widgetContainer;
				Anchor.enabled = true;
				UITriggerManager.Instance.Hide(this);
			}
		}
		else
		{
			Anchor.enabled = false;
		}
	}

	public void Hide()
	{
		Hiding = true;
		UITweener component = Icon.GetComponent<UITweener>();
		if ((bool)component)
		{
			component.Reset();
			component.Play(forward: true);
		}
	}

	public void Show()
	{
		Hiding = false;
		Anchor.enabled = false;
		Anchor.transform.localPosition = Vector2.zero;
		UITweener component = Icon.GetComponent<UITweener>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		Icon.alpha = 1f;
	}

	private void OnTweenEnded()
	{
		if ((bool)Anchor)
		{
			Anchor.widgetContainer = null;
		}
		base.gameObject.SetActive(value: false);
		UITriggerManager.Instance.Pool(this);
	}
}

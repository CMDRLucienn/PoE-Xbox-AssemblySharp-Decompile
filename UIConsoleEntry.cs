using System;
using UnityEngine;

public class UIConsoleEntry : MonoBehaviour
{
	public UILabel Label;

	public GameObject Collider;

	private UIGlossaryEnabledLabel m_GlossaryEnabledLabel;

	private Console.ConsoleMessage m_Message;

	private Color m_Color;

	private UITable m_ChildParent;

	private int m_RepositionTable;

	private bool m_isChild;

	private static uint s_SortId = 0u;

	private static string s_FormatString = "".PadRight(uint.MaxValue.ToString().Length, '0');

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnColliderHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider);
		uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnColliderTooltip));
		UIEventListener uIEventListener3 = UIEventListener.Get(Collider);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnColliderClick));
		m_GlossaryEnabledLabel = Label.GetComponent<UIGlossaryEnabledLabel>();
	}

	public void Set(Console.ConsoleMessage message)
	{
		m_Message = message;
		if (!m_GlossaryEnabledLabel)
		{
			m_GlossaryEnabledLabel = Label.GetComponent<UIGlossaryEnabledLabel>();
		}
		m_GlossaryEnabledLabel.enabled = string.IsNullOrEmpty(m_Message.m_verbosemessage);
		m_Color = message.m_color;
		Label.text = m_Message.m_message;
		Label.color = m_Color;
		if ((bool)m_ChildParent)
		{
			GameUtilities.DestroyImmediate(m_ChildParent.gameObject);
			m_ChildParent = null;
		}
		if (message.Children != null && message.Children.Count > 0)
		{
			GameObject gameObject = new GameObject("Children");
			Transform obj = gameObject.transform;
			obj.parent = base.gameObject.transform;
			obj.localPosition = new Vector3(20f, 0f, 0f);
			obj.localScale = Vector3.one;
			obj.localRotation = Quaternion.identity;
			UIAnchor uIAnchor = gameObject.AddComponent<UIAnchor>();
			uIAnchor.DisableX = true;
			uIAnchor.widgetContainer = Label;
			uIAnchor.pixelOffset.y = -2f;
			uIAnchor.side = UIAnchor.Side.Bottom;
			m_ChildParent = gameObject.AddComponent<UITable>();
			m_ChildParent.columns = 1;
			m_ChildParent.padding.y = 2f;
			foreach (Console.ConsoleMessage child in message.Children)
			{
				UIConsoleEntry component = UnityEngine.Object.Instantiate(UIConsole.Instance.EntryPrefab.gameObject).GetComponent<UIConsoleEntry>();
				Transform obj2 = component.transform;
				obj2.parent = gameObject.transform;
				obj2.localPosition = Vector3.zero;
				obj2.localScale = UIConsole.Instance.EntryPrefab.transform.localScale;
				obj2.localRotation = Quaternion.identity;
				component.Set(child);
				component.m_isChild = true;
				component.gameObject.SetActive(value: true);
			}
			m_ChildParent.gameObject.SetActive(value: false);
			m_RepositionTable = 2;
		}
		else
		{
			Label.transform.localPosition = new Vector3(0f, -18f, 0f);
		}
		base.gameObject.name = s_SortId.ToString(s_FormatString);
		s_SortId++;
	}

	public void Reposition()
	{
		if ((bool)m_ChildParent)
		{
			m_ChildParent.Reposition();
		}
	}

	private void Update()
	{
		Label.lineWidth = (int)UIConsole.Instance.MaxWidth + (m_isChild ? (-20) : 0);
		if (m_RepositionTable > 0)
		{
			m_RepositionTable--;
			UIConsole.Instance.RepositionCurrentTable();
		}
	}

	private void OnColliderTooltip(GameObject sender, bool over)
	{
		if (!string.IsNullOrEmpty(m_Message.m_verbosemessage))
		{
			if (over)
			{
				UICombatLogTooltip.GlobalShow(Label, NGUITools.StripSymbols(m_Message.m_verbosemessage));
			}
			else
			{
				UICombatLogTooltip.GlobalHide();
			}
		}
		else
		{
			m_GlossaryEnabledLabel.OnColliderTooltip(sender, over);
		}
	}

	private void OnColliderHover(GameObject sender, bool over)
	{
		if (string.IsNullOrEmpty(m_Message.m_verbosemessage))
		{
			m_GlossaryEnabledLabel.OnColliderHover(sender, over);
		}
		else if (over)
		{
			Label.color = Color.white;
		}
		else
		{
			Label.color = m_Color;
		}
		if (!over)
		{
			UICombatLogTooltip.GlobalHide();
		}
	}

	private void OnColliderClick(GameObject sender)
	{
		if (m_Message.OnClickCallback != null)
		{
			m_Message.OnClickCallback();
			return;
		}
		if ((bool)m_ChildParent)
		{
			m_ChildParent.gameObject.SetActive(!m_ChildParent.gameObject.activeSelf);
			UIConsole.Instance.RepositionCurrentTable();
			m_RepositionTable = 3;
			return;
		}
		OnColliderTooltip(null, over: true);
		if (string.IsNullOrEmpty(m_Message.m_verbosemessage))
		{
			m_GlossaryEnabledLabel.OnColliderClick(sender);
		}
	}
}

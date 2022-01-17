using System;
using UnityEngine;

public class UIActionBarChanterPhrase : MonoBehaviour
{
	public UIWidget BarBackground;

	public UIWidget BarForeground;

	public const int BarForegroundBorders = 8;

	private const int ShiftAmount = 12;

	public UITexture Icon;

	public UIWidget ActiveFrame;

	private Phrase m_Phrase;

	public int Shift;

	public float Width => BarBackground.transform.localScale.x;

	public float Right => base.transform.localPosition.x + Width;

	public Phrase Phrase => m_Phrase;

	public void SetPhrase(Phrase phrase, int shift)
	{
		if (m_Phrase != phrase || Shift != shift)
		{
			Shift = shift;
			base.name = phrase.name;
			m_Phrase = phrase;
			Refresh();
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Icon.gameObject);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		UIEventListener uIEventListener2 = UIEventListener.Get(Icon.gameObject);
		uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnChildRightClick));
	}

	private void OnChildTooltip(GameObject go, bool show)
	{
		OnTooltip(show);
	}

	private void OnChildRightClick(GameObject go)
	{
		OnRightClick();
	}

	private void OnDisable()
	{
		UIAbilityTooltip.GlobalHide();
	}

	private void OnTooltip(bool isOver)
	{
		if (isOver)
		{
			UIAbilityTooltip.GlobalShow(Icon, UIAbilityBar.GetSelectedForBars(), m_Phrase);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	private void OnRightClick()
	{
		UIItemInspectManager.Examine(Phrase, UIAbilityBar.GetSelectedForBars());
	}

	public void Refresh()
	{
		if ((bool)m_Phrase)
		{
			Icon.mainTexture = m_Phrase.Icon;
			BarForeground.transform.localScale = new Vector3(8f + m_Phrase.Recitation * (float)UIActionBarChanter.Instance.PixelsPerSecond, BarForeground.transform.localScale.y, 1f);
			BarForeground.transform.localPosition = new Vector3(BarForeground.transform.localPosition.x, Shift * 12, BarForeground.transform.localPosition.z);
			BarBackground.transform.localScale = new Vector3(m_Phrase.Duration * (float)UIActionBarChanter.Instance.PixelsPerSecond, BarBackground.transform.localScale.y, 1f);
			BarBackground.transform.localPosition = new Vector3(BarBackground.transform.localPosition.x, Shift * 12, BarBackground.transform.localPosition.z);
		}
	}
}

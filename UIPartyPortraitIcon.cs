using System;
using UnityEngine;

public class UIPartyPortraitIcon : MonoBehaviour
{
	public enum IconAction
	{
		LevelUp,
		WantToTalk,
		None
	}

	public IconAction action = IconAction.None;

	public GameObject Collider;

	public UIWidget TooltipBackground;

	private CharacterStats m_characterStats;

	private PartyMemberAI m_partyMemberAI;

	private NPCDialogue m_partyMemberDiag;

	public CharacterStats Stats
	{
		get
		{
			return m_characterStats;
		}
		set
		{
			m_characterStats = value;
		}
	}

	public PartyMemberAI AI
	{
		get
		{
			return m_partyMemberAI;
		}
		set
		{
			m_partyMemberAI = value;
		}
	}

	public NPCDialogue Dialogue
	{
		get
		{
			return m_partyMemberDiag;
		}
		set
		{
			m_partyMemberDiag = value;
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(TooltipBackground.gameObject);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		if (action == IconAction.LevelUp)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(Collider.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnLevelUpClick));
		}
		else if (action == IconAction.WantToTalk)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(Collider.gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnConversationFlagClick));
		}
	}

	private void OnLevelUpClick(GameObject sender)
	{
		if (!GameState.InCombat)
		{
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.LevelUp);
			int endingLevel = Math.Min(m_characterStats.GetMaxLevelCanLevelUpTo(), m_characterStats.Level + 1);
			HideTooltip();
			UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.LevelUp, m_partyMemberAI.gameObject, 0, endingLevel, m_characterStats.Experience);
		}
		else
		{
			Console.AddMessage(GUIUtils.GetText(1857));
		}
	}

	private void OnConversationFlagClick(GameObject sender)
	{
		HideTooltip();
		GameState.s_playerCharacter.GetComponent<PartyMemberAI>().ExclusiveSelect();
		if ((bool)m_partyMemberDiag)
		{
			GameState.s_playerCharacter.ObjectClicked(m_partyMemberDiag);
		}
	}

	private void OnChildTooltip(GameObject go, bool show)
	{
		OnTooltip(show);
	}

	private void OnDisable()
	{
		HideTooltip();
	}

	private void OnTooltip(bool isOver)
	{
		if (isOver)
		{
			ShowTooltip();
		}
		else
		{
			HideTooltip();
		}
	}

	private void HideTooltip()
	{
		UIPartyPortraitIconTooltip.GlobalHide();
	}

	private void ShowTooltip()
	{
		switch (action)
		{
		case IconAction.LevelUp:
			if (Stats != null)
			{
				UIPartyPortraitIconTooltip.GlobalShow(TooltipBackground, GUIUtils.Format(1315, Stats.Name()));
			}
			break;
		case IconAction.WantToTalk:
			if (Stats != null)
			{
				UIPartyPortraitIconTooltip.GlobalShow(TooltipBackground, GUIUtils.Format(1564, Stats.Name()));
			}
			break;
		case IconAction.None:
			break;
		}
	}
}

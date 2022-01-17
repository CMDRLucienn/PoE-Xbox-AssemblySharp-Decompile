using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPartyManager : UIHudWindow
{
	private static UIPartyManager s_Instance;

	public bool AddingNewPartyMember;

	public UILabel CharacterName;

	public UILabel CharacterClass;

	public UILabel CharacterLevel;

	public UILabel CharacterStatus;

	public UILabel Dash;

	public GameObject LineDivider;

	private GameObject m_SelectedCharacter;

	public UIPartyManagementParty Party;

	public UIPartyManagementRoster Roster;

	public UIMultiSpriteImageButton AcceptButton;

	public UIMultiSpriteImageButton CancelButton;

	private UIMessageBox m_CloseConfirmMb;

	private List<GameObject> m_PendingToBench = new List<GameObject>();

	private List<GameObject> m_PendingToParty = new List<GameObject>();

	private Stronghold m_Stronghold;

	private bool m_Accept;

	private bool m_Cancel;

	public static UIPartyManager Instance => s_Instance;

	public bool ChoicesAvailable => Stronghold.Instance.GetCompanions().Count > 0;

	public List<GameObject> PendingToBench => m_PendingToBench;

	public List<GameObject> PendingToParty => m_PendingToParty;

	public void SelectCharacter(GameObject character)
	{
		m_SelectedCharacter = character;
		LoadCharacterInfo(character);
		UpdateSelectedIcon();
	}

	public void DeselectCharacter(GameObject character)
	{
		if (character == m_SelectedCharacter)
		{
			m_SelectedCharacter = null;
			LoadCharacterInfo(null);
			UpdateSelectedIcon();
		}
	}

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		m_Stronghold = GameState.Stronghold;
		UIMultiSpriteImageButton acceptButton = AcceptButton;
		acceptButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(acceptButton.onClick, new UIEventListener.VoidDelegate(Accept));
		UIMultiSpriteImageButton cancelButton = CancelButton;
		cancelButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(cancelButton.onClick, new UIEventListener.VoidDelegate(Cancel));
	}

	protected override void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Accept(GameObject go)
	{
		m_Accept = true;
		HideWindow();
	}

	private void Cancel(GameObject go)
	{
		m_Cancel = true;
		HideWindow();
	}

	private void LoadCharacterInfo(GameObject character)
	{
		if (character == null)
		{
			CharacterName.gameObject.SetActive(value: false);
			CharacterClass.gameObject.SetActive(value: false);
			CharacterLevel.gameObject.SetActive(value: false);
			CharacterStatus.gameObject.SetActive(value: false);
			Dash.gameObject.SetActive(value: false);
			LineDivider.SetActive(value: false);
			return;
		}
		CharacterName.gameObject.SetActive(value: true);
		CharacterClass.gameObject.SetActive(value: true);
		CharacterLevel.gameObject.SetActive(value: true);
		CharacterStatus.gameObject.SetActive(value: true);
		Dash.gameObject.SetActive(value: true);
		LineDivider.SetActive(value: true);
		PartyMemberAI component = character.GetComponent<PartyMemberAI>();
		StoredCharacterInfo component2 = character.GetComponent<StoredCharacterInfo>();
		if ((bool)component)
		{
			CharacterStats component3 = character.GetComponent<CharacterStats>();
			CharacterName.text = CharacterStats.Name(character.gameObject);
			CharacterClass.text = GUIUtils.GetClassString(component3.CharacterClass, component3.Gender);
			CharacterLevel.text = GUIUtils.GetText(373, CharacterStats.GetGender(character)) + " " + component3.ScaledLevel;
		}
		else if ((bool)component2)
		{
			CharacterName.text = component2.DisplayName;
			CharacterClass.text = GUIUtils.GetClassString((CharacterStats.Class)component2.Class, component2.Gender);
			CharacterLevel.text = GUIUtils.GetText(373, CharacterStats.GetGender(character)) + " " + component2.Level;
		}
		if (PartyMemberAI.OnlyPrimaryPartyMembers.Contains(component) && component.IsActiveInParty)
		{
			CharacterStatus.text = GUIUtils.GetText(402, CharacterStats.GetGender(character));
		}
		else if ((bool)component2 && !GameState.Stronghold.IsAvailable(component2))
		{
			CharacterStatus.text = GUIUtils.GetText(709, CharacterStats.GetGender(character)) + ": " + GameState.Stronghold.WhyNotAvailableString(component2);
		}
		else
		{
			CharacterStatus.text = GUIUtils.GetText(401, CharacterStats.GetGender(character));
		}
	}

	protected override void Show()
	{
		if (!base.AlternateMode && (!GameState.Instance.CurrentMapIsStronghold || !Stronghold.Instance.Activated) && !AddingNewPartyMember)
		{
			HideWindow();
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.GetText((!GameState.Instance.CurrentMapIsStronghold) ? 1442 : 990));
			return;
		}
		if (GameState.InCombat)
		{
			HideWindow();
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.GetText(1887));
			return;
		}
		m_Accept = false;
		m_Cancel = false;
		Party.Reload();
		Roster.Reload();
		SelectCharacter(null);
	}

	protected override bool Hide(bool forced)
	{
		AddingNewPartyMember = false;
		if ((bool)m_CloseConfirmMb && !forced)
		{
			return false;
		}
		if (m_Accept)
		{
			foreach (GameObject item in m_PendingToBench)
			{
				if (!item.GetComponent<Player>())
				{
					m_Stronghold.StoreCompanion(item);
				}
			}
			foreach (GameObject item2 in m_PendingToParty)
			{
				StoredCharacterInfo component = item2.GetComponent<StoredCharacterInfo>();
				if ((bool)component)
				{
					int nextAvailablePrimarySlot = PartyMemberAI.NextAvailablePrimarySlot;
					if (nextAvailablePrimarySlot >= 0)
					{
						component.RestoreSlot = nextAvailablePrimarySlot;
						m_Stronghold.CompanionActivation(component.GUID, active: true);
					}
				}
			}
			m_PendingToBench.Clear();
			m_PendingToParty.Clear();
			return base.Hide(forced);
		}
		if (m_Cancel)
		{
			m_PendingToBench.Clear();
			m_PendingToParty.Clear();
			return base.Hide(forced);
		}
		if (!forced && (m_PendingToBench.Count > 0 || m_PendingToParty.Count > 0))
		{
			m_CloseConfirmMb = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, "", GUIUtils.GetText(213));
			UIMessageBox closeConfirmMb = m_CloseConfirmMb;
			closeConfirmMb.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(closeConfirmMb.OnDialogEnd, new UIMessageBox.OnEndDialog(OnConfirmEnd));
			return false;
		}
		m_PendingToBench.Clear();
		m_PendingToParty.Clear();
		return base.Hide(forced);
	}

	private void OnConfirmEnd(UIMessageBox.Result result, UIMessageBox owner)
	{
		m_CloseConfirmMb = null;
		switch (result)
		{
		case UIMessageBox.Result.AFFIRMATIVE:
			m_Accept = true;
			HideWindow();
			break;
		case UIMessageBox.Result.NEGATIVE:
			m_Cancel = true;
			HideWindow();
			break;
		default:
			m_Accept = false;
			m_Cancel = false;
			break;
		}
	}

	public void BenchCharacter(GameObject go)
	{
		bool flag = false;
		PartyMemberAI component = go.GetComponent<PartyMemberAI>();
		if (component != null && PartyMemberAI.PartyMembers.Contains(component))
		{
			m_PendingToBench.Add(go);
			flag = true;
		}
		if (m_PendingToParty.Remove(go))
		{
			flag = true;
		}
		if (flag)
		{
			SoundSetComponent component2 = go.GetComponent<SoundSetComponent>();
			if ((bool)component2 && (bool)component2.SoundSet)
			{
				component2.SoundSet.PlaySound(go, SoundSet.SoundAction.Goodbye, -1, skipIfConversing: false, ignoreListenerVolume: true);
			}
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ManagerDismissCharacter);
		}
	}

	public void PartyCharacter(GameObject go)
	{
		bool flag = false;
		PartyMemberAI component = go.GetComponent<PartyMemberAI>();
		if (component == null || !PartyMemberAI.PartyMembers.Contains(component))
		{
			m_PendingToParty.Add(go);
			flag = true;
		}
		if (m_PendingToBench.Remove(go))
		{
			flag = true;
		}
		if (flag)
		{
			SoundSetComponent component2 = go.GetComponent<SoundSetComponent>();
			if ((bool)component2 && (bool)component2.SoundSet)
			{
				component2.SoundSet.PlaySound(go, SoundSet.SoundAction.Hello, -1, skipIfConversing: false, ignoreListenerVolume: true);
			}
			GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.ManagerRecruitCharacter);
		}
	}

	private void UpdateSelectedIcon()
	{
		UIPartyManagementIcon[] componentsInChildren = GetComponentsInChildren<UIPartyManagementIcon>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Select(m_SelectedCharacter);
		}
	}

	public void Reload()
	{
		Party.Reload();
		Roster.Reload();
	}
}

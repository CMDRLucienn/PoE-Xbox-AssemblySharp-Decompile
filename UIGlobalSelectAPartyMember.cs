using System;
using System.Linq;
using UnityEngine;

public class UIGlobalSelectAPartyMember : MonoBehaviour, ISelectACharacter
{
	private static UIGlobalSelectAPartyMember s_Instance;

	private CharacterStats m_SelectedCharacter;

	public static UIGlobalSelectAPartyMember Instance => s_Instance ?? Initialize();

	public CharacterStats SelectedCharacter
	{
		get
		{
			if (!m_SelectedCharacter && (bool)GameState.s_playerCharacter)
			{
				SelectedCharacter = GameState.s_playerCharacter.GetComponent<CharacterStats>();
			}
			return m_SelectedCharacter;
		}
		set
		{
			if (m_SelectedCharacter != value)
			{
				m_SelectedCharacter = value;
				if (this.OnSelectedCharacterChanged != null)
				{
					this.OnSelectedCharacterChanged(m_SelectedCharacter);
				}
			}
		}
	}

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public static UIGlobalSelectAPartyMember Initialize()
	{
		if (!s_Instance)
		{
			s_Instance = UIWindowManager.Instance.gameObject.AddComponent<UIGlobalSelectAPartyMember>();
		}
		return s_Instance;
	}

	private void Awake()
	{
		PartyMemberAI.OnAnySelectionChanged += OnGameSelectionChanged;
		OnGameSelectionChanged(null, null);
	}

	private void OnDestroy()
	{
		PartyMemberAI.OnAnySelectionChanged -= OnGameSelectionChanged;
	}

	private void OnGameSelectionChanged(object sender, EventArgs e)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if ((bool)onlyPrimaryPartyMember && onlyPrimaryPartyMember.Selected)
			{
				SelectedCharacter = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
				return;
			}
		}
		PartyMemberAI partyMemberAI = PartyMemberAI.OnlyPrimaryPartyMembers.FirstOrDefault();
		if ((bool)partyMemberAI)
		{
			SelectedCharacter = partyMemberAI.GetComponent<CharacterStats>();
		}
	}
}

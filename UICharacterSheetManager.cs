using System;
using UnityEngine;

public class UICharacterSheetManager : UIHudWindow, ISelectACharacterMutable, ISelectACharacter
{
	private enum RecordsScreen
	{
		PARTY,
		PERSONAL
	}

	public UICharacterSheetContentManager Content;

	public UICharacterSheetRecordsGenerator Records;

	private UIRadioButtonGroup m_RecordTabGroup;

	public UIMultiSpriteImageButton RecordsParty;

	public UIMultiSpriteImageButton RecordsPersonal;

	public UIMultiSpriteImageButton LevelUp;

	public UILootPartySelector Selector;

	public GameObject PreviousButton;

	public GameObject NextButton;

	private PartyMemberAI m_SelectedPartyMember;

	private RecordsScreen m_CurrentRecordsScreen;

	public static UICharacterSheetManager Instance { get; private set; }

	public CharacterStats SelectedCharacter
	{
		get
		{
			return UIGlobalSelectAPartyMember.Instance.SelectedCharacter;
		}
		set
		{
			UIGlobalSelectAPartyMember.Instance.SelectedCharacter = value;
		}
	}

	public override int CyclePosition => 1;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Awake()
	{
		Instance = this;
		UIGlobalSelectAPartyMember.Instance.OnSelectedCharacterChanged += OnGlobalSelectionChanged;
		UIEventListener uIEventListener = UIEventListener.Get(NextButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnNext));
		UIEventListener uIEventListener2 = UIEventListener.Get(PreviousButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnPrevious));
		UIMultiSpriteImageButton recordsPersonal = RecordsPersonal;
		recordsPersonal.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(recordsPersonal.onClick, new UIEventListener.VoidDelegate(OnPersonalRecords));
		UIMultiSpriteImageButton recordsParty = RecordsParty;
		recordsParty.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(recordsParty.onClick, new UIEventListener.VoidDelegate(OnPartyRecords));
		if ((bool)LevelUp)
		{
			UIMultiSpriteImageButton levelUp = LevelUp;
			levelUp.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(levelUp.onClick, new UIEventListener.VoidDelegate(OnLevelUp));
		}
		m_RecordTabGroup = NGUITools.FindInParents<UIRadioButtonGroup>(RecordsParty.gameObject);
		Selector.OnSelectCharacter += OnSelectCharacter;
	}

	private void OnSelectCharacter(GameObject sender)
	{
		SelectCharacter(sender.GetComponentInParent<UILootPartyIcon>().PartyMember);
	}

	private void OnPersonalRecords(GameObject sender)
	{
		SelectRecordsTab(RecordsScreen.PERSONAL);
	}

	private void OnPartyRecords(GameObject sender)
	{
		SelectRecordsTab(RecordsScreen.PARTY);
	}

	private void OnLevelUp(GameObject sender)
	{
		CharacterStats selectedCharacter = SelectedCharacter;
		int endingLevel = Mathf.Min(selectedCharacter.Level + 1, selectedCharacter.GetMaxLevelCanLevelUpTo());
		UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.LevelUp, m_SelectedPartyMember.gameObject, 0, endingLevel, selectedCharacter.Experience);
	}

	private void OnNext(GameObject go)
	{
		SelectCharacter(PartyHelper.SeekNextPartyMember(m_SelectedPartyMember));
	}

	private void OnPrevious(GameObject go)
	{
		SelectCharacter(PartyHelper.SeekPreviousPartyMember(m_SelectedPartyMember));
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)UIGlobalSelectAPartyMember.Instance)
		{
			UIGlobalSelectAPartyMember.Instance.OnSelectedCharacterChanged -= OnGlobalSelectionChanged;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void SelectRecordsTab(RecordsScreen screen)
	{
		m_CurrentRecordsScreen = screen;
		Records.Reload(screen == RecordsScreen.PERSONAL);
		RecordsParty.ForceHighlight(screen == RecordsScreen.PARTY);
		RecordsPersonal.ForceHighlight(screen == RecordsScreen.PERSONAL);
	}

	public void RefreshCharacter()
	{
		if ((bool)SelectedCharacter && Window.gameObject.activeInHierarchy)
		{
			LoadCharacter(SelectedCharacter);
		}
	}

	public void SelectCharacter(int index)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (index == 0)
			{
				SelectedCharacter = onlyPrimaryPartyMember.GetComponent<CharacterStats>();
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				break;
			}
			index--;
		}
	}

	public void SelectCharacter(MonoBehaviour pai)
	{
		if ((bool)pai)
		{
			SelectedCharacter = pai.GetComponent<CharacterStats>();
		}
	}

	public void LoadCharacter(CharacterStats character)
	{
		if ((bool)character)
		{
			m_SelectedPartyMember = character.GetComponent<PartyMemberAI>();
			Content.LoadCharacter(character.gameObject);
			Records.Reload(m_CurrentRecordsScreen == RecordsScreen.PERSONAL);
			if ((bool)LevelUp)
			{
				LevelUp.enabled = (bool)character && character.Level < character.GetMaxLevelCanLevelUpTo();
			}
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
		}
	}

	private void OnGlobalSelectionChanged(CharacterStats character)
	{
		if (WindowActive())
		{
			LoadCharacter(character);
		}
	}

	public override void HandleInput()
	{
		base.HandleInput();
		SelectCharacter(GameInput.NumberPressed - 1);
		if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_TAB, handle: true))
		{
			if (GameInput.GetShiftkey())
			{
				SelectCharacter(PartyHelper.SeekPreviousPartyMember(m_SelectedPartyMember));
			}
			else
			{
				SelectCharacter(PartyHelper.SeekNextPartyMember(m_SelectedPartyMember));
			}
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if (GameInput.GetKeyUp(KeyCode.LeftArrow) || GameInput.GetKeyUp(KeyCode.UpArrow))
		{
			SelectCharacter(PartyHelper.SeekPreviousPartyMember(m_SelectedPartyMember));
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if (GameInput.GetKeyUp(KeyCode.RightArrow) || GameInput.GetKeyDown(KeyCode.DownArrow))
		{
			SelectCharacter(PartyHelper.SeekNextPartyMember(m_SelectedPartyMember));
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
	}

	protected override void Show()
	{
		Selector.LoadParty();
		RecordKeeper.Instance.FindCompanions();
		SelectRecordsTab(RecordsScreen.PARTY);
		if (m_RecordTabGroup != null)
		{
			m_RecordTabGroup.DoSelect(RecordsParty.gameObject);
		}
		RefreshCharacter();
	}

	protected override void Unsuspended()
	{
		SelectCharacter(m_SelectedPartyMember);
	}
}

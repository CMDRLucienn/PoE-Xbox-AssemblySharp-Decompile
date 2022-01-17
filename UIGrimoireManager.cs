using System;
using UnityEngine;

public class UIGrimoireManager : UIHudWindow, ISelectACharacter
{
	private static UIGrimoireManager m_Instance;

	[HideInInspector]
	public Grimoire LoadedGrimoire;

	public UITexture BookIcon;

	public UILabel BookLabel;

	public GameObject PreviousButton;

	public GameObject NextButton;

	public UIGrimoireInSpells SpellsInGrimoire;

	public UIGrimoireKnownSpells SpellsKnown;

	public UIGrimoireLevelButtons LevelButtons;

	public static UIGrimoireManager Instance => m_Instance;

	public CharacterStats SelectedCharacter { get; private set; }

	public bool CanEditGrimoire { get; private set; }

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public void Reload()
	{
		SpellsInGrimoire.Reload();
		SpellsKnown.Reload(LevelButtons.CurrentLevel);
	}

	private void Awake()
	{
		m_Instance = this;
		UIEventListener uIEventListener = UIEventListener.Get(NextButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnNext));
		UIEventListener uIEventListener2 = UIEventListener.Get(PreviousButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnPrevious));
	}

	private void Update()
	{
		if (WindowActive() || !GameInput.GetControlUp(MappedControl.EDIT_SPELLS, handle: false))
		{
			return;
		}
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		if ((bool)selectedForBars)
		{
			CharacterStats component = selectedForBars.GetComponent<CharacterStats>();
			if ((bool)component && component.CharacterClass == CharacterStats.Class.Wizard)
			{
				ShowWindow();
			}
		}
	}

	private void OnNext(GameObject go)
	{
		if ((bool)SelectedCharacter)
		{
			SelectCharacter(PartyHelper.SeekNextPartyMember(SelectedCharacter.GetComponent<PartyMemberAI>()));
		}
	}

	private void OnPrevious(GameObject go)
	{
		if ((bool)SelectedCharacter)
		{
			SelectCharacter(PartyHelper.SeekPreviousPartyMember(SelectedCharacter.GetComponent<PartyMemberAI>()));
		}
	}

	public override void HandleInput()
	{
		if (GameInput.GetControlUp(MappedControl.EDIT_SPELLS))
		{
			HideWindow();
		}
		if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_TAB, handle: true))
		{
			LevelButtons.IncLevel();
		}
		SelectCharacter(GameInput.NumberPressed - 1);
	}

	protected override void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		SelectedCharacter = null;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void CoolDownGrimoire()
	{
		if ((bool)SelectedCharacter)
		{
			SelectedCharacter.CoolDownGrimoire();
		}
	}

	protected override void Show()
	{
		if (!(LoadedGrimoire == null))
		{
			return;
		}
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		if (!selectedForBars)
		{
			GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
			foreach (GameObject gameObject in selectedPartyMembers)
			{
				if ((bool)gameObject)
				{
					Equipment component = gameObject.GetComponent<Equipment>();
					if ((bool)component && (bool)component.CurrentItems.Grimoire)
					{
						SelectCharacter(gameObject.GetComponent<PartyMemberAI>());
						LoadGrimoire(component.CurrentItems.Grimoire.GetComponent<Grimoire>(), canEdit: true);
						break;
					}
				}
			}
		}
		else
		{
			Equipment component2 = selectedForBars.GetComponent<Equipment>();
			if ((bool)component2 && (bool)component2.CurrentItems.Grimoire)
			{
				SelectCharacter(selectedForBars.GetComponent<PartyMemberAI>());
				LoadGrimoire(component2.CurrentItems.Grimoire.GetComponent<Grimoire>(), canEdit: true);
			}
		}
		if (LoadedGrimoire == null)
		{
			HideWindow();
		}
		else
		{
			SpellsKnown.Reload(LevelButtons.CurrentLevel);
		}
	}

	protected override bool Hide(bool forced)
	{
		LoadedGrimoire = null;
		UIGlobalInventory.Instance.HideMessage();
		return base.Hide(forced);
	}

	public void SelectCharacter(PartyMemberAI holder)
	{
		SelectedCharacter = (holder ? holder.GetComponent<CharacterStats>() : null);
		if (this.OnSelectedCharacterChanged != null)
		{
			this.OnSelectedCharacterChanged(SelectedCharacter);
		}
		SpellsInGrimoire.Reload();
		SpellsKnown.Reload(LevelButtons.CurrentLevel);
	}

	public void SelectCharacter(int index)
	{
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (index == 0)
			{
				SelectCharacter(onlyPrimaryPartyMember);
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				break;
			}
			index--;
		}
	}

	public void LoadGrimoire(Grimoire grim, bool canEdit)
	{
		LoadedGrimoire = grim;
		CanEditGrimoire = canEdit;
		Item component = grim.GetComponent<Item>();
		if ((bool)component)
		{
			BookLabel.text = component.Name;
			BookIcon.mainTexture = component.GetIconLargeTexture();
		}
		else
		{
			BookLabel.text = GUIUtils.GetText(409);
			BookIcon.mainTexture = null;
		}
		SpellsInGrimoire.Reload();
		SpellsKnown.Reload(LevelButtons.CurrentLevel);
	}
}

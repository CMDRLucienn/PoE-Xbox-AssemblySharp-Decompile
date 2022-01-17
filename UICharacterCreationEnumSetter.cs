using UnityEngine;

public class UICharacterCreationEnumSetter : UICharacterCreationElementSetter
{
	public enum EnumType
	{
		GENDER,
		CLASS,
		RACE,
		SUBRACE,
		CULTURE,
		BACKGROUND,
		BODY_TYPE,
		VOICE_TYPE,
		DEITY,
		RELIGION,
		ABILITY,
		TALENTS,
		ABILITY_MASTERY,
		COUNT
	}

	public static CharacterStats.Race[] ValidRaces;

	public static CharacterStats.Class[] ValidClasses;

	public static Gender[] ValidGenders;

	public static CharacterStats.Subrace[][] ValidSubracesByRace;

	public static CharacterStats.Culture[] ValidCultures;

	public static CharacterStats.Race[] ValidRacialBodyTypes;

	public static Religion.Deity[] ValidDeities;

	public static Religion.PaladinOrder[] ValidPaladinOrders;

	public static CharacterStats.Background[][] ValidBackgrounds;

	public static CharacterStats.AttributeScoreType[][] GodTierAttributes;

	public static CharacterStats.AttributeScoreType[][] ProTierAttributes;

	public EnumType SetType;

	public CharacterStats.Race Race;

	public CharacterStats.Subrace Subrace;

	public CharacterStats.Race RacialBodyType;

	public Gender Gender;

	public CharacterStats.Class Class;

	public CharacterStats.Culture Culture;

	public CharacterStats.Background Background;

	public SoundSet CharacterVoiceSet;

	public Religion.Deity Deity;

	public Religion.PaladinOrder PaladinOrder;

	public AbilityProgressionTable.UnlockableAbility UnlockableAbility;

	public GameObject AbilityObject;

	public static CharacterStats.Culture s_PendingCulture;

	private static int s_soundSetActionIndex;

	static UICharacterCreationEnumSetter()
	{
		ValidRaces = new CharacterStats.Race[6]
		{
			CharacterStats.Race.Human,
			CharacterStats.Race.Aumaua,
			CharacterStats.Race.Dwarf,
			CharacterStats.Race.Elf,
			CharacterStats.Race.Orlan,
			CharacterStats.Race.Godlike
		};
		ValidClasses = new CharacterStats.Class[11]
		{
			CharacterStats.Class.Barbarian,
			CharacterStats.Class.Chanter,
			CharacterStats.Class.Cipher,
			CharacterStats.Class.Druid,
			CharacterStats.Class.Fighter,
			CharacterStats.Class.Monk,
			CharacterStats.Class.Paladin,
			CharacterStats.Class.Priest,
			CharacterStats.Class.Ranger,
			CharacterStats.Class.Rogue,
			CharacterStats.Class.Wizard
		};
		ValidGenders = new Gender[2]
		{
			Gender.Male,
			Gender.Female
		};
		ValidCultures = new CharacterStats.Culture[7]
		{
			CharacterStats.Culture.Aedyr,
			CharacterStats.Culture.DeadfireArchipelago,
			CharacterStats.Culture.IxamitlPlains,
			CharacterStats.Culture.OldVailia,
			CharacterStats.Culture.Ruatai,
			CharacterStats.Culture.TheLivingLands,
			CharacterStats.Culture.TheWhiteThatWends
		};
		ValidRacialBodyTypes = new CharacterStats.Race[5]
		{
			CharacterStats.Race.Human,
			CharacterStats.Race.Aumaua,
			CharacterStats.Race.Dwarf,
			CharacterStats.Race.Elf,
			CharacterStats.Race.Orlan
		};
		ValidDeities = new Religion.Deity[5]
		{
			Religion.Deity.Berath,
			Religion.Deity.Eothas,
			Religion.Deity.Magran,
			Religion.Deity.Skaen,
			Religion.Deity.Wael
		};
		ValidPaladinOrders = new Religion.PaladinOrder[5]
		{
			Religion.PaladinOrder.BleakWalkers,
			Religion.PaladinOrder.DarcozziPaladini,
			Religion.PaladinOrder.GoldpactKnights,
			Religion.PaladinOrder.KindWayfarers,
			Religion.PaladinOrder.ShieldbearersOfStElcga
		};
		s_PendingCulture = CharacterStats.Culture.Undefined;
		s_soundSetActionIndex = 0;
		ValidSubracesByRace = new CharacterStats.Subrace[15][];
		ValidSubracesByRace[7] = new CharacterStats.Subrace[2]
		{
			CharacterStats.Subrace.Coastal_Aumaua,
			CharacterStats.Subrace.Island_Aumaua
		};
		ValidSubracesByRace[3] = new CharacterStats.Subrace[2]
		{
			CharacterStats.Subrace.Mountain_Dwarf,
			CharacterStats.Subrace.Boreal_Dwarf
		};
		ValidSubracesByRace[2] = new CharacterStats.Subrace[2]
		{
			CharacterStats.Subrace.Wood_Elf,
			CharacterStats.Subrace.Snow_Elf
		};
		ValidSubracesByRace[4] = new CharacterStats.Subrace[4]
		{
			CharacterStats.Subrace.Death_Godlike,
			CharacterStats.Subrace.Fire_Godlike,
			CharacterStats.Subrace.Moon_Godlike,
			CharacterStats.Subrace.Nature_Godlike
		};
		ValidSubracesByRace[1] = new CharacterStats.Subrace[3]
		{
			CharacterStats.Subrace.Meadow_Human,
			CharacterStats.Subrace.Ocean_Human,
			CharacterStats.Subrace.Savannah_Human
		};
		ValidSubracesByRace[5] = new CharacterStats.Subrace[2]
		{
			CharacterStats.Subrace.Hearth_Orlan,
			CharacterStats.Subrace.Wild_Orlan
		};
		ValidBackgrounds = new CharacterStats.Background[12][];
		ValidBackgrounds[1] = new CharacterStats.Background[10]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Colonist,
			CharacterStats.Background.Dissident,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Priest,
			CharacterStats.Background.Slave
		};
		ValidBackgrounds[2] = new CharacterStats.Background[9]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Explorer,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Raider,
			CharacterStats.Background.Slave
		};
		ValidBackgrounds[3] = new CharacterStats.Background[9]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Dissident,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Philosopher,
			CharacterStats.Background.Scholar
		};
		ValidBackgrounds[4] = new CharacterStats.Background[10]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Artist,
			CharacterStats.Background.Colonist,
			CharacterStats.Background.Dissident,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Slave
		};
		ValidBackgrounds[5] = new CharacterStats.Background[9]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Dissident,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Scholar,
			CharacterStats.Background.Slave
		};
		ValidBackgrounds[6] = new CharacterStats.Background[8]
		{
			CharacterStats.Background.Colonist,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Explorer,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Mercenary,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Scientist
		};
		ValidBackgrounds[7] = new CharacterStats.Background[7]
		{
			CharacterStats.Background.Aristocrat,
			CharacterStats.Background.Drifter,
			CharacterStats.Background.Explorer,
			CharacterStats.Background.Hunter,
			CharacterStats.Background.Laborer,
			CharacterStats.Background.Merchant,
			CharacterStats.Background.Mystic
		};
		GodTierAttributes = new CharacterStats.AttributeScoreType[54][];
		GodTierAttributes[5] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Constitution
		};
		GodTierAttributes[11] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Intellect,
			CharacterStats.AttributeScoreType.Constitution
		};
		GodTierAttributes[10] = new CharacterStats.AttributeScoreType[1] { CharacterStats.AttributeScoreType.Intellect };
		GodTierAttributes[7] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Intellect,
			CharacterStats.AttributeScoreType.Might
		};
		GodTierAttributes[1] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Constitution,
			CharacterStats.AttributeScoreType.Resolve
		};
		GodTierAttributes[8] = new CharacterStats.AttributeScoreType[1];
		GodTierAttributes[3] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Intellect,
			CharacterStats.AttributeScoreType.Might
		};
		GodTierAttributes[9] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Constitution,
			CharacterStats.AttributeScoreType.Dexterity
		};
		GodTierAttributes[6] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Perception
		};
		GodTierAttributes[2] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Perception
		};
		GodTierAttributes[4] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Intellect
		};
		ProTierAttributes = new CharacterStats.AttributeScoreType[54][];
		ProTierAttributes[5] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Dexterity,
			CharacterStats.AttributeScoreType.Intellect
		};
		ProTierAttributes[11] = new CharacterStats.AttributeScoreType[1];
		ProTierAttributes[10] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Dexterity
		};
		ProTierAttributes[7] = new CharacterStats.AttributeScoreType[1];
		ProTierAttributes[1] = new CharacterStats.AttributeScoreType[1] { CharacterStats.AttributeScoreType.Might };
		ProTierAttributes[8] = new CharacterStats.AttributeScoreType[3]
		{
			CharacterStats.AttributeScoreType.Might,
			CharacterStats.AttributeScoreType.Intellect,
			CharacterStats.AttributeScoreType.Constitution
		};
		ProTierAttributes[3] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Resolve,
			CharacterStats.AttributeScoreType.Dexterity
		};
		ProTierAttributes[9] = new CharacterStats.AttributeScoreType[1] { CharacterStats.AttributeScoreType.Might };
		ProTierAttributes[6] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Intellect,
			CharacterStats.AttributeScoreType.Dexterity
		};
		ProTierAttributes[2] = new CharacterStats.AttributeScoreType[2]
		{
			CharacterStats.AttributeScoreType.Dexterity,
			CharacterStats.AttributeScoreType.Intellect
		};
		ProTierAttributes[4] = new CharacterStats.AttributeScoreType[1] { CharacterStats.AttributeScoreType.Dexterity };
	}

	public void FreeData()
	{
		if (CharacterVoiceSet != null && base.Owner.Character.VoiceSet != CharacterVoiceSet)
		{
			GameUtilities.Destroy(CharacterVoiceSet);
		}
	}

	private void UpdateButtonHighlight()
	{
		bool overridePressed = false;
		UICharacterCreationManager.AbilitySelectionState abilitySelectionState = null;
		bool flag = false;
		switch (SetType)
		{
		case EnumType.GENDER:
			overridePressed = base.Owner.Character.Gender == Gender;
			break;
		case EnumType.CLASS:
			overridePressed = base.Owner.Character.Class == Class;
			break;
		case EnumType.RACE:
			overridePressed = base.Owner.Character.Race == Race;
			break;
		case EnumType.SUBRACE:
			overridePressed = base.Owner.Character.Subrace == Subrace;
			break;
		case EnumType.CULTURE:
			overridePressed = s_PendingCulture == Culture;
			break;
		case EnumType.BACKGROUND:
			overridePressed = base.Owner.Character.Background == Background;
			break;
		case EnumType.BODY_TYPE:
			overridePressed = base.Owner.Character.RacialBodyType == RacialBodyType;
			break;
		case EnumType.VOICE_TYPE:
			overridePressed = base.Owner.Character.VoiceSet != null && CharacterVoiceSet != null && base.Owner.Character.VoiceSet.DisplayName.StringID == CharacterVoiceSet.DisplayName.StringID;
			break;
		case EnumType.DEITY:
			overridePressed = base.Owner.Character.Deity == Deity;
			break;
		case EnumType.RELIGION:
			overridePressed = base.Owner.Character.PaladinOrder == PaladinOrder;
			break;
		case EnumType.ABILITY:
		case EnumType.TALENTS:
			overridePressed = ((SetType == EnumType.ABILITY) ? UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState() : UICharacterCreationManager.Instance.GetCurrentTalentSelectionState())?.SelectedAbilities.Contains(UnlockableAbility) ?? false;
			flag = true;
			break;
		case EnumType.ABILITY_MASTERY:
			abilitySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			overridePressed = abilitySelectionState != null && abilitySelectionState.SelectedAbilities.Find((AbilityProgressionTable.UnlockableAbility abl) => abl.Ability == UnlockableAbility.Ability) != null;
			flag = true;
			break;
		}
		UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
		if (!component)
		{
			return;
		}
		if (flag)
		{
			component.ForceDown(val: true);
			UITexture[] componentsInChildren = base.transform.parent.GetComponentsInChildren<UITexture>(includeInactive: true);
			foreach (UITexture uITexture in componentsInChildren)
			{
				if ((bool)uITexture && uITexture.name == "SelectionBorder")
				{
					uITexture.enabled = overridePressed;
				}
			}
			UISprite[] componentsInChildren2 = base.transform.parent.GetComponentsInChildren<UISprite>(includeInactive: true);
			foreach (UISprite uISprite in componentsInChildren2)
			{
				if ((bool)uISprite && uISprite.name == "ButtonBackground")
				{
					uISprite.enabled = overridePressed;
					break;
				}
			}
		}
		else
		{
			component.SetOverridePressed(state: false);
			component.SetOverridePressed(overridePressed);
		}
	}

	public override void SignalValueChanged(ValueType type)
	{
		UpdateButtonHighlight();
	}

	private void OnHover(bool isOver)
	{
	}

	public override void SetIfUndefined()
	{
		UICharacterCreationController owner = base.Owner;
		switch (SetType)
		{
		case EnumType.GENDER:
			if (owner.Character.Gender == Gender.Male)
			{
				owner.Character.Gender = Gender;
			}
			owner.SignalValueChanged(ValueType.Gender);
			break;
		case EnumType.CLASS:
			if (owner.Character.Class == CharacterStats.Class.Undefined)
			{
				owner.Character.Class = Class;
				UICharacterCreationManager.Instance.CalculateAbilitySelectionStates(owner.Character);
				UICharacterCreationManager.Instance.CalculateAutoGrantAbilities(owner.Character);
			}
			owner.SignalValueChanged(ValueType.Class);
			break;
		case EnumType.RACE:
			if (owner.Character.Race == CharacterStats.Race.Undefined)
			{
				owner.Character.Race = Race;
				owner.Character.Subrace = CharacterStats.Subrace.Undefined;
			}
			owner.SignalValueChanged(ValueType.Race);
			break;
		case EnumType.SUBRACE:
			if (owner.Character.Subrace == CharacterStats.Subrace.Undefined)
			{
				owner.Character.Subrace = UICharacterCreationManager.Instance.GetLastPickedSubraceForRace(owner.Character.Race);
			}
			owner.SignalValueChanged(ValueType.Subrace);
			break;
		case EnumType.CULTURE:
			if (s_PendingCulture == CharacterStats.Culture.Undefined)
			{
				s_PendingCulture = Culture;
				owner.Character.Culture = s_PendingCulture;
				owner.Character.Background = CharacterStats.Background.Undefined;
			}
			owner.SignalValueChanged(ValueType.Culture);
			break;
		case EnumType.BACKGROUND:
			if (owner.Character.Background == CharacterStats.Background.Undefined)
			{
				owner.Character.Background = ValidBackgrounds[(int)owner.Character.Culture][0];
			}
			owner.SignalValueChanged(ValueType.Background);
			break;
		case EnumType.BODY_TYPE:
			if (owner.Character.RacialBodyType == CharacterStats.Race.Undefined)
			{
				owner.Character.RacialBodyType = RacialBodyType;
			}
			owner.SignalValueChanged(ValueType.BodyType);
			break;
		case EnumType.VOICE_TYPE:
			if (owner.Character.VoiceSet == null)
			{
				owner.Character.VoiceSet = CharacterVoiceSet;
			}
			owner.SignalValueChanged(ValueType.Voice);
			break;
		case EnumType.DEITY:
			if (owner.Character.Deity == Religion.Deity.None)
			{
				owner.Character.Deity = Deity;
			}
			owner.SignalValueChanged(ValueType.Deity);
			break;
		case EnumType.RELIGION:
			if (owner.Character.PaladinOrder == Religion.PaladinOrder.None)
			{
				owner.Character.PaladinOrder = PaladinOrder;
			}
			owner.SignalValueChanged(ValueType.Religion);
			break;
		case EnumType.ABILITY:
			owner.SignalValueChanged(ValueType.Ability);
			break;
		case EnumType.TALENTS:
			owner.SignalValueChanged(ValueType.Talent);
			break;
		case EnumType.ABILITY_MASTERY:
			owner.SignalValueChanged(ValueType.Ability);
			break;
		}
	}

	private void DeselectTalent()
	{
		UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
		if (currentTalentSelectionState == null)
		{
			return;
		}
		HandleSkillPointAdjustment(AbilityProgressionTable.GetGenericTalent(UnlockableAbility.Ability), add: false);
		currentTalentSelectionState.SelectedAbilities.Remove(UnlockableAbility);
		UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
		if ((bool)component)
		{
			component.ForceDown(val: false);
		}
		UITexture[] componentsInChildren = base.transform.parent.GetComponentsInChildren<UITexture>(includeInactive: true);
		foreach (UITexture uITexture in componentsInChildren)
		{
			if ((bool)uITexture && uITexture.name == "SelectionBorder")
			{
				uITexture.enabled = false;
				break;
			}
		}
		UISprite[] componentsInChildren2 = base.transform.parent.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren2)
		{
			if ((bool)uISprite && uISprite.name == "ButtonBackground")
			{
				uISprite.enabled = false;
				break;
			}
		}
	}

	private void DeselectAllSelectedTalents()
	{
		UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
		if (currentTalentSelectionState == null)
		{
			return;
		}
		UICharacterCreationPopulateEnumSetters[] array = Object.FindObjectsOfType<UICharacterCreationPopulateEnumSetters>();
		foreach (UICharacterCreationPopulateEnumSetters uICharacterCreationPopulateEnumSetters in array)
		{
			if (uICharacterCreationPopulateEnumSetters.Enum != EnumType.TALENTS)
			{
				continue;
			}
			UICharacterCreationEnumSetter[] componentsInChildren = uICharacterCreationPopulateEnumSetters.GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
			foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter in componentsInChildren)
			{
				if (uICharacterCreationEnumSetter != this && uICharacterCreationEnumSetter.SetType == EnumType.TALENTS && currentTalentSelectionState.SelectedAbilities.Contains(uICharacterCreationEnumSetter.UnlockableAbility))
				{
					uICharacterCreationEnumSetter.DeselectTalent();
				}
			}
		}
	}

	private void DeselectAbility()
	{
		if (SetType == EnumType.ABILITY)
		{
			UICharacterCreationManager.AbilitySelectionState currentAbilitySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentAbilitySelectionState == null)
			{
				return;
			}
			currentAbilitySelectionState.SelectedAbilities.Remove(UnlockableAbility);
		}
		else if (SetType == EnumType.ABILITY_MASTERY)
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (currentSpellMasterySelectionState == null)
			{
				return;
			}
			currentSpellMasterySelectionState.SelectedAbilities.RemoveAll((AbilityProgressionTable.UnlockableAbility abl) => UnlockableAbility.Ability == abl.Ability);
		}
		UIImageButtonRevised component = GetComponent<UIImageButtonRevised>();
		if ((bool)component)
		{
			component.ForceDown(val: false);
		}
		UITexture[] componentsInChildren = base.transform.parent.GetComponentsInChildren<UITexture>(includeInactive: true);
		foreach (UITexture uITexture in componentsInChildren)
		{
			if ((bool)uITexture && uITexture.name == "SelectionBorder")
			{
				uITexture.enabled = false;
			}
		}
		UISprite[] componentsInChildren2 = base.transform.parent.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite uISprite in componentsInChildren2)
		{
			if ((bool)uISprite && uISprite.name == "ButtonBackground")
			{
				uISprite.enabled = false;
				break;
			}
		}
	}

	private void DeselectAllSelectedAbilities()
	{
		UICharacterCreationManager.AbilitySelectionState abilitySelectionState = null;
		if (SetType == EnumType.ABILITY)
		{
			abilitySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (abilitySelectionState == null)
			{
				return;
			}
		}
		else if (SetType == EnumType.ABILITY_MASTERY)
		{
			abilitySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (abilitySelectionState == null)
			{
				return;
			}
		}
		UICharacterCreationPopulateEnumSetters[] array = Object.FindObjectsOfType<UICharacterCreationPopulateEnumSetters>();
		foreach (UICharacterCreationPopulateEnumSetters uICharacterCreationPopulateEnumSetters in array)
		{
			if (uICharacterCreationPopulateEnumSetters.Enum != EnumType.ABILITY && uICharacterCreationPopulateEnumSetters.Enum != EnumType.ABILITY_MASTERY)
			{
				continue;
			}
			UICharacterCreationEnumSetter[] componentsInChildren = uICharacterCreationPopulateEnumSetters.GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
			foreach (UICharacterCreationEnumSetter enumSetter in componentsInChildren)
			{
				if (enumSetter != this && ((enumSetter.SetType == EnumType.ABILITY && abilitySelectionState.SelectedAbilities.Contains(enumSetter.UnlockableAbility)) || (enumSetter.SetType == EnumType.ABILITY_MASTERY && abilitySelectionState.SelectedAbilities.Find((AbilityProgressionTable.UnlockableAbility abl) => abl.Ability == enumSetter.UnlockableAbility.Ability) != null)))
				{
					enumSetter.DeselectAbility();
				}
			}
		}
	}

	private void HandleSkillPointAdjustment(GenericTalent talent, bool add)
	{
		UICharacterCreationManager.Character activeCharacter = UICharacterCreationManager.Instance.ActiveCharacter;
		for (int i = 0; i < 6; i++)
		{
			int skillAdjustment = talent.GetSkillAdjustment((CharacterStats.SkillType)i);
			activeCharacter.SkillValueDeltas[i] += (add ? skillAdjustment : (-skillAdjustment));
		}
	}

	public override void Set()
	{
		UICharacterCreationController owner = base.Owner;
		switch (SetType)
		{
		case EnumType.GENDER:
			owner.Character.Gender = Gender;
			owner.Character.SetDefaultAppearance();
			owner.SignalValueChanged(ValueType.Gender);
			break;
		case EnumType.CLASS:
			if (owner.Character.Class != Class)
			{
				owner.Character.PaladinOrder = Religion.PaladinOrder.None;
				owner.Character.Deity = Religion.Deity.None;
				owner.Character.Class = Class;
				UICharacterCreationManager.Instance.CalculateAbilitySelectionStates(owner.Character);
				UICharacterCreationManager.Instance.CalculateAutoGrantAbilities(owner.Character);
				owner.SignalValueChanged(ValueType.Class);
			}
			owner.Character.Class = Class;
			break;
		case EnumType.RACE:
			if (owner.Character.Race != Race)
			{
				owner.Character.Subrace = CharacterStats.Subrace.Undefined;
			}
			owner.Character.Race = Race;
			if (Race != CharacterStats.Race.Godlike)
			{
				owner.Character.RacialBodyType = Race;
			}
			else
			{
				owner.Character.RacialBodyType = CharacterStats.Race.Undefined;
				if (!CharacterStats.SubraceIsGodlike(owner.Character.Subrace))
				{
					owner.Character.Subrace = UICharacterCreationManager.Instance.GetLastPickedSubraceForRace(Race);
				}
			}
			owner.Character.SetDefaultAppearance(UICharacterCreationManager.Instance.GetLastPickedSubraceForRace(Race), owner.Character.Gender);
			owner.SignalValueChanged(ValueType.Race);
			break;
		case EnumType.SUBRACE:
			owner.Character.Subrace = Subrace;
			UICharacterCreationManager.Instance.SetLastPickedSubraceForRace(owner.Character.Race, Subrace);
			owner.Character.SetDefaultAppearance();
			owner.SignalValueChanged(ValueType.Subrace);
			break;
		case EnumType.CULTURE:
			s_PendingCulture = Culture;
			owner.Character.Culture = Culture;
			owner.Character.Background = CharacterStats.Background.Undefined;
			owner.SignalValueChanged(ValueType.Culture);
			break;
		case EnumType.BACKGROUND:
			owner.Character.Background = Background;
			owner.SignalValueChanged(ValueType.Background);
			break;
		case EnumType.BODY_TYPE:
			owner.Character.RacialBodyType = RacialBodyType;
			owner.SignalValueChanged(ValueType.BodyType);
			break;
		case EnumType.VOICE_TYPE:
		{
			owner.Character.VoiceSet = CharacterVoiceSet;
			SoundSet.SoundAction[] array = new SoundSet.SoundAction[4]
			{
				SoundSet.SoundAction.Selected,
				SoundSet.SoundAction.Leading,
				SoundSet.SoundAction.Attack,
				SoundSet.SoundAction.Scouting
			};
			CharacterVoiceSet.PlaySound(UICharacterCreationManager.Instance.TargetCharacter, array[s_soundSetActionIndex], -1, skipIfConversing: false, ignoreListenerVolume: true);
			s_soundSetActionIndex++;
			if (s_soundSetActionIndex >= array.Length)
			{
				s_soundSetActionIndex = 0;
			}
			owner.SignalValueChanged(ValueType.Voice);
			break;
		}
		case EnumType.ABILITY:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentSpellMasterySelectionState == null)
			{
				break;
			}
			currentSpellMasterySelectionState.LastSelectedAbility = UnlockableAbility;
			if (currentSpellMasterySelectionState.SelectedAbilities.Contains(UnlockableAbility))
			{
				DeselectAbility();
				currentSpellMasterySelectionState.LastSelectedAbility = null;
			}
			else if (currentSpellMasterySelectionState.SelectedAbilities.Count < currentSpellMasterySelectionState.Points || currentSpellMasterySelectionState.Points == 1)
			{
				if (currentSpellMasterySelectionState.Points == 1)
				{
					DeselectAllSelectedAbilities();
				}
				currentSpellMasterySelectionState.SelectedAbilities.Add(UnlockableAbility);
			}
			else
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.Error);
			}
			owner.SignalValueChanged(ValueType.Ability);
			break;
		}
		case EnumType.TALENTS:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentSpellMasterySelectionState == null)
			{
				break;
			}
			if (currentSpellMasterySelectionState.SelectedAbilities.Contains(UnlockableAbility))
			{
				DeselectTalent();
				owner.SignalValueChanged(ValueType.Talent);
			}
			else if (currentSpellMasterySelectionState.SelectedAbilities.Count < currentSpellMasterySelectionState.Points || currentSpellMasterySelectionState.Points == 1)
			{
				if (currentSpellMasterySelectionState.Points == 1)
				{
					DeselectAllSelectedTalents();
				}
				currentSpellMasterySelectionState.SelectedAbilities.Add(UnlockableAbility);
				HandleSkillPointAdjustment(AbilityProgressionTable.GetGenericTalent(UnlockableAbility.Ability), add: true);
			}
			else
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.Error);
			}
			owner.SignalValueChanged(ValueType.Talent);
			break;
		}
		case EnumType.ABILITY_MASTERY:
		{
			UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
			if (currentSpellMasterySelectionState == null)
			{
				break;
			}
			currentSpellMasterySelectionState.LastSelectedAbility = UnlockableAbility;
			if (currentSpellMasterySelectionState.SelectedAbilities.Contains(UnlockableAbility))
			{
				DeselectAbility();
				currentSpellMasterySelectionState.LastSelectedAbility = null;
			}
			else if (currentSpellMasterySelectionState.SelectedAbilities.Count < currentSpellMasterySelectionState.Points || currentSpellMasterySelectionState.Points == 1)
			{
				if (currentSpellMasterySelectionState.Points == 1)
				{
					DeselectAllSelectedAbilities();
				}
				currentSpellMasterySelectionState.SelectedAbilities.Add(UnlockableAbility);
			}
			else
			{
				GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.Error);
			}
			owner.SignalValueChanged(ValueType.Ability);
			break;
		}
		case EnumType.DEITY:
			owner.Character.Deity = Deity;
			owner.SignalValueChanged(ValueType.Deity);
			break;
		case EnumType.RELIGION:
			owner.Character.PaladinOrder = PaladinOrder;
			owner.SignalValueChanged(ValueType.Religion);
			break;
		}
	}
}

using UnityEngine;

public class UICharacterCreationController : MonoBehaviour
{
	public enum ControllerType
	{
		None,
		ROOT,
		SEX,
		RACE,
		RACE_SUBRACE,
		CLASS,
		CULTURE,
		APPEARANCE,
		APPEARANCE_PORTRAIT,
		VOICE,
		ATTRIBUTES,
		BACKGROUND,
		BODY_TYPE,
		NAME,
		DEITY,
		RELIGION,
		ABILITIES,
		TALENTS,
		SKILLS,
		ANIMAL_NAME,
		SPELL_MASTERY,
		Count
	}

	public bool AlwaysVisible;

	public int EndsStage = -1;

	public ControllerType Type;

	public UICharacterCreationController SucceededBy;

	public GUIDatabaseString ControllerSubtitle;

	public GameObject[] AlsoShow;

	public GameObject[] AlsoHide;

	public UIPanel DescriptionScreen;

	public bool ShowAlternatePaperDollModel;

	public UICharacterCreationManager.Character Character => UICharacterCreationManager.Instance.ActiveCharacter;

	private static void DebugLog(object message)
	{
		Debug.Log(message);
	}

	public void HandleSkip()
	{
		if (Type == ControllerType.ABILITIES)
		{
			SignalValueChanged(UICharacterCreationElement.ValueType.Ability);
		}
		else if (Type == ControllerType.TALENTS)
		{
			SignalValueChanged(UICharacterCreationElement.ValueType.Talent);
		}
		else if (Type == ControllerType.APPEARANCE)
		{
			if (UICharacterCreationAppearanceSetter.s_pendingPortraitIndex > 0)
			{
				Character.PortraitIndex = UICharacterCreationAppearanceSetter.s_pendingPortraitIndex;
			}
		}
		else if (Type == ControllerType.SPELL_MASTERY)
		{
			SignalValueChanged(UICharacterCreationElement.ValueType.Ability);
		}
	}

	public bool ShouldSkip()
	{
		if (Type == ControllerType.BODY_TYPE && Character.Race != CharacterStats.Race.Godlike)
		{
			return true;
		}
		if (Type == ControllerType.ABILITIES && UICharacterCreationManager.Instance.AbilitySelectionStates.Count == 0)
		{
			return true;
		}
		if (Type == ControllerType.TALENTS && (UICharacterCreationManager.Instance.TalentSelectionStates.Count == 0 || Character.CoreData.Level == UICharacterCreationManager.Instance.EndingLevel))
		{
			return true;
		}
		if (Type == ControllerType.ANIMAL_NAME && (Character.Class != CharacterStats.Class.Ranger || (UICharacterCreationManager.Instance.CreationType == UICharacterCreationManager.CharacterCreationType.LevelUp && UICharacterCreationManager.Instance.ActiveCharacter.StartingLevel > 0)))
		{
			return true;
		}
		if (Type == ControllerType.DEITY && Character.Class != CharacterStats.Class.Priest)
		{
			return true;
		}
		if (Type == ControllerType.RELIGION && Character.Class != CharacterStats.Class.Paladin)
		{
			return true;
		}
		if (Type == ControllerType.SKILLS && (Character.CoreData.Level == 0 || Character.CoreData.Level == UICharacterCreationManager.Instance.EndingLevel))
		{
			return true;
		}
		if (Type == ControllerType.ATTRIBUTES && UICharacterCreationManager.Instance.CreationType == UICharacterCreationManager.CharacterCreationType.LevelUp && (Character.CoreData.Level != 0 || (!Character.CoreData.IsPlayerCharacter && !Character.CoreData.IsHiredAdventurer)))
		{
			return true;
		}
		if (Type == ControllerType.SPELL_MASTERY && UICharacterCreationManager.Instance.SpellMasterySelectionStates.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool ShouldAdvanceInternal()
	{
		switch (Type)
		{
		case ControllerType.ABILITIES:
			if (UICharacterCreationManager.Instance.AbilitySelectionStateIndex + 1 < UICharacterCreationManager.Instance.AbilitySelectionStates.Count)
			{
				return true;
			}
			break;
		case ControllerType.TALENTS:
			if (UICharacterCreationManager.Instance.TalentSelectionStateIndex + 1 < UICharacterCreationManager.Instance.TalentSelectionStates.Count)
			{
				return true;
			}
			break;
		case ControllerType.SPELL_MASTERY:
			if (UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex + 1 < UICharacterCreationManager.Instance.SpellMasterySelectionStates.Count)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public string GetSubTitle()
	{
		switch (Type)
		{
		case ControllerType.ABILITIES:
		{
			UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentTalentSelectionState != null)
			{
				return currentTalentSelectionState.CategoryName;
			}
			break;
		}
		case ControllerType.TALENTS:
		{
			UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentTalentSelectionState != null)
			{
				return GUIUtils.GetText(808);
			}
			break;
		}
		}
		return ControllerSubtitle.GetText();
	}

	public void AdvanceInternal()
	{
		switch (Type)
		{
		case ControllerType.ABILITIES:
			UICharacterCreationManager.Instance.AbilitySelectionStateIndex++;
			Populate();
			break;
		case ControllerType.TALENTS:
			UICharacterCreationManager.Instance.TalentSelectionStateIndex++;
			Populate();
			break;
		case ControllerType.SPELL_MASTERY:
			UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex++;
			Populate();
			break;
		case ControllerType.SKILLS:
		case ControllerType.ANIMAL_NAME:
			break;
		}
	}

	public bool ReadyForNextStage()
	{
		switch (Type)
		{
		case ControllerType.SEX:
			return true;
		case ControllerType.CLASS:
			return Character.Class != CharacterStats.Class.Undefined;
		case ControllerType.RACE:
			return Character.Race != CharacterStats.Race.Undefined;
		case ControllerType.RACE_SUBRACE:
			return Character.Subrace != CharacterStats.Subrace.Undefined;
		case ControllerType.CULTURE:
			return UICharacterCreationEnumSetter.s_PendingCulture != CharacterStats.Culture.Undefined;
		case ControllerType.BACKGROUND:
			return Character.Background != CharacterStats.Background.Undefined;
		case ControllerType.BODY_TYPE:
			return Character.RacialBodyType != CharacterStats.Race.Undefined;
		case ControllerType.NAME:
			return !string.IsNullOrEmpty(Character.Name);
		case ControllerType.ANIMAL_NAME:
			if (string.IsNullOrEmpty(Character.Animal_Name) && Character.Class == CharacterStats.Class.Ranger)
			{
				return Character.StartingLevel > 0;
			}
			return true;
		case ControllerType.VOICE:
			return Character.VoiceSet != null;
		case ControllerType.DEITY:
			if (Character.Deity == Religion.Deity.None)
			{
				return Character.Class != CharacterStats.Class.Priest;
			}
			return true;
		case ControllerType.RELIGION:
			if (Character.PaladinOrder == Religion.PaladinOrder.None)
			{
				return Character.Class != CharacterStats.Class.Paladin;
			}
			return true;
		case ControllerType.ABILITIES:
			if (UICharacterCreationManager.Instance.AbilitySelectionStates.Count > 0 && UICharacterCreationManager.Instance.AbilitySelectionStateIndex >= 0)
			{
				UICharacterCreationManager.AbilitySelectionState currentAbilitySelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
				return currentAbilitySelectionState.Points - currentAbilitySelectionState.SelectedAbilities.Count == 0;
			}
			return true;
		case ControllerType.TALENTS:
			if (UICharacterCreationManager.Instance.TalentSelectionStates.Count > 0 && UICharacterCreationManager.Instance.TalentSelectionStateIndex >= 0)
			{
				UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
				return currentTalentSelectionState.Points - currentTalentSelectionState.SelectedAbilities.Count == 0;
			}
			return true;
		case ControllerType.SPELL_MASTERY:
			if (UICharacterCreationManager.Instance.SpellMasterySelectionStates.Count > 0 && UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex >= 0)
			{
				UICharacterCreationManager.AbilitySelectionState currentSpellMasterySelectionState = UICharacterCreationManager.Instance.GetCurrentSpellMasterySelectionState();
				return currentSpellMasterySelectionState.Points - currentSpellMasterySelectionState.SelectedAbilities.Count == 0;
			}
			return true;
		case ControllerType.SKILLS:
			return true;
		default:
			return true;
		}
	}

	public void OnNextButtonPressed()
	{
		switch (Type)
		{
		case ControllerType.APPEARANCE:
			if (UICharacterCreationAppearanceSetter.s_pendingPortraitIndex > 0)
			{
				UICharacterCreationManager.Instance.ActiveCharacter.PortraitIndex = UICharacterCreationAppearanceSetter.s_pendingPortraitIndex;
			}
			break;
		case ControllerType.CULTURE:
			if (UICharacterCreationEnumSetter.s_PendingCulture != 0)
			{
				UICharacterCreationManager.Instance.ActiveCharacter.Culture = UICharacterCreationEnumSetter.s_PendingCulture;
			}
			break;
		}
	}

	private void HandlePaperDoll()
	{
		if (UICharacterCreationManager.Instance.GetCurrentController() == this)
		{
			GameObject s_ModelOverride = PE_Paperdoll.s_ModelOverride;
			if (ShowAlternatePaperDollModel)
			{
				UICharacterCreationManager.Instance.SetAlternatePaperDollModel();
			}
			else
			{
				PE_Paperdoll.s_ModelOverride = null;
			}
			if (s_ModelOverride != PE_Paperdoll.s_ModelOverride)
			{
				UICharacterCreationManager.Instance.SignalValueChanged(UICharacterCreationElement.ValueType.BodyType);
			}
		}
	}

	public void SignalValueChanged(UICharacterCreationElement.ValueType valueType)
	{
		HandlePaperDoll();
		UICharacterCreationManager.Instance.SignalValueChanged(valueType);
	}

	public void Show()
	{
		Populate();
		switch (Type)
		{
		case ControllerType.ABILITIES:
			SignalValueChanged(UICharacterCreationElement.ValueType.Ability);
			break;
		case ControllerType.TALENTS:
			SignalValueChanged(UICharacterCreationElement.ValueType.Talent);
			break;
		case ControllerType.SKILLS:
			SignalValueChanged(UICharacterCreationElement.ValueType.Skill);
			break;
		case ControllerType.SPELL_MASTERY:
			SignalValueChanged(UICharacterCreationElement.ValueType.Ability);
			break;
		case ControllerType.ANIMAL_NAME:
			break;
		}
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
		GameObject[] alsoShow = AlsoShow;
		foreach (GameObject gameObject in alsoShow)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: true);
				if ((bool)gameObject.GetComponent<UIPanel>())
				{
					gameObject.GetComponent<UIPanel>().Refresh();
				}
			}
		}
		alsoShow = AlsoHide;
		foreach (GameObject gameObject2 in alsoShow)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: false);
			}
		}
		if (DescriptionScreen != null)
		{
			UICharacterCreationStringGetter[] componentsInChildren = DescriptionScreen.GetComponentsInChildren<UICharacterCreationStringGetter>();
			foreach (UICharacterCreationStringGetter uICharacterCreationStringGetter in componentsInChildren)
			{
				if (uICharacterCreationStringGetter.IsDescriptionText2())
				{
					uICharacterCreationStringGetter.DataSource = ((Type == ControllerType.RACE) ? UICharacterCreationStringGetter.StringDataSource.RACE_DESC2 : ((Type == ControllerType.CLASS) ? UICharacterCreationStringGetter.StringDataSource.CLASS_DESC2 : ((Type == ControllerType.CULTURE) ? UICharacterCreationStringGetter.StringDataSource.CULTURE_DESC2 : ((Type == ControllerType.BACKGROUND) ? UICharacterCreationStringGetter.StringDataSource.BACKGROUND_DESC2 : ((Type == ControllerType.ABILITIES) ? UICharacterCreationStringGetter.StringDataSource.ABILITY_DESC2 : ((Type == ControllerType.TALENTS) ? UICharacterCreationStringGetter.StringDataSource.TALENT_DESC2 : ((Type == ControllerType.ATTRIBUTES) ? UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC2 : ((Type == ControllerType.SKILLS) ? UICharacterCreationStringGetter.StringDataSource.SKILL_DESC2 : ((Type == ControllerType.SPELL_MASTERY) ? UICharacterCreationStringGetter.StringDataSource.SPELL_MASTERY_DESC2 : UICharacterCreationStringGetter.StringDataSource.DESC2_NONE)))))))));
				}
				else if (uICharacterCreationStringGetter.IsDescriptionText())
				{
					uICharacterCreationStringGetter.DataSource = ((Type == ControllerType.SEX) ? UICharacterCreationStringGetter.StringDataSource.SEX_DESC : ((Type == ControllerType.RACE) ? UICharacterCreationStringGetter.StringDataSource.RACE_DESC : ((Type == ControllerType.RACE_SUBRACE) ? UICharacterCreationStringGetter.StringDataSource.SUBRACE_DESC : ((Type == ControllerType.CLASS) ? UICharacterCreationStringGetter.StringDataSource.CLASS_DESC : ((Type == ControllerType.CULTURE) ? UICharacterCreationStringGetter.StringDataSource.CULTURE_DESC : ((Type == ControllerType.BACKGROUND) ? UICharacterCreationStringGetter.StringDataSource.BACKGROUND_DESC : ((Type == ControllerType.ABILITIES) ? UICharacterCreationStringGetter.StringDataSource.ABILITY_DESC : ((Type == ControllerType.TALENTS) ? UICharacterCreationStringGetter.StringDataSource.TALENT_DESC : ((Type == ControllerType.ATTRIBUTES) ? UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC : ((Type == ControllerType.DEITY) ? UICharacterCreationStringGetter.StringDataSource.DEITY_DESC : ((Type == ControllerType.RELIGION) ? UICharacterCreationStringGetter.StringDataSource.PALADIN_ORDER_DESC : ((Type == ControllerType.SKILLS) ? UICharacterCreationStringGetter.StringDataSource.SKILL_DESC : ((Type == ControllerType.SPELL_MASTERY) ? UICharacterCreationStringGetter.StringDataSource.SPELL_MASTERY_DESC : uICharacterCreationStringGetter.DataSource)))))))))))));
				}
				else
				{
					uICharacterCreationStringGetter.DataSource = ((Type != ControllerType.SEX) ? ((Type == ControllerType.RACE) ? UICharacterCreationStringGetter.StringDataSource.RACE : ((Type == ControllerType.RACE_SUBRACE) ? UICharacterCreationStringGetter.StringDataSource.SUBRACE : ((Type == ControllerType.CLASS) ? UICharacterCreationStringGetter.StringDataSource.CLASS : ((Type == ControllerType.CULTURE) ? UICharacterCreationStringGetter.StringDataSource.CULTURE : ((Type == ControllerType.BACKGROUND) ? UICharacterCreationStringGetter.StringDataSource.BACKGROUND : ((Type == ControllerType.BODY_TYPE) ? UICharacterCreationStringGetter.StringDataSource.RACE : ((Type == ControllerType.ABILITIES) ? UICharacterCreationStringGetter.StringDataSource.ABILITY : ((Type == ControllerType.TALENTS) ? UICharacterCreationStringGetter.StringDataSource.TALENT : ((Type == ControllerType.ATTRIBUTES) ? UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE : ((Type == ControllerType.DEITY) ? UICharacterCreationStringGetter.StringDataSource.DEITY : ((Type == ControllerType.RELIGION) ? UICharacterCreationStringGetter.StringDataSource.PALADIN_ORDER : ((Type == ControllerType.SPELL_MASTERY) ? UICharacterCreationStringGetter.StringDataSource.SPELL_MASTERY : ((Type == ControllerType.SKILLS) ? UICharacterCreationStringGetter.StringDataSource.SKILL : uICharacterCreationStringGetter.DataSource))))))))))))) : UICharacterCreationStringGetter.StringDataSource.SEX);
				}
				SetInitialStringGetterValues(uICharacterCreationStringGetter);
			}
		}
		HandlePaperDoll();
	}

	public void SetInitialStringGetterValues(UICharacterCreationStringGetter stringGetter)
	{
		if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.SKILL_DESC)
		{
			stringGetter.SetText(GUIUtils.GetText(1810));
		}
		else if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.SKILL)
		{
			stringGetter.SetText(GUIUtils.GetText(901));
		}
		else if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.SKILL_DESC2)
		{
			stringGetter.SetText(string.Empty);
		}
		else if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE)
		{
			stringGetter.SetText(GUIUtils.GetText(306));
		}
		else if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC)
		{
			stringGetter.SetText(GUIUtils.GetText(1809));
		}
		else if (stringGetter.DataSource == UICharacterCreationStringGetter.StringDataSource.ATTRIBUTE_DESC2)
		{
			stringGetter.SetText(string.Empty);
		}
	}

	public void Deactivate()
	{
		if (AlwaysVisible)
		{
			return;
		}
		base.gameObject.SetActive(value: false);
		GameObject[] alsoShow = AlsoShow;
		foreach (GameObject gameObject in alsoShow)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: false);
			}
		}
		alsoShow = AlsoHide;
		foreach (GameObject gameObject2 in alsoShow)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: true);
			}
		}
	}

	protected void Populate()
	{
		UICharacterCreationPopulateEnumSetters[] componentsInChildren = GetComponentsInChildren<UICharacterCreationPopulateEnumSetters>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Initialize(Character);
		}
	}
}

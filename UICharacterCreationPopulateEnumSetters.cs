using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICharacterCreationPopulateEnumSetters : MonoBehaviour
{
	public UICharacterCreationEnumSetter.EnumType Enum;

	public GameObject baseObject;

	private List<GameObject> m_TempChildren = new List<GameObject>();

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Initialize(UICharacterCreationManager.Character character)
	{
		GameObject gameObject = null;
		baseObject.SetActive(value: false);
		baseObject.transform.localPosition = Vector3.zero;
		UIImageButtonRevised[] componentsInChildren = baseObject.GetComponentsInChildren<UIImageButtonRevised>(includeInactive: true);
		foreach (UIImageButtonRevised uIImageButtonRevised in componentsInChildren)
		{
			if (uIImageButtonRevised != null)
			{
				uIImageButtonRevised.ForceDown(val: false);
			}
		}
		foreach (GameObject tempChild in m_TempChildren)
		{
			UICharacterCreationEnumSetter[] componentsInChildren2 = tempChild.GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
			if (componentsInChildren2 != null)
			{
				UICharacterCreationEnumSetter[] array = componentsInChildren2;
				foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter in array)
				{
					if ((bool)uICharacterCreationEnumSetter)
					{
						uICharacterCreationEnumSetter.FreeData();
					}
				}
			}
			tempChild.SetActive(value: false);
			GameUtilities.Destroy(tempChild);
			tempChild.transform.parent = null;
		}
		m_TempChildren.Clear();
		Array array2 = null;
		List<int> list = new List<int>();
		List<GenericTalent.TalentCategory> list2 = new List<GenericTalent.TalentCategory>();
		switch (Enum)
		{
		case UICharacterCreationEnumSetter.EnumType.CLASS:
			array2 = UICharacterCreationEnumSetter.ValidClasses;
			break;
		case UICharacterCreationEnumSetter.EnumType.GENDER:
			array2 = UICharacterCreationEnumSetter.ValidGenders;
			break;
		case UICharacterCreationEnumSetter.EnumType.RACE:
			array2 = UICharacterCreationEnumSetter.ValidRaces;
			break;
		case UICharacterCreationEnumSetter.EnumType.SUBRACE:
			array2 = UICharacterCreationEnumSetter.ValidSubracesByRace[(int)character.Race];
			break;
		case UICharacterCreationEnumSetter.EnumType.CULTURE:
			array2 = UICharacterCreationEnumSetter.ValidCultures;
			break;
		case UICharacterCreationEnumSetter.EnumType.BACKGROUND:
			array2 = UICharacterCreationEnumSetter.ValidBackgrounds[(int)character.Culture];
			break;
		case UICharacterCreationEnumSetter.EnumType.BODY_TYPE:
			array2 = UICharacterCreationEnumSetter.ValidRacialBodyTypes;
			break;
		case UICharacterCreationEnumSetter.EnumType.VOICE_TYPE:
		{
			PlayerVoiceSetList playerVoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
			if ((bool)playerVoiceSetList)
			{
				array2 = playerVoiceSetList.GetPrioritySortedVoiceSets(character.Gender, character.Subrace, character.Race, character.Culture, character.Class);
			}
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.ABILITY:
		{
			UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentAbilitySelectionState();
			if (currentTalentSelectionState != null)
			{
				AbilityProgressionTable abilityProgressionTable2 = AbilityProgressionTable.LoadAbilityProgressionTable(character.Class.ToString());
				int level2 = character.CoreData.Level;
				character.CoreData.Level = currentTalentSelectionState.Level;
				for (int k = 0; k < currentTalentSelectionState.SelectedAbilities.Count; k++)
				{
					character.CoreData.RemoveKnownSkill(currentTalentSelectionState.SelectedAbilities[k].Ability);
				}
				array2 = abilityProgressionTable2.GetAbilities(character.CoreData, currentTalentSelectionState.Category, AbilityProgressionTable.DefaultUnlockFilterFlags);
				character.CoreData.Level = level2;
			}
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.TALENTS:
		{
			UICharacterCreationManager.AbilitySelectionState currentTalentSelectionState = UICharacterCreationManager.Instance.GetCurrentTalentSelectionState();
			if (currentTalentSelectionState != null)
			{
				AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
				int level = character.CoreData.Level;
				character.CoreData.Level = currentTalentSelectionState.Level;
				for (int j = 0; j < currentTalentSelectionState.SelectedAbilities.Count; j++)
				{
					character.CoreData.RemoveKnownSkill(currentTalentSelectionState.SelectedAbilities[j].Ability);
				}
				array2 = abilityProgressionTable.GetAbilities(character.CoreData, currentTalentSelectionState.Category, AbilityProgressionTable.DefaultUnlockFilterFlags);
				character.CoreData.Level = level;
			}
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY:
		{
			CharacterStats targetStats = UICharacterCreationManager.Instance.TargetCharacter.GetComponent<CharacterStats>();
			int maxSpellLevel = CharacterStats.MaxMasteredAbilitiesAllowed(targetStats.CharacterClass, UICharacterCreationManager.Instance.EndingLevel) - UICharacterCreationManager.Instance.SpellMasterySelectionStates.Count + UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex + 1;
			List<GameObject> selectedMasteredAbilities = UICharacterCreationManager.Instance.GetMasteredAbilities();
			array2 = targetStats.ActiveAbilities.Where((GenericAbility abl) => abl is GenericSpell && (abl as GenericSpell).SpellLevel <= maxSpellLevel && abl.MasteryLevel == 0 && (selectedMasteredAbilities.Find((GameObject match) => GenericAbility.NameComparer.Instance.Equals(match.GetComponent<GenericAbility>(), abl.GetComponent<GenericAbility>())) == null || UICharacterCreationManager.Instance.SpellMasterySelectionStates[UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex].SelectedAbilities.Find((AbilityProgressionTable.UnlockableAbility selectedAbl) => GenericAbility.NameComparer.Instance.Equals(abl.GetComponent<GenericAbility>(), selectedAbl.Ability.GetComponent<GenericAbility>())) != null) && targetStats.FindMasteredAbilityInstance(abl) == null).ToArray();
			break;
		}
		case UICharacterCreationEnumSetter.EnumType.DEITY:
			array2 = UICharacterCreationEnumSetter.ValidDeities;
			break;
		case UICharacterCreationEnumSetter.EnumType.RELIGION:
			array2 = UICharacterCreationEnumSetter.ValidPaladinOrders;
			break;
		}
		if (array2 == null)
		{
			return;
		}
		foreach (object item in array2)
		{
			if (!gameObject)
			{
				gameObject = UnityEngine.Object.Instantiate(baseObject);
				gameObject.transform.parent = baseObject.transform.parent;
				gameObject.transform.localScale = baseObject.transform.localScale;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.SetActive(value: true);
				m_TempChildren.Add(gameObject);
			}
			UICharacterCreationEnumSetter[] array = gameObject.GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
			foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter2 in array)
			{
				uICharacterCreationEnumSetter2.SetType = Enum;
				switch (Enum)
				{
				case UICharacterCreationEnumSetter.EnumType.GENDER:
					uICharacterCreationEnumSetter2.Gender = (Gender)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.CLASS:
					uICharacterCreationEnumSetter2.Class = (CharacterStats.Class)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.RACE:
					uICharacterCreationEnumSetter2.Race = (CharacterStats.Race)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.SUBRACE:
					uICharacterCreationEnumSetter2.Subrace = (CharacterStats.Subrace)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.CULTURE:
					uICharacterCreationEnumSetter2.Culture = (CharacterStats.Culture)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.BACKGROUND:
					uICharacterCreationEnumSetter2.Background = (CharacterStats.Background)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.BODY_TYPE:
					uICharacterCreationEnumSetter2.RacialBodyType = (CharacterStats.Race)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.VOICE_TYPE:
					uICharacterCreationEnumSetter2.CharacterVoiceSet = UnityEngine.Object.Instantiate((SoundSet)item);
					break;
				case UICharacterCreationEnumSetter.EnumType.ABILITY:
				{
					uICharacterCreationEnumSetter2.UnlockableAbility = (AbilityProgressionTable.UnlockableAbility)item;
					uICharacterCreationEnumSetter2.AbilityObject = uICharacterCreationEnumSetter2.UnlockableAbility.Ability;
					int spellLevel = AbilityProgressionTable.GetSpellLevel(uICharacterCreationEnumSetter2.UnlockableAbility.Ability);
					if (spellLevel > 0 && !list.Contains(spellLevel))
					{
						list.Add(spellLevel);
					}
					break;
				}
				case UICharacterCreationEnumSetter.EnumType.TALENTS:
				{
					uICharacterCreationEnumSetter2.UnlockableAbility = (AbilityProgressionTable.UnlockableAbility)item;
					uICharacterCreationEnumSetter2.AbilityObject = uICharacterCreationEnumSetter2.UnlockableAbility.Ability;
					GenericTalent.TalentCategory category = AbilityProgressionTable.GetGenericTalent(uICharacterCreationEnumSetter2.UnlockableAbility.Ability).Category;
					if (category == GenericTalent.TalentCategory.Undefined)
					{
						UIDebug.Instance.LogOnScreenWarning(string.Concat("Talent: ", uICharacterCreationEnumSetter2.UnlockableAbility.Ability, " has an undefined Talent Category set. This must be fixed to show up in game."), UIDebug.Department.Design, 10f);
					}
					else if (!list2.Contains(category))
					{
						list2.Add(category);
					}
					break;
				}
				case UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY:
				{
					if (uICharacterCreationEnumSetter2.UnlockableAbility == null)
					{
						uICharacterCreationEnumSetter2.UnlockableAbility = new AbilityProgressionTable.UnlockableAbility();
					}
					uICharacterCreationEnumSetter2.UnlockableAbility.Ability = ((GenericAbility)item).gameObject;
					uICharacterCreationEnumSetter2.AbilityObject = uICharacterCreationEnumSetter2.UnlockableAbility.Ability;
					int spellLevel = AbilityProgressionTable.GetSpellLevel(uICharacterCreationEnumSetter2.AbilityObject);
					if (spellLevel > 0 && !list.Contains(spellLevel))
					{
						list.Add(spellLevel);
					}
					break;
				}
				case UICharacterCreationEnumSetter.EnumType.DEITY:
					uICharacterCreationEnumSetter2.Deity = (Religion.Deity)item;
					break;
				case UICharacterCreationEnumSetter.EnumType.RELIGION:
					uICharacterCreationEnumSetter2.PaladinOrder = (Religion.PaladinOrder)item;
					break;
				}
			}
			UILabel[] componentsInChildren3 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			foreach (UILabel uILabel in componentsInChildren3)
			{
				switch (Enum)
				{
				case UICharacterCreationEnumSetter.EnumType.GENDER:
					uILabel.text = GUIUtils.GetGenderString((Gender)item);
					break;
				case UICharacterCreationEnumSetter.EnumType.CLASS:
					uILabel.text = GUIUtils.GetClassString((CharacterStats.Class)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.RACE:
					uILabel.text = GUIUtils.GetRaceString((CharacterStats.Race)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.SUBRACE:
					uILabel.text = GUIUtils.GetSubraceString((CharacterStats.Subrace)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.CULTURE:
					uILabel.text = GUIUtils.GetCultureString((CharacterStats.Culture)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.BACKGROUND:
					uILabel.text = GUIUtils.GetBackgroundString((CharacterStats.Background)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.BODY_TYPE:
					uILabel.text = GUIUtils.GetRaceString((CharacterStats.Race)item, character.Gender);
					break;
				case UICharacterCreationEnumSetter.EnumType.VOICE_TYPE:
					uILabel.text = ((SoundSet)item).DisplayName.GetText();
					break;
				case UICharacterCreationEnumSetter.EnumType.ABILITY:
					uILabel.text = AbilityProgressionTable.GetAbilityName(((AbilityProgressionTable.UnlockableAbility)item).Ability);
					break;
				case UICharacterCreationEnumSetter.EnumType.TALENTS:
					uILabel.text = AbilityProgressionTable.GetAbilityName(((AbilityProgressionTable.UnlockableAbility)item).Ability);
					break;
				case UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY:
					uILabel.text = AbilityProgressionTable.GetAbilityName(((GenericAbility)item).gameObject);
					break;
				case UICharacterCreationEnumSetter.EnumType.DEITY:
					uILabel.text = GUIUtils.GetDeityString((Religion.Deity)item);
					break;
				case UICharacterCreationEnumSetter.EnumType.RELIGION:
					uILabel.text = GUIUtils.GetPaladinOrderString((Religion.PaladinOrder)item, character.Gender);
					break;
				}
			}
			UITexture componentInChildren = gameObject.GetComponentInChildren<UITexture>();
			if ((bool)componentInChildren)
			{
				Shader shader = Shader.Find("Unlit/Transparent Colored");
				if (shader != null)
				{
					componentInChildren.material = new Material(shader);
				}
				componentInChildren.color = Color.white;
				GameObject ability2 = ((Enum == UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY) ? ((GenericAbility)item).gameObject : ((AbilityProgressionTable.UnlockableAbility)item).Ability);
				componentInChildren.material.mainTexture = AbilityProgressionTable.GetAbilityIcon(ability2);
			}
			gameObject = null;
		}
		UICharacterCreationEnumSetter[] componentsInChildren4 = m_TempChildren[0].GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
		if (componentsInChildren4.Length != 0)
		{
			componentsInChildren4[0].SetIfUndefined();
		}
		else
		{
			UICharacterCreationManager.Instance.RefreshAll();
		}
		if (Enum == UICharacterCreationEnumSetter.EnumType.ABILITY)
		{
			AbilityProgressionTable.UnlockableAbility unlockableAbility = null;
			UICharacterCreationManager.AbilitySelectionState abilitySelectionState = UICharacterCreationManager.Instance.AbilitySelectionStates[UICharacterCreationManager.Instance.AbilitySelectionStateIndex];
			if (abilitySelectionState != null)
			{
				unlockableAbility = abilitySelectionState.SelectedAbilities.FindLast((AbilityProgressionTable.UnlockableAbility ability) => ability != null);
			}
			SelectBestAbilityLevel(list, unlockableAbility?.Ability);
		}
		else if (Enum == UICharacterCreationEnumSetter.EnumType.TALENTS)
		{
			UICharacterCreationManager.AbilitySelectionState abilitySelectionState2 = UICharacterCreationManager.Instance.TalentSelectionStates[UICharacterCreationManager.Instance.TalentSelectionStateIndex];
			AbilityProgressionTable.UnlockableAbility talentToFind = null;
			if (abilitySelectionState2 != null)
			{
				talentToFind = abilitySelectionState2.SelectedAbilities.FindLast((AbilityProgressionTable.UnlockableAbility ability) => ability != null);
			}
			SelectBestTalentCategory(list2, talentToFind);
		}
		else if (Enum == UICharacterCreationEnumSetter.EnumType.ABILITY_MASTERY)
		{
			AbilityProgressionTable.UnlockableAbility unlockableAbility2 = null;
			UICharacterCreationManager.AbilitySelectionState abilitySelectionState3 = UICharacterCreationManager.Instance.SpellMasterySelectionStates[UICharacterCreationManager.Instance.SpellMasterySelectionStateIndex];
			if (abilitySelectionState3 != null)
			{
				unlockableAbility2 = abilitySelectionState3.SelectedAbilities.FindLast((AbilityProgressionTable.UnlockableAbility ability) => ability != null);
			}
			SelectBestAbilityLevel(list, unlockableAbility2?.Ability);
		}
		UIGrid component = GetComponent<UIGrid>();
		if ((bool)component)
		{
			component.Reposition();
		}
	}

	private void SelectBestTalentCategory(List<GenericTalent.TalentCategory> categoriesUsed, AbilityProgressionTable.UnlockableAbility talentToFind)
	{
		UICharacterCreationTalentCategoryButton uICharacterCreationTalentCategoryButton = null;
		UICharacterCreationTalentCategoryButton uICharacterCreationTalentCategoryButton2 = null;
		UICharacterCreationTalentCategoryButton[] array = UnityEngine.Object.FindObjectsOfType<UICharacterCreationTalentCategoryButton>();
		if (talentToFind != null)
		{
			GenericTalent.TalentCategory category = AbilityProgressionTable.GetGenericTalent(talentToFind.Ability).Category;
			for (int i = 0; i < array.Length; i++)
			{
				uICharacterCreationTalentCategoryButton2 = array[i];
				if (!(uICharacterCreationTalentCategoryButton2 == null))
				{
					if (uICharacterCreationTalentCategoryButton == null && uICharacterCreationTalentCategoryButton2.TalentCategory == category)
					{
						uICharacterCreationTalentCategoryButton = uICharacterCreationTalentCategoryButton2;
					}
					if (categoriesUsed.Contains(uICharacterCreationTalentCategoryButton2.TalentCategory))
					{
						uICharacterCreationTalentCategoryButton2.EnableButton();
					}
					else
					{
						uICharacterCreationTalentCategoryButton2.DisableButton();
					}
				}
			}
		}
		if (uICharacterCreationTalentCategoryButton == null)
		{
			UICharacterCreationTalentCategoryButton[] array2 = array;
			foreach (UICharacterCreationTalentCategoryButton uICharacterCreationTalentCategoryButton3 in array2)
			{
				if (categoriesUsed.Contains(uICharacterCreationTalentCategoryButton3.TalentCategory))
				{
					uICharacterCreationTalentCategoryButton3.EnableButton();
					if (uICharacterCreationTalentCategoryButton == null || uICharacterCreationTalentCategoryButton.TalentCategory > uICharacterCreationTalentCategoryButton3.TalentCategory)
					{
						uICharacterCreationTalentCategoryButton = uICharacterCreationTalentCategoryButton3;
					}
				}
				else
				{
					uICharacterCreationTalentCategoryButton3.DisableButton();
				}
			}
		}
		if ((bool)uICharacterCreationTalentCategoryButton)
		{
			uICharacterCreationTalentCategoryButton.ShowTalentCategory();
		}
	}

	private void SelectBestAbilityLevel(List<int> spellLevelsUsed, GameObject abilityToFind)
	{
		UICharacterCreationAbilityLevelButton uICharacterCreationAbilityLevelButton = null;
		UICharacterCreationAbilityLevelButton uICharacterCreationAbilityLevelButton2 = null;
		UICharacterCreationAbilityLevelButton[] array = UnityEngine.Object.FindObjectsOfType<UICharacterCreationAbilityLevelButton>();
		if (abilityToFind != null)
		{
			int spellLevel = AbilityProgressionTable.GetSpellLevel(abilityToFind);
			for (int i = 0; i < array.Length; i++)
			{
				uICharacterCreationAbilityLevelButton2 = array[i];
				if (!(uICharacterCreationAbilityLevelButton2 == null))
				{
					if (uICharacterCreationAbilityLevelButton == null && uICharacterCreationAbilityLevelButton2.AbilityLevel == spellLevel)
					{
						uICharacterCreationAbilityLevelButton = uICharacterCreationAbilityLevelButton2;
					}
					if (spellLevelsUsed.Contains(uICharacterCreationAbilityLevelButton2.AbilityLevel))
					{
						uICharacterCreationAbilityLevelButton2.EnableButton();
					}
					else
					{
						uICharacterCreationAbilityLevelButton2.DisableButton();
					}
				}
			}
		}
		if (uICharacterCreationAbilityLevelButton == null)
		{
			UICharacterCreationAbilityLevelButton[] array2 = array;
			foreach (UICharacterCreationAbilityLevelButton uICharacterCreationAbilityLevelButton3 in array2)
			{
				if (spellLevelsUsed.Contains(uICharacterCreationAbilityLevelButton3.AbilityLevel))
				{
					uICharacterCreationAbilityLevelButton3.EnableButton();
					if (uICharacterCreationAbilityLevelButton == null || uICharacterCreationAbilityLevelButton.AbilityLevel < uICharacterCreationAbilityLevelButton3.AbilityLevel)
					{
						uICharacterCreationAbilityLevelButton = uICharacterCreationAbilityLevelButton3;
					}
				}
				else
				{
					uICharacterCreationAbilityLevelButton3.DisableButton();
				}
			}
		}
		if ((bool)uICharacterCreationAbilityLevelButton)
		{
			uICharacterCreationAbilityLevelButton.ShowLevelGroup();
		}
		if (array.Length == 0)
		{
			return;
		}
		array[0].AbilityButtonPanel.alpha = 0f;
		foreach (int item in spellLevelsUsed)
		{
			if (item > 0)
			{
				array[0].AbilityButtonPanel.alpha = 1f;
				break;
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UICharacterCreationManager : UIHudWindow
{
	public enum CharacterCreationType
	{
		NewPlayer,
		NewCompanion,
		LevelUp,
		Count
	}

	public delegate void StageChanged(int stage);

	public class AbilitySelectionState
	{
		public int Level;

		public AbilityProgressionTable.CategoryFlag Category;

		public string CategoryName;

		public string PointUnlockDescription;

		public int Points;

		public List<AbilityProgressionTable.UnlockableAbility> SelectedAbilities;

		public AbilityProgressionTable.UnlockableAbility LastSelectedAbility;
	}

	public class Character
	{
		public struct AddedAbilityPair
		{
			public int Level;

			public GameObject AddedAbility;

			public AddedAbilityPair(int level, GameObject ability)
			{
				Level = level;
				AddedAbility = ability;
			}
		}

		public CharacterStats.CoreData CoreData;

		public string CharacterPortraitLargePath = "";

		public string CharacterPortraitSmallPath = "";

		public int PortraitIndex = -1;

		public int[] SkillValueDeltas = new int[6];

		public int[] SkillRankDeltas = new int[6];

		public int SkillPointsToSpend;

		public Color SkinColor = Color.white;

		public Color HairColor = Color.white;

		public Color MajorColor = Color.white;

		public Color MinorColor = Color.white;

		public int HairModelVariation = 1;

		public int HairMaterialVariation = 1;

		public int FacialHairModelVariation;

		public int FacialHairMaterialVariation = 1;

		public int HeadModelVariation = 1;

		public int HeadMaterialVariation = 1;

		public string SkinOverride = "";

		public string NudeModelOverride = "";

		public EquipmentSetData ClassEquipmentSetData;

		public SoundSet VoiceSet;

		public int StartingLevel;

		public List<AddedAbilityPair> AddedUnlockables = new List<AddedAbilityPair>();

		public string Name = string.Empty;

		public string Animal_Name = string.Empty;

		public Gender Gender
		{
			get
			{
				return CoreData.Gender;
			}
			set
			{
				CoreData.Gender = value;
			}
		}

		public CharacterStats.Race Race
		{
			get
			{
				return CoreData.Race;
			}
			set
			{
				CoreData.Race = value;
			}
		}

		public CharacterStats.Race RacialBodyType
		{
			get
			{
				return CoreData.RacialBodyType;
			}
			set
			{
				CoreData.RacialBodyType = value;
			}
		}

		public CharacterStats.Subrace Subrace
		{
			get
			{
				return CoreData.Subrace;
			}
			set
			{
				CoreData.Subrace = value;
			}
		}

		public CharacterStats.Class Class
		{
			get
			{
				return CoreData.Class;
			}
			set
			{
				CoreData.Class = value;
			}
		}

		public CharacterStats.Culture Culture
		{
			get
			{
				return CoreData.Culture;
			}
			set
			{
				CoreData.Culture = value;
			}
		}

		public CharacterStats.Background Background
		{
			get
			{
				return CoreData.Background;
			}
			set
			{
				CoreData.Background = value;
			}
		}

		public Religion.Deity Deity
		{
			get
			{
				return CoreData.Deity;
			}
			set
			{
				CoreData.Deity = value;
			}
		}

		public Religion.PaladinOrder PaladinOrder
		{
			get
			{
				return CoreData.PaladinOrder;
			}
			set
			{
				CoreData.PaladinOrder = value;
			}
		}

		public int[] BaseStats => CoreData.BaseStats;

		public int[] SkillValues => CoreData.SkillValues;

		public Character()
		{
			int num = 0;
			while (CoreData != null && num < BaseStats.Length)
			{
				BaseStats[num] = 10;
				num++;
			}
			for (int i = 0; i < SkillValueDeltas.Length; i++)
			{
				SkillValueDeltas[i] = 0;
				SkillRankDeltas[i] = 0;
			}
		}

		public int GetModelVariation(UICharacterCreationAppearanceSetter.AppearanceType AppearanceType)
		{
			return AppearanceType switch
			{
				UICharacterCreationAppearanceSetter.AppearanceType.FACIAL_HAIR => FacialHairModelVariation, 
				UICharacterCreationAppearanceSetter.AppearanceType.HAIR => HairModelVariation, 
				UICharacterCreationAppearanceSetter.AppearanceType.HEAD => HeadModelVariation, 
				_ => 0, 
			};
		}

		public Character CopyOf()
		{
			Character character = new Character();
			CoreData = CoreData.Copy();
			character.Name = string.Copy(Name);
			character.Animal_Name = Animal_Name;
			character.VoiceSet = VoiceSet;
			character.SkinColor = SkinColor;
			character.HairColor = HairColor;
			character.MajorColor = MajorColor;
			character.MinorColor = MinorColor;
			character.CharacterPortraitLargePath = CharacterPortraitLargePath;
			character.CharacterPortraitSmallPath = CharacterPortraitSmallPath;
			character.HairModelVariation = HairModelVariation;
			character.HairMaterialVariation = HairMaterialVariation;
			character.FacialHairModelVariation = FacialHairModelVariation;
			character.FacialHairMaterialVariation = FacialHairMaterialVariation;
			character.HeadModelVariation = HeadModelVariation;
			character.HeadMaterialVariation = HeadMaterialVariation;
			character.StartingLevel = StartingLevel;
			character.SkillPointsToSpend = SkillPointsToSpend;
			for (int i = 0; i < SkillValueDeltas.Length; i++)
			{
				character.SkillValueDeltas[i] = SkillValueDeltas[i];
				character.SkillRankDeltas[i] = SkillRankDeltas[i];
			}
			return character;
		}

		public void GetFrom(GameObject character)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			NPCAppearance component2 = character.GetComponent<NPCAppearance>();
			Portrait component3 = character.GetComponent<Portrait>();
			PartyMemberAI component4 = character.GetComponent<PartyMemberAI>();
			CoreData = component.GetCopyOfCoreData();
			Name = component.Name();
			VoiceSet = component4.SoundSet;
			SkinColor = component2.skinColor;
			HairColor = component2.hairColor;
			MinorColor = component2.secondaryColor;
			MajorColor = component2.primaryColor;
			CharacterPortraitSmallPath = component3.TextureSmallPath;
			CharacterPortraitLargePath = component3.TextureLargePath;
			HairModelVariation = (component2.hasHair ? component2.hairAppearance.modelVariation : 0);
			HairMaterialVariation = component2.hairAppearance.materialVariation;
			FacialHairModelVariation = (component2.hasFacialHair ? component2.facialHairAppearance.modelVariation : 0);
			FacialHairMaterialVariation = component2.facialHairAppearance.materialVariation;
			HeadModelVariation = component2.headAppearance.modelVariation;
			HeadMaterialVariation = component2.headAppearance.materialVariation;
			SkinOverride = component2.skinOverride;
			NudeModelOverride = component2.nudeModelOverride;
			StartingLevel = component.Level;
			SkillPointsToSpend = component.RemainingSkillPoints;
		}

		public void ApplyTo(GameObject character, bool reloadEquipment, UICharacterCreationElement.ValueType type)
		{
			CharacterStats component = character.GetComponent<CharacterStats>();
			NPCAppearance component2 = character.GetComponent<NPCAppearance>();
			PartyMemberAI component3 = character.GetComponent<PartyMemberAI>();
			Portrait component4 = character.GetComponent<Portrait>();
			ValidateVariations();
			component.OverrideName = Name;
			component.Gender = Gender;
			component.CharacterRace = Race;
			component.CharacterSubrace = Subrace;
			component.RacialBodyType = RacialBodyType;
			component.CharacterClass = Class;
			component.CharacterCulture = Culture;
			component.CharacterBackground = Background;
			component.Deity = Deity;
			component.PaladinOrder = PaladinOrder;
			component.BaseDexterity = BaseStats[2];
			component.BaseMight = BaseStats[1];
			component.BaseResolve = BaseStats[0];
			component.BaseIntellect = BaseStats[3];
			component.BasePerception = BaseStats[5];
			component.BaseConstitution = BaseStats[4];
			component.RemainingSkillPoints = SkillPointsToSpend;
			foreach (AddedAbilityPair addedUnlockable in AddedUnlockables)
			{
				AbilityProgressionTable.AddAbilityToCharacter(addedUnlockable.AddedAbility, component, causeIsGameplay: true);
			}
			if (type == UICharacterCreationElement.ValueType.Portrait || type == UICharacterCreationElement.ValueType.All)
			{
				component4.TextureLargePath = CharacterPortraitLargePath;
				component4.TextureSmallPath = CharacterPortraitSmallPath;
			}
			bool flag = NPCAppearance.ConvertRace(component.RacialBodyType) != component2.racialBodyType || component2.gender != NPCAppearance.ConvertGender(component.Gender);
			component2.gender = NPCAppearance.ConvertGender(Gender);
			component2.race = NPCAppearance.ConvertRace(Race);
			component2.subrace = NPCAppearance.ConvertSubrace(Instance.GetLastPickedSubraceForRace(Race));
			component2.racialBodyType = NPCAppearance.ConvertRace(RacialBodyType);
			component2.skinColor = SkinColor;
			component2.hairColor = HairColor;
			component2.primaryColor = MajorColor;
			component2.secondaryColor = MinorColor;
			component2.hasHair = HairModelVariation > 0;
			component2.hasFacialHair = FacialHairModelVariation > 0;
			component2.facialHairAppearance.modelVariation = FacialHairModelVariation;
			component2.facialHairAppearance.materialVariation = FacialHairMaterialVariation;
			component2.facialHairAppearance.bodyPiece = AppearancePiece.BodyPiece.Facialhair;
			component2.hairAppearance.modelVariation = HairModelVariation;
			component2.hairAppearance.materialVariation = HairMaterialVariation;
			component2.hairAppearance.bodyPiece = AppearancePiece.BodyPiece.Hair;
			component2.headAppearance.modelVariation = HeadModelVariation;
			component2.headAppearance.materialVariation = HeadMaterialVariation;
			component2.headAppearance.bodyPiece = AppearancePiece.BodyPiece.Head;
			component2.skinOverride = SkinOverride;
			component2.nudeModelOverride = NudeModelOverride;
			if (ClassEquipmentSetData != null && (reloadEquipment || flag))
			{
				MirrorCharacterUtils.LoadEquipment(character, ClassEquipmentSetData.Equipment);
			}
			if (flag)
			{
				Transform transform = GameUtilities.FindSkeletonTransform(character);
				if (transform != null)
				{
					transform.name = "old_skel";
					GameUtilities.DestroyImmediate(transform.gameObject);
				}
				Animator component5 = character.GetComponent<Animator>();
				if ((bool)component5)
				{
					GameUtilities.DestroyImmediate(component5);
				}
			}
			if (type == UICharacterCreationElement.ValueType.Color)
			{
				component2.ApplyTints();
			}
			if (reloadEquipment || flag || type == UICharacterCreationElement.ValueType.BodyPart || type == UICharacterCreationElement.ValueType.Race || type == UICharacterCreationElement.ValueType.Subrace || type == UICharacterCreationElement.ValueType.All)
			{
				component2.Generate();
			}
			if (PE_Paperdoll.IsObjectPaperdoll(character))
			{
				Animator component6 = character.gameObject.GetComponent<Animator>();
				if ((bool)component6)
				{
					component6.updateMode = AnimatorUpdateMode.UnscaledTime;
				}
			}
			AnimationController component7 = character.gameObject.GetComponent<AnimationController>();
			if ((bool)component7)
			{
				component7.BindComponents();
			}
			if ((reloadEquipment || flag) && ClassEquipmentSetData != null)
			{
				MirrorCharacterUtils.LoadEquipment(character, ClassEquipmentSetData.Equipment);
			}
			if ((bool)component3)
			{
				UIPartyPortrait[] array = UnityEngine.Object.FindObjectsOfType(typeof(UIPartyPortrait)) as UIPartyPortrait[];
				foreach (UIPartyPortrait uIPartyPortrait in array)
				{
					if (uIPartyPortrait.PartyMemberAI == component3)
					{
						uIPartyPortrait.ReloadPartyMember();
					}
				}
				component3.SoundSet = VoiceSet;
			}
			if (PE_Paperdoll.IsObjectPaperdoll(character))
			{
				NPCAppearance.ReplaceTexturesWithHDTextures(character, createNewMaterials: true);
			}
		}

		public void SetDefaultAppearance()
		{
			SetDefaultAppearance(Subrace, Gender);
		}

		public void SetDefaultAppearance(CharacterStats.Subrace subrace, Gender gender)
		{
			if (subrace == CharacterStats.Subrace.Undefined)
			{
				SetDefaultAppearance(Instance.GetLastPickedSubraceForRace(Race), Gender);
				return;
			}
			DefaultAppearanceList.DefaultAppearanceSet matchingAppearanceSet = DefaultAppearanceList.GetMatchingAppearanceSet(subrace, gender);
			if (matchingAppearanceSet != null)
			{
				if ((bool)ColorListManager.GetColorList(ColorListManager.ColorPickerType.Major, subrace))
				{
					MajorColor = ColorListManager.GetColorList(ColorListManager.ColorPickerType.Major, subrace).GetUnsortedColor(matchingAppearanceSet.PrimaryColorIndex);
				}
				if ((bool)ColorListManager.GetColorList(ColorListManager.ColorPickerType.Minor, subrace))
				{
					MinorColor = ColorListManager.GetColorList(ColorListManager.ColorPickerType.Minor, subrace).GetUnsortedColor(matchingAppearanceSet.SecondaryColorIndex);
				}
				if ((bool)ColorListManager.GetColorList(ColorListManager.ColorPickerType.Hair, subrace))
				{
					HairColor = ColorListManager.GetColorList(ColorListManager.ColorPickerType.Hair, subrace).GetUnsortedColor(matchingAppearanceSet.HairColorIndex);
				}
				if ((bool)ColorListManager.GetColorList(ColorListManager.ColorPickerType.Skin, subrace))
				{
					SkinColor = ColorListManager.GetColorList(ColorListManager.ColorPickerType.Skin, subrace).GetUnsortedColor(matchingAppearanceSet.SkinColorIndex);
				}
				HeadModelVariation = matchingAppearanceSet.HeadVariation;
				HairModelVariation = matchingAppearanceSet.HairVariation - 1;
				FacialHairModelVariation = matchingAppearanceSet.FacialHairVariation - 1;
			}
			else
			{
				SkinColor = UIColorPairSelector.GetRandomColor(ColorListManager.ColorPickerType.Skin, subrace);
				HairColor = UIColorPairSelector.GetRandomColor(ColorListManager.ColorPickerType.Hair, subrace);
			}
			PE_Paperdoll.PaperdollCharacter.GetComponent<NPCAppearance>().ApplyTints();
		}

		public void ValidateVariations()
		{
			int totalModelVariations = GetTotalModelVariations(this, UICharacterCreationAppearanceSetter.AppearanceType.FACIAL_HAIR);
			if (FacialHairModelVariation > totalModelVariations || FacialHairModelVariation < 0)
			{
				FacialHairModelVariation = 0;
			}
			totalModelVariations = GetTotalModelVariations(this, UICharacterCreationAppearanceSetter.AppearanceType.HAIR);
			if (HairModelVariation > totalModelVariations || HairModelVariation < 0)
			{
				HairModelVariation = 0;
			}
			totalModelVariations = GetTotalModelVariations(this, UICharacterCreationAppearanceSetter.AppearanceType.HEAD);
			if (HeadModelVariation > totalModelVariations || HeadModelVariation <= 0)
			{
				HeadModelVariation = 1;
			}
			if (Race == CharacterStats.Race.Godlike)
			{
				HairModelVariation = HeadModelVariation;
			}
			if (Race == CharacterStats.Race.Aumaua || Race == CharacterStats.Race.Godlike || Subrace == CharacterStats.Subrace.Wood_Elf)
			{
				FacialHairModelVariation = 0;
			}
		}

		private string GetSubraceSubfolder(CharacterStats.Subrace subrace)
		{
			return subrace switch
			{
				CharacterStats.Subrace.Death_Godlike => "GODD", 
				CharacterStats.Subrace.Fire_Godlike => "GODF", 
				CharacterStats.Subrace.Moon_Godlike => "GODM", 
				CharacterStats.Subrace.Nature_Godlike => "GODN", 
				CharacterStats.Subrace.Wild_Orlan => "WOR", 
				_ => "Default", 
			};
		}

		public string GetModelPath(UICharacterCreationAppearanceSetter.AppearanceType appearanceType)
		{
			AppearancePiece.BodyPiece bodyPiece = AppearancePiece.BodyPiece.Hair;
			switch (appearanceType)
			{
			case UICharacterCreationAppearanceSetter.AppearanceType.FACIAL_HAIR:
				bodyPiece = AppearancePiece.BodyPiece.Facialhair;
				break;
			case UICharacterCreationAppearanceSetter.AppearanceType.HAIR:
				bodyPiece = AppearancePiece.BodyPiece.Hair;
				break;
			case UICharacterCreationAppearanceSetter.AppearanceType.HEAD:
				bodyPiece = AppearancePiece.BodyPiece.Head;
				break;
			}
			if (appearanceType == UICharacterCreationAppearanceSetter.AppearanceType.HEAD)
			{
				return $"{AppearancePiece.CHARACTER_PATH}{NPCAppearance.ConvertGender(Gender).ToString()}/{NPCAppearance.ConvertRace(RacialBodyType).ToString()}/{bodyPiece}/{GetSubraceSubfolder(Subrace)}/";
			}
			return $"{AppearancePiece.CHARACTER_PATH}{NPCAppearance.ConvertGender(Gender).ToString()}/{NPCAppearance.ConvertRace(RacialBodyType).ToString()}/{bodyPiece}/";
		}
	}

	private static UICharacterCreationManager s_Instance;

	public CharacterCreationType CreationType;

	public string[] Backgrounds = new string[3];

	public UIDynamicLoadTexture CharacterCreationBackground;

	public string NewCompanionPrefabString;

	public GameObject[] StartsDisabled;

	public UITexture CharacterRenderZone;

	public GameObject CharacterPlatform;

	public UICharacterCreationController[] RootControllers;

	private UICharacterCreationController RootController;

	public UICharacterCreationController AttributesController;

	private List<UICharacterCreationController> m_Controllers = new List<UICharacterCreationController>();

	private int m_controllerIndex;

	private int m_maxControllerStage;

	public UILabel[] ControllerSubtitles;

	private GameObject m_TargetCharacter;

	[HideInInspector]
	public int EndingLevel;

	public int EndingXp;

	[HideInInspector]
	public int PlayerCost;

	private Character m_Character = new Character();

	public StageChanged OnStageChanged;

	public int m_CurrentStage;

	private int m_startingControllerIndex;

	public int LastStage = 6;

	public Dictionary<CharacterStats.Race, CharacterStats.Subrace> LastPickedSubrace = new Dictionary<CharacterStats.Race, CharacterStats.Subrace>();

	private bool m_closing;

	public int AbilitySelectionStateIndex;

	public List<AbilitySelectionState> AbilitySelectionStates = new List<AbilitySelectionState>();

	public int TalentSelectionStateIndex;

	public List<AbilitySelectionState> TalentSelectionStates = new List<AbilitySelectionState>();

	public List<AbilitySelectionState> SpellMasterySelectionStates = new List<AbilitySelectionState>();

	public int SpellMasterySelectionStateIndex;

	public List<GameObject> AutoGrantedAbilities = new List<GameObject>();

	public int NumMasteredAbilitiesAllowed;

	public int TotalPointBuy = 20;

	public int MaxStatGulf = 14;

	public int StatHardMinimum = 8;

	public int StatHardMaximum = 18;

	public int BaseStat = 8;

	public const int StatDefault = 10;

	public GameObject RightParchmentBG;

	public GameObject DoneButton;

	public GameObject ExitButton;

	private const float INACTIVE_BUTTON_ALPHA = 0.35f;

	private EquipmentSetDataList m_Loadouts;

	private float m_PaperdollAngle;

	public static UICharacterCreationManager Instance => s_Instance;

	public GameObject TargetCharacter => m_TargetCharacter;

	public Character ActiveCharacter => RootCharacter;

	public Character RootCharacter => m_Character;

	public int CurrentStage
	{
		get
		{
			return m_CurrentStage;
		}
		set
		{
			if (m_CurrentStage != value)
			{
				m_CurrentStage = value;
				if (OnStageChanged != null)
				{
					OnStageChanged(m_CurrentStage);
				}
			}
		}
	}

	public List<GameObject> GetSelectedAndGrantedAbilities()
	{
		List<GameObject> selectedAbilities = GetSelectedAbilities();
		selectedAbilities.AddRange(AutoGrantedAbilities);
		selectedAbilities.AddRange(GetMasteredAbilities());
		return selectedAbilities;
	}

	public List<GameObject> GetSelectedAbilities()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (AbilitySelectionState abilitySelectionState in Instance.AbilitySelectionStates)
		{
			if (abilitySelectionState == null)
			{
				continue;
			}
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
			{
				list.Add(selectedAbility.Ability);
			}
		}
		return list;
	}

	public List<GameObject> GetSelectedTalents()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (AbilitySelectionState talentSelectionState in Instance.TalentSelectionStates)
		{
			if (talentSelectionState == null)
			{
				continue;
			}
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in talentSelectionState.SelectedAbilities)
			{
				list.Add(selectedAbility.Ability);
			}
		}
		return list;
	}

	public List<GameObject> GetMasteredAbilities()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (AbilitySelectionState spellMasterySelectionState in Instance.SpellMasterySelectionStates)
		{
			if (spellMasterySelectionState == null)
			{
				continue;
			}
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in spellMasterySelectionState.SelectedAbilities)
			{
				list.Add(selectedAbility.Ability);
			}
		}
		return list;
	}

	public AbilitySelectionState GetCurrentAbilitySelectionState()
	{
		if (Instance.AbilitySelectionStateIndex >= AbilitySelectionStates.Count)
		{
			return null;
		}
		return AbilitySelectionStates[Instance.AbilitySelectionStateIndex];
	}

	public AbilitySelectionState GetCurrentTalentSelectionState()
	{
		if (Instance.TalentSelectionStateIndex >= TalentSelectionStates.Count)
		{
			return null;
		}
		return TalentSelectionStates[Instance.TalentSelectionStateIndex];
	}

	public AbilitySelectionState GetCurrentSpellMasterySelectionState()
	{
		if (Instance.SpellMasterySelectionStateIndex >= SpellMasterySelectionStates.Count)
		{
			return null;
		}
		return SpellMasterySelectionStates[Instance.SpellMasterySelectionStateIndex];
	}

	public void RemovePreviousAbilitySelections()
	{
		foreach (AbilitySelectionState abilitySelectionState in Instance.AbilitySelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
			{
				ActiveCharacter.CoreData.RemoveKnownSkill(selectedAbility.Ability);
			}
		}
	}

	public void RemovePreviousTalentSelections()
	{
		foreach (AbilitySelectionState talentSelectionState in Instance.TalentSelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in talentSelectionState.SelectedAbilities)
			{
				ActiveCharacter.CoreData.RemoveKnownSkill(selectedAbility.Ability);
			}
		}
	}

	private int CalculateNumberOfPointsAbleToUseForSelectionState(AbilityProgressionTable table, AbilitySelectionState selectionState, Character character)
	{
		int level = character.CoreData.Level;
		character.CoreData.Level = selectionState.Level;
		AbilityProgressionTable.UnlockableAbility[] abilities = table.GetAbilities(character.CoreData, selectionState.Category, AbilityProgressionTable.DefaultUnlockFilterFlags);
		character.CoreData.Level = level;
		int val = ((abilities != null) ? abilities.Length : 0);
		return Math.Min(selectionState.Points, val);
	}

	public void CalculateAbilitySelectionStates(Character character)
	{
		RemovePreviousAbilitySelections();
		AbilitySelectionStates.Clear();
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(character.Class.ToString());
		for (int i = character.StartingLevel + 1; i <= EndingLevel; i++)
		{
			List<AbilityProgressionTable.AbilityPointUnlock> levelAbilityPoints = abilityProgressionTable.GetLevelAbilityPoints(i);
			foreach (AbilityProgressionTable.AbilityPointUnlock item in levelAbilityPoints)
			{
				int num = 0;
				while (item != null && num < item.CategoryPointPairs.Length)
				{
					if (item.CategoryPointPairs[num].Category != AbilityProgressionTable.CategoryFlag.Talent)
					{
						AbilitySelectionState abilitySelectionState = new AbilitySelectionState();
						abilitySelectionState.Category = item.CategoryPointPairs[num].Category;
						abilitySelectionState.CategoryName = abilityProgressionTable.GetCategoryName(abilitySelectionState.Category);
						abilitySelectionState.PointUnlockDescription = item.CategoryPointPairs[num].UnlockDescription.GetText(character.Gender);
						abilitySelectionState.Points = item.CategoryPointPairs[num].PointsGranted;
						abilitySelectionState.Level = i;
						int num2 = CalculateNumberOfPointsAbleToUseForSelectionState(abilityProgressionTable, abilitySelectionState, character);
						if (num2 > 0)
						{
							abilitySelectionState.Points = num2;
							abilitySelectionState.SelectedAbilities = new List<AbilityProgressionTable.UnlockableAbility>();
							AbilitySelectionStates.Add(abilitySelectionState);
						}
					}
					num++;
				}
			}
			levelAbilityPoints.Clear();
		}
		AbilitySelectionStateIndex = 0;
	}

	public void CalculateTalentSelectionStates(Character character)
	{
		RemovePreviousTalentSelections();
		TalentSelectionStates.Clear();
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(character.Class.ToString());
		AbilityProgressionTable table = AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
		for (int i = character.StartingLevel + 1; i <= EndingLevel; i++)
		{
			List<AbilityProgressionTable.AbilityPointUnlock> levelAbilityPoints = abilityProgressionTable.GetLevelAbilityPoints(i);
			foreach (AbilityProgressionTable.AbilityPointUnlock item in levelAbilityPoints)
			{
				int num = 0;
				while (item != null && num < item.CategoryPointPairs.Length)
				{
					if (item.CategoryPointPairs[num].Category == AbilityProgressionTable.CategoryFlag.Talent)
					{
						AbilitySelectionState abilitySelectionState = new AbilitySelectionState();
						abilitySelectionState.Category = item.CategoryPointPairs[num].Category;
						abilitySelectionState.CategoryName = abilityProgressionTable.GetCategoryName(abilitySelectionState.Category);
						abilitySelectionState.PointUnlockDescription = item.CategoryPointPairs[num].UnlockDescription.GetText(character.Gender);
						abilitySelectionState.Points = item.CategoryPointPairs[num].PointsGranted;
						abilitySelectionState.Level = i;
						int num2 = CalculateNumberOfPointsAbleToUseForSelectionState(table, abilitySelectionState, character);
						if (num2 > 0)
						{
							abilitySelectionState.Points = num2;
							abilitySelectionState.SelectedAbilities = new List<AbilityProgressionTable.UnlockableAbility>();
							TalentSelectionStates.Add(abilitySelectionState);
						}
					}
					num++;
				}
			}
			levelAbilityPoints.Clear();
		}
		TalentSelectionStateIndex = 0;
	}

	public void CalculateSpellMasterySelectionStates(Character character)
	{
		CharacterStats targetStats = Instance.TargetCharacter.GetComponent<CharacterStats>();
		int numMasteredAbilities = targetStats.GetNumMasteredAbilities();
		Instance.NumMasteredAbilitiesAllowed = CharacterStats.MaxMasteredAbilitiesAllowed(targetStats.CharacterClass, EndingLevel);
		int num = Instance.NumMasteredAbilitiesAllowed - numMasteredAbilities;
		targetStats.ActiveAbilities.Where((GenericAbility abl) => abl is GenericSpell && SpellMax.Instance.SpellPerEncounterLevelLookup(Instance.ActiveCharacter.Class, (abl as GenericSpell).SpellLevel) <= Instance.EndingLevel && abl.MasteryLevel == 0 && targetStats.FindMasteredAbilityInstance(abl) == null).ToArray();
		for (int i = 0; i < num; i++)
		{
			AbilitySelectionState abilitySelectionState = new AbilitySelectionState();
			abilitySelectionState.Points = 1;
			abilitySelectionState.SelectedAbilities = new List<AbilityProgressionTable.UnlockableAbility>();
			SpellMasterySelectionStates.Add(abilitySelectionState);
		}
		SpellMasterySelectionStateIndex = 0;
	}

	public void CalculateAutoGrantAbilities(Character character)
	{
		AutoGrantedAbilities.Clear();
		int level = character.CoreData.Level;
		character.CoreData.Level = EndingLevel;
		AbilityProgressionTable abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(character.CoreData.Class.ToString());
		if (abilityProgressionTable != null)
		{
			AbilityProgressionTable.UnlockableAbility[] abilities = abilityProgressionTable.GetAbilities(character.CoreData, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
			if (abilities != null)
			{
				AbilityProgressionTable.UnlockableAbility[] array = abilities;
				foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility in array)
				{
					AutoGrantedAbilities.Add(unlockableAbility.Ability);
				}
			}
		}
		AIController component = Instance.TargetCharacter.GetComponent<AIController>();
		if ((bool)component && component.SummonedCreatureList.Count > 0)
		{
			CharacterStats component2 = component.SummonedCreatureList[0].GetComponent<CharacterStats>();
			if ((bool)component2)
			{
				PartyMemberAI component3 = component2.GetComponent<PartyMemberAI>();
				if ((bool)component3 && !component3.IsPet)
				{
					abilityProgressionTable = AbilityProgressionTable.LoadAbilityProgressionTable(component2.CharacterClass.ToString());
					if (abilityProgressionTable != null)
					{
						CharacterStats.CoreData copyOfCoreData = component2.GetCopyOfCoreData();
						copyOfCoreData.Level = EndingLevel;
						AbilityProgressionTable.UnlockableAbility[] abilities2 = abilityProgressionTable.GetAbilities(copyOfCoreData, AbilityProgressionTable.AllCategoryFlags, AbilityProgressionTable.DefaultAutoGrantFilterFlags);
						if (abilities2 != null)
						{
							AbilityProgressionTable.UnlockableAbility[] array = abilities2;
							foreach (AbilityProgressionTable.UnlockableAbility unlockableAbility2 in array)
							{
								AutoGrantedAbilities.Add(unlockableAbility2.Ability);
							}
						}
					}
				}
			}
		}
		character.CoreData.Level = level;
	}

	public void OpenCharacterCreation(CharacterCreationType type, GameObject character, int playerCost, int endingLevel, int endingXp)
	{
		if (UIConversationManager.Instance.WindowActive())
		{
			return;
		}
		if (!base.IsVisible)
		{
			if (!UIWindowManager.Instance.WindowCanShow(this))
			{
				return;
			}
			m_TargetCharacter = character;
			CreationType = type;
			EndingLevel = endingLevel;
			EndingXp = endingXp;
			PlayerCost = playerCost;
			if (CreationType == CharacterCreationType.LevelUp)
			{
				LastStage = 1;
			}
			else
			{
				LastStage = 7;
			}
			if (CreationType != CharacterCreationType.LevelUp)
			{
				UICharacterCustomizeManager.LoadPortraitCache();
				PreloadClassTables();
				m_Loadouts = Resources.Load("Data/Lists/ClassLoadoutList") as EquipmentSetDataList;
				PlayerVoiceSetList playerVoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
				if (playerVoiceSetList != null)
				{
					foreach (PlayerVoiceSetList.VoiceSetPreferedData soundSet in playerVoiceSetList.SoundSets)
					{
						if (soundSet != null && soundSet.PlayerSoundSet != null && soundSet.PlayerSoundSet.DialogOverride != null)
						{
							GameResources.LoadDialogueAudio(soundSet.PlayerSoundSet.DialogOverride.Filename);
						}
					}
				}
			}
			else
			{
				AbilityProgressionTable.LoadAbilityProgressionTable("Talents");
				CharacterStats component = character.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					AbilityProgressionTable.LoadAbilityProgressionTable(component.CharacterClass.ToString());
				}
			}
			AbilitySelectionStateIndex = 0;
			TalentSelectionStateIndex = 0;
			SpellMasterySelectionStateIndex = 0;
			m_CurrentStage = 0;
			ShowWindow();
			if (CreationType == CharacterCreationType.NewPlayer)
			{
				StartCoroutine(FadeOutAfterDelay());
			}
			else
			{
				FadeManager.Instance.CancelFade(FadeManager.FadeType.AreaTransition);
				FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
			}
			m_closing = false;
			if (CreationType == CharacterCreationType.NewPlayer)
			{
				MusicManager.Instance.PlayScriptedMusic("mus_final\\mus_global_character_creation", blockCombatMusic: false, MusicManager.FadeType.FadeOutFadeIn, 0.4f, 0.5f, 0f, loop: true);
			}
		}
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -5f);
		HandleDoneVisibility(IsCharacterCreationReadyForCompletion());
		if ((bool)ExitButton)
		{
			ExitButton.SetActive(value: true);
		}
	}

	private IEnumerator FadeOutAfterDelay()
	{
		FadeManager.Instance.CancelFade(FadeManager.FadeType.AreaTransition);
		FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0f);
		int i = 0;
		while (i < 2)
		{
			yield return null;
			int num = i + 1;
			i = num;
		}
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f);
	}

	public int GetRemainingAttributePoints(Character activeCharacter)
	{
		int num = 0;
		for (int i = 0; i < activeCharacter.BaseStats.Length; i++)
		{
			num += activeCharacter.BaseStats[i] - BaseStat;
		}
		return Instance.TotalPointBuy - num;
	}

	public bool AllowIncStat(Character activeCharacter, CharacterStats.AttributeScoreType stat)
	{
		if (GetRemainingAttributePoints(activeCharacter) <= 0 || activeCharacter.BaseStats[(int)stat] >= StatHardMaximum)
		{
			return false;
		}
		return true;
	}

	public bool AllowDecStat(Character activeCharacter, CharacterStats.AttributeScoreType stat)
	{
		if (activeCharacter.BaseStats[(int)stat] <= StatHardMinimum)
		{
			return false;
		}
		return true;
	}

	private void Awake()
	{
		s_Instance = this;
		UICharacterCreationController[] array = UnityEngine.Object.FindObjectsOfType(typeof(UICharacterCreationController)) as UICharacterCreationController[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
	}

	protected override void OnDestroy()
	{
		m_Loadouts = null;
		m_TargetCharacter = null;
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (m_Controllers != null)
		{
			m_Controllers.Clear();
		}
		if (AbilitySelectionStates != null)
		{
			foreach (AbilitySelectionState abilitySelectionState in AbilitySelectionStates)
			{
				abilitySelectionState.SelectedAbilities.Clear();
			}
			AbilitySelectionStates.Clear();
		}
		if (TalentSelectionStates != null)
		{
			foreach (AbilitySelectionState talentSelectionState in TalentSelectionStates)
			{
				talentSelectionState.SelectedAbilities.Clear();
			}
			TalentSelectionStates.Clear();
		}
		if (SpellMasterySelectionStates != null)
		{
			foreach (AbilitySelectionState spellMasterySelectionState in SpellMasterySelectionStates)
			{
				spellMasterySelectionState.SelectedAbilities.Clear();
			}
			SpellMasterySelectionStates.Clear();
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(CharacterRenderZone.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPaperdoll));
	}

	private void OnDragPaperdoll(GameObject go, Vector2 disp)
	{
		m_PaperdollAngle += disp.x * 0.5f;
		PE_Paperdoll.LookAt(m_PaperdollAngle);
	}

	protected override bool Hide(bool forced)
	{
		PE_Paperdoll.DisableCamera();
		return true;
	}

	protected override void Show()
	{
		GameObject[] startsDisabled = StartsDisabled;
		for (int i = 0; i < startsDisabled.Length; i++)
		{
			startsDisabled[i].SetActive(value: false);
		}
		switch (CreationType)
		{
		case CharacterCreationType.NewPlayer:
			PE_Paperdoll.CreateCameraCharacterCreation();
			break;
		case CharacterCreationType.NewCompanion:
			PE_Paperdoll.CreateCameraRecruitment();
			break;
		case CharacterCreationType.LevelUp:
			PE_Paperdoll.CreateCameraLevelUp();
			break;
		}
		PE_Paperdoll.SetRenderSize(new Rect(0f, 0f, CharacterRenderZone.transform.localScale.x, CharacterRenderZone.transform.localScale.y));
		CharacterRenderZone.mainTexture = PE_Paperdoll.RenderImage;
		RootController = RootControllers[(int)CreationType];
		CalculateAllControllers();
		if (CreationType == CharacterCreationType.NewPlayer || CreationType == CharacterCreationType.NewCompanion)
		{
			CharacterStats component = m_TargetCharacter.GetComponent<CharacterStats>();
			component.BaseConstitution = BaseStat;
			component.BaseMight = BaseStat;
			component.BaseIntellect = BaseStat;
			component.BaseDexterity = BaseStat;
			component.BasePerception = BaseStat;
			component.BaseResolve = BaseStat;
		}
		RootController.Character.GetFrom(m_TargetCharacter);
		PE_Paperdoll.LoadCharacter(m_TargetCharacter);
		if (CreationType != 0)
		{
			GameObject obj = UnityEngine.Object.Instantiate(CharacterPlatform);
			obj.transform.parent = PE_Paperdoll.PaperdollCharacter.transform;
			GameUtilities.RecursiveSetLayer(obj, PE_Paperdoll.PaperdollLayer);
		}
		RootController.Activate();
		RootController.Show();
		Instance.CalculateAbilitySelectionStates(ActiveCharacter);
		if (CreationType == CharacterCreationType.LevelUp)
		{
			SetLastPickedSubraceForRace(ActiveCharacter.Race, ActiveCharacter.Subrace);
			Instance.CalculateTalentSelectionStates(ActiveCharacter);
			Instance.CalculateSpellMasterySelectionStates(ActiveCharacter);
			if (RootController.Character.CoreData.Level > 0)
			{
				RootController.Character.SkillPointsToSpend += 6;
			}
		}
		else
		{
			ActiveCharacter.Gender = ((OEIRandom.DieRoll(2) != 1) ? Gender.Female : Gender.Male);
			for (int j = 0; j < UICharacterCreationEnumSetter.ValidRaces.Length; j++)
			{
				LastPickedSubrace.Add(UICharacterCreationEnumSetter.ValidRaces[j], UICharacterCreationEnumSetter.ValidSubracesByRace[(int)UICharacterCreationEnumSetter.ValidRaces[j]][OEIRandom.Index(UICharacterCreationEnumSetter.ValidSubracesByRace[(int)UICharacterCreationEnumSetter.ValidRaces[j]].Length)]);
			}
			ActiveCharacter.Subrace = LastPickedSubrace[CharacterStats.Race.Human];
			ActiveCharacter.Class = CharacterStats.Class.Fighter;
			ActiveCharacter.Culture = CharacterStats.Culture.Undefined;
			ActiveCharacter.SetDefaultAppearance();
		}
		RootController.SignalValueChanged(UICharacterCreationElement.ValueType.All);
		Instance.CalculateAutoGrantAbilities(ActiveCharacter);
		SelectNextController();
		m_startingControllerIndex = m_controllerIndex;
		CharacterCreationBackground.SetPath(Backgrounds[(int)CreationType]);
	}

	public CharacterStats.Subrace GetLastPickedSubraceForRace(CharacterStats.Race pickedRace)
	{
		if (LastPickedSubrace != null && LastPickedSubrace.ContainsKey(pickedRace))
		{
			return LastPickedSubrace[pickedRace];
		}
		return UICharacterCreationEnumSetter.ValidSubracesByRace[(int)pickedRace][0];
	}

	public void SetLastPickedSubraceForRace(CharacterStats.Race pickedRace, CharacterStats.Subrace newSubrace)
	{
		if (LastPickedSubrace != null)
		{
			if (LastPickedSubrace.ContainsKey(pickedRace))
			{
				LastPickedSubrace[pickedRace] = newSubrace;
			}
			else
			{
				LastPickedSubrace.Add(pickedRace, newSubrace);
			}
		}
	}

	public void PreloadClassTables()
	{
		AbilityProgressionTable.LoadAbilityProgressionTable("Barbarian");
		AbilityProgressionTable.LoadAbilityProgressionTable("Cipher");
		AbilityProgressionTable.LoadAbilityProgressionTable("Chanter");
		AbilityProgressionTable.LoadAbilityProgressionTable("Fighter");
		AbilityProgressionTable.LoadAbilityProgressionTable("Ranger");
		AbilityProgressionTable.LoadAbilityProgressionTable("Druid");
		AbilityProgressionTable.LoadAbilityProgressionTable("Wizard");
		AbilityProgressionTable.LoadAbilityProgressionTable("Paladin");
		AbilityProgressionTable.LoadAbilityProgressionTable("Priest");
		AbilityProgressionTable.LoadAbilityProgressionTable("Rogue");
		AbilityProgressionTable.LoadAbilityProgressionTable("Monk");
	}

	private void CalculateAllControllers()
	{
		m_Controllers.Clear();
		m_Controllers.Add(RootController);
		UICharacterCreationStage[] componentsInChildren = RootController.GetComponentsInChildren<UICharacterCreationStage>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			m_Controllers.Add(null);
		}
		UICharacterCreationStage[] array = componentsInChildren;
		foreach (UICharacterCreationStage uICharacterCreationStage in array)
		{
			m_Controllers[uICharacterCreationStage.Stage + 1] = uICharacterCreationStage.ShowController;
		}
		for (int k = 0; k < m_Controllers.Count; k++)
		{
			if (GetController(k).SucceededBy != null)
			{
				m_Controllers.Insert(k + 1, GetController(k).SucceededBy);
			}
		}
	}

	private void CancelCharacterCreation()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(CancelCharacterCreation));
		m_closing = false;
		if (CreationType != CharacterCreationType.LevelUp)
		{
			PlayerVoiceSetList playerVoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
			if (playerVoiceSetList != null)
			{
				foreach (PlayerVoiceSetList.VoiceSetPreferedData soundSet in playerVoiceSetList.SoundSets)
				{
					if (soundSet != null && soundSet.PlayerSoundSet != null && soundSet.PlayerSoundSet.DialogOverride != null)
					{
						GameResources.UnloadDialogueAudio(soundSet.PlayerSoundSet.DialogOverride.Filename);
					}
				}
			}
			GameResources.ClearPrefabReference(PlayerVoiceSetList.DefaultPlayerSoundSetList);
		}
		UICharacterCreationEnumSetter[] componentsInChildren = GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
		if (componentsInChildren != null)
		{
			UICharacterCreationEnumSetter[] array = componentsInChildren;
			foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter in array)
			{
				if ((bool)uICharacterCreationEnumSetter)
				{
					uICharacterCreationEnumSetter.FreeData();
				}
			}
		}
		if (CreationType == CharacterCreationType.NewPlayer)
		{
			MusicManager.Instance.FadeOutAreaMusic(resetWhenFaded: true);
			Debug.Log("\n");
			Debug.Log("------- LOAD TO MAIN MENU --------\n\n");
			SceneManager.LoadScene("MainMenu");
			return;
		}
		if (CreationType == CharacterCreationType.NewCompanion)
		{
			Persistence component = m_TargetCharacter.GetComponent<Persistence>();
			if ((bool)component)
			{
				PersistenceManager.RemoveObject(component);
			}
			GameUtilities.Destroy(m_TargetCharacter);
		}
		HideWindow();
		UIWindowManager.Instance.RecreateWindow(typeof(UICharacterCreationManager));
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f, AudioFadeMode.MusicAndFx);
		Resources.UnloadUnusedAssets();
	}

	private void PushGrimoireSpells()
	{
		Equipment component = m_TargetCharacter.GetComponent<Equipment>();
		if (!(component != null) || component.CurrentItems == null || !component.CurrentItems.Grimoire)
		{
			return;
		}
		Grimoire component2 = component.CurrentItems.Grimoire.GetComponent<Grimoire>();
		foreach (GenericSpell item in from spell in m_TargetCharacter.GetComponent<CharacterStats>().ActiveAbilities.OfType<GenericSpell>()
			where !spell.ProhibitFromGrimoire && spell.SpellClass == CharacterStats.Class.Wizard
			select spell)
		{
			GenericSpell[] spellData = component2.Spells[item.SpellLevel - 1].SpellData;
			if (!item || spellData.Contains(item, GenericAbility.NameComparer.Instance))
			{
				continue;
			}
			GenericSpell element = item;
			Persistence component3 = item.GetComponent<Persistence>();
			if ((bool)component3)
			{
				GameObject gameObject = GameResources.LoadPrefab(component3.Prefab, instantiate: false) as GameObject;
				if ((bool)gameObject)
				{
					element = gameObject.GetComponent<GenericSpell>();
				}
			}
			spellData.PushIfSpace(element);
		}
	}

	private void CloseCharacterCreation()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(CloseCharacterCreation));
		CharacterStats component = m_TargetCharacter.GetComponent<CharacterStats>();
		bool reloadEquipment = true;
		RootCharacter.ApplyTo(m_TargetCharacter, reloadEquipment, UICharacterCreationElement.ValueType.All);
		component.HandleAttackMeleeAttachment(removeExisting: true);
		if (CreationType != CharacterCreationType.LevelUp)
		{
			PlayerVoiceSetList playerVoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
			if (playerVoiceSetList != null)
			{
				foreach (PlayerVoiceSetList.VoiceSetPreferedData soundSet in playerVoiceSetList.SoundSets)
				{
					if (soundSet != null && soundSet.PlayerSoundSet != null && soundSet.PlayerSoundSet.DialogOverride != null)
					{
						GameResources.UnloadDialogueAudio(soundSet.PlayerSoundSet.DialogOverride.Filename);
					}
				}
			}
			GameResources.ClearPrefabReference(PlayerVoiceSetList.DefaultPlayerSoundSetList);
			object component2 = component;
			DataManager.AdjustFromData(ref component2);
			component.ClearAllAbilities();
			component.RefreshAllAbilities();
			if ((bool)component)
			{
				component.Experience = EndingXp;
				component.LevelUpSingleLevel();
			}
			PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
			if ((bool)inventory)
			{
				inventory.currencyTotalValue.v -= PlayerCost;
			}
			if (CreationType == CharacterCreationType.NewPlayer)
			{
				m_TargetCharacter.name = "Player_" + RootCharacter.Name;
			}
			else if (CreationType == CharacterCreationType.NewCompanion)
			{
				m_TargetCharacter.name = "Companion_P_" + RootCharacter.Name;
			}
		}
		else if (CreationType == CharacterCreationType.LevelUp)
		{
			Health component3 = m_TargetCharacter.GetComponent<Health>();
			float num = 0f;
			if ((bool)component3)
			{
				num = component3.MaxHealth - component3.CurrentHealth;
			}
			foreach (GenericAbility activeAbility in component.ActiveAbilities)
			{
				if (activeAbility.Passive && activeAbility.HasLevelScalingStatusEffect)
				{
					activeAbility.ForceDeactivate(activeAbility.Owner);
				}
			}
			component.Experience = EndingXp;
			component.LevelUpSingleLevel();
			component.StealthSkill += RootCharacter.SkillValueDeltas[0];
			component.AthleticsSkill += RootCharacter.SkillValueDeltas[1];
			component.LoreSkill += RootCharacter.SkillValueDeltas[2];
			component.MechanicsSkill += RootCharacter.SkillValueDeltas[3];
			component.SurvivalSkill += RootCharacter.SkillValueDeltas[4];
			if ((bool)component3)
			{
				component3.CurrentHealth = component3.MaxHealth - num;
			}
			GameObject gameObject = GameUtilities.FindAnimalCompanion(component.gameObject);
			if ((bool)gameObject)
			{
				CharacterStats component4 = gameObject.GetComponent<CharacterStats>();
				if ((bool)component4)
				{
					component4.LevelUpToLevel(component.Level);
				}
			}
			UICharacterSheetManager.Instance.RefreshCharacter();
		}
		foreach (AbilitySelectionState abilitySelectionState in AbilitySelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
			{
				if ((bool)component)
				{
					AbilityProgressionTable.AddAbilityToCharacter(selectedAbility, component);
				}
			}
		}
		foreach (AbilitySelectionState talentSelectionState in TalentSelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility2 in talentSelectionState.SelectedAbilities)
			{
				if ((bool)component)
				{
					AbilityProgressionTable.AddAbilityToCharacter(selectedAbility2, component);
				}
			}
		}
		foreach (AbilitySelectionState spellMasterySelectionState in SpellMasterySelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility3 in spellMasterySelectionState.SelectedAbilities)
			{
				if ((bool)component)
				{
					GenericAbility genericAbility = AbilityProgressionTable.GetGenericAbility(selectedAbility3.Ability);
					GenericAbility.MasterAbility(component, genericAbility);
				}
			}
		}
		PushGrimoireSpells();
		if (CreationType != CharacterCreationType.LevelUp)
		{
			Equipment component5 = m_TargetCharacter.GetComponent<Equipment>();
			if (component5 != null && component5.CurrentItems != null && (bool)component5.CurrentItems.Grimoire)
			{
				component5.CurrentItems.Grimoire.GetComponent<Grimoire>().PrimaryOwnerName = RootCharacter.Name;
			}
		}
		if (CreationType != CharacterCreationType.LevelUp || (ActiveCharacter.StartingLevel == 0 && CreationType == CharacterCreationType.LevelUp))
		{
			IEnumerable<GenericAbility> source = component.ActiveAbilities.Where((GenericAbility abil) => abil is ChanterTrait);
			if (source.Any())
			{
				Chant chant = component.InstantiateAbility(UIChantEditor.Instance.EmptyChantPrefab, GenericAbility.AbilityType.Ability) as Chant;
				Phrase[] knownPhrases = (source.First() as ChanterTrait).GetKnownPhrases();
				chant.Phrases = new Phrase[knownPhrases.Length];
				Array.Copy(knownPhrases, chant.Phrases, Math.Min(knownPhrases.Length, 10));
				chant.OverrideName = GUIUtils.Format(1632, 1);
				chant.InstantiatePhrases();
			}
			foreach (GenericAbility activeAbility2 in component.ActiveAbilities)
			{
				Summon component6 = activeAbility2.GetComponent<Summon>();
				if (component6 != null)
				{
					component6.CreatureName = RootCharacter.Animal_Name;
				}
			}
		}
		if (CreationType == CharacterCreationType.NewPlayer)
		{
			StringTableManager.PlayerGender = component.Gender;
			MusicManager.Instance.EndScriptedMusic();
		}
		UICharacterCustomizeManager.ClearPortraitCache();
		if (CreationType == CharacterCreationType.NewCompanion && (bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumAdventuresCreated);
		}
		UICharacterCreationEnumSetter[] componentsInChildren = GetComponentsInChildren<UICharacterCreationEnumSetter>(includeInactive: true);
		if (componentsInChildren != null)
		{
			UICharacterCreationEnumSetter[] array = componentsInChildren;
			foreach (UICharacterCreationEnumSetter uICharacterCreationEnumSetter in array)
			{
				if ((bool)uICharacterCreationEnumSetter)
				{
					uICharacterCreationEnumSetter.FreeData();
				}
			}
		}
		HideWindow();
		UIWindowManager.Instance.RecreateWindow(typeof(UICharacterCreationManager));
		if (CreationType == CharacterCreationType.NewCompanion && (bool)m_TargetCharacter.GetComponent<PartyMemberAI>())
		{
			if (PartyMemberAI.NextAvailablePrimarySlot >= 0)
			{
				PartyMemberAI partyMemberAI = PartyMemberAI.AddToActiveParty(m_TargetCharacter, fromScript: false);
				if (partyMemberAI != null)
				{
					partyMemberAI.gameObject.transform.position = GameUtilities.NearestUnoccupiedLocation(partyMemberAI.gameObject.transform.position, partyMemberAI.Mover.Radius, 20f, partyMemberAI.Mover);
				}
			}
			else
			{
				GameState.Stronghold.StoreCompanion(m_TargetCharacter);
				UIPartyManager.Instance.ToggleAlt();
			}
		}
		if (CreationType == CharacterCreationType.NewPlayer)
		{
			ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnCharacterCreationClosed);
			BackerUnlockTracker.Instance.Activate();
		}
		else if (CreationType == CharacterCreationType.LevelUp)
		{
			FadeManager instance2 = FadeManager.Instance;
			instance2.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance2.OnFadeEnded, new FadeManager.OnFadeEnd(TriggerLevelUpTutorial));
		}
		if (CreationType == CharacterCreationType.NewPlayer)
		{
			if (Conditionals.CommandLineArg("bb") || !GameState.Instance.CurrentMap.SceneName.ToLower().Contains("encampment"))
			{
				FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f, AudioFadeMode.MusicAndFx);
			}
		}
		else
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, 0.75f, AudioFadeMode.MusicAndFx);
		}
		GameResources.ClearPrefabReferences(typeof(AbilityProgressionTable));
		if (CreationType != 0)
		{
			Resources.UnloadUnusedAssets();
		}
		m_closing = false;
	}

	private void TriggerLevelUpTutorial()
	{
		TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.LEVEL_UP_COMPLETE);
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TriggerLevelUpTutorial));
	}

	public void HandleCharacterCreationCancel()
	{
		if (!m_closing)
		{
			AudioFadeMode audioMode = ((CreationType == CharacterCreationType.NewPlayer) ? AudioFadeMode.Music : AudioFadeMode.None);
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(CancelCharacterCreation));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0.75f, audioMode);
			m_closing = true;
			XboxOneNativeWrapper.Instance.SetPresence("main_menu");
		}
	}

	public void HandleLevelLooping()
	{
		int num = EndingLevel - 1;
		Health component = TargetCharacter.GetComponent<Health>();
		float num2 = 0f;
		if ((bool)component)
		{
			num2 = component.MaxHealth - component.CurrentHealth;
		}
		CharacterStats component2 = TargetCharacter.GetComponent<CharacterStats>();
		foreach (GenericAbility activeAbility in component2.ActiveAbilities)
		{
			if (activeAbility.Passive && activeAbility.HasLevelScalingStatusEffect)
			{
				activeAbility.ForceDeactivate(activeAbility.Owner);
			}
		}
		component2.Experience = EndingXp;
		component2.LevelUpSingleLevel();
		component2.StealthSkill += RootCharacter.SkillValueDeltas[0];
		component2.AthleticsSkill += RootCharacter.SkillValueDeltas[1];
		component2.LoreSkill += RootCharacter.SkillValueDeltas[2];
		component2.MechanicsSkill += RootCharacter.SkillValueDeltas[3];
		component2.SurvivalSkill += RootCharacter.SkillValueDeltas[4];
		ActiveCharacter.StartingLevel = EndingLevel;
		if ((bool)component)
		{
			component.CurrentHealth -= num2;
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(component2.gameObject);
		if ((bool)gameObject)
		{
			CharacterStats component3 = gameObject.GetComponent<CharacterStats>();
			if ((bool)component3)
			{
				component3.LevelUpToLevel(component2.Level);
			}
		}
		EndingLevel++;
		component2.RemainingSkillPoints = ActiveCharacter.SkillPointsToSpend;
		foreach (AbilitySelectionState abilitySelectionState in AbilitySelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
			{
				if ((bool)component2)
				{
					AbilityProgressionTable.AddAbilityToCharacter(selectedAbility, component2, executeSummons: true);
				}
			}
		}
		foreach (AbilitySelectionState talentSelectionState in TalentSelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility2 in talentSelectionState.SelectedAbilities)
			{
				if ((bool)component2)
				{
					AbilityProgressionTable.AddAbilityToCharacter(selectedAbility2, component2);
				}
			}
		}
		foreach (AbilitySelectionState spellMasterySelectionState in SpellMasterySelectionStates)
		{
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility3 in spellMasterySelectionState.SelectedAbilities)
			{
				if ((bool)component2)
				{
					GenericAbility genericAbility = AbilityProgressionTable.GetGenericAbility(selectedAbility3.Ability);
					if ((bool)genericAbility)
					{
						GenericAbility.MasterAbility(component2, genericAbility);
					}
				}
			}
		}
		RootCharacter.ApplyTo(m_TargetCharacter, reloadEquipment: false, UICharacterCreationElement.ValueType.All);
		RootController.Character.GetFrom(m_TargetCharacter);
		RootController.Character.SkillPointsToSpend += 6;
		AbilitySelectionStates.Clear();
		TalentSelectionStates.Clear();
		SpellMasterySelectionStates.Clear();
		CalculateAllControllers();
		AbilitySelectionStateIndex = 0;
		TalentSelectionStateIndex = 0;
		SpellMasterySelectionStateIndex = 0;
		for (int i = 0; i < RootCharacter.SkillValueDeltas.Length; i++)
		{
			RootCharacter.SkillValueDeltas[i] = 0;
			RootCharacter.SkillRankDeltas[i] = 0;
		}
		Instance.CalculateAbilitySelectionStates(ActiveCharacter);
		Instance.CalculateTalentSelectionStates(ActiveCharacter);
		Instance.CalculateSpellMasterySelectionStates(ActiveCharacter);
		RootController.SignalValueChanged(UICharacterCreationElement.ValueType.All);
		Instance.CalculateAutoGrantAbilities(ActiveCharacter);
		UICharacterCreationStage[] componentsInChildren = GetComponentsInChildren<UICharacterCreationStage>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			TweenPosition componentInChildren = componentsInChildren[j].GetComponentInChildren<TweenPosition>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Play(forward: false);
			}
		}
		PushGrimoireSpells();
		if (num == 0 && CreationType == CharacterCreationType.LevelUp)
		{
			IEnumerable<GenericAbility> source = component2.ActiveAbilities.Where((GenericAbility abil) => abil is ChanterTrait);
			if (source.Any())
			{
				Chant chant = component2.InstantiateAbility(UIChantEditor.Instance.EmptyChantPrefab, GenericAbility.AbilityType.Ability) as Chant;
				Phrase[] knownPhrases = (source.First() as ChanterTrait).GetKnownPhrases();
				chant.Phrases = new Phrase[knownPhrases.Length];
				Array.Copy(knownPhrases, chant.Phrases, Math.Min(knownPhrases.Length, 10));
				chant.OverrideName = GUIUtils.Format(1632, 1);
				chant.InstantiatePhrases();
			}
			foreach (GenericAbility activeAbility2 in component2.ActiveAbilities)
			{
				Summon component4 = activeAbility2.GetComponent<Summon>();
				if (component4 != null)
				{
					component4.CreatureName = RootCharacter.Animal_Name;
				}
			}
		}
		CurrentStage = 0;
		SetActiveController(m_Controllers[0]);
		SelectNextController();
		m_startingControllerIndex = m_controllerIndex;
		RefreshNextButtons();
	}

	private void OnSpellPerEncounterLevelUpDialogConfirmed(UIMessageBox.Result result, UIMessageBox sender)
	{
		CharacterStats component = TargetCharacter.GetComponent<CharacterStats>();
		sender.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Remove(sender.OnDialogEnd, new UIMessageBox.OnEndDialog(OnSpellPerEncounterLevelUpDialogConfirmed));
		if (!(component == null))
		{
			if (component.GetMaxLevelCanLevelUpTo() > EndingLevel)
			{
				HandleLevelLooping();
			}
			else
			{
				StartClosingSequence();
			}
		}
	}

	private void StartClosingSequence()
	{
		if (!m_closing)
		{
			AudioFadeMode audioMode = ((CreationType == CharacterCreationType.NewPlayer) ? AudioFadeMode.Music : AudioFadeMode.None);
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.FinalSelect);
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(CloseCharacterCreation));
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, 0.75f, audioMode);
			m_closing = true;
		}
	}

	public void HandleCharacterCreationComplete()
	{
		if (CreationType == CharacterCreationType.LevelUp)
		{
			CharacterStats component = TargetCharacter.GetComponent<CharacterStats>();
			if (component == null)
			{
				return;
			}
			if (component.GetMaxLevelCanLevelUpTo() > EndingLevel)
			{
				HandleLevelLooping();
				return;
			}
		}
		StartClosingSequence();
		XboxOneNativeWrapper.Instance.SetPresence(SceneManager.GetActiveScene().name.ToLowerInvariant());
	}

	public bool IsCharacterCreationReadyForCompletion()
	{
		foreach (UICharacterCreationController controller in m_Controllers)
		{
			if (!controller.ReadyForNextStage())
			{
				return false;
			}
		}
		return true;
	}

	public void HandleDoneVisibility(bool enabled)
	{
		if (!DoneButton)
		{
			return;
		}
		UIMultiSpriteImageButton component = DoneButton.GetComponent<UIMultiSpriteImageButton>();
		CharacterStats component2 = TargetCharacter.GetComponent<CharacterStats>();
		UILabel[] componentsInChildren = DoneButton.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			if (uILabel.name == "LevelLabel")
			{
				if (CreationType == CharacterCreationType.LevelUp && component2.GetMaxLevelCanLevelUpTo() != EndingLevel)
				{
					component.Label = uILabel;
				}
				uILabel.gameObject.SetActive(CreationType == CharacterCreationType.LevelUp && component2.GetMaxLevelCanLevelUpTo() != EndingLevel);
			}
			else
			{
				if (CreationType != CharacterCreationType.LevelUp || component2.GetMaxLevelCanLevelUpTo() == EndingLevel)
				{
					component.Label = uILabel;
				}
				uILabel.gameObject.SetActive(CreationType != CharacterCreationType.LevelUp || component2.GetMaxLevelCanLevelUpTo() == EndingLevel);
			}
		}
		if ((bool)component)
		{
			component.enabled = enabled;
		}
	}

	public void EndStage()
	{
		UICharacterCreationController currentController = GetCurrentController();
		if (currentController != null)
		{
			if (currentController.ReadyForNextStage())
			{
				if (currentController.Type == UICharacterCreationController.ControllerType.ABILITIES)
				{
					foreach (AbilitySelectionState abilitySelectionState in Instance.AbilitySelectionStates)
					{
						foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
						{
							AbilityProgressionTable.AddAbilityToCoreData(selectedAbility.Ability, ActiveCharacter.CoreData);
						}
					}
				}
				if (currentController.Type == UICharacterCreationController.ControllerType.TALENTS)
				{
					foreach (AbilitySelectionState talentSelectionState in Instance.TalentSelectionStates)
					{
						foreach (AbilityProgressionTable.UnlockableAbility selectedAbility2 in talentSelectionState.SelectedAbilities)
						{
							AbilityProgressionTable.AddAbilityToCoreData(selectedAbility2.Ability, ActiveCharacter.CoreData);
						}
					}
				}
				currentController.OnNextButtonPressed();
				if (currentController.ShouldAdvanceInternal())
				{
					currentController.AdvanceInternal();
					SetSubtitle(currentController.GetSubTitle());
				}
				else
				{
					SelectNextController();
				}
				RefreshNextButtons();
			}
		}
		else
		{
			HandleCharacterCreationComplete();
		}
		HandleDoneVisibility(IsCharacterCreationReadyForCompletion());
	}

	public void PressBack()
	{
		if (m_controllerIndex > 1)
		{
			int num = m_controllerIndex;
			int num2 = num;
			UICharacterCreationController controller = GetController(m_controllerIndex);
			while (num >= 0)
			{
				if (controller.Type == UICharacterCreationController.ControllerType.ABILITIES && AbilitySelectionStateIndex > 0)
				{
					AbilitySelectionStateIndex = ((num != num2) ? (AbilitySelectionStates.Count - 1) : (AbilitySelectionStateIndex - 1));
					break;
				}
				if (controller.Type == UICharacterCreationController.ControllerType.TALENTS && TalentSelectionStateIndex > 0)
				{
					TalentSelectionStateIndex = ((num != num2) ? (TalentSelectionStates.Count - 1) : (TalentSelectionStateIndex - 1));
					break;
				}
				if (controller.Type == UICharacterCreationController.ControllerType.SPELL_MASTERY && SpellMasterySelectionStateIndex > 0)
				{
					SpellMasterySelectionStateIndex = ((num != num2) ? (SpellMasterySelectionStates.Count - 1) : (SpellMasterySelectionStateIndex - 1));
					break;
				}
				if (num != num2 && !controller.ShouldSkip())
				{
					break;
				}
				num--;
				controller = GetController(num);
			}
			SetActiveController(controller);
		}
		RefreshNextButtons();
	}

	public void PressOkay()
	{
		EndStage();
	}

	public bool CanGoBack()
	{
		UICharacterCreationController currentController = GetCurrentController();
		if (m_controllerIndex <= m_startingControllerIndex)
		{
			if ((bool)currentController && currentController.Type == UICharacterCreationController.ControllerType.SPELL_MASTERY)
			{
				return SpellMasterySelectionStateIndex > 0;
			}
			return false;
		}
		return true;
	}

	public void RefreshNextButtons()
	{
		if (!(GetCurrentController() != null))
		{
			return;
		}
		UICharacterCreationNavControl[] componentsInChildren = GetComponentsInChildren<UICharacterCreationNavControl>(includeInactive: true);
		foreach (UICharacterCreationNavControl uICharacterCreationNavControl in componentsInChildren)
		{
			uICharacterCreationNavControl.Refresh();
			if (uICharacterCreationNavControl.Control != 0)
			{
				continue;
			}
			UICharacterCreationController componentInParent = uICharacterCreationNavControl.GetComponentInParent<UICharacterCreationController>();
			if (!componentInParent || !(GetCurrentController() != componentInParent))
			{
				UIMultiSpriteImageButton spriteButton = uICharacterCreationNavControl.SpriteButton;
				if ((bool)spriteButton)
				{
					spriteButton.enabled = GetCurrentController().ReadyForNextStage();
				}
			}
		}
	}

	public void SignalValueChanged(UICharacterCreationElement.ValueType valueType)
	{
		UICharacterCreationController currentController = GetCurrentController();
		if ((bool)currentController && (bool)currentController.DescriptionScreen)
		{
			UIDraggablePanel[] componentsInChildren = currentController.DescriptionScreen.GetComponentsInChildren<UIDraggablePanel>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ResetPosition();
			}
		}
		UICharacterCreationElement[] componentsInChildren2 = GetComponentsInChildren<UICharacterCreationElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].SignalValueChanged(valueType);
		}
		RefreshCharacter(valueType);
		RefreshNextButtons();
		HandleDoneVisibility(IsCharacterCreationReadyForCompletion());
	}

	public void RefreshCharacter(UICharacterCreationElement.ValueType valueType)
	{
		bool reloadEquipment = false;
		CharacterStats.Class @class = ((ActiveCharacter.Class == CharacterStats.Class.Undefined) ? CharacterStats.Class.Fighter : ActiveCharacter.Class);
		CharacterStats.Culture culture = ((UICharacterCreationEnumSetter.s_PendingCulture == CharacterStats.Culture.Undefined) ? CharacterStats.Culture.Aedyr : UICharacterCreationEnumSetter.s_PendingCulture);
		if (!PE_Paperdoll.PaperdollCharacter)
		{
			return;
		}
		PE_Paperdoll.HandleAlternateModelDisable();
		if (CreationType != CharacterCreationType.LevelUp)
		{
			EquipmentSetData[] equipmentSets = m_Loadouts.EquipmentSets;
			foreach (EquipmentSetData equipmentSetData in equipmentSets)
			{
				if ((equipmentSetData.Class == @class || equipmentSetData.Class == CharacterStats.Class.Undefined) && (equipmentSetData.Culture == culture || equipmentSetData.Culture == CharacterStats.Culture.Undefined) && ActiveCharacter.ClassEquipmentSetData != equipmentSetData)
				{
					MirrorCharacterUtils.LoadEquipment(PE_Paperdoll.PaperdollCharacter, equipmentSetData.Equipment);
					ActiveCharacter.ClassEquipmentSetData = equipmentSetData;
					reloadEquipment = true;
				}
			}
		}
		ActiveCharacter.ApplyTo(PE_Paperdoll.PaperdollCharacter, reloadEquipment, valueType);
		PE_Paperdoll.HandleAlternateModelEnable(PE_Paperdoll.GetAlternateModel(m_TargetCharacter));
	}

	public void SetAlternatePaperDollModel()
	{
		PE_Paperdoll.s_ModelOverride = null;
		foreach (AbilitySelectionState abilitySelectionState in Instance.AbilitySelectionStates)
		{
			if (abilitySelectionState == null)
			{
				continue;
			}
			foreach (AbilityProgressionTable.UnlockableAbility selectedAbility in abilitySelectionState.SelectedAbilities)
			{
				if (selectedAbility.Ability.GetComponent<Spiritshift>() != null)
				{
					PE_Paperdoll.s_ModelOverride = selectedAbility.Ability.GetComponent<Spiritshift>().form;
					return;
				}
			}
		}
	}

	public void RefreshAll()
	{
		foreach (UICharacterCreationController controller in m_Controllers)
		{
			controller.SignalValueChanged(UICharacterCreationElement.ValueType.All);
		}
	}

	public void SelectNextController()
	{
		if (!(GetCurrentController() == null))
		{
			int num = m_controllerIndex + 1;
			UICharacterCreationController controller = GetController(num);
			while ((bool)controller && controller.ShouldSkip())
			{
				controller.HandleSkip();
				controller = GetController(++num);
			}
			if (controller != null && controller.Type == UICharacterCreationController.ControllerType.ABILITIES)
			{
				AbilitySelectionStateIndex = 0;
			}
			else if (controller != null && controller.Type == UICharacterCreationController.ControllerType.TALENTS)
			{
				TalentSelectionStateIndex = 0;
			}
			else if (controller != null && controller.Type == UICharacterCreationController.ControllerType.SPELL_MASTERY)
			{
				SpellMasterySelectionStateIndex = 0;
			}
			SetActiveController(controller);
		}
	}

	public void AdvanceToLatestControllerUpTo(UICharacterCreationController newController)
	{
		UICharacterCreationController uICharacterCreationController = null;
		for (int i = 0; i <= m_Controllers.Count; i++)
		{
			uICharacterCreationController = GetController(i);
			if (uICharacterCreationController == newController || !uICharacterCreationController.ReadyForNextStage() || uICharacterCreationController.ShouldAdvanceInternal())
			{
				break;
			}
		}
		if (uICharacterCreationController != null)
		{
			SetActiveController(uICharacterCreationController);
		}
	}

	public void SetActiveController(UICharacterCreationController newController)
	{
		if ((bool)newController && newController.ShouldSkip())
		{
			return;
		}
		UICharacterCreationController currentController = GetCurrentController();
		if (newController == null)
		{
			if ((bool)currentController)
			{
				currentController.Deactivate();
			}
			m_controllerIndex = m_Controllers.Count;
			CurrentStage = GetStageNumberForController(GetCurrentController());
			m_maxControllerStage = Math.Max(CurrentStage, m_maxControllerStage);
		}
		for (int i = 0; i < m_Controllers.Count; i++)
		{
			if (!newController)
			{
				break;
			}
			if (GetController(i) == newController)
			{
				if ((bool)currentController)
				{
					currentController.Deactivate();
				}
				m_controllerIndex = i;
				newController.Activate();
				newController.Show();
				SetSubtitle(newController.GetSubTitle());
				CurrentStage = GetStageNumberForController(GetCurrentController());
				m_maxControllerStage = Math.Max(CurrentStage, m_maxControllerStage);
				break;
			}
		}
		HandleDoneVisibility(IsCharacterCreationReadyForCompletion());
		RefreshNextButtons();
	}

	public UICharacterCreationController GetController(int index)
	{
		if (index >= m_Controllers.Count || index < 0)
		{
			return null;
		}
		return m_Controllers[index];
	}

	public UICharacterCreationController GetCurrentController()
	{
		return GetController(m_controllerIndex);
	}

	public UICharacterCreationController.ControllerType GetCurrentControllerType()
	{
		UICharacterCreationController currentController = GetCurrentController();
		if ((bool)currentController)
		{
			return currentController.Type;
		}
		return UICharacterCreationController.ControllerType.None;
	}

	public int GetStageNumberForController(UICharacterCreationController controller)
	{
		int result = 0;
		foreach (UICharacterCreationController controller2 in m_Controllers)
		{
			if (!(controller2 == controller))
			{
				if (controller2.EndsStage != -1)
				{
					result = controller2.EndsStage + 1;
				}
				continue;
			}
			return result;
		}
		return result;
	}

	public bool StackIsAtRoot()
	{
		return m_Controllers.Count <= 1;
	}

	private void SetSubtitle(string text)
	{
		UILabel[] controllerSubtitles = ControllerSubtitles;
		for (int i = 0; i < controllerSubtitles.Length; i++)
		{
			controllerSubtitles[i].text = text;
		}
	}

	public static bool CharacterCanHaveFacialHair(Character character)
	{
		if (character.Race != CharacterStats.Race.Godlike && character.Race != CharacterStats.Race.Aumaua && character.Subrace != CharacterStats.Subrace.Wood_Elf)
		{
			return character.Subrace != CharacterStats.Subrace.Wild_Orlan;
		}
		return false;
	}

	public static int GetTotalModelVariations(Character character, UICharacterCreationAppearanceSetter.AppearanceType atype)
	{
		if ((character.Race == CharacterStats.Race.Godlike || character.Subrace == CharacterStats.Subrace.Wild_Orlan) && atype == UICharacterCreationAppearanceSetter.AppearanceType.HAIR)
		{
			return 1;
		}
		if (atype == UICharacterCreationAppearanceSetter.AppearanceType.FACIAL_HAIR && !CharacterCanHaveFacialHair(character))
		{
			return 0;
		}
		return CountGameObjectsIn(Resources.LoadAll(character.GetModelPath(atype)));
	}

	private static int CountGameObjectsIn(UnityEngine.Object[] array)
	{
		if (array == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GameObject)
			{
				num++;
			}
		}
		return num;
	}
}

using System;
using System.Text;
using UnityEngine;

public class Consumable : Item
{
	public enum ConsumableType
	{
		Ingestible,
		Scroll,
		Figurine,
		Trap,
		Potion,
		Drug,
		Count,
		None
	}

	public enum UsageMode
	{
		Fixed,
		PerEncounter,
		PerRest
	}

	public ConsumableType Type;

	public int UsageCount = 1;

	public UsageMode UsageType;

	[Tooltip("Determines the scroll's level if the item is type of scroll.")]
	[Persistent]
	public int Level;

	[Persistent]
	protected int m_usageCounter;

	[HideInInspector]
	public Consumable m_originalItem;

	[Tooltip("The ID of the animation to play when using the consumable.")]
	public int AnimationVariation = 100;

	public bool IsFoodOrDrug
	{
		get
		{
			if (Type != ConsumableType.Drug)
			{
				return Type == ConsumableType.Ingestible;
			}
			return true;
		}
	}

	public bool IsFoodDrugOrPotion => IsTypeFoodPotionDrug(Type);

	public bool CanUseViaPaperdoll
	{
		get
		{
			if (UsableOutOfCombat)
			{
				return !GetComponent<AttackBase>();
			}
			return false;
		}
	}

	public bool UsableInCombat
	{
		get
		{
			GenericAbility component = GetComponent<GenericAbility>();
			if ((bool)component)
			{
				return !component.NonCombatOnly;
			}
			return false;
		}
	}

	public bool UsableOutOfCombat
	{
		get
		{
			GenericAbility component = GetComponent<GenericAbility>();
			if ((bool)component)
			{
				return !component.CombatOnly;
			}
			return true;
		}
	}

	public int UsesRemaining => UsageCount - m_usageCounter;

	private int ScrollLevel => Level * 2;

	public static bool IsTypeFoodPotionDrug(ConsumableType type)
	{
		if (type != 0 && type != ConsumableType.Potion)
		{
			return type == ConsumableType.Drug;
		}
		return true;
	}

	public override void Start()
	{
		base.Start();
		if (MaxStackSize > 1 && (UsageCount > 1 || UsageType != 0))
		{
			Debug.LogError("Stacks of multi-use, per encounter or per rest consumables are not allowed. Stack size set to 1 for: " + base.Name);
			MaxStackSize = 1;
		}
		if (UsageType == UsageMode.PerEncounter)
		{
			GameState.OnCombatEnd += HandleGameUtilitiesOnCombatEnd;
		}
		if (UsageType == UsageMode.PerRest)
		{
			GameState.OnResting += HandleGameOnResting;
		}
	}

	public override void OnDestroy()
	{
		if (UsageType == UsageMode.PerEncounter)
		{
			GameState.OnCombatEnd -= HandleGameUtilitiesOnCombatEnd;
		}
		if (UsageType == UsageMode.PerRest)
		{
			GameState.OnResting -= HandleGameOnResting;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void HandleGameUtilitiesOnCombatEnd(object sender, EventArgs e)
	{
		m_usageCounter = 0;
	}

	private void HandleGameOnResting(object sender, EventArgs e)
	{
		m_usageCounter = 0;
	}

	public static GenericAbility.CooldownMode GetMostRestictiveCooldownMode(MonoBehaviour behavior)
	{
		Consumable component = ComponentUtils.GetComponent<Consumable>(behavior);
		GenericAbility component2 = ComponentUtils.GetComponent<GenericAbility>(behavior);
		GenericAbility.CooldownMode cooldownMode = GenericAbility.CooldownMode.None;
		if ((bool)component)
		{
			switch (component.UsageType)
			{
			case UsageMode.Fixed:
				cooldownMode = GenericAbility.CooldownMode.Charged;
				break;
			case UsageMode.PerEncounter:
				cooldownMode = GenericAbility.CooldownMode.PerEncounter;
				break;
			case UsageMode.PerRest:
				cooldownMode = GenericAbility.CooldownMode.PerRest;
				break;
			}
		}
		if ((bool)component2)
		{
			return (GenericAbility.CooldownMode)Mathf.Max((int)cooldownMode, (int)component2.CooldownType);
		}
		return cooldownMode;
	}

	public bool LoreIsEnough(CharacterStats character)
	{
		if (character != null)
		{
			return character.CalculateSkill(CharacterStats.SkillType.Lore) >= ScrollLevel;
		}
		return false;
	}

	public bool CanUse(CharacterStats character)
	{
		if (!character)
		{
			return false;
		}
		if (Type == ConsumableType.Scroll && !LoreIsEnough(character))
		{
			return false;
		}
		if (character.HasStatusEffectOfType(StatusEffect.ModifiedStat.CantUseFoodDrinkDrugs) && IsFoodOrDrug)
		{
			return false;
		}
		GenericAbility component = GetComponent<GenericAbility>();
		if ((bool)component)
		{
			if (component.CombatOnly && !GameState.InCombat)
			{
				return false;
			}
			if (component.NonCombatOnly && GameState.InCombat)
			{
				return false;
			}
		}
		return m_usageCounter < UsageCount;
	}

	public string WhyCannotUse(CharacterStats stats)
	{
		if (stats.HasStatusEffectOfType(StatusEffect.ModifiedStat.CantUseFoodDrinkDrugs) && IsFoodOrDrug)
		{
			return GUIUtils.Format(2230, CharacterStats.Name(stats));
		}
		if (!UsableOutOfCombat && !GameState.InCombat)
		{
			return GUIUtils.GetText(1753, CharacterStats.GetGender(stats));
		}
		if (!UsableInCombat && GameState.InCombat)
		{
			return GUIUtils.GetText(1887, CharacterStats.GetGender(stats));
		}
		if (UsesRemaining <= 0)
		{
			return GUIUtils.GetText(456, CharacterStats.GetGender(stats)) + ": " + DisplayName;
		}
		if (Type == ConsumableType.Scroll && !LoreIsEnough(stats))
		{
			return GUIUtils.Format(CharacterStats.GetGender(stats), 1715, ScrollLevel, stats.Name(), stats.CalculateSkill(CharacterStats.SkillType.Lore));
		}
		return "";
	}

	public static int GetMaxUsableScrollLevel(int skill)
	{
		return skill / 2;
	}

	public void UseImmediately(GameObject owner)
	{
		GenericAbility genericAbility = SetUpUse(owner);
		if ((bool)genericAbility)
		{
			genericAbility.Activate(owner);
		}
	}

	public void StartUse(GameObject owner)
	{
		GenericAbility genericAbility = SetUpUse(owner);
		if ((bool)genericAbility)
		{
			genericAbility.TriggerFromUI();
		}
	}

	protected virtual GenericAbility SetUpUse(GameObject owner)
	{
		GenericAbility component = GetComponent<GenericAbility>();
		if ((bool)component && (bool)owner)
		{
			GenericAbility genericAbility = GameResources.Instantiate<GenericAbility>(component);
			genericAbility.gameObject.SetActive(value: true);
			genericAbility.transform.parent = owner.transform;
			genericAbility.Owner = owner;
			genericAbility.ForceInit();
			Persistence component2 = genericAbility.gameObject.GetComponent<Persistence>();
			if (component2 != null)
			{
				component2.IsActive = true;
			}
			Consumable component3 = genericAbility.GetComponent<Consumable>();
			if (component3 != null)
			{
				component3.m_originalItem = this;
				component3.UsageCount = 1;
				component3.UsageType = UsageMode.Fixed;
				component3.m_usageCounter = 0;
			}
			return genericAbility;
		}
		return null;
	}

	public void EndUse(GameObject owner)
	{
		if ((bool)m_originalItem)
		{
			if (m_originalItem.UsageType == UsageMode.Fixed && m_originalItem.m_usageCounter + 1 >= m_originalItem.UsageCount)
			{
				if (m_originalItem.Location == ItemLocation.Dragged)
				{
					UIGlobalInventory.Instance.DestroyItem(m_originalItem, 1);
				}
				else if ((bool)m_originalItem.StoredInventory)
				{
					m_originalItem.StoredInventory.DestroyItem(m_originalItem, 1);
				}
			}
			else
			{
				m_originalItem.m_usageCounter++;
			}
			m_originalItem = null;
		}
		if (Type == ConsumableType.Trap && owner != null && owner.GetComponent<PartyMemberAI>() != null && (bool)AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(AchievementTracker.TrackedAchievementStat.NumTrapItemsUsed);
		}
	}

	public override string GetString(GameObject owner)
	{
		CharacterStats character = (owner ? owner.GetComponent<CharacterStats>() : null);
		StringBuilder stringBuilder = new StringBuilder();
		if (Type == ConsumableType.Scroll && ScrollLevel != 0)
		{
			string fstring = (LoreIsEnough(character) ? ("[" + NGUITools.EncodeColor(Color.white) + "]{0}[-]") : ("[" + NGUITools.EncodeColor(Color.red) + "]{0}[-]"));
			string text = StringUtility.Format(GUIUtils.GetText(76), ScrollLevel.ToString(), GUIUtils.GetText(36));
			stringBuilder.AppendLine(StringUtility.Format(fstring, text));
		}
		if (UsageCount != 1 || UsageType != 0)
		{
			string text2 = ((UsesRemaining != UsageCount) ? StringUtility.Format(GUIUtils.GetText(451), UsesRemaining, UsageCount) : UsageCount.ToString());
			if (UsageType == UsageMode.PerEncounter)
			{
				text2 = GUIUtils.Format(449, text2);
			}
			else if (UsageType == UsageMode.PerRest)
			{
				text2 = GUIUtils.Format(450, text2);
			}
			stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(448), text2));
		}
		stringBuilder.Append(base.GetString(owner));
		return stringBuilder.ToString().Trim();
	}
}

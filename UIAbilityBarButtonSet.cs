using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIAbilityBarButtonSet : MonoBehaviour
{
	public enum AbilityButtonAction
	{
		TALK_TO,
		QUICK_ITEM,
		CAST_SPELL_ABILITY,
		CLASS_EDIT,
		SUB_SPELLS,
		WEAPON_SET,
		WEAPON_SET_SUBMENU,
		EMPTY,
		HOTKEY_SUBMENU,
		AI_TOGGLE,
		CAST_SPELL_ABILITY_MASTERED,
		SUB_ITEMS,
		MASTERED_SUBMENU,
		WATCHER_SUBMENU,
		READONLY_ABILITY,
		COUNT
	}

	public GameObject ButtonTemplate;

	public UIGrid Grid;

	public UISprite SetIcon;

	public UIWidget AllCollider;

	public UIWidget LeftAnchorPoint;

	public GameObject LabelTab;

	public UILabel Label;

	private UIPanel m_Panel;

	[HideInInspector]
	public UIAbilityBarButton SubrowOwner;

	private bool m_DoRefresh;

	public Vector2 FramePadding = new Vector2(9f, 9f);

	private List<UIAbilityBarButton> m_Buttons = new List<UIAbilityBarButton>();

	public bool Locked;

	public bool WantsHide;

	public float Width
	{
		get
		{
			if (base.gameObject.activeSelf)
			{
				return (float)m_Buttons.Count((UIAbilityBarButton but) => but.gameObject.activeSelf) * Grid.cellWidth + FramePadding.x;
			}
			return 0f;
		}
	}

	public int ButtonCount
	{
		get
		{
			for (int i = 0; i < m_Buttons.Count; i++)
			{
				if (!m_Buttons[i].gameObject.activeSelf)
				{
					return i;
				}
			}
			return m_Buttons.Count;
		}
	}

	public bool Active
	{
		get
		{
			if (ButtonCount > 0)
			{
				return base.gameObject.activeSelf;
			}
			return false;
		}
	}

	public int SubLevel { get; set; }

	public bool Hovered
	{
		get
		{
			if (!SubrowOwner || !SubrowOwner.Hovered)
			{
				return UIAbilityBar.Instance.RaycastSubrow(SubLevel);
			}
			return true;
		}
	}

	private void Start()
	{
		ButtonTemplate.transform.localPosition = new Vector3(-10000f, -10000f, -100f);
		ButtonTemplate.SetActive(value: false);
	}

	private void Update()
	{
		if (m_DoRefresh)
		{
			Refresh();
			UIAbilityBar.Instance.Reposition(SubLevel);
		}
	}

	private void LateUpdate()
	{
		if (WantsHide)
		{
			base.gameObject.SetActive(value: false);
			WantsHide = false;
		}
	}

	private void OnDisable()
	{
		SubrowOwner = null;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		if (!m_Panel)
		{
			m_Panel = GetComponentInParent<UIPanel>();
		}
		if ((bool)m_Panel)
		{
			m_Panel.Refresh();
		}
	}

	public UIAbilityBarButtonSet[] ToggleSubrow(UIAbilityBarButton sender, int numSets)
	{
		if (sender == SubrowOwner)
		{
			UIAbilityBar.Instance.UnlockSubrow(SubLevel + 1);
			return null;
		}
		return ShowSubrow(sender, numSets);
	}

	public UIAbilityBarButtonSet[] ShowSubrow(UIAbilityBarButton sender, int numSets)
	{
		return UIAbilityBar.Instance.ShowSubrow(sender, SubLevel + 1, numSets);
	}

	public UIAbilityBarButtonSet[] ShowSubrowIfNew(UIAbilityBarButton sender, int numSets)
	{
		return UIAbilityBar.Instance.ShowSubrowIfNew(sender, SubLevel + 1, numSets);
	}

	public void Close()
	{
		UIAbilityBar.Instance.UnlockSubrow(SubLevel);
		UIAbilityBar.Instance.HideSubrow(SubLevel);
	}

	public bool SubrowActive()
	{
		if (UIAbilityBar.Instance.SubrowActive(SubLevel + 1))
		{
			return base.gameObject.activeSelf;
		}
		return false;
	}

	public void SetButtonsChants(CharacterStats stats)
	{
		SetIdentification(null, null);
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && ability is Chant, (GenericAbility ability) => (ability as Chant).UiIndex, AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsWeaponControls(CharacterStats stats)
	{
		SetIdentification(null, null);
		Equipment component = stats.GetComponent<Equipment>();
		PartyMemberAI component2 = stats.GetComponent<PartyMemberAI>();
		if ((bool)component && (bool)component2 && !component2.Secondary)
		{
			UIInventoryGridItem.GlowType bgType = UIInventoryGridItem.GlowType.NONE;
			WeaponSet weaponSet = null;
			if (component.SelectedWeaponSetSerialized >= 0 && component.SelectedWeaponSetSerialized < component.WeaponSets.Length)
			{
				weaponSet = component.WeaponSets[component.SelectedWeaponSetSerialized];
			}
			if (weaponSet != null && weaponSet.PrimaryWeapon != null)
			{
				bgType = UIInventoryGridItem.DetermineEquipModGlowColor(weaponSet.PrimaryWeapon.GetComponent<Equippable>());
			}
			SetButton(0, stats.gameObject, AbilityButtonAction.WEAPON_SET_SUBMENU, (weaponSet != null && !weaponSet.Empty()) ? ((Texture2D)weaponSet.GetTooltipIcon()) : UIAbilityBar.Instance.UnarmedIcon, null, bgType);
			m_Buttons[0].CountLabel.gameObject.SetActive(value: true);
			for (int i = 0; i < component.WeaponSets.Length; i++)
			{
				if (component.WeaponSets[i] == weaponSet)
				{
					m_Buttons[0].CountLabel.text = RomanNumeral.Convert(i + 1);
					break;
				}
			}
			HideButtons(1);
		}
		else
		{
			HideButtons(0);
		}
		m_DoRefresh = true;
	}

	public void SetButtonsWeaponSet(CharacterStats stats, int weaponSetIndex)
	{
		SetIdentification((weaponSetIndex == 0) ? GUIUtils.GetText(221) : null, null);
		WeaponSet weaponSet = stats.GetComponent<Equipment>().WeaponSets[weaponSetIndex];
		Texture2D texture2D = null;
		Texture2D icon = null;
		bool flag = false;
		UIInventoryGridItem.GlowType glowType = UIInventoryGridItem.GlowType.NONE;
		UIInventoryGridItem.GlowType bgType = UIInventoryGridItem.GlowType.NONE;
		if (weaponSet != null)
		{
			if (weaponSet.Empty())
			{
				texture2D = UIAbilityBar.Instance.UnarmedIcon;
				icon = UIAbilityBar.Instance.UnarmedIcon;
			}
			else
			{
				if ((bool)weaponSet.PrimaryWeapon)
				{
					texture2D = weaponSet.PrimaryWeapon.GetIconTexture();
					flag = weaponSet.PrimaryWeapon.BothPrimaryAndSecondarySlot;
					glowType = UIInventoryGridItem.DetermineEquipModGlowColor(weaponSet.PrimaryWeapon.GetComponent<Equippable>());
				}
				if (flag)
				{
					icon = texture2D;
					bgType = glowType;
				}
				else if ((bool)weaponSet.SecondaryWeapon)
				{
					icon = weaponSet.SecondaryWeapon.GetIconTexture();
					bgType = UIInventoryGridItem.DetermineEquipModGlowColor(weaponSet.SecondaryWeapon.GetComponent<Equippable>());
				}
			}
		}
		SetButton(0, stats.gameObject, AbilityButtonAction.WEAPON_SET, texture2D, null, glowType);
		m_Buttons[0].SetTargetIndex(weaponSetIndex);
		m_Buttons[0].CountLabel.text = "";
		m_Buttons[0].CountLabel.gameObject.SetActive(value: false);
		SetButton(1, stats.gameObject, AbilityButtonAction.WEAPON_SET, icon, null, bgType);
		m_Buttons[1].SetTargetIndex(weaponSetIndex);
		m_Buttons[1].CountLabel.text = RomanNumeral.Convert(weaponSetIndex + 1);
		m_Buttons[1].CountLabel.gameObject.SetActive(value: true);
		if (flag)
		{
			SetButtonAlpha(1, 0.35f);
		}
		HideButtons(2);
		m_DoRefresh = true;
	}

	public void SetButtonsItemPerRestAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perRest");
		int c = 0;
		c = AddQuickItemButtons(stats, (Item item) => (bool)item.GetComponent<Consumable>() && Consumable.GetMostRestictiveCooldownMode(item) == GenericAbility.CooldownMode.PerRest, AbilityButtonAction.QUICK_ITEM, c);
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType) && ability.CooldownType == GenericAbility.CooldownMode.PerRest, AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsItemPerEncounterAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perEncounter");
		int c = 0;
		c = AddQuickItemButtons(stats, (Item item) => (bool)item.GetComponent<Consumable>() && Consumable.GetMostRestictiveCooldownMode(item) == GenericAbility.CooldownMode.PerEncounter, AbilityButtonAction.QUICK_ITEM, c);
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType) && ability.CooldownType == GenericAbility.CooldownMode.PerEncounter, AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsItemPerStrongholdTurnAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perStrongholdTurn");
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType) && ability.CooldownType == GenericAbility.CooldownMode.PerStrongholdTurn, AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsItemOtherCooldownAbilities(CharacterStats stats)
	{
		SetIdentification(null, null);
		int c = 0;
		c = AddQuickItemButtons(stats, delegate(Item item)
		{
			GenericAbility.CooldownMode mostRestictiveCooldownMode = Consumable.GetMostRestictiveCooldownMode(item);
			return (bool)item.GetComponent<Consumable>() && mostRestictiveCooldownMode != GenericAbility.CooldownMode.PerEncounter && mostRestictiveCooldownMode != GenericAbility.CooldownMode.PerRest;
		}, AbilityButtonAction.QUICK_ITEM, c);
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType) && ability.CooldownType != GenericAbility.CooldownMode.PerEncounter && ability.CooldownType != GenericAbility.CooldownMode.PerRest && ability.CooldownType != GenericAbility.CooldownMode.PerStrongholdTurn, AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsOtherQuickitems(CharacterStats stats)
	{
		SetIdentification(null, null);
		int c = 0;
		c = AddQuickItemButtons(stats, (Item item) => !item.GetComponent<Consumable>(), AbilityButtonAction.QUICK_ITEM, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsItemTriggeredAbilities(CharacterStats stats)
	{
		SetIdentification(null, null);
		SetButtonsPassiveEquipmentAbilityButtons(stats.GetComponent<Equipment>(), 0);
	}

	public void SetButtonsItemsBase(CharacterStats stats)
	{
		SetIdentification(null, null);
		AIController component = ComponentUtils.GetComponent<AIController>(stats);
		int startButton = 0;
		if (!component || component.SummonType == AIController.AISummonType.NotSummoned || UIAbilityBarButton.CharacterHasSubrowContent(AbilityButtonAction.SUB_ITEMS, stats.gameObject, -1))
		{
			SetButton(startButton++, stats.gameObject, AbilityButtonAction.SUB_ITEMS, UIAbilityBar.Instance.ItemsIcon);
		}
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsClassSpells(CharacterStats stats)
	{
		SetIdentification(null, null);
		int num = 0;
		if ((bool)SpellMax.Instance && CharacterStats.IsPlayableClass(stats.CharacterClass))
		{
			int maxSpellLevel = SpellMax.Instance.GetMaxSpellLevel(stats.gameObject);
			for (int i = 1; i <= maxSpellLevel; i++)
			{
				SetButton(num, stats.gameObject, AbilityButtonAction.SUB_SPELLS, UIAbilityBar.Instance.GetClassIcon(stats.CharacterClass), RomanNumeral.Convert(i), UIInventoryGridItem.GlowType.NONE);
				m_Buttons[num].SetTargetIndex(i);
				num++;
			}
		}
		HideButtons(num);
		m_DoRefresh = true;
	}

	public void SetButtonsAnticlassSpells(CharacterStats stats)
	{
		SetIdentification(null, null);
		int startButton = AddAbilityButtons(stats, delegate(GenericAbility ability)
		{
			GenericSpell genericSpell = ability as GenericSpell;
			return (bool)genericSpell && ability.MasteryLevel == 0 && genericSpell.StatusEffectGrantingSpell == null && (genericSpell.SpellClass != stats.CharacterClass || !CharacterStats.IsPlayableClass(genericSpell.SpellClass)) && !GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType);
		}, AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsMasteredSpells(CharacterStats stats)
	{
		SetIdentification(GUIUtils.Format(2251, GUIUtils.GetClassString(stats.CharacterClass, stats.Gender)), "");
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => ability.MasteryLevel > 0, AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsWatcherAbilities(CharacterStats stats)
	{
		SetIdentification(null, "");
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => ability.IsWatcherAbility, AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsEncounterAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perEncounter");
		int c = 0;
		int num = 0;
		for (int i = 0; i < stats.ActiveAbilities.Count; i++)
		{
			if (stats.ActiveAbilities[i].MasteryLevel > 0)
			{
				num++;
			}
		}
		if (num >= 3)
		{
			UIAbilityBarButton uIAbilityBarButton = SetButton(c++, stats.gameObject, AbilityButtonAction.MASTERED_SUBMENU, UIAbilityBar.Instance.GetClassIcon(stats.CharacterClass));
			uIAbilityBarButton.TopIcon.enabled = true;
			uIAbilityBarButton.TopIcon.mainTexture = UIAbilityBar.Instance.MasteredIconOverlay;
		}
		else
		{
			c = AddAbilityButtons(stats, (GenericAbility ability) => ability.MasteryLevel > 0, AbilityButtonAction.CAST_SPELL_ABILITY, c);
		}
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && !ability.Modal && !(ability is GenericSpell) && !(ability is Chant) && !(ability is GenericCipherAbility) && ability.MasteryLevel <= 0 && ability.CooldownType == GenericAbility.CooldownMode.PerEncounter && !GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType), AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsRestAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perRest");
		int c = 0;
		int num = 0;
		for (int i = 0; i < stats.ActiveAbilities.Count; i++)
		{
			if (stats.ActiveAbilities[i].IsWatcherAbility)
			{
				num++;
			}
		}
		if (num >= 3)
		{
			SetButton(c++, stats.gameObject, AbilityButtonAction.WATCHER_SUBMENU, UIAbilityBar.Instance.WatcherIcon);
		}
		else
		{
			c = AddAbilityButtons(stats, (GenericAbility ability) => ability.IsWatcherAbility, AbilityButtonAction.CAST_SPELL_ABILITY, c);
		}
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && !ability.Modal && !(ability is GenericSpell) && !(ability is Chant) && !(ability is GenericCipherAbility) && !ability.IsWatcherAbility && ability.CooldownType == GenericAbility.CooldownMode.PerRest && !GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType), AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsStrongholdTurnAbilities(CharacterStats stats)
	{
		SetIdentification(null, "perStrongholdTurn");
		int c = 0;
		c = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && !ability.Modal && !(ability is GenericSpell) && !(ability is Chant) && !(ability is GenericCipherAbility) && !ability.IsWatcherAbility && ability.CooldownType == GenericAbility.CooldownMode.PerStrongholdTurn && !GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType), AbilityButtonAction.CAST_SPELL_ABILITY, c);
		HideButtons(c);
		m_DoRefresh = true;
	}

	public void SetButtonsOtherCooldownAbilities(CharacterStats stats)
	{
		SetIdentification(null, null);
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => !ability.Passive && !ability.Modal && !(ability is GenericSpell) && !(ability is Chant) && !(ability is GenericCipherAbility) && ability.CooldownType != GenericAbility.CooldownMode.PerRest && ability.CooldownType != GenericAbility.CooldownMode.PerEncounter && ability.CooldownType != GenericAbility.CooldownMode.PerStrongholdTurn && !GenericAbility.AbilityTypeIsAnyEquipment(ability.EffectType), AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsStolenAbilities(CharacterStats stats)
	{
		SetIdentification(null, null);
		int startButton = AddAbilityButtons(stats, delegate(GenericAbility ability)
		{
			GenericSpell genericSpell = ability as GenericSpell;
			return (bool)genericSpell && genericSpell.StatusEffectGrantingSpell != null;
		}, AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsModalAbilities(CharacterStats stats, GenericAbility.ActivationGroup modalGroup)
	{
		SetIdentification(null, "perModal");
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => ability.Modal && !(ability is Chant) && !(ability is GenericSpell) && !ability.Passive && ability.Grouping == modalGroup, AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	private int AddQuickItemButtons(CharacterStats stats, Func<Item, bool> condition, AbilityButtonAction action, int c)
	{
		PartyMemberAI component = stats.GetComponent<PartyMemberAI>();
		if ((bool)component && (bool)component.QuickbarInventory)
		{
			foreach (InventoryItem item in component.QuickbarInventory.ItemList.OrderBy((InventoryItem ii) => ii.uiSlot))
			{
				if (item != null && (bool)item.baseItem && condition(item.baseItem))
				{
					SetButton(c++, item.baseItem.gameObject, action, item.baseItem.GetIconTexture(), null, UIInventoryGridItem.GlowType.NONE).SetAction(action, item);
				}
			}
			return c;
		}
		return c;
	}

	private int AddAbilityButtons(CharacterStats stats, Func<GenericAbility, bool> condition, AbilityButtonAction action, int c)
	{
		return AddAbilityButtons(stats, condition, null, action, c);
	}

	private int AddAbilityButtons(CharacterStats stats, Func<GenericAbility, bool> condition, Func<GenericAbility, int> sortkey, AbilityButtonAction action, int c)
	{
		bool flag = false;
		PartyMemberAI component = stats.GetComponent<PartyMemberAI>();
		if ((bool)component && component.SummonType == AIController.AISummonType.Summoned)
		{
			flag = true;
		}
		foreach (GenericAbility item in stats.ActiveAbilities.OrderBy((GenericAbility a) => a.Sortkey))
		{
			if ((!flag || !GenericAbility.AbilityTypeIsAnyEquipment(item.EffectType)) && (bool)item && item.IsVisibleOnUI && !item.TriggeredAutomatically && PrerequisiteData.CheckVisibilityPrerequisites(stats.gameObject, null, item.ActivationPrerequisites, null) && condition(item))
			{
				AbilityButtonAction abilityButtonAction = action;
				if (abilityButtonAction == AbilityButtonAction.CAST_SPELL_ABILITY && item.MasteryLevel > 0)
				{
					abilityButtonAction = AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED;
				}
				UIAbilityBarButton uIAbilityBarButton = SetButton(c, item.gameObject, abilityButtonAction, item.Icon);
				if (sortkey != null)
				{
					uIAbilityBarButton.name = sortkey(item).ToString("0000") + ".ActionBarButton";
				}
				c++;
			}
		}
		return c;
	}

	private int SetButtonsPassiveEquipmentAbilityButtons(Equipment equipment, int c)
	{
		SetIdentification(null, null);
		foreach (Equippable currentItem in equipment.CurrentItems)
		{
			if (!currentItem)
			{
				continue;
			}
			foreach (ItemModComponent attachedItemMod in currentItem.AttachedItemMods)
			{
				if ((bool)attachedItemMod.Ability && attachedItemMod.Mod.AbilityTriggeredOn != 0 && attachedItemMod.Ability.CooldownType != 0)
				{
					SetButton(c, attachedItemMod.Ability.gameObject, currentItem.gameObject, AbilityButtonAction.READONLY_ABILITY, currentItem.GetIconTexture(), null, UIInventoryGridItem.GlowType.NONE);
					c++;
				}
			}
		}
		HideButtons(c);
		m_DoRefresh = true;
		return c;
	}

	public static bool HasPassiveEquipmentAbilityButtons(Equipment equipment)
	{
		foreach (Equippable currentItem in equipment.CurrentItems)
		{
			if (!currentItem)
			{
				continue;
			}
			foreach (ItemModComponent attachedItemMod in currentItem.AttachedItemMods)
			{
				if ((bool)attachedItemMod.Ability && attachedItemMod.Mod.AbilityTriggeredOn != 0 && attachedItemMod.Ability.CooldownType != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetButtonsMisc(GameObject[] characters)
	{
		SetIdentification(null, null);
		int startButton = 0;
		bool flag = characters.Length != 0;
		for (int i = 0; i < characters.Length; i++)
		{
			if ((bool)characters[i] && !GameUtilities.ActiveAIControllerIsPartyMemberAI(characters[i]))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			SetButton(startButton++, characters, AbilityButtonAction.AI_TOGGLE, UIAbilityBar.Instance.AiActiveIcon);
		}
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	public void SetButtonsMisc(CharacterStats cs)
	{
		SetIdentification(null, null);
		int num = 0;
		if (GameUtilities.ActiveAIControllerIsPartyMemberAI(cs.gameObject))
		{
			SetButton(num++, cs.gameObject, AbilityButtonAction.AI_TOGGLE, UIAbilityBar.Instance.AiActiveIcon);
		}
		if ((bool)cs.GetComponent<NPCDialogue>())
		{
			SetButton(num, cs.gameObject, AbilityButtonAction.TALK_TO, UIAbilityBar.Instance.TalkIcon);
			num++;
		}
		switch (cs.CharacterClass)
		{
		case CharacterStats.Class.Chanter:
			SetButton(num, cs.gameObject, AbilityButtonAction.CLASS_EDIT, UIAbilityBar.Instance.ChanterEditIcon);
			num++;
			break;
		case CharacterStats.Class.Wizard:
		{
			Grimoire grimoire = Grimoire.Find(cs.gameObject);
			if ((bool)grimoire)
			{
				SetButton(num, cs.gameObject, AbilityButtonAction.CLASS_EDIT, grimoire.GetComponent<Item>().GetIconTexture());
			}
			num++;
			break;
		}
		}
		HideButtons(num);
		m_DoRefresh = true;
	}

	public void SetButtonsHotkeys(CharacterStats stats)
	{
		if ((bool)LabelTab && SubLevel > 0)
		{
			SetIdentification(GUIUtils.GetText(1662), null);
		}
		else
		{
			SetIdentification(null, null);
		}
		CharacterHotkeyBindings characterHotkeyBindings = CharacterHotkeyBindings.Get(stats.gameObject);
		int num = 0;
		if ((bool)characterHotkeyBindings && characterHotkeyBindings.AbilityHotkeys != null)
		{
			foreach (KeyControl key in characterHotkeyBindings.AbilityHotkeys.Keys)
			{
				int num2 = characterHotkeyBindings.AbilityHotkeys[key];
				for (int i = 0; i < stats.ActiveAbilities.Count; i++)
				{
					if (CharacterHotkeyBindings.GetDictionaryKey(stats.ActiveAbilities[i]) == num2)
					{
						GenericAbility genericAbility = stats.ActiveAbilities[i];
						SetButton(num, genericAbility.gameObject, AbilityButtonAction.CAST_SPELL_ABILITY, genericAbility.Icon);
						num++;
						break;
					}
				}
			}
		}
		HideButtons(num);
		m_DoRefresh = true;
	}

	public static bool ShowOnSpellBar(GenericAbility ability, CharacterStats stats, int spellLevel)
	{
		if (ability.Passive)
		{
			return false;
		}
		GenericSpell genericSpell = ability as GenericSpell;
		GenericCipherAbility genericCipherAbility = ability as GenericCipherAbility;
		if ((bool)genericSpell)
		{
			if (genericSpell.SpellClass != stats.CharacterClass || genericSpell.StatusEffectGrantingSpell != null || ability.MasteryLevel > 0)
			{
				return false;
			}
			if (spellLevel > 0 && genericSpell.SpellLevel != spellLevel)
			{
				return false;
			}
			if (stats.CharacterClass == CharacterStats.Class.Wizard && genericSpell.NeedsGrimoire)
			{
				Equipment component = stats.GetComponent<Equipment>();
				if (component == null || component.CurrentItems == null || component.CurrentItems.Grimoire == null)
				{
					return false;
				}
				Grimoire component2 = component.CurrentItems.Grimoire.GetComponent<Grimoire>();
				if (component2 == null || !component2.HasSpell(genericSpell))
				{
					return false;
				}
				return true;
			}
			return true;
		}
		if ((bool)genericCipherAbility)
		{
			if (ability.MasteryLevel > 0)
			{
				return false;
			}
			if (spellLevel > 0 && genericCipherAbility.SpellLevel != spellLevel)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetButtonsSubspells(CharacterStats stats, int spellLevel)
	{
		if ((bool)SubrowOwner)
		{
			SetIdentification(GUIUtils.FormatSpellLevel(stats.CharacterClass, spellLevel), null);
		}
		else
		{
			SetIdentification(null, null);
		}
		int startButton = AddAbilityButtons(stats, (GenericAbility ability) => ShowOnSpellBar(ability, stats, spellLevel), AbilityButtonAction.CAST_SPELL_ABILITY, 0);
		HideButtons(startButton);
		m_DoRefresh = true;
	}

	private void SetIdentification(string text, string sprite)
	{
		if (!base.gameObject || !LabelTab)
		{
			return;
		}
		if (string.IsNullOrEmpty(text))
		{
			LabelTab.SetActive(value: false);
			Label.text = "";
		}
		else
		{
			LabelTab.SetActive(value: true);
			Label.text = text;
		}
		if (string.IsNullOrEmpty(sprite))
		{
			SetIcon.gameObject.SetActive(value: false);
			return;
		}
		SetIcon.gameObject.SetActive(value: true);
		SetIcon.spriteName = sprite;
		UIActionBarTooltipTrigger component = SetIcon.GetComponent<UIActionBarTooltipTrigger>();
		if ((bool)component)
		{
			switch (sprite)
			{
			case "perEncounter":
				component.enabled = true;
				component.Text.StringID = 175;
				component.Text.StringTable = DatabaseString.StringTableType.Cyclopedia;
				break;
			case "perRest":
				component.enabled = true;
				component.Text.StringID = 177;
				component.Text.StringTable = DatabaseString.StringTableType.Cyclopedia;
				break;
			case "perModal":
				component.enabled = true;
				component.Text.StringID = 77;
				component.Text.StringTable = DatabaseString.StringTableType.Gui;
				break;
			case "perStrongholdTurn":
			{
				component.enabled = true;
				string text2 = GUIUtils.GetText(1563).Replace("{0}", "").Trim();
				text2 = (component.OverrideText = char.ToUpper(text2[0]) + text2.Substring(1));
				break;
			}
			default:
				component.enabled = false;
				break;
			}
		}
	}

	private void HideButtons(int startButton)
	{
		if (base.gameObject == null)
		{
			return;
		}
		base.gameObject.SetActive(startButton > 0);
		for (int i = startButton; i < m_Buttons.Count; i++)
		{
			if ((bool)m_Buttons[i])
			{
				m_Buttons[i].gameObject.SetActive(value: false);
			}
		}
	}

	private UIAbilityBarButton SetButton(int index, GameObject target, AbilityButtonAction action, Texture2D icon)
	{
		return SetButton(index, target, action, icon, null, UIInventoryGridItem.GlowType.NONE);
	}

	private void SetButton(int index, GameObject[] targets, AbilityButtonAction action, Texture2D icon)
	{
		UIAbilityBarButton uIAbilityBarButton = SetButton(index, null, action, icon, null, UIInventoryGridItem.GlowType.NONE);
		uIAbilityBarButton.MultiTargets = new GameObject[targets.Length];
		targets.CopyTo(uIAbilityBarButton.MultiTargets, 0);
	}

	private UIAbilityBarButton SetButton(int index, GameObject target, AbilityButtonAction action, Texture2D icon, string label, UIInventoryGridItem.GlowType bgType)
	{
		return SetButton(index, target, null, action, icon, label, bgType);
	}

	private UIAbilityBarButton SetButton(int index, GameObject target, GameObject targetParent, AbilityButtonAction action, Texture2D icon, string label, UIInventoryGridItem.GlowType bgType)
	{
		while (index >= m_Buttons.Count)
		{
			UIAbilityBarButton component = NGUITools.AddChild(Grid.gameObject, ButtonTemplate).GetComponent<UIAbilityBarButton>();
			component.Owner = this;
			m_Buttons.Add(component);
		}
		UIAbilityBarButton uIAbilityBarButton = m_Buttons[index];
		SetButtonIcon(index, icon);
		uIAbilityBarButton.SetAction(action, target, targetParent);
		SetButtonAlpha(index, 1f);
		if (!string.IsNullOrEmpty(label))
		{
			uIAbilityBarButton.Label.gameObject.SetActive(value: true);
			uIAbilityBarButton.Label.text = label;
		}
		else
		{
			uIAbilityBarButton.Label.gameObject.SetActive(value: false);
		}
		uIAbilityBarButton.gameObject.name = index.ToString("0000") + ".ActionBarButton";
		uIAbilityBarButton.gameObject.SetActive(value: true);
		uIAbilityBarButton.ShowModItemBackground(bgType);
		uIAbilityBarButton.TopIcon.enabled = false;
		return uIAbilityBarButton;
	}

	private void SetButtonIcon(int index, Texture2D icon)
	{
		UIAbilityBarButton uIAbilityBarButton = m_Buttons[index];
		Vector3 localScale = uIAbilityBarButton.Icon.transform.localScale;
		uIAbilityBarButton.Icon.mainTexture = icon;
		uIAbilityBarButton.Icon.gameObject.SetActive(icon != null);
		uIAbilityBarButton.Icon.transform.localScale = localScale;
	}

	private void SetButtonAlpha(int index, float alpha)
	{
		UIImageButtonRevised imageButton = m_Buttons[index].ImageButton;
		Color neutralColor = imageButton.NeutralColor;
		neutralColor.a = alpha;
		imageButton.SetNeutralColor(neutralColor);
		Color mousedColor = imageButton.MousedColor;
		mousedColor.a = alpha;
		imageButton.SetMousedColor(mousedColor);
	}

	public void Refresh()
	{
		m_DoRefresh = false;
		Grid.Reposition();
	}

	public void RefreshHotkeys()
	{
		foreach (UIAbilityBarButton button in m_Buttons)
		{
			button.RefreshHotkey();
		}
	}

	public bool Valid()
	{
		if (m_Buttons.Count > 0)
		{
			return m_Buttons[0].gameObject.activeSelf;
		}
		return false;
	}

	public int GetButtonIndex(UIAbilityBarButton button)
	{
		for (int i = 0; i < m_Buttons.Count; i++)
		{
			if (button == m_Buttons[i])
			{
				return i;
			}
		}
		return -1;
	}

	public UIAbilityBarButton GetButtonByIndex(int index)
	{
		if (index < m_Buttons.Count && m_Buttons[index].gameObject.activeSelf)
		{
			return m_Buttons[index];
		}
		return null;
	}
}

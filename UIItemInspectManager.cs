using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIItemInspectManager : UIHudWindow
{
	public UILabel TitleLabel;

	public UILabel ItemTypeLabel;

	public UILabel EffectTextLabel;

	public UICapitularLabel FlavorTextLabel;

	public UITexture ImageTexture;

	public UITexture LargeImageTexture;

	public UIDynamicLoadTexture PencilSketch;

	public UIWidget IconBackground;

	public UIWidget LargeImageBackground;

	public UIAnchor TitleSepAnchor;

	public UIWidget TitleSep;

	public UITable LayoutScrollArea;

	public GameObject EnchantParent;

	public UILabel LblEnchantLabel;

	public UILabel LblEnchantValue;

	public UIItemInspectGoals Goals;

	public UIItemInspectStringEffects StringEffectDisplay;

	public UIMultiSpriteImageButton EnchantButton;

	public UIMultiSpriteImageButton CompareButton;

	public UIMultiSpriteImageButton LearnSpellButton;

	public UIMultiSpriteImageButton ExamineButton;

	public UIMultiSpriteImageButton SoulbindButton;

	public UIGrid ButtonsGrid;

	public UIDraggablePanel DragPanel;

	public bool IsLargeConfiguration;

	public Collider[] AdditionalDragHandlers;

	private bool m_IsStore;

	private bool m_NoEnchant;

	[HideInInspector]
	public bool LearnSpellAllowed;

	private bool SoulbindUnlockMode;

	private StatusEffect.ModifiedStat m_InspectStat = StatusEffect.ModifiedStat.NoEffect;

	private bool m_InspectOffhand;

	private DamagePacket.DamageType m_InspectDamageType = DamagePacket.DamageType.None;

	private const int MaxWindows = 3;

	private const float PerWindowOffset = 40f;

	private static List<UIItemInspectManager> s_ActiveWindows = new List<UIItemInspectManager>();

	private static Stack<UIItemInspectManager> s_WindowPool = new Stack<UIItemInspectManager>();

	private static List<UIItemInspectManager> s_ActiveLargeWindows = new List<UIItemInspectManager>();

	private static Stack<UIItemInspectManager> s_LargeWindowPool = new Stack<UIItemInspectManager>();

	private static UIItemInspectManager s_Last = null;

	private UIItemInspectManager m_Child;

	private bool m_NeedsReload;

	private static CharacterStats s_SpellLearner;

	private static GenericSpell s_SpellLearnee;

	public GameObject ObjectOwner { get; set; }

	public MonoBehaviour InspectionObject { get; set; }

	public bool NoDescription { get; set; }

	public bool NoCompare { get; set; }

	protected static bool ShouldUseLarge(GameObject target)
	{
		if (!target)
		{
			return false;
		}
		if ((bool)target.GetComponent<EquipmentSoulbind>())
		{
			return true;
		}
		if ((bool)target.GetComponent<GenericAbility>())
		{
			return true;
		}
		if (target.name.StartsWith("Lore_Book_"))
		{
			return true;
		}
		return false;
	}

	protected static UIItemInspectManager Create(bool large)
	{
		List<UIItemInspectManager> list = s_ActiveWindows;
		Stack<UIItemInspectManager> stack = s_WindowPool;
		GameObject prefab = UIWindowManager.Instance.ExamineBoxPrefab;
		if (large)
		{
			list = s_ActiveLargeWindows;
			stack = s_LargeWindowPool;
			prefab = UIWindowManager.Instance.ExamineBoxLargePrefab;
		}
		if (stack.Count <= 0 && list.Count < 3)
		{
			UIItemInspectManager component = NGUITools.AddChild(UIWindowManager.Instance.gameObject, prefab).GetComponent<UIItemInspectManager>();
			UIWindowManager.Instance.WindowCreated(component.GetComponent<UIHudWindow>());
			component.Init();
			stack.Push(component);
		}
		if (stack.Count > 0)
		{
			s_Last = stack.Pop();
		}
		else
		{
			s_Last = null;
		}
		return s_Last;
	}

	public static void ExamineStore(MonoBehaviour inspect, GameObject owner)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ObjectOwner = owner;
			uIItemInspectManager.m_IsStore = true;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void ExamineNoEnchant(MonoBehaviour inspect, GameObject owner)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ObjectOwner = owner;
			uIItemInspectManager.m_NoEnchant = true;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void ExamineLearn(MonoBehaviour inspect, GameObject learner)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ObjectOwner = learner;
			uIItemInspectManager.LearnSpellAllowed = true;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void Examine(StatusEffect.ModifiedStat stat, MonoBehaviour inspect, DamagePacket.DamageType damageType = DamagePacket.DamageType.None, bool offhand = false)
	{
		UIItemInspectManager uIItemInspectManager = Create(large: false);
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.m_InspectStat = stat;
			uIItemInspectManager.m_InspectDamageType = damageType;
			uIItemInspectManager.m_InspectOffhand = offhand;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void ExamineNoDescription(MonoBehaviour inspect)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.NoDescription = true;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void Examine(MonoBehaviour inspect)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void Examine(MonoBehaviour inspect, GameObject owner, bool dim = false)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ObjectOwner = owner;
			uIItemInspectManager.DimsBackgroundTemp = dim;
			uIItemInspectManager.ShowWindow();
		}
	}

	public static void ExamineSoulbindUnlock(EquipmentSoulbind inspect, GameObject owner)
	{
		UIItemInspectManager uIItemInspectManager = Create(ShouldUseLarge(inspect.gameObject));
		if ((bool)uIItemInspectManager)
		{
			uIItemInspectManager.InspectionObject = inspect;
			uIItemInspectManager.ObjectOwner = owner;
			uIItemInspectManager.SoulbindUnlockMode = true;
			uIItemInspectManager.NoCompare = true;
			uIItemInspectManager.ShowWindow();
		}
	}

	private void Awake()
	{
		UIMultiSpriteImageButton enchantButton = EnchantButton;
		enchantButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(enchantButton.onClick, new UIEventListener.VoidDelegate(OnEnchant));
		UIMultiSpriteImageButton compareButton = CompareButton;
		compareButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(compareButton.onClick, new UIEventListener.VoidDelegate(OnCompare));
		UIMultiSpriteImageButton learnSpellButton = LearnSpellButton;
		learnSpellButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(learnSpellButton.onClick, new UIEventListener.VoidDelegate(OnLearnSpell));
		UIMultiSpriteImageButton examineButton = ExamineButton;
		examineButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(examineButton.onClick, new UIEventListener.VoidDelegate(OnExamine));
		UIMultiSpriteImageButton soulbindButton = SoulbindButton;
		soulbindButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(soulbindButton.onClick, new UIEventListener.VoidDelegate(OnSoulbind));
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UIEventListener uIEventListener = UIEventListener.Get(componentsInChildren[i]);
			uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnRightClick));
		}
		for (int j = 0; j < AdditionalDragHandlers.Length; j++)
		{
			if ((bool)AdditionalDragHandlers[j])
			{
				UIEventListener uIEventListener2 = UIEventListener.Get(AdditionalDragHandlers[j].gameObject);
				uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(base.OnDragged));
			}
		}
	}

	private void Update()
	{
		if (m_NeedsReload)
		{
			Reload();
		}
	}

	private void OnRightClick(GameObject sender)
	{
		HideWindow();
	}

	protected override void Show()
	{
		if (IsLargeConfiguration)
		{
			s_ActiveLargeWindows.Add(this);
		}
		else
		{
			s_ActiveWindows.Add(this);
		}
		if (!InspectionObject)
		{
			HideWindow();
			return;
		}
		ScriptEvent component = InspectionObject.GetComponent<ScriptEvent>();
		if ((bool)component)
		{
			component.ExecuteScript(ScriptEvent.ScriptEvents.OnItemInspected);
		}
		SetPosition(new Vector2(1f, -1f) * (s_ActiveWindows.Count + s_ActiveLargeWindows.Count) * 40f);
		Reload();
	}

	protected override bool Hide(bool forced)
	{
		m_IsStore = false;
		m_NoEnchant = false;
		ObjectOwner = null;
		InspectionObject = null;
		m_InspectStat = StatusEffect.ModifiedStat.NoEffect;
		m_InspectDamageType = DamagePacket.DamageType.None;
		s_ActiveWindows.Remove(this);
		s_ActiveLargeWindows.Remove(this);
		if (IsLargeConfiguration)
		{
			s_LargeWindowPool.Push(this);
		}
		else
		{
			s_WindowPool.Push(this);
		}
		m_Child = null;
		LearnSpellAllowed = false;
		SoulbindUnlockMode = false;
		NoDescription = false;
		NoCompare = false;
		return base.Hide(forced);
	}

	private void OnCompare(GameObject sender)
	{
		if ((bool)m_Child && m_Child.WindowActive())
		{
			m_Child.HideWindow();
			m_Child = null;
		}
		CharacterStats characterStats = ((!UILootManager.Instance.IsVisible) ? UIInventoryManager.Instance.SelectedCharacter : UILootManager.Instance.SelectedCharacter);
		if (!characterStats)
		{
			return;
		}
		IEnumerable<Item.UIEquippedItem> comparisonTargets = UIInventoryEquipment.GetComparisonTargets(InspectionObject.GetComponent<Equippable>(), characterStats.GetComponent<Equipment>());
		if (comparisonTargets.Any())
		{
			Examine(comparisonTargets.First().item, characterStats.gameObject);
			if ((bool)s_Last)
			{
				s_Last.SetPosition(Window.transform.localPosition - new Vector3(-490f, 0f, 0f));
			}
			m_Child = s_Last;
		}
	}

	private void OnEnchant(GameObject sender)
	{
		if (!GameState.InCombat)
		{
			UICraftingManager.Instance.EnchantMode = true;
			UICraftingManager.Instance.EnchantTarget = InspectionObject.GetComponent<Item>();
			HideWindow();
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.ShowWindow();
		}
	}

	private void OnExamine(GameObject sender)
	{
		QuestAsset component = InspectionObject.GetComponent<QuestAsset>();
		if ((bool)component)
		{
			UIQuestAssetWindow.Instance.LoadAsset(component);
			UIQuestAssetWindow.Instance.ShowWindow();
		}
	}

	private void OnSoulbind(GameObject sender)
	{
		EquipmentSoulbind component = InspectionObject.GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			if (component.IsBound)
			{
				UISoulbindMessages.TryUnbind(component);
			}
			else
			{
				UISoulbindMessages.TryBind(component, ObjectOwner);
			}
		}
	}

	private void OnLearnSpell(GameObject sender)
	{
		CharacterStats component = ObjectOwner.GetComponent<CharacterStats>();
		GenericSpell component2 = InspectionObject.GetComponent<GenericSpell>();
		if (!component2)
		{
			Debug.LogError(string.Concat(ObjectOwner.name, " tried to learn '", InspectionObject, "' but it wasn't a GenericSpell."));
		}
		else
		{
			QueryLearnSpell(component, component2);
		}
	}

	public static void QueryLearnSpell(CharacterStats stats, GenericSpell spell)
	{
		if (UIGrimoireManager.Instance.CanEditGrimoire)
		{
			bool flag = stats != null && stats.ActiveAbilities.Contains(spell, GenericAbility.NameComparer.Instance);
			if (stats.CharacterClass != CharacterStats.Class.Wizard)
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(1790, CharacterStats.GetGender(stats)));
			}
			else if (!flag)
			{
				s_SpellLearnee = spell;
				s_SpellLearner = stats;
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, GUIUtils.GetText(418), GUIUtils.Format(420, GUIUtils.Format(294, spell.CostToLearn), CharacterStats.Name(stats), GenericAbility.Name(spell)));
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnLearnSpellEnd));
			}
			else
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(418), GUIUtils.Format(419, CharacterStats.Name(stats), GenericAbility.Name(spell)));
			}
		}
	}

	public static void OnLearnSpellEnd(UIMessageBox.Result result, UIMessageBox owner)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			GenericSpell genericSpell = s_SpellLearnee;
			if (!genericSpell)
			{
				Debug.LogError(string.Concat(s_SpellLearner.name, " tried to learn '", s_SpellLearnee, "' but it wasn't a GenericSpell."));
				return;
			}
			PlayerInventory inventory = GameState.s_playerCharacter.Inventory;
			if (inventory.currencyTotalValue.v < (float)genericSpell.CostToLearn)
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(418), GUIUtils.GetText(421));
			}
			else
			{
				inventory.currencyTotalValue.v -= genericSpell.CostToLearn;
				AbilityProgressionTable.AddAbilityToCharacter(genericSpell.gameObject, s_SpellLearner, causeIsGameplay: true);
				UIGrimoireManager.Instance.Reload();
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(418), GUIUtils.Format(422, CharacterStats.Name(s_SpellLearner), GenericAbility.Name(genericSpell)));
			}
		}
		s_SpellLearner = null;
		s_SpellLearnee = null;
	}

	public static string GetEquippableItemType(MonoBehaviour obj, GameObject owner, Equippable equippable)
	{
		return GetEquippableItemTypeWithRarity(obj, owner, equippable, showUnique: true);
	}

	public static string GetEquippableItemTypeWithRarity(MonoBehaviour obj, GameObject owner, Equippable equippable, bool showUnique)
	{
		string text = "";
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < equippable.Slots.Length; i++)
		{
			if (i == 11 || i == 12)
			{
				if (flag2)
				{
					continue;
				}
				flag2 = true;
				Weapon weapon = equippable as Weapon;
				if ((bool)weapon)
				{
					WeaponSpecializationData.WeaponType weaponType = weapon.WeaponType;
					text = text + GUIUtils.GetWeaponTypeIDs(weaponType) + GUIUtils.Comma();
					continue;
				}
			}
			if (i == 4 || i == 3)
			{
				if (flag)
				{
					continue;
				}
				flag = true;
			}
			if (equippable.Slots[i].Val)
			{
				if (i == 2)
				{
					Armor component = obj.GetComponent<Armor>();
					text = ((!component) ? (text + GUIUtils.GetText(1426) + GUIUtils.Comma()) : (text + GUIUtils.Format(441, GUIUtils.GetArmorCategoryString(component.ArmorCategory)) + GUIUtils.Comma()));
				}
				else
				{
					text = text + GUIUtils.GetEquipmentSlotString((Equippable.EquipmentSlot)i) + GUIUtils.Comma();
				}
			}
		}
		if (equippable.PrimaryWeaponSlot || equippable.SecondaryWeaponSlot)
		{
			text = ((!equippable.BothPrimaryAndSecondarySlot) ? (text + GUIUtils.GetText(1806) + GUIUtils.Comma()) : (text + GUIUtils.GetText(1805) + GUIUtils.Comma()));
		}
		if (showUnique)
		{
			string uniqueString = equippable.GetUniqueString();
			if (!string.IsNullOrEmpty(uniqueString))
			{
				text = uniqueString + GUIUtils.Comma() + text;
			}
		}
		if (text.Length >= GUIUtils.Comma().Length)
		{
			text = text.Remove(text.Length - GUIUtils.Comma().Length);
		}
		return text;
	}

	public static string GetEffectTextFlat(MonoBehaviour obj, GameObject owner, bool verbose = true)
	{
		StringEffects stringEffects = new StringEffects();
		return (GetEffectText(obj, owner, stringEffects, verbose) + "\n" + AttackBase.StringEffects(stringEffects, targets: true)).Trim();
	}

	public static string GetEffectText(MonoBehaviour obj, GameObject owner, StringEffects stringEffects, bool verbose = true)
	{
		Equippable.UiDetailedContent = true;
		StringBuilder stringBuilder = new StringBuilder();
		Item component = obj.GetComponent<Item>();
		Phrase component2 = obj.GetComponent<Phrase>();
		GenericAbility component3 = obj.GetComponent<GenericAbility>();
		GenericTalent component4 = obj.GetComponent<GenericTalent>();
		AttackBase component5 = obj.GetComponent<AttackBase>();
		ItemMod component6 = obj.GetComponent<ItemMod>();
		EquipmentSoulbind component7 = obj.GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			Equippable equippable = component as Equippable;
			if ((bool)equippable)
			{
				if (verbose)
				{
					string equippableItemType = GetEquippableItemType(obj, null, equippable);
					if (equippableItemType.Length > 0)
					{
						stringBuilder.AppendLine(equippableItemType);
					}
				}
				if (equippable.RestrictedToClass != null && equippable.RestrictedToClass.Length != 0)
				{
					stringBuilder.Append(GUIUtils.GetText(1004));
					stringBuilder.Append(" ");
					stringBuilder.Append(TextUtils.FuncJoin((CharacterStats.Class cl) => GUIUtils.GetClassString(cl, Gender.Neuter), equippable.RestrictedToClass, GUIUtils.Comma()));
					stringBuilder.AppendLine();
				}
			}
		}
		else if ((bool)component6)
		{
			stringBuilder.AppendLine(component6.GetEffects(null, StatusEffectFormatMode.InspectWindow, stringEffects));
		}
		if ((bool)component7)
		{
			string text = "";
			if (component7.BindableClasses.Length != 0)
			{
				text = GUIUtils.Format(2034, TextUtils.FuncJoin((CharacterStats.Class cl) => GUIUtils.GetClassString(cl, Gender.Neuter), component7.BindableClasses, GUIUtils.Comma()));
			}
			if (component7.IsBound)
			{
				if (!CharacterStats.TryGetName(component7.BoundGuid, out var charName))
				{
					charName = (string.IsNullOrEmpty(component7.CachedBoundOwnerOverrideName) ? component7.CachedBoundOwnerName.GetText() : component7.CachedBoundOwnerOverrideName);
				}
				stringBuilder.AppendGuiFormat(2037, charName);
				if (!string.IsNullOrEmpty(text))
				{
					stringBuilder.AppendGuiFormat(1731, text);
				}
				stringBuilder.AppendLine();
			}
			else if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.AppendLine(text);
			}
		}
		if ((bool)component)
		{
			string text2 = NGUITools.StripColorSymbols(component.GetString(owner));
			if (text2.Length > 0)
			{
				stringBuilder.AppendLine(text2);
			}
		}
		if ((bool)component3)
		{
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(component3.GetString(summary: true, stringEffects, owner, StatusEffectFormatMode.InspectWindow)));
		}
		if ((bool)component4)
		{
			AIController aIController = (owner ? owner.GetComponent<AIController>() : null);
			bool onAnimalCompanion = (bool)aIController && aIController.SummonType == AIController.AISummonType.AnimalCompanion;
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(component4.GetString(owner, StatusEffectFormatMode.InspectWindow, onAnimalCompanion)));
		}
		if ((bool)component5 && !component && !component3)
		{
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(component5.GetString(null, owner, stringEffects)));
		}
		Equippable equippable2 = component as Equippable;
		if ((bool)equippable2)
		{
			if (equippable2.IsPrefab)
			{
				ItemMod[] itemMods = equippable2.ItemMods;
				foreach (ItemMod itemMod in itemMods)
				{
					if ((bool)itemMod)
					{
						if (itemMod.IsQualityMod)
						{
							stringBuilder.Append(itemMod.DisplayName.GetText() + ": ");
						}
						string value = NGUITools.StripColorSymbols(itemMod.GetEffects(null, StatusEffectFormatMode.InspectWindow, null).TrimEnd().Replace("\n", "\n\r"));
						if (string.IsNullOrEmpty(value))
						{
							value = "[url=itemmod://" + itemMod.name + "]" + itemMod.DisplayName.GetText() + "[/url]";
						}
						stringBuilder.AppendLine(value);
					}
				}
			}
			else
			{
				foreach (ItemModComponent attachedItemMod in equippable2.AttachedItemMods)
				{
					if (attachedItemMod.Mod.IsQualityMod)
					{
						stringBuilder.Append(attachedItemMod.Mod.DisplayName.GetText() + ": ");
					}
					string value2 = NGUITools.StripColorSymbols(attachedItemMod.Mod.GetEffects(attachedItemMod, StatusEffectFormatMode.InspectWindow, null).TrimEnd().Replace("\n", "\n\r"));
					if (string.IsNullOrEmpty(value2))
					{
						value2 = "[url=itemmod://" + attachedItemMod.Mod.name + "]" + attachedItemMod.Mod.DisplayName.GetText() + "[/url]";
					}
					stringBuilder.AppendLine(value2);
				}
			}
		}
		if ((bool)component2)
		{
			stringBuilder.AppendLine(NGUITools.StripColorSymbols(component2.GetString(null, owner, stringEffects)));
		}
		if ((bool)component && verbose && !component.IsQuestItem && !(component is CampingSupplies) && !(component is Currency))
		{
			stringBuilder.Append(GUIUtils.GetText(1499));
			stringBuilder.Append(": ");
			stringBuilder.AppendLine(GUIUtils.Format(466, component.GetDefaultSellValue()));
		}
		Equippable.UiDetailedContent = false;
		return stringBuilder.ToString();
	}

	public static bool ReloadWindowsForObject(GameObject go, bool soulbindUnlockMode)
	{
		bool result = false;
		for (int i = 0; i < s_ActiveWindows.Count; i++)
		{
			MonoBehaviour inspectionObject = s_ActiveWindows[i].InspectionObject;
			if ((bool)inspectionObject && inspectionObject.gameObject == go)
			{
				s_ActiveWindows[i].FlagNeedsReload();
				s_ActiveWindows[i].SoulbindUnlockMode = soulbindUnlockMode;
				result = true;
			}
		}
		for (int j = 0; j < s_ActiveLargeWindows.Count; j++)
		{
			MonoBehaviour inspectionObject2 = s_ActiveLargeWindows[j].InspectionObject;
			if ((bool)inspectionObject2 && inspectionObject2.gameObject == go)
			{
				s_ActiveLargeWindows[j].FlagNeedsReload();
				s_ActiveLargeWindows[j].SoulbindUnlockMode = soulbindUnlockMode;
				result = true;
			}
		}
		return result;
	}

	public void FlagNeedsReload()
	{
		m_NeedsReload = true;
	}

	public void Reload()
	{
		m_NeedsReload = false;
		DragPanel.ResetPosition();
		StringBuilder stringBuilder = new StringBuilder();
		GenericAbility genericAbility = null;
		AttackBase attackBase = null;
		if ((bool)InspectionObject)
		{
			attackBase = InspectionObject.GetComponent<AttackBase>();
			genericAbility = InspectionObject.GetComponent<GenericAbility>();
			if ((bool)attackBase)
			{
				attackBase.UICleanStatusEffects();
			}
			if ((bool)genericAbility)
			{
				genericAbility.UICleanStatusEffects();
			}
		}
		EnchantButton.gameObject.SetActive(value: false);
		CompareButton.gameObject.SetActive(value: false);
		LearnSpellButton.gameObject.SetActive(value: false);
		ExamineButton.gameObject.SetActive(value: false);
		SoulbindButton.gameObject.SetActive(value: false);
		EnchantParent.gameObject.SetActive(value: false);
		ItemTypeLabel.text = "";
		ImageTexture.mainTexture = null;
		LargeImageTexture.mainTexture = null;
		string path = "";
		TitleSepAnchor.widgetContainer = IconBackground;
		TitleSepAnchor.side = UIAnchor.Side.Right;
		if (InspectionObject != null)
		{
			CharacterStats component = InspectionObject.GetComponent<CharacterStats>();
			if (m_InspectStat != StatusEffect.ModifiedStat.NoEffect)
			{
				CharacterStats.SkillType skillType = StatusEffect.ModifiedStatToSkillType(m_InspectStat);
				CharacterStats.AttributeScoreType attributeScoreType = StatusEffect.ModifiedStatToAttributeScoreType(m_InspectStat);
				CharacterStats.DefenseType defenseType = StatusEffect.ModifiedStatToDefenseType(m_InspectStat);
				string text = "";
				if ((bool)component)
				{
					if (skillType != CharacterStats.SkillType.Count)
					{
						TitleLabel.text = GUIUtils.GetSkillTypeString(skillType);
						text = component.CalculateSkill(skillType) + GUIUtils.Format(1731, UICharacterSheetContentManager.GetSkillEffectsInverted(component, skillType, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = GUIUtils.GetSkillTypeDescriptionString(skillType);
					}
					else if (attributeScoreType != CharacterStats.AttributeScoreType.Count)
					{
						TitleLabel.text = GUIUtils.GetAttributeScoreTypeString(attributeScoreType);
						text = component.GetAttributeScore(attributeScoreType) + GUIUtils.Format(1731, UICharacterSheetContentManager.GetAttributeEffectsInverted(component, attributeScoreType, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = GUIUtils.GetAttributeScoreDescriptionString(attributeScoreType);
					}
					else if (defenseType != CharacterStats.DefenseType.None)
					{
						TitleLabel.text = GUIUtils.GetDefenseTypeString(defenseType);
						text = component.CalculateDefense(defenseType) + GUIUtils.Format(1731, UICharacterSheetContentManager.GetDefenseEffectsInverted(component, defenseType, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = GUIUtils.GetDefenseTypeDescription(defenseType);
					}
					else if (m_InspectStat == StatusEffect.ModifiedStat.InterruptBonus)
					{
						TitleLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 173);
						text = component.ComputeInterruptHelper().ToString("#0") + GUIUtils.Format(1731, UICharacterSheetContentManager.GetInterruptEffectsInverted(component, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 174);
					}
					else if (m_InspectStat == StatusEffect.ModifiedStat.ConcentrationBonus)
					{
						TitleLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 159);
						text = component.ComputeConcentrationHelper().ToString("#0") + GUIUtils.Format(1731, UICharacterSheetContentManager.GetConcentrationEffectsInverted(component, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 160);
					}
					else if (m_InspectStat == StatusEffect.ModifiedStat.DamageThreshhold)
					{
						TitleLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 157);
						if (m_InspectDamageType != DamagePacket.DamageType.All && m_InspectDamageType != DamagePacket.DamageType.None)
						{
							TitleLabel.text += GUIUtils.Format(1731, GUIUtils.GetDamageTypeString(m_InspectDamageType));
						}
						text = component.CalcDT(m_InspectDamageType, isVeilPiercing: false).ToString("#0") + GUIUtils.Format(1731, UICharacterSheetContentManager.GetDamageThresholdEffectsInverted(component, m_InspectDamageType, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 158);
					}
					else if (m_InspectStat == StatusEffect.ModifiedStat.Damage)
					{
						TitleLabel.text = GUIUtils.GetText(428);
						Equipment component2 = InspectionObject.GetComponent<Equipment>();
						AttackBase attack = ((!component2) ? null : (m_InspectOffhand ? component2.SecondaryAttack : component2.PrimaryAttack));
						DamageInfo damageInfo = new DamageInfo(null, 0f, attack);
						component.AdjustDamageForUi(damageInfo);
						text = damageInfo.GetAdjustedDamageRangeString() + GUIUtils.Format(1731, UICharacterSheetContentManager.GetDamageEffectsInverted(component, attack, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 194);
					}
					else if (m_InspectStat == StatusEffect.ModifiedStat.Accuracy)
					{
						TitleLabel.text = GUIUtils.GetText(369);
						Equipment component3 = InspectionObject.GetComponent<Equipment>();
						AttackBase attack2 = ((!component3) ? null : (m_InspectOffhand ? component3.SecondaryAttack : component3.PrimaryAttack));
						text = component.CalculateAccuracyForUi(attack2, null, null) + GUIUtils.Format(1731, UICharacterSheetContentManager.GetAccuracyEffectsInverted(component, attack2, GUIUtils.Comma(), UIGlobalColor.LinkStyle.NONE));
						FlavorTextLabel.text = StringTableManager.GetText(DatabaseString.StringTableType.Cyclopedia, 84);
					}
				}
				string text2 = CharacterStats.Name(component) + ": " + text;
				if ((bool)Glossary.Instance)
				{
					text2 = Glossary.Instance.AddUrlTags(text2);
				}
				EffectTextLabel.text = text2;
			}
			else
			{
				Item component4 = InspectionObject.GetComponent<Item>();
				Phrase component5 = InspectionObject.GetComponent<Phrase>();
				GenericTalent component6 = InspectionObject.GetComponent<GenericTalent>();
				EquipmentSoulbind component7 = InspectionObject.GetComponent<EquipmentSoulbind>();
				ItemMod component8 = InspectionObject.GetComponent<ItemMod>();
				if ((bool)component4)
				{
					LargeImageTexture.alpha = 1f;
					LargeImageTexture.mainTexture = component4.GetIconLargeTexture();
					LargeImageTexture.MakePixelPerfect();
					TitleLabel.text = component4.Name;
					if (component4.DescriptionText.IsValidString)
					{
						stringBuilder.AppendLine(component4.DescriptionText.GetText());
						stringBuilder.AppendLine();
					}
				}
				if ((bool)component7 && (bool)ObjectOwner)
				{
					SoulbindButton.gameObject.SetActive(!component7.IsBound || !component7.CannotUnbind);
				}
				else
				{
					SoulbindButton.gameObject.SetActive(value: false);
				}
				if ((bool)component7)
				{
					SoulbindButton.Label.GetComponent<GUIStringLabel>().SetString(component7.IsBound ? 2031 : 2030);
					string extraDescription = component7.GetExtraDescription();
					if (!string.IsNullOrEmpty(extraDescription))
					{
						stringBuilder.AppendLine(extraDescription);
						stringBuilder.AppendLine();
					}
					path = component7.GetPencilSketch();
				}
				if ((bool)InspectionObject.GetComponent<QuestAsset>())
				{
					ExamineButton.gameObject.SetActive(value: true);
				}
				Equippable equippable = component4 as Equippable;
				if ((bool)equippable)
				{
					BackerContent component9 = InspectionObject.GetComponent<BackerContent>();
					if ((bool)component9)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
						stringBuilder.Append(GUIUtils.GetText(994));
						stringBuilder.Append(" ");
						stringBuilder.Append(component9.BackerName);
					}
					bool flag = ((bool)InspectionObject.GetComponent<Shield>() || (bool)InspectionObject.GetComponent<Armor>() || equippable is Weapon) && !component7;
					bool flag2 = flag && !equippable.IsPrefab;
					if ((bool)equippable.EquippedOwner)
					{
						CharacterStats component10 = equippable.EquippedOwner.GetComponent<CharacterStats>();
						if ((bool)component10)
						{
							if (component10.IsEquipmentLocked)
							{
								flag2 = false;
							}
							else
							{
								Equipment component11 = component10.GetComponent<Equipment>();
								if ((bool)component11 && component11.IsSlotLocked(equippable.EquippedSlot))
								{
									flag2 = false;
								}
							}
						}
					}
					CharacterStats characterStats = ((!UILootManager.Instance || !UILootManager.Instance.IsVisible) ? UIInventoryManager.Instance.SelectedCharacter : UILootManager.Instance.SelectedCharacter);
					if ((bool)characterStats && !NoCompare)
					{
						Equipment component12 = characterStats.GetComponent<Equipment>();
						IEnumerable<Item.UIEquippedItem> comparisonTargets = UIInventoryEquipment.GetComparisonTargets(InspectionObject.GetComponent<Equippable>(), component12);
						CompareButton.gameObject.SetActive((bool)component12 && comparisonTargets.Any() && !component12.CurrentItems.Contains(equippable));
					}
					else
					{
						CompareButton.gameObject.SetActive(value: false);
					}
					if (flag && (bool)LblEnchantValue && (bool)LblEnchantValue)
					{
						LblEnchantLabel.text = GUIUtils.GetText(1987) + ": ";
						LblEnchantValue.text = GUIUtils.Format(451, equippable.TotalItemModValue(), ItemMod.MaximumModValue);
					}
					EnchantParent.gameObject.SetActive(flag);
					bool active = flag2 && !m_IsStore && !m_NoEnchant && !GameState.InCombat;
					EnchantButton.gameObject.SetActive(active);
					string equippableItemType = GetEquippableItemType(InspectionObject, null, equippable);
					if (equippableItemType.Length > 0)
					{
						ItemTypeLabel.text = equippableItemType;
					}
				}
				else if ((bool)component5)
				{
					ImageTexture.alpha = 1f;
					if ((bool)component5.Icon)
					{
						ImageTexture.mainTexture = component5.Icon;
					}
					ImageTexture.MakePixelPerfect();
					TitleLabel.text = component5.DisplayName.GetText();
					if (component5.Description.IsValidString)
					{
						stringBuilder.AppendLine(component5.Description.GetText());
					}
				}
				else if ((bool)genericAbility)
				{
					ImageTexture.alpha = 1f;
					if ((bool)genericAbility.Icon)
					{
						ImageTexture.mainTexture = genericAbility.Icon;
					}
					ImageTexture.MakePixelPerfect();
					if (!component4)
					{
						TitleLabel.text = GenericAbility.Name(genericAbility);
					}
					if (genericAbility.Description.IsValidString && !component4)
					{
						stringBuilder.AppendLine(genericAbility.Description.GetText());
					}
					if ((bool)attackBase)
					{
						ItemTypeLabel.text = attackBase.GetKeywordsString();
					}
					LearnSpellButton.gameObject.SetActive(LearnSpellAllowed);
				}
				else if ((bool)component6)
				{
					ImageTexture.alpha = 1f;
					if ((bool)component6.Icon)
					{
						ImageTexture.mainTexture = component6.Icon;
					}
					ImageTexture.MakePixelPerfect();
					if (component6.Description.IsValidString)
					{
						stringBuilder.AppendLine(component6.Description.GetText());
					}
					TitleLabel.text = component6.Name(ObjectOwner);
				}
				else if ((bool)component8)
				{
					TitleLabel.text = component8.DisplayName.GetText();
				}
				else
				{
					BackerContent component13 = InspectionObject.GetComponent<BackerContent>();
					if ((bool)component13)
					{
						ImageTexture.alpha = 0f;
						TitleLabel.text = component13.BackerName;
						stringBuilder.AppendLine(component13.BackerDescription.GetText());
						stringBuilder.AppendLine();
						stringBuilder.AppendLine();
						stringBuilder.Append(GUIUtils.GetText(994, CharacterStats.GetGender(component13)));
						stringBuilder.Append(' ');
						stringBuilder.Append(component13.BackerName);
					}
					if ((bool)component)
					{
						TitleLabel.text = component.Name();
					}
				}
				StringEffects stringEffects = new StringEffects();
				string text3 = GetEffectText(InspectionObject, ObjectOwner, stringEffects, verbose: false).TrimEnd();
				StringEffectDisplay.Load(stringEffects);
				if (!StringEffectDisplay.Empty)
				{
					text3 = text3 + "\n" + GUIUtils.GetText(1604);
				}
				if ((bool)component4 && !component4.IsQuestItem && !(component4 is CampingSupplies) && !(component4 is Currency))
				{
					string text4 = "";
					if (ItemTypeLabel.text.Length > 0)
					{
						text4 += "\n";
					}
					text4 = text4 + GUIUtils.GetText(1499) + ": " + GUIUtils.Format(466, component4.GetDefaultSellValue());
					ItemTypeLabel.text += text4;
				}
				if ((bool)Glossary.Instance)
				{
					text3 = Glossary.Instance.AddUrlTags(text3);
				}
				EffectTextLabel.text = text3.Trim();
				if ((bool)Goals)
				{
					Goals.Set(component7, SoulbindUnlockMode);
				}
				FlavorTextLabel.text = stringBuilder.ToString().TrimEnd();
			}
			if (NoDescription)
			{
				FlavorTextLabel.text = "";
			}
			if ((bool)PencilSketch)
			{
				PencilSketch.SetPath(path);
			}
			if ((bool)LargeImageTexture.mainTexture)
			{
				ImageTexture.mainTexture = null;
			}
			TitleSepAnchor.pixelOffset.y = 0f;
			if ((bool)ImageTexture.mainTexture)
			{
				IconBackground.transform.localScale = new Vector3(ImageTexture.transform.localScale.x + 12f, ImageTexture.transform.localScale.y + 12f, 1f);
				if (TitleLabel.processedText.Contains("\n"))
				{
					TitleSepAnchor.pixelOffset.y = 0f - (float)TitleLabel.font.size;
				}
				TitleSepAnchor.widgetContainer = IconBackground;
			}
			else if ((bool)LargeImageTexture.mainTexture)
			{
				Vector3 localScale = LargeImageTexture.transform.localScale;
				if (localScale.x > 78f)
				{
					float num = localScale.y / localScale.x;
					localScale.x = 78f;
					localScale.y = localScale.x * num;
				}
				else if (localScale.y > 78f)
				{
					float num2 = localScale.x / localScale.y;
					localScale.y = 78f;
					localScale.x = localScale.y * num2;
				}
				LargeImageTexture.transform.localScale = localScale;
				LargeImageBackground.transform.localScale = new Vector3(localScale.x + 12f, localScale.y + 12f, 1f);
				if (TitleLabel.processedText.Contains("\n"))
				{
					TitleSepAnchor.pixelOffset.y = (0f - (float)TitleLabel.font.size) * 0.5f;
				}
				TitleSepAnchor.widgetContainer = LargeImageBackground;
			}
			else
			{
				TitleSepAnchor.widgetContainer = IconBackground;
				TitleSepAnchor.side = UIAnchor.Side.Left;
			}
			ImageTexture.alpha = (ImageTexture.mainTexture ? 1f : 0f);
			LargeImageTexture.alpha = (LargeImageTexture.mainTexture ? 1f : 0f);
			IconBackground.alpha = (ImageTexture.mainTexture ? (2f / 3f) : 0f);
			LargeImageBackground.alpha = (LargeImageTexture.mainTexture ? (2f / 3f) : 0f);
			UIWidget widget = ((ImageTexture.alpha > 0f) ? ImageTexture : LargeImageTexture);
			TitleLabel.GetComponent<UIShrinkOpposingWidget>().Widget = widget;
			EffectTextLabel.gameObject.SetActive(!string.IsNullOrEmpty(EffectTextLabel.text));
			UIWidgetUtils.UpdateDependents(base.gameObject, 2);
			ButtonsGrid.Reposition();
			LayoutScrollArea.Reposition();
			DragPanel.ResetPosition();
		}
		else
		{
			TitleLabel.text = "";
			EffectTextLabel.text = "";
			ImageTexture.alpha = 0f;
			DragPanel.ResetPosition();
			ButtonsGrid.Reposition();
		}
	}
}

using System;
using System.Linq;
using AI.Achievement;
using AI.Player;
using UnityEngine;

public class UIAbilityBarButton : MonoBehaviour
{
	public class EmptyHotkey : ITooltipContent
	{
		public string GetTooltipContent(GameObject owner)
		{
			return GUIUtils.GetText(1664);
		}

		public string GetTooltipName(GameObject owner)
		{
			return GUIUtils.GetText(1663);
		}

		public Texture GetTooltipIcon()
		{
			return null;
		}
	}

	private GameObject m_Target;

	private GameObject m_TargetParent;

	private InventoryItem m_TargetItem;

	private int m_TargetIndex = -1;

	private UIAbilityBarButtonSet.AbilityButtonAction m_Action;

	public UITexture Icon;

	public UITexture TopIcon;

	public UILabel Label;

	public UIWidget Background;

	public UISprite BackgroundMod;

	public UIWidget Overlay;

	public UILabel CountLabel;

	public GameObject CountShadow;

	public UILabel HotkeyLabel;

	private UIImageButtonRevised m_LabelButton;

	private UIImageButtonRevised m_IconButton;

	public Color DisabledColor = Color.red;

	public FanFillTimer FanFill;

	public GameObject MasteryOverlayObject;

	private int m_lastActiveAbilitesCount;

	private GameObject m_ModalActiveVfx;

	private Equippable m_ListenedEquipItem;

	public GameObject[] MultiTargets { get; set; }

	public UIImageButtonRevised ImageButton
	{
		get
		{
			if ((bool)Icon)
			{
				m_IconButton = Icon.GetComponent<UIImageButtonRevised>();
			}
			return m_IconButton;
		}
	}

	public UIAbilityBarButtonSet Owner { get; set; }

	public UIAbilityBarButtonSet[] SubrowSets { get; private set; }

	public bool Disabled { get; private set; }

	public bool Hovered { get; private set; }

	public InventoryItem TargetItem => m_TargetItem;

	public GenericAbility TargetAbility
	{
		get
		{
			if (!m_Target)
			{
				return null;
			}
			return m_Target.GetComponent<GenericAbility>();
		}
	}

	public GameObject Target => m_Target;

	public int TargetIndex => m_TargetIndex;

	public UIAbilityBarButtonSet.AbilityButtonAction TargetAction => m_Action;

	public Item TargetAnyItem
	{
		get
		{
			if (TargetItem != null)
			{
				return TargetItem.baseItem;
			}
			if ((bool)Target && TargetAction == UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET)
			{
				Equipment component = Target.GetComponent<Equipment>();
				if ((bool)component)
				{
					WeaponSet weaponSet = component.WeaponSets[m_TargetIndex];
					return Owner.GetButtonIndex(this) switch
					{
						0 => weaponSet.PrimaryWeapon, 
						1 => weaponSet.SecondaryWeapon, 
						_ => null, 
					};
				}
				return null;
			}
			return null;
		}
	}

	public bool ActionIsSubrow()
	{
		if (m_Action != UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS && m_Action != UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU && m_Action != UIAbilityBarButtonSet.AbilityButtonAction.HOTKEY_SUBMENU && m_Action != UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS && m_Action != UIAbilityBarButtonSet.AbilityButtonAction.MASTERED_SUBMENU)
		{
			return m_Action == UIAbilityBarButtonSet.AbilityButtonAction.WATCHER_SUBMENU;
		}
		return true;
	}

	public bool ActionIsSubrowWithContent()
	{
		return CharacterHasSubrowContent(m_Action, m_Target, m_TargetIndex);
	}

	public static bool CharacterHasSubrowContent(UIAbilityBarButtonSet.AbilityButtonAction action, GameObject targetCharacter, int targetIndex)
	{
		switch (action)
		{
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS:
		{
			CharacterStats component4 = ComponentUtils.GetComponent<CharacterStats>(targetCharacter);
			if ((bool)component4)
			{
				for (int k = 0; k < component4.ActiveAbilities.Count; k++)
				{
					if (UIAbilityBarButtonSet.ShowOnSpellBar(component4.ActiveAbilities[k], component4, targetIndex))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU:
			return true;
		case UIAbilityBarButtonSet.AbilityButtonAction.HOTKEY_SUBMENU:
			return true;
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS:
		{
			CharacterStats component2 = ComponentUtils.GetComponent<CharacterStats>(targetCharacter);
			if ((bool)component2)
			{
				for (int j = 0; j < component2.ActiveAbilities.Count; j++)
				{
					GenericAbility genericAbility2 = component2.ActiveAbilities[j];
					if ((bool)genericAbility2 && GenericAbility.AbilityTypeIsAnyEquipment(genericAbility2.EffectType) && !genericAbility2.Passive && genericAbility2.IsVisibleOnUI && !genericAbility2.TriggeredAutomatically)
					{
						return true;
					}
				}
			}
			if (UIAbilityBarButtonSet.HasPassiveEquipmentAbilityButtons(ComponentUtils.GetComponent<Equipment>(targetCharacter)))
			{
				return true;
			}
			QuickbarInventory component3 = ComponentUtils.GetComponent<QuickbarInventory>(targetCharacter);
			if ((bool)component3 && component3.ItemList.Count > 0)
			{
				return true;
			}
			return false;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.MASTERED_SUBMENU:
		{
			CharacterStats component5 = ComponentUtils.GetComponent<CharacterStats>(targetCharacter);
			if ((bool)component5)
			{
				for (int l = 0; l < component5.ActiveAbilities.Count; l++)
				{
					GenericAbility genericAbility3 = component5.ActiveAbilities[l];
					if ((bool)genericAbility3 && genericAbility3.MasteryLevel > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.WATCHER_SUBMENU:
		{
			CharacterStats component = ComponentUtils.GetComponent<CharacterStats>(targetCharacter);
			if ((bool)component)
			{
				for (int i = 0; i < component.ActiveAbilities.Count; i++)
				{
					GenericAbility genericAbility = component.ActiveAbilities[i];
					if ((bool)genericAbility && genericAbility.IsWatcherAbility)
					{
						return true;
					}
				}
			}
			return false;
		}
		default:
			return false;
		}
	}

	private void OnEnable()
	{
		Update();
	}

	private void Start()
	{
		m_LabelButton = Label.GetComponent<UIImageButtonRevised>();
		UIEventListener uIEventListener = UIEventListener.Get(Background.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnChildHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Background.gameObject);
		uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		UIImageButtonRevised component = Icon.GetComponent<UIImageButtonRevised>();
		if ((bool)component)
		{
			component.SetWidgetRootPosition(new Vector3(0f, 0f, -1f));
		}
		Icon.transform.localPosition = new Vector3(0f, 0f, -1f);
		PartyMemberAI.OnAnySelectionChanged += OnSelectionChanged;
	}

	private void OnDestroy()
	{
		PartyMemberAI.OnAnySelectionChanged -= OnSelectionChanged;
	}

	private void Update()
	{
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		CharacterStats characterStats = null;
		if ((bool)selectedForBars)
		{
			characterStats = selectedForBars.GetComponent<CharacterStats>();
		}
		bool flag = false;
		RefreshFanFill(reset: false);
		if (Hovered && GameInput.Instance.LastKeyUp.KeyCode != 0)
		{
			KeyControl hotkey = new KeyControl(GameInput.Instance.LastKeyUp);
			hotkey.RemoveModifiers(GameState.Controls.Controls[46]);
			if ((bool)selectedForBars)
			{
				PartyMemberAI component = selectedForBars.GetComponent<PartyMemberAI>();
				if ((bool)component && (bool)m_Target)
				{
					GenericAbility ability = m_Target.GetComponent<GenericAbility>();
					if ((bool)ability)
					{
						GenericAbility genericAbility = characterStats.ActiveAbilities.Where((GenericAbility abil) => CharacterHotkeyBindings.GetDictionaryKey(abil) == CharacterHotkeyBindings.GetDictionaryKey(ability)).FirstOrDefault();
						if ((bool)genericAbility)
						{
							component.BindHotkey(hotkey, genericAbility);
						}
					}
					return;
				}
			}
		}
		if (ActionIsSubrow())
		{
			Disabled = !ActionIsSubrowWithContent();
			if (Hovered && !Disabled && (GameInput.MouseDelta.y <= 0f || !Owner.SubrowActive()))
			{
				ShowSubrow(locked: false);
			}
		}
		else if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY || m_Action == UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED)
		{
			GenericAbility genericAbility2 = (m_Target ? m_Target.GetComponent<GenericAbility>() : null);
			Disabled = !genericAbility2 || !genericAbility2.ReadyForUI;
		}
		else if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.READONLY_ABILITY)
		{
			Disabled = true;
		}
		else
		{
			Disabled = false;
		}
		Overlay.color = ((Disabled && !FanFill.IsRunning) ? DisabledColor : Color.white);
		if (SubrowSets != null && SubrowSets[0] != null && SubrowSets[0].SubrowOwner == this)
		{
			bool flag2 = true;
			UIAbilityBarButtonSet[] subrowSets = SubrowSets;
			foreach (UIAbilityBarButtonSet uIAbilityBarButtonSet in subrowSets)
			{
				if (uIAbilityBarButtonSet != null && (uIAbilityBarButtonSet.Locked || uIAbilityBarButtonSet.Hovered))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2 && Input.GetMouseButtonUp(0))
			{
				HideSubrow();
				UIAbilityTooltip.IgnoreHide = false;
				HideTooltip();
			}
			if (!flag2 && m_Action == UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS && (bool)m_Target)
			{
				CharacterStats component2 = m_Target.GetComponent<CharacterStats>();
				if ((bool)component2)
				{
					if (component2.ActiveAbilities.Count != m_lastActiveAbilitesCount)
					{
						SetUpSubspells(SubrowSets[0]);
					}
					m_lastActiveAbilitesCount = component2.ActiveAbilities.Count;
				}
			}
		}
		if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS)
		{
			int num = 0;
			if ((bool)SpellMax.Instance && characterStats != null && m_TargetIndex >= 1)
			{
				num = SpellMax.Instance.GetSpellCastMax(characterStats.gameObject, m_TargetIndex) - characterStats.SpellCastCount[m_TargetIndex - 1];
			}
			if (characterStats != null && m_TargetIndex >= 1 && (num <= 0 || !characterStats.CanCastSpells))
			{
				if (FanFill.IsRunning)
				{
					Icon.material.SetFloat("_Saturation", 0.1f);
					Overlay.color = DisabledColor;
				}
			}
			else if (Disabled)
			{
				Icon.material.SetFloat("_Saturation", 0.1f);
				Overlay.color = DisabledColor;
			}
			else
			{
				Icon.material.SetFloat("_Saturation", 1f);
				Overlay.color = Color.white;
				flag = IsSubspellActive(characterStats, m_TargetIndex);
			}
		}
		if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY || m_Action == UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED || m_Action == UIAbilityBarButtonSet.AbilityButtonAction.READONLY_ABILITY)
		{
			GenericAbility genericAbility3 = null;
			if ((bool)m_Target)
			{
				genericAbility3 = m_Target.GetComponent<GenericAbility>();
			}
			if (genericAbility3 != null)
			{
				bool flag3 = (genericAbility3.WhyNotReady & GenericAbility.NotReadyValue.AtMaxPer) != 0;
				Icon.material.SetFloat("_Saturation", flag3 ? 0.1f : 1f);
				AIState aIState = null;
				bool flag4 = false;
				if ((bool)genericAbility3.OwnerAI)
				{
					aIState = genericAbility3.OwnerAI.StateManager.CurrentState;
					if (aIState is HitReact || aIState is PathToPosition)
					{
						aIState = genericAbility3.OwnerAI.StateManager.QueuedState;
					}
					flag4 = (genericAbility3.OwnerAI.StateManager.FindState(typeof(TargetedAttack)) is TargetedAttack targetedAttack && targetedAttack.Ability == genericAbility3) || genericAbility3.OwnerAI.QueuedAbility == genericAbility3;
				}
				flag = (((bool)genericAbility3 && (genericAbility3.UiActivated || (aIState != null && aIState.CurrentAbility == genericAbility3) || (GameState.s_playerCharacter != null && GameState.s_playerCharacter.IsCasting(genericAbility3.OwnerAI, genericAbility3)) || flag4)) ? true : false);
				if (((bool)genericAbility3 && !(genericAbility3 is GenericSpell)) || genericAbility3.MasteryLevel > 0)
				{
					Chant chant = genericAbility3 as Chant;
					if ((bool)chant)
					{
						CountLabel.gameObject.SetActive(value: true);
						CountLabel.text = TextUtils.IndexToAlphabet(Mathf.Max(chant.UiIndex, 0));
					}
					else
					{
						int num2 = Mathf.Max(0, genericAbility3.UsesLeft());
						if (num2 == int.MaxValue)
						{
							CountLabel.gameObject.SetActive(value: false);
						}
						else
						{
							CountLabel.gameObject.SetActive(value: true);
							CountLabel.text = num2.ToString();
						}
					}
				}
				else
				{
					CountLabel.gameObject.SetActive(value: false);
				}
			}
		}
		else if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.QUICK_ITEM)
		{
			if (m_TargetItem != null && (bool)m_TargetItem.baseItem)
			{
				Consumable component3 = m_TargetItem.baseItem.GetComponent<Consumable>();
				if ((bool)component3)
				{
					Disabled = !component3.CanUse(characterStats);
				}
				else
				{
					Disabled = !(m_TargetItem.baseItem is Equippable);
				}
				if (m_TargetItem.stackSize == 0)
				{
					if ((bool)UIAbilityBar.Instance)
					{
						UIAbilityBar.Instance.Refresh();
					}
				}
				else if (m_TargetItem.stackSize > 1)
				{
					CountLabel.gameObject.SetActive(value: true);
					CountLabel.text = m_TargetItem.stackSize.ToString();
				}
				else if ((bool)component3)
				{
					CountLabel.gameObject.SetActive(value: true);
					CountLabel.text = component3.UsesRemaining.ToString();
				}
				else
				{
					CountLabel.gameObject.SetActive(value: false);
				}
			}
			else
			{
				CountLabel.gameObject.SetActive(value: false);
			}
		}
		else if (characterStats != null && m_Action == UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS)
		{
			int num3 = 0;
			if ((bool)SpellMax.Instance)
			{
				num3 = SpellMax.Instance.GetSpellCastMax(characterStats.gameObject, m_TargetIndex);
			}
			if (num3 == int.MaxValue)
			{
				CountLabel.gameObject.SetActive(value: false);
			}
			else
			{
				CountLabel.gameObject.SetActive(value: true);
				CountLabel.text = Mathf.Max(0, num3 - characterStats.SpellCastCount[m_TargetIndex - 1]).ToString();
			}
		}
		else if (m_Action == UIAbilityBarButtonSet.AbilityButtonAction.AI_TOGGLE)
		{
			CountLabel.gameObject.SetActive(value: false);
			if (m_Target != null)
			{
				PartyMemberAI component4 = m_Target.GetComponent<PartyMemberAI>();
				flag = (bool)component4 && component4.UseInstructionSet;
			}
			else if (MultiTargets != null)
			{
				flag = MultiTargets.Length != 0;
				for (int j = 0; j < MultiTargets.Length; j++)
				{
					PartyMemberAI partyMemberAI = (MultiTargets[j] ? MultiTargets[j].GetComponent<PartyMemberAI>() : null);
					if ((bool)partyMemberAI && !partyMemberAI.UseInstructionSet)
					{
						flag = false;
						break;
					}
				}
			}
		}
		else if (m_Action != UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET && m_Action != UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU)
		{
			CountLabel.gameObject.SetActive(value: false);
		}
		if ((bool)CountShadow)
		{
			CountShadow.gameObject.SetActive(CountLabel.gameObject.activeSelf && !string.IsNullOrEmpty(CountLabel.text));
		}
		if (flag)
		{
			if ((bool)m_ModalActiveVfx && !m_ModalActiveVfx.activeSelf)
			{
				ResetTrails();
			}
			if (!m_ModalActiveVfx)
			{
				m_ModalActiveVfx = UIAbilityBar.Instance.InstantiateModalVfx(base.transform, new Vector3((0f - Icon.transform.localScale.x) / 2f - 1f, (0f - Icon.transform.localScale.y) / 2f - 1f, -3f));
			}
			m_ModalActiveVfx.SetActive(value: true);
		}
		else if ((bool)m_ModalActiveVfx)
		{
			m_ModalActiveVfx.SetActive(value: false);
		}
		if ((bool)m_LabelButton)
		{
			m_LabelButton.enabled = !Disabled;
		}
		if ((bool)ImageButton)
		{
			ImageButton.enabled = !Disabled;
		}
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		if ((bool)m_ModalActiveVfx)
		{
			m_ModalActiveVfx.SetActive(value: false);
		}
	}

	public void ShowModItemBackground(UIInventoryGridItem.GlowType bgGlowType)
	{
		if (!(BackgroundMod == null))
		{
			BackgroundMod.spriteName = UIInventoryGridItem.GetModBgSpriteName(bgGlowType);
			BackgroundMod.alpha = ((bgGlowType != 0) ? 1f : 0f);
		}
	}

	public void RefreshHotkey()
	{
		KeyControl keyControl = default(KeyControl);
		PartyMemberAI selectedAIForBars = UIAbilityBar.GetSelectedAIForBars();
		if ((bool)selectedAIForBars && (bool)m_Target)
		{
			GenericAbility component = m_Target.GetComponent<GenericAbility>();
			if ((bool)component)
			{
				keyControl = selectedAIForBars.GetHotkeyFor(component);
			}
		}
		if (keyControl.KeyCode == KeyCode.None)
		{
			HotkeyLabel.text = "";
		}
		else
		{
			HotkeyLabel.text = keyControl.ToString();
		}
	}

	public void ShowSubrow(bool locked)
	{
		UIAbilityBarButtonSet[] array = null;
		if (m_Action != UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU)
		{
			array = ((m_Action != UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS) ? Owner.ShowSubrowIfNew(this, 1) : Owner.ShowSubrowIfNew(this, 6));
		}
		else
		{
			Equipment component = m_Target.GetComponent<Equipment>();
			CharacterStats component2 = m_Target.GetComponent<CharacterStats>();
			if ((bool)component && (!component2 || !component2.IsEquipmentLocked))
			{
				array = Owner.ShowSubrowIfNew(this, component.NumWeaponSets);
			}
		}
		if (array != null)
		{
			SubrowSets = array;
			SetSubrowLocked(locked);
			switch (m_Action)
			{
			case UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS:
				SetUpSubspells(SubrowSets[0]);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU:
				SetUpWeaponSets(SubrowSets);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.HOTKEY_SUBMENU:
				SetUpHotkeys(SubrowSets[0]);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS:
				SetUpItems(SubrowSets);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.MASTERED_SUBMENU:
				SetUpMasteredSpells(SubrowSets[0]);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.WATCHER_SUBMENU:
				SetUpWatcherAbilities(SubrowSets[0]);
				break;
			case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET:
			case UIAbilityBarButtonSet.AbilityButtonAction.EMPTY:
			case UIAbilityBarButtonSet.AbilityButtonAction.AI_TOGGLE:
			case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED:
				break;
			}
		}
	}

	public void HideSubrow()
	{
		if ((bool)UIAbilityBar.Instance && SubrowSets != null && SubrowSets.Length != 0)
		{
			UIAbilityBar.Instance.HideSubrow(SubrowSets[0].SubLevel);
		}
	}

	private void OnChildHover(GameObject go, bool over)
	{
		OnHover(over);
	}

	private void OnChildTooltip(GameObject go, bool show)
	{
		OnTooltip(show);
	}

	private void OnDisable()
	{
		UIAbilityTooltip.IgnoreHide = false;
		if (UIAbilityBar.HoveredButton == this)
		{
			UIAbilityBar.HoveredButton = null;
		}
		if (UIAbilityBar.HoveredButton == this)
		{
			HideTooltip();
		}
		Hovered = false;
	}

	private void OnHover(bool over)
	{
		UIAbilityBar.Instance.CancelSelection();
		TriggerHover(over);
	}

	public void TriggerHover(bool over)
	{
		if (over)
		{
			UIAbilityBar.HoveredButton = this;
			if (UIAbilityTooltip.VisibleThisFrame)
			{
				ShowTooltip();
				UIAbilityTooltip.IgnoreHide = true;
			}
		}
		else
		{
			if (UIAbilityBar.HoveredButton == this)
			{
				UIAbilityBar.HoveredButton = null;
			}
			HideTooltip();
		}
		Hovered = over;
	}

	private void OnTooltip(bool isOver)
	{
		if (isOver)
		{
			ShowTooltip();
		}
		else
		{
			HideTooltip();
		}
	}

	public void HideTooltip()
	{
		UIAbilityTooltip.GlobalHide();
		UIActionBarTooltip.GlobalHide();
	}

	public void ShowTooltip()
	{
		switch (m_Action)
		{
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY:
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED:
		case UIAbilityBarButtonSet.AbilityButtonAction.READONLY_ABILITY:
			UIAbilityTooltip.GlobalShow(Background, UIAbilityBar.GetSelectedForBars(), m_Target.GetComponent<GenericAbility>());
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.QUICK_ITEM:
			UIAbilityTooltip.GlobalShow(Background, UIAbilityBar.GetSelectedForBars(), m_Target.GetComponent<Item>());
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.TALK_TO:
			UIActionBarTooltip.GlobalShow(Background, GUIUtils.GetText(1098) + " " + CharacterStats.Name(m_Target));
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.CLASS_EDIT:
		{
			GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
			if ((bool)selectedForBars)
			{
				switch (selectedForBars.GetComponent<CharacterStats>().CharacterClass)
				{
				case CharacterStats.Class.Wizard:
					UIActionBarTooltip.GlobalShow(Background, GUIUtils.GetText(1506));
					break;
				case CharacterStats.Class.Chanter:
					UIActionBarTooltip.GlobalShow(Background, GUIUtils.GetText(1508));
					break;
				}
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET:
		{
			WeaponSet weaponSet = m_Target.GetComponent<Equipment>().WeaponSets[m_TargetIndex];
			Equippable equippable = null;
			switch (Owner.GetButtonIndex(this))
			{
			case 0:
				equippable = weaponSet.PrimaryWeapon;
				break;
			case 1:
				equippable = weaponSet.SecondaryWeapon;
				break;
			}
			if ((bool)equippable)
			{
				UIAbilityTooltipManager.Instance.Show(0, Background, UIAbilityBar.GetSelectedForBars(), equippable);
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.AI_TOGGLE:
			if ((bool)m_Target)
			{
				PartyMemberAI component = m_Target.GetComponent<PartyMemberAI>();
				string text2;
				if ((bool)component && (bool)component.InstructionSet)
				{
					string text = component.InstructionSet.DisplayName.GetText();
					text2 = ((!component.UseInstructionSet) ? GUIUtils.Format(2110, text) : GUIUtils.Format(2109, text));
				}
				else
				{
					text2 = GUIUtils.GetText(2113);
				}
				UIActionBarTooltip.GlobalShow(Background, text2 + ". " + GUIUtils.GetText(1796));
			}
			else
			{
				if (MultiTargets == null)
				{
					break;
				}
				bool flag = MultiTargets.Length != 0;
				for (int i = 0; i < MultiTargets.Length; i++)
				{
					PartyMemberAI partyMemberAI = (MultiTargets[i] ? MultiTargets[i].GetComponent<PartyMemberAI>() : null);
					if ((bool)partyMemberAI && !partyMemberAI.UseInstructionSet)
					{
						flag = false;
						break;
					}
				}
				UIActionBarTooltip.GlobalShow(Background, flag ? GUIUtils.GetText(2372) : GUIUtils.GetText(2373));
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS:
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU:
		case UIAbilityBarButtonSet.AbilityButtonAction.EMPTY:
		case UIAbilityBarButtonSet.AbilityButtonAction.HOTKEY_SUBMENU:
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS:
		case UIAbilityBarButtonSet.AbilityButtonAction.MASTERED_SUBMENU:
		case UIAbilityBarButtonSet.AbilityButtonAction.WATCHER_SUBMENU:
			break;
		}
	}

	public void SetAction(UIAbilityBarButtonSet.AbilityButtonAction type, GameObject target, GameObject targetParent)
	{
		m_Action = type;
		SetTargetItem(null);
		m_Target = target;
		MultiTargets = null;
		m_TargetParent = targetParent;
		if (MasteryOverlayObject != null)
		{
			MasteryOverlayObject.SetActive((bool)TargetAbility && TargetAbility.MasteryLevel > 0);
		}
		m_TargetIndex = -1;
		RefreshHotkey();
		RefreshFanFill(reset: true);
	}

	public void SetAction(UIAbilityBarButtonSet.AbilityButtonAction type, InventoryItem targetItem)
	{
		m_Action = type;
		SetTargetItem(targetItem);
		m_Target = targetItem.baseItem.gameObject;
		MultiTargets = null;
		m_TargetParent = null;
		if (MasteryOverlayObject != null)
		{
			MasteryOverlayObject.SetActive((bool)TargetAbility && TargetAbility.MasteryLevel > 0);
		}
		m_TargetIndex = -1;
		RefreshHotkey();
		RefreshFanFill(reset: true);
	}

	public void SetAction(UIAbilityBarButtonSet.AbilityButtonAction type, int targetIndex)
	{
		m_Action = type;
		SetTargetItem(null);
		m_Target = null;
		MultiTargets = null;
		m_TargetParent = null;
		MasteryOverlayObject.SetActive(value: false);
		m_TargetIndex = targetIndex;
		RefreshHotkey();
		RefreshFanFill(reset: true);
	}

	private void SetTargetItem(InventoryItem targetItem)
	{
		m_TargetItem = targetItem;
		if (targetItem != null && targetItem.baseItem != null && targetItem.baseItem.gameObject != null)
		{
			SubscribeInventoryItemModsChanged(targetItem.baseItem.gameObject.GetComponent<Equippable>());
		}
		else
		{
			UnsubscribeInventoryItemModsChanged();
		}
	}

	private void SubscribeInventoryItemModsChanged(Equippable equipComp)
	{
		if (!(equipComp == m_ListenedEquipItem))
		{
			if (m_ListenedEquipItem != null)
			{
				m_ListenedEquipItem.ItemModsChanged -= OnItemModsChanged;
			}
			m_ListenedEquipItem = equipComp;
			if (m_ListenedEquipItem != null)
			{
				m_ListenedEquipItem.ItemModsChanged -= OnItemModsChanged;
				m_ListenedEquipItem.ItemModsChanged += OnItemModsChanged;
				OnItemModsChanged(m_ListenedEquipItem);
			}
		}
	}

	private void UnsubscribeInventoryItemModsChanged()
	{
		if (m_ListenedEquipItem != null)
		{
			m_ListenedEquipItem.ItemModsChanged -= OnItemModsChanged;
		}
		m_ListenedEquipItem = null;
	}

	private void OnItemModsChanged(Equippable equipComp)
	{
		if (!(equipComp != m_ListenedEquipItem))
		{
			ShowModItemBackground(UIInventoryGridItem.DetermineEquipModGlowColor(equipComp));
		}
	}

	private void RefreshFanFill(bool reset)
	{
		GenericAbility targetAbility = TargetAbility;
		GenericSpell genericSpell = targetAbility as GenericSpell;
		GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
		CharacterStats characterStats = null;
		if ((bool)selectedForBars)
		{
			characterStats = selectedForBars.GetComponent<CharacterStats>();
		}
		bool flag = false;
		TeleportAbility teleportAbility = (targetAbility ? (targetAbility.Attack as TeleportAbility) : null);
		if ((bool)targetAbility && targetAbility.Modal && (bool)characterStats && characterStats.InModalRecovery(targetAbility.Grouping))
		{
			FanFill.Invert = false;
			FanFill.SetFanFill(characterStats.GetModalRecovery(targetAbility.Grouping), CharacterStats.ModalRecoveryTime);
			flag = true;
		}
		else if ((bool)genericSpell && genericSpell.StatusEffectGrantingSpell != null)
		{
			FanFill.Invert = true;
			FanFill.SetFanFill(genericSpell.StatusEffectGrantingSpell.TimeLeft, genericSpell.StatusEffectGrantingSpell.Duration);
			flag = true;
		}
		else if ((bool)characterStats && (((bool)genericSpell && genericSpell.NeedsGrimoire) || m_Action == UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS) && characterStats.CurrentGrimoireCooldown > 0f)
		{
			FanFill.Invert = false;
			FanFill.SetFanFill(characterStats.CurrentGrimoireCooldown, characterStats.GetGrimoireCooldown());
			flag = true;
		}
		else if ((bool)targetAbility && targetAbility.HasActivationPrerequisite(PrerequisiteType.CombatTimeAtLeast))
		{
			FanFill.Invert = true;
			FanFill.SetFanFill(GameState.InCombatDuration, targetAbility.RequiresCombatTimeAtLeast());
			flag = true;
		}
		else if ((bool)teleportAbility && teleportAbility.TeleportBackAfter > 0f && teleportAbility.TeleportBackTimer > 0f)
		{
			FanFill.Invert = false;
			FanFill.SetFanFill(teleportAbility.TeleportBackTimer, teleportAbility.TeleportBackAfter);
			flag = true;
		}
		if (reset && !flag)
		{
			FanFill.Stop();
		}
	}

	private void ResetTrails()
	{
		if ((bool)m_ModalActiveVfx)
		{
			GameUtilities.Destroy(m_ModalActiveVfx);
			m_ModalActiveVfx = null;
		}
	}

	public void SetTargetIndex(int index)
	{
		m_TargetIndex = index;
	}

	private void OnRightClick()
	{
		GameInput.HandleAllClicks();
		switch (m_Action)
		{
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY:
		case UIAbilityBarButtonSet.AbilityButtonAction.READONLY_ABILITY:
		{
			Equippable equippable2 = (m_TargetParent ? m_TargetParent.GetComponent<Equippable>() : null);
			if ((bool)equippable2)
			{
				UIItemInspectManager.Examine(equippable2);
			}
			else
			{
				UIItemInspectManager.Examine(m_Target.GetComponent<GenericAbility>());
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED:
			UIItemInspectManager.Examine(m_Target.GetComponent<GenericAbility>());
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.QUICK_ITEM:
			UIItemInspectManager.Examine(m_Target.GetComponent<Item>(), UIAbilityBar.GetSelectedForBars());
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU:
		{
			WeaponSet selectedWeaponSet = m_Target.GetComponent<Equipment>().CurrentItems.GetSelectedWeaponSet();
			if (!WeaponSet.IsNullOrEmpty(selectedWeaponSet) && (bool)selectedWeaponSet.PrimaryWeapon)
			{
				UIItemInspectManager.Examine(selectedWeaponSet.PrimaryWeapon, UIAbilityBar.GetSelectedForBars());
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET:
		{
			WeaponSet weaponSet = m_Target.GetComponent<Equipment>().WeaponSets[m_TargetIndex];
			Equippable equippable = null;
			switch (Owner.GetButtonIndex(this))
			{
			case 0:
				equippable = weaponSet.PrimaryWeapon;
				break;
			case 1:
				equippable = weaponSet.SecondaryWeapon;
				break;
			}
			if ((bool)equippable)
			{
				UIItemInspectManager.Examine(equippable, UIAbilityBar.GetSelectedForBars());
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.AI_TOGGLE:
			if ((bool)m_Target)
			{
				CharacterStats component = m_Target.GetComponent<CharacterStats>();
				if ((bool)component && PartyMemberInstructionSetList.InstructionSetList.GetClassSpellList(component.CharacterClass) != null)
				{
					UIAiCustomizerManager.Instance.SelectedCharacter = m_Target.GetComponent<CharacterStats>();
					UIAiCustomizerManager.Instance.ShowWindow();
				}
			}
			break;
		}
	}

	private void OnClick()
	{
		GameInput.HandleAllClicks();
		UIAbilityBar.Instance.CancelSelection();
		Trigger();
	}

	public void Trigger()
	{
		if (Disabled)
		{
			UIAbilityBarButtonSet.AbilityButtonAction action = m_Action;
			if (action != UIAbilityBarButtonSet.AbilityButtonAction.QUICK_ITEM)
			{
				return;
			}
			Item component = m_Target.GetComponent<Item>();
			GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
			if (component is Consumable && (bool)selectedForBars)
			{
				string text = (component as Consumable).WhyCannotUse(selectedForBars.GetComponent<CharacterStats>());
				if (!string.IsNullOrEmpty(text))
				{
					Console.AddMessage(text);
				}
			}
			return;
		}
		switch (m_Action)
		{
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY:
		{
			GenericAbility component11 = m_Target.GetComponent<GenericAbility>();
			component11.TriggerFromUI();
			component11.DeactivateOtherModal(onlyUi: true);
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.CAST_SPELL_ABILITY_MASTERED:
		{
			GenericAbility component4 = m_Target.GetComponent<GenericAbility>();
			component4.TriggerFromUI();
			component4.DeactivateOtherModal(onlyUi: true);
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.QUICK_ITEM:
		{
			Item j = m_Target.GetComponent<Item>();
			if (j is Consumable)
			{
				(j as Consumable).StartUse(UIAbilityBar.GetSelectedForBars().gameObject);
			}
			else
			{
				if (!(j is Equippable))
				{
					break;
				}
				GameObject selectedForBars3 = UIAbilityBar.GetSelectedForBars();
				if (!selectedForBars3)
				{
					break;
				}
				Equipment component6 = selectedForBars3.GetComponent<Equipment>();
				CharacterStats component7 = selectedForBars3.GetComponent<CharacterStats>();
				if ((bool)component6 && (bool)component7 && component7.CharacterClass == CharacterStats.Class.Wizard)
				{
					QuickbarInventory component8 = selectedForBars3.GetComponent<QuickbarInventory>();
					Equippable equippable = component6.UnEquip(Equippable.EquipmentSlot.Grimoire);
					InventoryItem inventoryItem = component8.ItemList.Where((InventoryItem i2) => i2.baseItem == j).First();
					Equippable equippable2;
					if (inventoryItem.stackSize > 1)
					{
						inventoryItem.SetStackSize(inventoryItem.stackSize - 1);
						equippable2 = (Equippable)inventoryItem.baseItem;
					}
					else
					{
						equippable2 = (Equippable)component8.TakeItem(inventoryItem).baseItem;
					}
					if ((bool)equippable)
					{
						component8.AddItem(equippable, 1, component8.GetSlotFor(j));
					}
					if ((bool)equippable2)
					{
						component6.Equip(equippable2);
					}
					UIAbilityBar.Instance.Refresh();
				}
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.TALK_TO:
		{
			PartyMemberAI component12 = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
			NPCDialogue component13 = m_Target.GetComponent<NPCDialogue>();
			component12.ExclusiveSelect();
			if (component13 != null)
			{
				GameState.s_playerCharacter.ObjectClicked(component13);
				CameraControl.Instance.FocusOnObject(m_Target, 0.6f);
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.CLASS_EDIT:
		{
			GameObject selectedForBars2 = UIAbilityBar.GetSelectedForBars();
			if (!selectedForBars2)
			{
				break;
			}
			CharacterStats component3 = selectedForBars2.GetComponent<CharacterStats>();
			switch (component3.CharacterClass)
			{
			case CharacterStats.Class.Wizard:
			{
				Grimoire grimoire = Grimoire.Find(selectedForBars2);
				if ((bool)grimoire)
				{
					UIGrimoireManager.Instance.SelectCharacter(selectedForBars2.GetComponent<PartyMemberAI>());
					UIGrimoireManager.Instance.LoadGrimoire(grimoire, canEdit: true);
					UIGrimoireManager.Instance.ShowWindow();
				}
				break;
			}
			case CharacterStats.Class.Chanter:
				UIChantEditor.Instance.ShowWindow();
				UIChantEditor.Instance.SelectedCharacter = component3;
				break;
			}
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_SPELLS:
			SubrowSets = Owner.ToggleSubrow(this, 1);
			if (SubrowSets != null)
			{
				SetSubrowLocked(locked: true);
				SetUpSubspells(SubrowSets[0]);
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET:
		{
			Equipment component9 = m_Target.GetComponent<Equipment>();
			if ((bool)component9)
			{
				component9.SelectWeaponSet(m_TargetIndex, enforceRecoveryPenalty: true);
				PartyMemberAI component10 = m_Target.GetComponent<PartyMemberAI>();
				if (component10 != null && component10.gameObject.activeInHierarchy)
				{
					component10.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: false);
				}
			}
			UIAbilityBar.Instance.RefreshWeaponSets();
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.WEAPON_SET_SUBMENU:
		{
			Equipment component5 = m_Target.GetComponent<Equipment>();
			if ((bool)component5)
			{
				component5.SelectNextWeaponSet();
			}
			UIAbilityBar.Instance.RefreshWeaponSets();
			HideSubrow();
			break;
		}
		case UIAbilityBarButtonSet.AbilityButtonAction.HOTKEY_SUBMENU:
			SubrowSets = Owner.ToggleSubrow(this, 1);
			if (SubrowSets != null)
			{
				SetSubrowLocked(locked: true);
				SetUpHotkeys(SubrowSets[0]);
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.SUB_ITEMS:
			SubrowSets = Owner.ToggleSubrow(this, 6);
			if (SubrowSets != null)
			{
				SetSubrowLocked(locked: true);
				SetUpItems(SubrowSets);
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.MASTERED_SUBMENU:
			SubrowSets = Owner.ToggleSubrow(this, 1);
			if (SubrowSets != null)
			{
				SetSubrowLocked(locked: true);
				SetUpMasteredSpells(SubrowSets[0]);
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.WATCHER_SUBMENU:
			SubrowSets = Owner.ToggleSubrow(this, 1);
			if (SubrowSets != null)
			{
				SetSubrowLocked(locked: true);
				SetUpWatcherAbilities(SubrowSets[0]);
			}
			break;
		case UIAbilityBarButtonSet.AbilityButtonAction.AI_TOGGLE:
		{
			if (m_Target != null)
			{
				PartyMemberAI component2 = m_Target.GetComponent<PartyMemberAI>();
				if ((bool)component2)
				{
					component2.UseInstructionSet = !component2.UseInstructionSet;
					component2.UpdateAggressionOfSummonedCreatures(includeCompanion: true);
				}
			}
			if (MultiTargets == null)
			{
				break;
			}
			bool flag = MultiTargets.Length != 0;
			for (int k = 0; k < MultiTargets.Length; k++)
			{
				PartyMemberAI partyMemberAI = (MultiTargets[k] ? MultiTargets[k].GetComponent<PartyMemberAI>() : null);
				if ((bool)partyMemberAI && !partyMemberAI.UseInstructionSet)
				{
					flag = false;
					break;
				}
			}
			for (int l = 0; l < MultiTargets.Length; l++)
			{
				PartyMemberAI partyMemberAI2 = (MultiTargets[l] ? MultiTargets[l].GetComponent<PartyMemberAI>() : null);
				if ((bool)partyMemberAI2)
				{
					partyMemberAI2.UseInstructionSet = !flag;
				}
			}
			break;
		}
		default:
			OnTooltip(isOver: true);
			return;
		}
		Owner.Close();
	}

	private void SetSubrowLocked(bool locked)
	{
	}

	private void SetUpSubspells(UIAbilityBarButtonSet subrow)
	{
		if ((bool)subrow)
		{
			CharacterStats component = m_Target.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				subrow.SetButtonsSubspells(component, m_TargetIndex);
			}
			else
			{
				Debug.LogError("AbilityBar: subspell button target doesn't have a CharacterStats.");
			}
		}
	}

	private bool IsSubspellActive(CharacterStats character, int spellLevel)
	{
		bool result = false;
		if (character != null)
		{
			PartyMemberAI component = character.GetComponent<PartyMemberAI>();
			{
				foreach (GenericAbility activeAbility in character.ActiveAbilities)
				{
					if (activeAbility.MasteryLevel <= 0 && (activeAbility is GenericSpell || activeAbility is GenericCipherAbility) && (spellLevel <= 0 || !(activeAbility is GenericSpell) || ((GenericSpell)activeAbility).SpellLevel == spellLevel) && (spellLevel <= 0 || !(activeAbility is GenericCipherAbility) || ((GenericCipherAbility)activeAbility).SpellLevel == spellLevel) && component.CurrentAbility == activeAbility)
					{
						return true;
					}
				}
				return result;
			}
		}
		return result;
	}

	private void SetUpWeaponSets(UIAbilityBarButtonSet[] subrows)
	{
		CharacterStats component = m_Target.GetComponent<CharacterStats>();
		if (!component)
		{
			Debug.LogError("AbilityBar: weaponset submenu button target doesn't have a CharacterStats.");
			return;
		}
		Equipment component2 = component.GetComponent<Equipment>();
		if ((bool)component2)
		{
			int num = 0;
			bool flag = false;
			for (int i = 0; i < component.MaxWeaponSets; i++)
			{
				bool flag2 = WeaponSet.IsNullOrEmpty((i < component2.WeaponSets.Length) ? component2.WeaponSets[i] : null);
				if (!flag2 || !flag)
				{
					if (flag2)
					{
						flag = true;
					}
					subrows[num].SetButtonsWeaponSet(component, i);
					num++;
				}
			}
		}
		UIAbilityBar.Instance.Reposition(1);
	}

	private void SetUpHotkeys(UIAbilityBarButtonSet subrow)
	{
		if ((bool)subrow)
		{
			CharacterStats component = m_Target.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				subrow.SetButtonsHotkeys(component);
			}
		}
	}

	private void SetUpItems(UIAbilityBarButtonSet[] subrows)
	{
		CharacterStats component = m_Target.GetComponent<CharacterStats>();
		subrows[0].SetButtonsItemPerEncounterAbilities(component);
		subrows[1].SetButtonsItemPerRestAbilities(component);
		subrows[2].SetButtonsItemPerStrongholdTurnAbilities(component);
		subrows[3].SetButtonsItemOtherCooldownAbilities(component);
		subrows[4].SetButtonsOtherQuickitems(component);
		subrows[5].SetButtonsItemTriggeredAbilities(component);
	}

	private void SetUpMasteredSpells(UIAbilityBarButtonSet subrow)
	{
		if ((bool)subrow)
		{
			CharacterStats component = m_Target.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				subrow.SetButtonsMasteredSpells(component);
			}
		}
	}

	private void SetUpWatcherAbilities(UIAbilityBarButtonSet subrow)
	{
		if ((bool)subrow)
		{
			CharacterStats component = m_Target.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				subrow.SetButtonsWatcherAbilities(component);
			}
		}
	}
}

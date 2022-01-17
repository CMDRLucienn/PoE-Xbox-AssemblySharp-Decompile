using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAbilityBar : MonoBehaviour
{
	public enum SetType
	{
		WeaponSets,
		Misc,
		ItemsBase,
		Chants,
		ClassSpells,
		AnticlassSpells,
		StolenAbilities,
		PerEncounterAbilities,
		PerRestAbilities,
		PerStrongholdTurnAbilities,
		OtherCooldownAbilities,
		ModalAbilities,
		Count
	}

	private const int CONTROLLER_MINIMUM_SET = 2;

	public static UIAbilityBarButton HoveredButton;

	public Texture2D TalkIcon;

	public Texture2D AiActiveIcon;

	public Texture2D AiInactiveIcon;

	public Texture2D ChanterIcon;

	public Texture2D ChanterEditIcon;

	public Texture2D PriestIcon;

	public Texture2D WizardIcon;

	public Texture2D NeutralIcon;

	public Texture2D CipherIcon;

	public Texture2D ChanterSpellIcon;

	public Texture2D DruidIcon;

	public Texture2D ItemsIcon;

	public Texture2D WatcherIcon;

	public Texture2D MasteredIconOverlay;

	public UIAnchor ControllerSelectionBox;

	private UICopyPanelAlpha m_ControllerSelectionBoxAlpha;

	public Texture2D UnarmedIcon;

	public GameObject ModalActiveVfx;

	public UIAbilityBarButtonSet SetTemplate;

	public UIAbilityBarRowBg BgTemplate;

	public UIAnchor TransitionAnchor;

	public int SetSpacing = 5;

	public int VertSpacing = 72;

	private List<List<UIAbilityBarButtonSet>> Sets = new List<List<UIAbilityBarButtonSet>>();

	private List<UIAbilityBarRowBg> Backgrounds = new List<UIAbilityBarRowBg>();

	private UIAbilityBarButtonSet[] m_Hotkeys;

	private GameObject m_Selected;

	private bool m_NeedsRefresh;

	private float m_RefreshTimer;

	private const float RefreshInterval = 1f;

	private int m_NoShowTime;

	private int m_SelectionRow = -1;

	private int m_SelectionSet;

	private int m_SelectionCell;

	private float m_SelectionTooltipDelay;

	public static UIAbilityBar Instance { get; private set; }

	public static GenericAbility HoveredAbility
	{
		get
		{
			if (!HoveredButton)
			{
				return null;
			}
			return HoveredButton.TargetAbility;
		}
	}

	public static InventoryItem HoveredInvItem
	{
		get
		{
			if (!HoveredButton)
			{
				return null;
			}
			return HoveredButton.TargetItem;
		}
	}

	public static Item HoveredItem
	{
		get
		{
			if (!HoveredButton)
			{
				return null;
			}
			return HoveredButton.TargetAnyItem;
		}
	}

	public GameObject InstantiateModalVfx(Transform parent, Vector3 local)
	{
		GameObject obj = UnityEngine.Object.Instantiate(Instance.ModalActiveVfx);
		Transform obj2 = obj.transform;
		obj2.parent = parent;
		obj2.localPosition = local;
		obj2.localScale = new Vector3(240f, 240f, 240f);
		return obj;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		Sets.Add(new List<UIAbilityBarButtonSet>());
		for (int i = 0; i < 18; i++)
		{
			Sets[0].Add(NGUITools.AddChild(base.gameObject, SetTemplate.gameObject).GetComponent<UIAbilityBarButtonSet>());
		}
		AllocateBackgrounds(0);
		SetTemplate.gameObject.SetActive(value: false);
		BgTemplate.gameObject.SetActive(value: false);
		PartyMemberAI.OnAnySelectionChanged += OnSelectionChanged;
		UIWindowManager.Instance.OnWindowHidden += OnWindowHidden;
		m_ControllerSelectionBoxAlpha = ControllerSelectionBox.GetComponent<UICopyPanelAlpha>();
		m_ControllerSelectionBoxAlpha.Multiplier = 0f;
		Refresh();
	}

	private void Update()
	{
		m_NoShowTime--;
		if (m_NeedsRefresh || (m_RefreshTimer -= Time.unscaledDeltaTime) <= 0f)
		{
			m_NeedsRefresh = false;
			m_RefreshTimer = 1f;
			Refresh();
		}
		if (GameInput.GetControlDownWithRepeat(MappedControl.UP_ABILITY, handle: true))
		{
			NavigateVertical(1);
		}
		else if (GameInput.GetControlDownWithRepeat(MappedControl.DOWN_ABILITY, handle: true))
		{
			NavigateVertical(-1);
		}
		if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_ABILITY, handle: true))
		{
			if (m_SelectionRow < 0)
			{
				SelectFirstButton();
			}
			else
			{
				NavigateHorizontal(1);
			}
		}
		else if (GameInput.GetControlDownWithRepeat(MappedControl.PREVIOUS_ABILITY, handle: true))
		{
			if (m_SelectionRow < 0)
			{
				SelectLastButton();
			}
			else
			{
				NavigateHorizontal(-1);
			}
		}
		UIAbilityBarButton selectedButton = GetSelectedButton();
		if ((bool)selectedButton && !selectedButton.gameObject.activeInHierarchy)
		{
			CancelSelection();
		}
		if (m_SelectionTooltipDelay > 0f)
		{
			m_SelectionTooltipDelay -= Time.unscaledDeltaTime;
			if (m_SelectionTooltipDelay <= 0f && (bool)selectedButton)
			{
				selectedButton.ShowTooltip();
			}
		}
		UIAbilityBarButton selectedButton2 = GetSelectedButton();
		if ((bool)selectedButton2 && !GameState.s_playerCharacter.IsCasting() && GameInput.GetControlUp(MappedControl.CAST_SELECTED_ABILITY))
		{
			selectedButton2.Trigger();
		}
		if (!m_Selected)
		{
			return;
		}
		if (GameInput.LmbAvailable())
		{
			CharacterHotkeyBindings characterHotkeyBindings = CharacterHotkeyBindings.Get(m_Selected);
			if ((bool)characterHotkeyBindings)
			{
				characterHotkeyBindings.Activate(GameInput.Instance.LastKeyUp);
			}
		}
		if (m_Hotkeys != null || (Sets.Count >= 2 && Sets[1].Any((UIAbilityBarButtonSet s) => s.gameObject.activeInHierarchy)))
		{
			return;
		}
		CharacterHotkeyBindings characterHotkeyBindings2 = CharacterHotkeyBindings.Get(m_Selected);
		if ((bool)characterHotkeyBindings2 && characterHotkeyBindings2.AbilityHotkeys != null && characterHotkeyBindings2.AbilityHotkeys.Count > 0)
		{
			CharacterStats component = m_Selected.GetComponent<CharacterStats>();
			if ((bool)component)
			{
				m_Hotkeys = ShowSubrow(null, 1);
				m_Hotkeys[0].SetButtonsHotkeys(component);
			}
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		if (!GameState.CurrentSceneIsTransitionScene())
		{
			m_NeedsRefresh = true;
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		Sets.Clear();
		Backgrounds.Clear();
		PartyMemberAI.OnAnySelectionChanged -= OnSelectionChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		GameObject selectedForBars = GetSelectedForBars();
		if (selectedForBars == null)
		{
			UIAbilityTooltipManager.Instance.HideAll();
		}
		if (!(selectedForBars != m_Selected))
		{
			return;
		}
		if ((bool)m_Selected)
		{
			m_Selected.GetComponent<CharacterStats>().UnbindAbilitiesChanged(OnAbilitiesChanged);
			CharacterHotkeyBindings characterHotkeyBindings = CharacterHotkeyBindings.Get(m_Selected);
			if ((bool)characterHotkeyBindings)
			{
				characterHotkeyBindings.OnHotkeyChanged -= OnHotkeyChanged;
			}
			QuickbarInventory component = m_Selected.GetComponent<QuickbarInventory>();
			if ((bool)component)
			{
				component.OnChanged -= OnQuickbarChanged;
			}
		}
		if ((bool)selectedForBars)
		{
			selectedForBars.GetComponent<CharacterStats>().BindAbilitiesChanged(OnAbilitiesChanged);
			CharacterHotkeyBindings characterHotkeyBindings2 = CharacterHotkeyBindings.Get(selectedForBars);
			if ((bool)characterHotkeyBindings2)
			{
				characterHotkeyBindings2.OnHotkeyChanged += OnHotkeyChanged;
			}
			QuickbarInventory component2 = selectedForBars.GetComponent<QuickbarInventory>();
			if ((bool)component2)
			{
				component2.OnChanged += OnQuickbarChanged;
			}
		}
		m_Selected = selectedForBars;
		m_NeedsRefresh = true;
		HideSubrow(1);
		m_NoShowTime = 1;
		CancelSelection();
	}

	private void OnQuickbarChanged(BaseInventory sender)
	{
		m_NeedsRefresh = true;
	}

	private void OnAbilitiesChanged(object sender, ListChangedEventArgs e)
	{
		m_NeedsRefresh = true;
	}

	private void OnHotkeyChanged(KeyControl key, GenericAbility newabil)
	{
		RefreshHotkeys();
	}

	public Texture2D GetClassIcon(CharacterStats.Class cl)
	{
		return cl switch
		{
			CharacterStats.Class.Priest => PriestIcon, 
			CharacterStats.Class.Wizard => WizardIcon, 
			CharacterStats.Class.Cipher => CipherIcon, 
			CharacterStats.Class.Chanter => ChanterIcon, 
			CharacterStats.Class.Druid => DruidIcon, 
			_ => NeutralIcon, 
		};
	}

	public void RefreshHotkeys()
	{
		CharacterStats component = m_Selected.GetComponent<CharacterStats>();
		if (m_Hotkeys != null)
		{
			m_Hotkeys[0].SetButtonsHotkeys(component);
		}
		Reposition(0);
		foreach (List<UIAbilityBarButtonSet> set in Sets)
		{
			foreach (UIAbilityBarButtonSet item in set)
			{
				item.RefreshHotkeys();
			}
		}
	}

	public void RefreshAbilities()
	{
		m_NeedsRefresh = true;
	}

	public void RefreshQuickItems()
	{
	}

	public void RefreshWeaponSets()
	{
		if (m_Selected != null)
		{
			CharacterStats component = m_Selected.GetComponent<CharacterStats>();
			if (component != null && Sets[0][0] != null)
			{
				Sets[0][0].SetButtonsWeaponControls(component);
			}
		}
		Reposition(0);
	}

	private void OnWindowHidden(UIHudWindow window)
	{
		Refresh();
	}

	public void Refresh()
	{
		if (GameState.IsLoading)
		{
			m_NeedsRefresh = true;
		}
		m_Selected = GetSelectedForBars();
		if (m_Selected != null)
		{
			CharacterStats component = m_Selected.GetComponent<CharacterStats>();
			if ((bool)component && Sets != null)
			{
				if (Sets[0][0] != null)
				{
					Sets[0][0].SetButtonsWeaponControls(component);
				}
				if (Sets[0][2] != null)
				{
					Sets[0][2].SetButtonsItemsBase(component);
				}
				if (Sets[0][3] != null)
				{
					Sets[0][3].SetButtonsChants(component);
				}
				if (Sets[0][4] != null)
				{
					Sets[0][4].SetButtonsClassSpells(component);
				}
				if (Sets[0][5] != null)
				{
					Sets[0][5].SetButtonsAnticlassSpells(component);
				}
				if (Sets[0][6] != null)
				{
					Sets[0][6].SetButtonsStolenAbilities(component);
				}
				if (Sets[0][7] != null)
				{
					Sets[0][7].SetButtonsEncounterAbilities(component);
				}
				if (Sets[0][8] != null)
				{
					Sets[0][8].SetButtonsRestAbilities(component);
				}
				if (Sets[0][9] != null)
				{
					Sets[0][9].SetButtonsStrongholdTurnAbilities(component);
				}
				if (Sets[0][10] != null)
				{
					Sets[0][10].SetButtonsOtherCooldownAbilities(component);
				}
				if (Sets[0][1] != null)
				{
					Sets[0][1].SetButtonsMisc(component);
				}
				for (GenericAbility.ActivationGroup activationGroup = GenericAbility.ActivationGroup.None; activationGroup < GenericAbility.ActivationGroup.Count; activationGroup++)
				{
					if (Sets[0][(int)(11 + activationGroup)] != null)
					{
						Sets[0][(int)(11 + activationGroup)].SetButtonsModalAbilities(component, activationGroup);
					}
				}
				if (m_Hotkeys != null)
				{
					m_Hotkeys[0].SetButtonsHotkeys(component);
				}
			}
		}
		else
		{
			if (Sets != null)
			{
				for (int i = 0; i < Sets.Count; i++)
				{
					for (int j = 0; j < Sets[i].Count; j++)
					{
						if (i != 0 || j != 1)
						{
							UIAbilityBarButtonSet uIAbilityBarButtonSet = Sets[i][j];
							if (uIAbilityBarButtonSet != null && uIAbilityBarButtonSet.gameObject != null)
							{
								uIAbilityBarButtonSet.gameObject.SetActive(value: false);
							}
						}
					}
				}
			}
			if (Sets[0][1] != null)
			{
				Sets[0][1].SetButtonsMisc(PartyMemberAI.SelectedPartyMembers);
			}
		}
		Reposition(0);
	}

	public void Reposition(int level)
	{
		float num = 0f;
		if (Sets == null || level >= Sets.Count || Sets[level] == null)
		{
			return;
		}
		foreach (UIAbilityBarButtonSet item in Sets[level])
		{
			if (!(item == null))
			{
				item.Refresh();
				item.transform.localPosition = new Vector3(num, item.transform.localPosition.y, item.transform.localPosition.z);
				if (!item.Valid())
				{
					item.gameObject.SetActive(value: false);
				}
				else
				{
					num += item.Width + (float)SetSpacing;
				}
			}
		}
		if (Backgrounds != null && level < Backgrounds.Count)
		{
			if (Sets[level].Where((UIAbilityBarButtonSet set) => set.gameObject.activeSelf).Any())
			{
				Backgrounds[level].gameObject.SetActive(value: true);
				Backgrounds[level].Sizer.transform.localScale = new Vector3(num, Backgrounds[level].Sizer.transform.localScale.y, 1f);
				UIWidgetUtils.UpdateDependents(Backgrounds[level].gameObject, 1);
			}
			else
			{
				Backgrounds[level].gameObject.SetActive(value: false);
			}
		}
	}

	public UIAbilityBarButtonSet[] ShowSubrowIfNew(UIAbilityBarButton sender, int level, int numSets)
	{
		if (level <= 0)
		{
			return null;
		}
		InitSubrow(level, numSets);
		if (Sets[level][0].SubrowOwner != sender || sender == null || Sets[level][0].WantsHide)
		{
			return ShowSubrow(sender, level, numSets);
		}
		return null;
	}

	public UIAbilityBarButtonSet[] ShowSubrow(UIAbilityBarButton sender, int level)
	{
		return ShowSubrowIfNew(sender, level, 1);
	}

	public UIAbilityBarButtonSet[] ShowSubrow(UIAbilityBarButton sender, int level, int numSets)
	{
		if (level <= 0)
		{
			return null;
		}
		if (m_NoShowTime > 0)
		{
			return null;
		}
		m_Hotkeys = null;
		InitSubrow(level, numSets);
		AllocateBackgrounds(level);
		Backgrounds[level].Show(sender ? sender.Background : null, level);
		for (int i = 0; i < numSets; i++)
		{
			Sets[level][i].Show();
			Sets[level][i].WantsHide = false;
			Sets[level][i].SubrowOwner = sender;
			Sets[level][i].SubLevel = level;
			Sets[level][i].transform.parent = Backgrounds[level].transform;
			Sets[level][i].transform.localPosition = Sets[0][0].transform.localPosition;
		}
		for (int j = numSets; j < Sets[level].Count; j++)
		{
			Sets[level][j].gameObject.SetActive(value: false);
		}
		UIWidgetUtils.UpdateDependents(base.gameObject, 2);
		UpdateTransitionAnchor();
		return Sets[level].ToArray();
	}

	protected void AllocateBackgrounds(int level)
	{
		while (Backgrounds.Count <= level)
		{
			Backgrounds.Add(NGUITools.AddChild(base.gameObject, BgTemplate.gameObject).GetComponent<UIAbilityBarRowBg>());
			UIAbilityBarRowBg uIAbilityBarRowBg = Backgrounds[Backgrounds.Count - 1];
			uIAbilityBarRowBg.transform.localPosition = new Vector3(uIAbilityBarRowBg.transform.localPosition.x, uIAbilityBarRowBg.transform.localPosition.y + (float)((Backgrounds.Count - 1) * VertSpacing), uIAbilityBarRowBg.transform.localPosition.z);
		}
	}

	public void LockSubrow(int level)
	{
		if (level <= 0)
		{
			return;
		}
		foreach (UIAbilityBarButtonSet item in Sets[level])
		{
			item.Locked = true;
		}
	}

	public void UnlockSubrow(int level)
	{
		if (level <= 0)
		{
			return;
		}
		foreach (UIAbilityBarButtonSet item in Sets[level])
		{
			item.Locked = false;
		}
	}

	public void HideSubrow(int level)
	{
		if (level <= 0)
		{
			return;
		}
		m_Hotkeys = null;
		if (level >= Sets.Count || Sets[level].Count <= 0 || Sets[level][0].Locked)
		{
			return;
		}
		foreach (UIAbilityBarButtonSet item in Sets[level])
		{
			item.gameObject.SetActive(value: false);
			item.SubrowOwner = null;
		}
		if (level < Backgrounds.Count && !Sets[level].Any((UIAbilityBarButtonSet s) => s.gameObject.activeSelf))
		{
			Backgrounds[level].gameObject.SetActive(value: false);
		}
		UpdateTransitionAnchor();
	}

	public bool RaycastSubrow(int level)
	{
		if (level < 0 || level >= Backgrounds.Count)
		{
			return false;
		}
		return UIWidget.RaycastAny(Backgrounds[level].gameObject, GameInput.MousePosition);
	}

	public UIAbilityBarRowBg HighestRowBackground()
	{
		for (int num = Backgrounds.Count - 1; num >= 0; num--)
		{
			if (Backgrounds[num].gameObject.activeSelf)
			{
				return Backgrounds[num];
			}
		}
		return null;
	}

	private void UpdateTransitionAnchor()
	{
		UIAbilityBarRowBg uIAbilityBarRowBg = HighestRowBackground();
		if ((bool)uIAbilityBarRowBg)
		{
			TransitionAnchor.widgetContainer = uIAbilityBarRowBg.Sizer;
		}
	}

	public bool SubrowActive(int level)
	{
		if (level <= 0)
		{
			return true;
		}
		InitSubrow(level, 1);
		if (Sets[level][0].gameObject.activeSelf)
		{
			return !Sets[level][0].WantsHide;
		}
		return false;
	}

	private void InitSubrow(int level, int numSets)
	{
		if (level <= 0)
		{
			return;
		}
		while (Sets.Count <= level)
		{
			Sets.Add(new List<UIAbilityBarButtonSet>());
		}
		int num = numSets - Sets[level].Count;
		if (num > 0)
		{
			SetTemplate.gameObject.SetActive(value: true);
			for (int i = 0; i < num; i++)
			{
				Sets[level].Add(NGUITools.AddChild(base.gameObject, SetTemplate.gameObject).GetComponent<UIAbilityBarButtonSet>());
			}
			SetTemplate.gameObject.SetActive(value: false);
		}
	}

	public int GetHighestActiveSubrow()
	{
		for (int num = Sets.Count - 1; num >= 0; num--)
		{
			if (Sets[num].Any((UIAbilityBarButtonSet s) => s.Active))
			{
				return num;
			}
		}
		return -1;
	}

	public int GetActiveSetsInSubrow(int row)
	{
		int num = 0;
		if (row >= 0 && row < Sets.Count)
		{
			for (int i = 0; i < Sets[row].Count; i++)
			{
				if (Sets[row][i].Active)
				{
					num++;
				}
			}
		}
		return num;
	}

	private void SelectFirstButton()
	{
		for (int i = 2; i < Sets[0].Count; i++)
		{
			if (Sets[0][i].Active)
			{
				UIAbilityBarButton buttonByIndex = Sets[0][i].GetButtonByIndex(0);
				if ((bool)buttonByIndex)
				{
					SetSelection(0, i, 0, buttonByIndex);
					break;
				}
			}
		}
	}

	private void SelectLastButton()
	{
		for (int num = Sets[0].Count - 1; num >= 2; num--)
		{
			if (Sets[0][num].Active)
			{
				int num2 = Sets[0][num].ButtonCount - 1;
				UIAbilityBarButton buttonByIndex = Sets[0][num].GetButtonByIndex(num2);
				if ((bool)buttonByIndex)
				{
					SetSelection(0, num, num2, buttonByIndex);
					break;
				}
			}
		}
	}

	public void CancelSelection()
	{
		UIAbilityBarButton selectedButton = GetSelectedButton();
		if ((bool)selectedButton)
		{
			selectedButton.TriggerHover(over: false);
		}
		m_ControllerSelectionBoxAlpha.Multiplier = 0f;
		m_SelectionCell = 0;
		m_SelectionRow = -1;
		m_SelectionSet = 2;
		m_SelectionTooltipDelay = 0f;
	}

	private void SetSelection(int row, int set, int cell, UIAbilityBarButton button = null)
	{
		UIAbilityBarButton selectedButton = GetSelectedButton();
		int selectionRow = m_SelectionRow;
		m_SelectionCell = cell;
		m_SelectionRow = row;
		m_SelectionSet = set;
		if (button == null)
		{
			button = GetSelectedButton();
		}
		if ((bool)selectedButton && selectedButton != button)
		{
			selectedButton.TriggerHover(over: false);
			if (selectionRow == m_SelectionRow)
			{
				selectedButton.HideSubrow();
			}
		}
		if (button == null)
		{
			CancelSelection();
			return;
		}
		m_SelectionTooltipDelay = UICamera.tooltipDelay;
		m_ControllerSelectionBoxAlpha.Multiplier = 1f;
		ControllerSelectionBox.widgetContainer = button.Background;
		button.TriggerHover(over: true);
	}

	private void NavigateHorizontal(int nav)
	{
		int num = m_SelectionCell;
		int num2 = m_SelectionSet;
		while (nav != 0)
		{
			UIAbilityBarButtonSet uIAbilityBarButtonSet = Sets[m_SelectionRow][num2];
			int count = Sets[m_SelectionRow].Count;
			if (nav > 0)
			{
				num++;
				if (num >= uIAbilityBarButtonSet.ButtonCount)
				{
					int num3 = count;
					for (int num4 = (num2 + 1) % count; num4 != num2; num4 = (num4 + 1) % count)
					{
						if (m_SelectionRow == 0 && num4 < 2)
						{
							num4 = 2;
						}
						if (Sets[m_SelectionRow][num4].Active)
						{
							num2 = num4;
							break;
						}
						num3--;
						if (num3 <= 0)
						{
							break;
						}
					}
					num = 0;
				}
			}
			else
			{
				num--;
				if (num < 0)
				{
					int num5 = count;
					for (int num6 = (num2 - 1 + count) % count; num6 != num2; num6 = (num6 - 1 + count) % count)
					{
						if (m_SelectionRow == 0 && num6 < 2)
						{
							num6 = count - 1;
						}
						if (Sets[m_SelectionRow][num6].Active)
						{
							num2 = num6;
							break;
						}
						num5--;
						if (num5 <= 0)
						{
							break;
						}
					}
					num = Sets[m_SelectionRow][num2].ButtonCount - 1;
				}
			}
			nav -= (int)Mathf.Sign(nav);
		}
		SetSelection(m_SelectionRow, num2, num);
	}

	private void NavigateVertical(int nav)
	{
		while (nav != 0)
		{
			if (nav > 0)
			{
				if (m_SelectionRow < 0)
				{
					SelectFirstButton();
					break;
				}
				UIAbilityBarButton buttonByIndex = Sets[m_SelectionRow][m_SelectionSet].GetButtonByIndex(m_SelectionCell);
				if (!buttonByIndex || !buttonByIndex.ActionIsSubrowWithContent())
				{
					break;
				}
				buttonByIndex.ShowSubrow(locked: false);
				int highestActiveSubrow = GetHighestActiveSubrow();
				int num = 0;
				for (num = 0; num < Sets[highestActiveSubrow].Count && !Sets[highestActiveSubrow][num].Active; num++)
				{
				}
				UIAbilityBarButton buttonByIndex2 = Sets[highestActiveSubrow][num].GetButtonByIndex(0);
				if ((bool)buttonByIndex2)
				{
					SetSelection(highestActiveSubrow, num, 0, buttonByIndex2);
				}
			}
			else
			{
				if (m_SelectionRow <= 0)
				{
					CancelSelection();
					break;
				}
				int highestActiveSubrow2 = GetHighestActiveSubrow();
				UIAbilityBarButton subrowOwner = Sets[highestActiveSubrow2].First((UIAbilityBarButtonSet s) => s.Active).SubrowOwner;
				HideSubrow(highestActiveSubrow2);
				SelectButton(subrowOwner);
			}
			nav -= (int)Mathf.Sign(nav);
		}
	}

	private void SelectButton(UIAbilityBarButton button)
	{
		for (int i = 0; i < Sets.Count; i++)
		{
			for (int j = 0; j < Sets[i].Count; j++)
			{
				if (Sets[i][j].Active)
				{
					int buttonIndex = Sets[i][j].GetButtonIndex(button);
					if (buttonIndex >= 0)
					{
						SetSelection(i, j, buttonIndex, button);
						return;
					}
				}
			}
		}
		CancelSelection();
	}

	public UIAbilityBarButton GetSelectedButton()
	{
		if (m_SelectionRow >= 0 && m_SelectionSet >= 0 && m_SelectionRow < Sets.Count && m_SelectionSet < Sets[m_SelectionRow].Count)
		{
			return Sets[m_SelectionRow][m_SelectionSet].GetButtonByIndex(m_SelectionCell);
		}
		return null;
	}

	public static GameObject GetSelectedForBars()
	{
		GameObject gameObject = null;
		GameObject[] selectedPartyMembers = PartyMemberAI.SelectedPartyMembers;
		foreach (GameObject gameObject2 in selectedPartyMembers)
		{
			if (!(gameObject2 == null))
			{
				if (!(gameObject == null))
				{
					return null;
				}
				gameObject = gameObject2;
			}
		}
		return gameObject;
	}

	public static PartyMemberAI GetSelectedAIForBars()
	{
		GameObject selectedForBars = GetSelectedForBars();
		if ((bool)selectedForBars)
		{
			return selectedForBars.GetComponent<PartyMemberAI>();
		}
		return null;
	}
}

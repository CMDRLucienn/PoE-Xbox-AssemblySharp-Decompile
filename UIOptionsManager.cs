using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIOptionsManager : UIHudWindow
{
	[Flags]
	public enum OptionsPage
	{
		MENU = 1,
		GAME = 2,
		AUTOPAUSE = 4,
		DIFFICULTY = 8,
		GRAPHICS = 0x10,
		SOUND = 0x20,
		CONTROLS = 0x40,
		CUSTOM = 0x80,
		CUSTOM2 = 0x100
	}

	public enum OptionsMenuLayout
	{
		InGame,
		MainMenu,
		Difficulty
	}

	public enum Option
	{
		EXPERT_MODE,
		TRIAL_OF_IRON,
		DEPRECATED1,
		AUTOPAUSE,
		AUTOPAUSE_CENTER,
		AUTOPAUSE_ALL,
		AUTOPAUSE_STOP_ON_COMBAT,
		AUTOPAUSE_STOP_ON_SIGHTED,
		BOOL_OPTION,
		DIFFICULTY,
		COUNT
	}

	public GameObject NormalSubWindow;

	public GameObject NewGameSubWindow;

	private const int OptionsPageCount = 7;

	private int[] PageOrder = new int[9] { 0, 1, 3, 2, 4, 5, 6, 7, 8 }; //Added 7 and 8, changed int 7 to 9
	// Added Custom and Custom 2 below
	private OptionsPage m_ValidOptionPages = OptionsPage.MENU | OptionsPage.GAME | OptionsPage.AUTOPAUSE | OptionsPage.DIFFICULTY | OptionsPage.GRAPHICS | OptionsPage.SOUND | OptionsPage.CONTROLS | OptionsPage.CUSTOM | OptionsPage.CUSTOM2;

	private GameMode m_GameMode = new GameMode();

	private ControlMapping m_Controls = new ControlMapping();
	
	public UIRadioButtonGroup PageButtonGroup;

	private UIGrid m_PageButtonGrid;

	private UIMultiSpriteImageButton[] m_PageButtons;

	public int TabButtonHeight;

	public UIMultiSpriteImageButton QuitButton;

	public UIMultiSpriteImageButton LoadButton;

	public UIMultiSpriteImageButton SaveButton;

	public UIMultiSpriteImageButton AcceptButton;

	public UIMultiSpriteImageButton DefControlsButton;

	public UIMultiSpriteImageButton PageButtonPrefab;

	public GameObject[] Pages;

	public int[] PageTitleStringIds;

	private UIOptionsTag[] m_Options;

	private UIOptionsSliderTag[] m_Sliders;

	public UIOptionsSliderGroup CombatTimerSlider;

	public UIOptionsSliderGroup AutoslowThresholdSlider;

	private UIOptionsSliderGroup[] m_VolumeSliders;

	public UIOptionsSliderGroup TooltipDelay;

	public UIOptionsSliderGroup FontSize;

	public UIOptionsSliderGroup GammaSlider;

	public UIOptionsSliderGroup ScrollSpeed;

	public UIOptionsSliderGroup AreaLootRangeSlider;

	public UIOptionsSliderGroup VoiceFrequency;

	public UIOptionsSliderGroup QualitySlider;

	public UIOptionsSliderGroup FrameRateMaxSlider;

	public UIOptionsSliderGroup OcclusionOpacitySlider;

	public UILabel FrameRateMaxValue;

	public UIDropdownMenu ResolutionDropdown;

	public UIDropdownMenu LanguageDropdown;

	public UIMultiSpriteImageButton ApplyResolutionButton;

	public UIOptionsControlManager ControlManager;

	private bool DialogUp;

	private bool m_WarnedDifficulty;

	private MappedControl m_SettingControl;

	private int m_SettingIndex;

	private QuickCheckbox _blueCircles;

	private QuickCheckbox _blueCirclesBg;

	private QuickCheckbox _alwaysShowCircles;

	private QuickCheckbox _unlockCombatInv;

	private QuickCheckbox _fixBackerNames;

	private QuickCheckbox _removeMovingRecovery;

	private QuickDropdown<IEModOptions.FastSneakOptions> _fastSneak;

	private QuickCheckbox _improvedAi;

	private QuickCheckbox _disableFriendlyFire;

	private QuickCheckbox _oneTooltip;

	private QuickCheckbox _disableEngagement;

	private QuickDropdown<IEModOptions.NerfedXpTable> _nerfedXpCmb;

	private QuickCheckbox _lootShuffler;

	private QuickCheckbox _gameSpeed;

	private QuickCheckbox _combatOnly;

	private QuickCheckbox _bonusSpellsPerDay;

	private QuickCheckbox _targetTurnedEnemies;

	private QuickCheckbox _npcDispositionFix;

	private QuickDropdown<IEModOptions.PerEncounterSpells> _perEncounterSpellsCmb;

	private QuickDropdown<IEModOptions.CipherStartingFocus> _cipherStartingFocusCmb;

	private QuickDropdown<IEModOptions.ExtraSpellsInGrimoire> _extraGrimoireSpellsCmb;

	private QuickDropdown<IEModOptions.AutoSaveSetting> _autosaveCmb;

	private QuickDropdown<IEModOptions.MaxAdventurersOptions> _maxAdventurersCmb;

	private QuickDropdown<IEModOptions.MaxCampingSuppliesOptions> _maxCampingSuppliesOptionsCmb;

	private QuickCheckbox _hideAntiClassSpells;

	private QuickCheckbox _hideWeaponEffects;

	private QuickCheckbox _autoLoadCustomStats;

	private QuickCheckbox _enableCheatKeys;

	private QuickCheckbox _chanterPhraseCount;

	private QuickCheckbox _capesHidden;

	private QuickCheckbox _allInventorySlots;

	/*[NewMember]
	private QuickCheckbox _spiritShiftToggle;
	*/
	/*
	[NewMember]
	private QuickCheckbox _xpForMonsterKills;
	*/

	private QuickCheckbox _disableBackerDialog;

	private QuickCheckbox _enableCustomUI;

	public static UIOptionsManager Instance { get; private set; }

	public float CombatTimerSetting
	{
		get
		{
			return CombatTimerSlider.Setting;
		}
		set
		{
			CombatTimerSlider.Setting = (int)value;
		}
	}

	public int AutoslowThresholdSetting
	{
		get
		{
			return (int)AutoslowThresholdSlider.Setting;
		}
		set
		{
			AutoslowThresholdSlider.Setting = value;
		}
	}

	public float TooltipDelaySetting
	{
		get
		{
			return TooltipDelay.Setting;
		}
		set
		{
			TooltipDelay.Setting = value;
		}
	}

	public float FontSizeSetting
	{
		get
		{
			return FontSize.Setting / 100f;
		}
		set
		{
			FontSize.Setting = value * 100f;
		}
	}

	public float GammaSetting
	{
		get
		{
			return GammaSlider.Setting;
		}
		set
		{
			GammaSlider.Setting = value;
		}
	}

	public float AreaLootRangeSetting
	{
		get
		{
			return AreaLootRangeSlider.Setting;
		}
		set
		{
			AreaLootRangeSlider.Setting = value;
		}
	}

	public float VoiceFrequencySetting
	{
		get
		{
			return VoiceFrequency.Setting / 100f;
		}
		set
		{
			VoiceFrequency.Setting = value * 100f;
		}
	}

	public int FrameRateMaxSetting
	{
		get
		{
			if (FrameRateMaxSlider.Slider.Setting == FrameRateMaxSlider.Slider.Range - 1)
			{
				return -1;
			}
			return (int)FrameRateMaxSlider.Setting;
		}
		set
		{
			if (value < 0)
			{
				FrameRateMaxSlider.Slider.Setting = FrameRateMaxSlider.Slider.Range - 1;
			}
			else
			{
				FrameRateMaxSlider.Setting = value;
			}
		}
	}

	public ControlMapping Controls => m_Controls;

	public bool Accepted { get; private set; }

	public bool UnsavedChanges
	{
		get
		{
			if (!m_GameMode.Matches(GameState.Mode))
			{
				return true;
			}
			if (!m_Controls.Matches(GameState.Controls))
			{
				return true;
			}
			return false;
		}
	}

	public override int CyclePosition => 5;

	public bool WaitToSet { get; private set; }

	public float GetGammaSetting()
	{
		return m_GameMode.Option.Gamma;
	}

	public float GetVolumeSetting(MusicManager.SoundCategory category)
	{
		return m_GameMode.Option.GetVolume(category);
	}

	public void SetVolumeSetting(MusicManager.SoundCategory category, float setting)
	{
		m_GameMode.Option.SetVolume(category, setting);
	}

	public void ForceUpdateFullscreen()
	{
		m_GameMode.SetOption(GameOption.BoolOption.FULLSCREEN, Screen.fullScreen);
		ResetControls();
	}

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void dup_Start()
	{
		SetMenuLayout(OptionsMenuLayout.InGame);
		UIMultiSpriteImageButton quitButton = QuitButton;
		quitButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(quitButton.onClick, new UIEventListener.VoidDelegate(OnQuitClicked));
		UIMultiSpriteImageButton saveButton = SaveButton;
		saveButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(saveButton.onClick, new UIEventListener.VoidDelegate(OnSaveClicked));
		UIMultiSpriteImageButton loadButton = LoadButton;
		loadButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(loadButton.onClick, new UIEventListener.VoidDelegate(OnLoadClicked));
		m_Options = GetComponentsInChildren<UIOptionsTag>(includeInactive: true);
		m_Sliders = GetComponentsInChildren<UIOptionsSliderTag>(includeInactive: true);
		PageButtonGroup.OnRadioSelectionChanged += OnChangePage;
		m_PageButtonGrid = PageButtonGroup.GetComponent<UIGrid>();
		m_PageButtons = new UIMultiSpriteImageButton[7];
		m_PageButtons[0] = PageButtonPrefab;
		for (int i = 0; i < 7; i++)
		{
			if (i > 0)
			{
				m_PageButtons[i] = NGUITools.AddChild(PageButtonPrefab.transform.parent.gameObject, PageButtonPrefab.gameObject).GetComponent<UIMultiSpriteImageButton>();
			}
			if (i < PageTitleStringIds.Length)
			{
				GUIStringLabel.Get(m_PageButtons[i].Label).SetString(PageTitleStringIds[i]);
			}
			else
			{
				Debug.LogWarning("Not enough strings provided for every options tab in OptionsManager.");
			}
			m_PageButtons[i].name = PageOrder[i] + "." + m_PageButtons[i].name;
		}
		m_PageButtonGrid.Reposition();
		UIOptionsTag[] options = m_Options;
		foreach (UIOptionsTag uIOptionsTag in options)
		{
			if ((bool)uIOptionsTag.Checkbox)
			{
				UICheckbox checkbox = uIOptionsTag.Checkbox;
				checkbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(checkbox.onStateChange, new UICheckbox.OnStateChange(OnCheckChanged));
			}
		}
		UIOptionsSliderTag[] sliders = m_Sliders;
		for (int j = 0; j < sliders.Length; j++)
		{
			UIOptionsSliderGroup component = sliders[j].GetComponent<UIOptionsSliderGroup>();
			if ((bool)component)
			{
				component.OnChanged = (UIOptionsSliderGroup.OnSettingChanged)Delegate.Combine(component.OnChanged, new UIOptionsSliderGroup.OnSettingChanged(OnSliderChanged));
			}
		}
		UIOptionsSlider slider = CombatTimerSlider.Slider;
		slider.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider.OnChanged, new UIOptionsSlider.OnSettingChanged(OnCombatTimerChanged));
		UIOptionsSlider slider2 = AutoslowThresholdSlider.Slider;
		slider2.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider2.OnChanged, new UIOptionsSlider.OnSettingChanged(OnAutoslowThresholdChanged));
		UIOptionsSlider slider3 = TooltipDelay.Slider;
		slider3.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider3.OnChanged, new UIOptionsSlider.OnSettingChanged(OnTooltipDelayChanged));
		UIOptionsSlider slider4 = FontSize.Slider;
		slider4.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider4.OnChanged, new UIOptionsSlider.OnSettingChanged(OnFontSizeChanged));
		UIOptionsSlider slider5 = GammaSlider.Slider;
		slider5.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider5.OnChanged, new UIOptionsSlider.OnSettingChanged(OnGammaSliderChanged));
		UIOptionsSlider slider6 = AreaLootRangeSlider.Slider;
		slider6.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider6.OnChanged, new UIOptionsSlider.OnSettingChanged(OnAreaLootSliderChanged));
		UIOptionsSlider slider7 = VoiceFrequency.Slider;
		slider7.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider7.OnChanged, new UIOptionsSlider.OnSettingChanged(OnVoiceFrequencyChanged));
		UIOptionsSlider slider8 = ScrollSpeed.Slider;
		slider8.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider8.OnChanged, new UIOptionsSlider.OnSettingChanged(OnScrollSpeedChanged));
		LanguageDropdown.OnDropdownOptionChanged += OnLanguageChanged;
		ResolutionDropdown.OnDropdownOptionChanged += OnResolutionChanged;
		UIOptionsSlider slider9 = QualitySlider.Slider;
		slider9.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider9.OnChanged, new UIOptionsSlider.OnSettingChanged(OnQualityChanged));
		UIOptionsSlider slider10 = FrameRateMaxSlider.Slider;
		slider10.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider10.OnChanged, new UIOptionsSlider.OnSettingChanged(OnMaxFPSChanged));
		UIOptionsSlider slider11 = OcclusionOpacitySlider.Slider;
		slider11.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider11.OnChanged, new UIOptionsSlider.OnSettingChanged(OnOcclusionOpacityChanged));
		m_VolumeSliders = new UIOptionsSliderGroup[4];
		UIOptionsVolumeSlider[] componentsInChildren = GetComponentsInChildren<UIOptionsVolumeSlider>(includeInactive: true);
		foreach (UIOptionsVolumeSlider uIOptionsVolumeSlider in componentsInChildren)
		{
			if (m_VolumeSliders[(int)uIOptionsVolumeSlider.Category] == null)
			{
				UIOptionsSliderGroup component2 = uIOptionsVolumeSlider.GetComponent<UIOptionsSliderGroup>();
				m_VolumeSliders[(int)uIOptionsVolumeSlider.Category] = component2;
				UIOptionsSlider slider12 = component2.Slider;
				slider12.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider12.OnChanged, new UIOptionsSlider.OnSettingChanged(OnVolumeChanged));
			}
		}
		UIMultiSpriteImageButton acceptButton = AcceptButton;
		acceptButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(acceptButton.onClick, new UIEventListener.VoidDelegate(OnAcceptClick));
		UIMultiSpriteImageButton defControlsButton = DefControlsButton;
		defControlsButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(defControlsButton.onClick, new UIEventListener.VoidDelegate(OnRestoreDefaultControls));
		UIMultiSpriteImageButton applyResolutionButton = ApplyResolutionButton;
		applyResolutionButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(applyResolutionButton.onClick, new UIEventListener.VoidDelegate(OnApplyResolution));
	}

	private void Start()
	{
		//NEW CODE HERE
		var exampleCheckbox =
			this.ComponentsInDescendants<UIOptionsTag>(true).Single(
				opt => opt.Checkbox && opt.BoolSuboption == GameOption.BoolOption.SCREEN_EDGE_SCROLLING)
				.transform;

		Prefabs.QuickCheckbox = exampleCheckbox.Component<UIOptionsTag>();
		Prefabs.QuickDropdown = ResolutionDropdown.transform.parent.gameObject;
		Prefabs.QuickButton = UIOptionsManager.Instance.PageButtonPrefab.gameObject;
		Prefabs.QuickPage = Pages[5];

		var pageParent = Pages[5].transform.parent;

		var ieModOptions = new QuickPage(pageParent, "IEModOptions_Settings_Page");
		var ieModDisposition = new QuickPage(pageParent, "IEModOptions_Disposition_Page");
		Pages = Pages.Concat(new[] {
				ieModOptions.GameObject,
				ieModDisposition.GameObject
			}).ToArray();

		//ORIGINAL CODE
		SetMenuLayout(OptionsMenuLayout.InGame);
		UIMultiSpriteImageButton quitButton = QuitButton;
		quitButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(quitButton.onClick, new UIEventListener.VoidDelegate(OnQuitClicked));
		UIMultiSpriteImageButton saveButton = SaveButton;
		saveButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(saveButton.onClick, new UIEventListener.VoidDelegate(OnSaveClicked));
		UIMultiSpriteImageButton loadButton = LoadButton;
		loadButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(loadButton.onClick, new UIEventListener.VoidDelegate(OnLoadClicked));
		m_Options = GetComponentsInChildren<UIOptionsTag>(includeInactive: true);
		m_Sliders = GetComponentsInChildren<UIOptionsSliderTag>(includeInactive: true);
		
		//NEW CODE
		var quickFactory = new QuickFactory()
		{
			CurrentParent = Pages[7].transform
		};

		//File.WriteAllText("ComboboxDump.txt", UnityObjectDumper.PrintUnityGameObject(exampleDropdown.gameObject, null, x => false));
		quickFactory.CurrentParent = Pages[7].transform;

		var column1Top = new Vector3(-210, 330, 0);

		//The following are the controls that appear in the GUI of the mod.
		_oneTooltip = quickFactory.Checkbox(() => IEModOptions.OneTooltip);
		_oneTooltip.Transform.localPosition = column1Top;

		_disableEngagement = quickFactory.Checkbox(() => IEModOptions.DisableEngagement);
		_disableEngagement.LocalPosition = column1Top.Plus(y: -30);

		_blueCircles = quickFactory.Checkbox(() => IEModOptions.BlueCircles);
		_blueCircles.Transform.localPosition = column1Top.Plus(y: -60);

		_blueCirclesBg = quickFactory.Checkbox(() => IEModOptions.BlueCirclesBG);
		_blueCirclesBg.Transform.localPosition = column1Top.Plus(x: +30, y: -90);

		_blueCircles.IsChecked.HasChanged += x => {
			if (x.Value)
			{
				_blueCirclesBg.OptionsTagComponent.Enable();
			}
			else
			{
				_blueCirclesBg.IsChecked.Value = false;
				_blueCirclesBg.OptionsTagComponent.Disable();
			}
		};
		_blueCircles.IsChecked.NotifyChange();

		_alwaysShowCircles = quickFactory.Checkbox(() => IEModOptions.AlwaysShowCircles);
		_alwaysShowCircles.Transform.localPosition = column1Top.Plus(y: -120);

		_unlockCombatInv = quickFactory.Checkbox(() => IEModOptions.UnlockCombatInv);
		_unlockCombatInv.Transform.localPosition = column1Top.Plus(y: -150);

		_npcDispositionFix = quickFactory.Checkbox(() => IEModOptions.NPCDispositionFix);
		_npcDispositionFix.Transform.localPosition = column1Top.Plus(y: -180);

		_removeMovingRecovery = quickFactory.Checkbox(() => IEModOptions.RemoveMovingRecovery);
		_removeMovingRecovery.Transform.localPosition = column1Top.Plus(y: -210);

		//_fastSneak = quickFactory.Checkbox(() => IEModOptions.FastSneak);
		//_fastSneak.Transform.localPosition = column1Top.Plus(y:-240);

		/* * * TJH 8/28/2015. Improved AI Excluded from v2.0 
		_improvedAi = quickFactory.Checkbox(() => IEModOptions.ImprovedAI);
		_improvedAi.Transform.localPosition = column1Top.Plus(y: -270);
		* * */

		_disableFriendlyFire = quickFactory.Checkbox(() => IEModOptions.DisableFriendlyFire);
		/* * * TJH 8/28/2015. Moved up since Improved AI and Fast Sneak are no longer present here * * */
		_disableFriendlyFire.Transform.localPosition = column1Top.Plus(y: -240);

		var column2Top = column1Top.Plus(x: +420);

		_lootShuffler = quickFactory.Checkbox(() => IEModOptions.LootShuffler);
		_lootShuffler.Transform.localPosition = column2Top;

		_gameSpeed = quickFactory.Checkbox(() => IEModOptions.GameSpeedMod);
		_gameSpeed.Transform.localPosition = column2Top.Plus(y: -30);

		_combatOnly = quickFactory.Checkbox(() => IEModOptions.CombatOnlyMod);
		_combatOnly.Transform.localPosition = column2Top.Plus(y: -60);

		_bonusSpellsPerDay = quickFactory.Checkbox(() => IEModOptions.BonusSpellsPerDay);
		_bonusSpellsPerDay.Transform.localPosition = column2Top.Plus(y: -90);

		_targetTurnedEnemies = quickFactory.Checkbox(() => IEModOptions.TargetTurnedEnemies);
		_targetTurnedEnemies.Transform.localPosition = column2Top.Plus(y: -120);

		_fixBackerNames = quickFactory.Checkbox(() => IEModOptions.FixBackerNames);
		_fixBackerNames.Transform.localPosition = column2Top.Plus(y: -150);

		_disableBackerDialog = quickFactory.Checkbox(() => IEModOptions.DisableBackerDialogs);
		_disableBackerDialog.Transform.localPosition = column2Top.Plus(x: +30, y: -180);

		_fixBackerNames.IsChecked.OnChange(v => {
			if (v.Value)
			{
				_disableBackerDialog.OptionsTagComponent.Enable();
			}
			else
			{
				_disableBackerDialog.IsChecked.Value = false;
				_disableBackerDialog.OptionsTagComponent.Disable();
			}
		});
		_fixBackerNames.IsChecked.NotifyChange();

		_enableCustomUI = quickFactory.Checkbox(() => IEModOptions.EnableCustomUi);
		_enableCustomUI.LocalPosition = column2Top.Plus(y: -210);

		_hideAntiClassSpells = quickFactory.Checkbox(() => IEModOptions.HideAnticlassSpells);
		_hideAntiClassSpells.LocalPosition = column2Top.Plus(y: -240);

		_hideWeaponEffects = quickFactory.Checkbox(() => IEModOptions.HideWeaponEffects);
		_hideWeaponEffects.LocalPosition = column2Top.Plus(y: -270);

		_autoLoadCustomStats = quickFactory.Checkbox(() => IEModOptions.AutoLoadCustomStats);
		_autoLoadCustomStats.LocalPosition = column1Top.Plus(y: -270);

		_enableCheatKeys = quickFactory.Checkbox(() => IEModOptions.EnableCheatKeys);
		_enableCheatKeys.LocalPosition = column1Top.Plus(y: -300);

		_chanterPhraseCount = quickFactory.Checkbox(() => IEModOptions.ChanterPhraseCount);
		_chanterPhraseCount.LocalPosition = column2Top.Plus(y: -300);

		var centerCmbTop = new Vector3(-80, -40, 0);
		const int cmbLabelWidth = 300;
		const int cmbWidth = 515;

		_nerfedXpCmb = quickFactory.EnumDropdown(() => IEModOptions.NerfedXPTableSetting);

		_nerfedXpCmb.Width = cmbWidth;
		_nerfedXpCmb.LabelWidth = cmbLabelWidth;
		_nerfedXpCmb.LocalPosition = centerCmbTop;

		_perEncounterSpellsCmb = quickFactory.EnumDropdown(() => IEModOptions.PerEncounterSpellsSetting);
		_perEncounterSpellsCmb.LabelWidth = cmbLabelWidth;
		_perEncounterSpellsCmb.Width = cmbWidth;
		_perEncounterSpellsCmb.LocalPosition = centerCmbTop.Plus(y: -30);

		_extraGrimoireSpellsCmb = quickFactory.EnumDropdown(() => IEModOptions.ExtraWizardSpells);
		_extraGrimoireSpellsCmb.Width = cmbWidth;
		_extraGrimoireSpellsCmb.LabelWidth = cmbLabelWidth;
		_extraGrimoireSpellsCmb.Transform.localPosition = centerCmbTop.Plus(y: -60);

		_cipherStartingFocusCmb = quickFactory.EnumDropdown(() => IEModOptions.CipherStartingFocusSetting);
		_cipherStartingFocusCmb.LabelWidth = cmbLabelWidth;
		_cipherStartingFocusCmb.Width = cmbWidth;
		_cipherStartingFocusCmb.LocalPosition = centerCmbTop.Plus(y: -90);

		_autosaveCmb = quickFactory.EnumDropdown(() => IEModOptions.AutosaveSetting);
		_autosaveCmb.Width = cmbWidth;
		_autosaveCmb.LabelWidth = cmbLabelWidth;
		_autosaveCmb.Transform.localPosition = centerCmbTop.Plus(y: -120);

		_fastSneak = quickFactory.EnumDropdown(() => IEModOptions.FastSneak);
		_fastSneak.Width = cmbWidth;
		_fastSneak.LabelWidth = cmbLabelWidth;
		_fastSneak.Transform.localPosition = centerCmbTop.Plus(y: -150);

		_maxAdventurersCmb = quickFactory.EnumDropdown(() => IEModOptions.MaxAdventurersCount);
		_maxAdventurersCmb.Width = cmbWidth;
		_maxAdventurersCmb.LabelWidth = cmbLabelWidth;
		_maxAdventurersCmb.Transform.localPosition = centerCmbTop.Plus(y: -180);

		_maxCampingSuppliesOptionsCmb = quickFactory.EnumDropdown(() => IEModOptions.MaxCampingSupplies);
		_maxCampingSuppliesOptionsCmb.Width = cmbWidth;
		_maxCampingSuppliesOptionsCmb.LabelWidth = cmbLabelWidth;
		_maxCampingSuppliesOptionsCmb.Transform.localPosition = centerCmbTop.Plus(y: -210);

		_capesHidden = quickFactory.Checkbox(() => IEModOptions.CapesHidden);
		_capesHidden.LocalPosition = column1Top.Plus(y: -327);

		_allInventorySlots = quickFactory.Checkbox(() => IEModOptions.AllInventorySlots);
		_allInventorySlots.LocalPosition = column2Top.Plus(y: -327);

		// Pallegina dispositions mod page
		quickFactory.CurrentParent = ieModDisposition.Transform;

		var favoredDisposition1 = quickFactory.EnumDropdown(() => IEModOptions.PalleginaFavored1);
		favoredDisposition1.Width = 150;
		favoredDisposition1.LabelWidth = cmbLabelWidth;
		favoredDisposition1.LocalPosition = new Vector3(-60, cmbLabelWidth, 0);

		var favoredDisposition2 = quickFactory.EnumDropdown(() => IEModOptions.PalleginaFavored2);
		favoredDisposition2.LabelWidth = 0;
		favoredDisposition2.Width = 150;
		favoredDisposition2.LocalPosition = new Vector3(100, cmbLabelWidth, 0);

		var disDisposition1 = quickFactory.EnumDropdown(() => IEModOptions.PalleginaDisfavored1);
		disDisposition1.Width = 150;
		disDisposition1.LabelWidth = cmbLabelWidth;
		disDisposition1.LocalPosition = new Vector3(-60, 250, 0);

		var disDisposition2 = quickFactory.EnumDropdown(() => IEModOptions.PalleginaDisfavored2);
		disDisposition2.LocalPosition = new Vector3(100, 250, 0);
		disDisposition2.LabelWidth = 0;
		disDisposition2.Width = 150;

		//REPLACES ORIGINAL CODE WITH SOME TWEAKED LINES (mostly due to changed number of tabs)
		PageButtonGroup.OnRadioSelectionChanged += new UIRadioButtonGroup.RadioSelectionChanged(OnChangePage); // changed to Event
		m_PageButtonGrid = PageButtonGroup.GetComponent<UIGrid>();
		m_PageButtons = new UIMultiSpriteImageButton[9]; //Changed from 7 to 9
		m_PageButtons[0] = PageButtonPrefab;
		for (int i = 0; i < 9; i++) //Changed from 7 to 9
		{
			if (i > 0)
			{
				m_PageButtons[i] = NGUITools.AddChild(PageButtonPrefab.transform.parent.gameObject, PageButtonPrefab.gameObject).GetComponent<UIMultiSpriteImageButton>();
			}
			if (i < PageTitleStringIds.Length)
			{
				GUIStringLabel.Get(m_PageButtons[i].Label).SetString(PageTitleStringIds[i]);
			}
			else if(i == this.PageTitleStringIds.Length) // added this line
			{
				GUIStringLabel.Get(this.m_PageButtons[i].Label).FormatString = "IE Mod"; // added this line
				//Debug.LogWarning("Not enough strings provided for every options tab in OptionsManager.");
			}
			else if (i == this.PageTitleStringIds.Length + 1)
            {
				GUIStringLabel.Get(this.m_PageButtons[i].Label).FormatString = "Dispositions"; // added this line
				//Debug.LogWarning("Not enough strings provided for every options tab in OptionsManager.");
			}
			m_PageButtons[i].name = PageOrder[i] + "." + m_PageButtons[i].name;
		}
		//PURE ORIGINAL CODE NOW
		m_PageButtonGrid.Reposition();
		UIOptionsTag[] options = m_Options;
		foreach (UIOptionsTag uIOptionsTag in options)
		{
			if ((bool)uIOptionsTag.Checkbox)
			{
				UICheckbox checkbox = uIOptionsTag.Checkbox;
				checkbox.onStateChange = (UICheckbox.OnStateChange)Delegate.Combine(checkbox.onStateChange, new UICheckbox.OnStateChange(OnCheckChanged));
			}
		}
		UIOptionsSliderTag[] sliders = m_Sliders;
		for (int j = 0; j < sliders.Length; j++)
		{
			UIOptionsSliderGroup component = sliders[j].GetComponent<UIOptionsSliderGroup>();
			if ((bool)component)
			{
				component.OnChanged = (UIOptionsSliderGroup.OnSettingChanged)Delegate.Combine(component.OnChanged, new UIOptionsSliderGroup.OnSettingChanged(OnSliderChanged));
			}
		}
		UIOptionsSlider slider = CombatTimerSlider.Slider;
		slider.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider.OnChanged, new UIOptionsSlider.OnSettingChanged(OnCombatTimerChanged));
		UIOptionsSlider slider2 = AutoslowThresholdSlider.Slider;
		slider2.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider2.OnChanged, new UIOptionsSlider.OnSettingChanged(OnAutoslowThresholdChanged));
		UIOptionsSlider slider3 = TooltipDelay.Slider;
		slider3.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider3.OnChanged, new UIOptionsSlider.OnSettingChanged(OnTooltipDelayChanged));
		UIOptionsSlider slider4 = FontSize.Slider;
		slider4.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider4.OnChanged, new UIOptionsSlider.OnSettingChanged(OnFontSizeChanged));
		UIOptionsSlider slider5 = GammaSlider.Slider;
		slider5.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider5.OnChanged, new UIOptionsSlider.OnSettingChanged(OnGammaSliderChanged));
		UIOptionsSlider slider6 = AreaLootRangeSlider.Slider;
		slider6.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider6.OnChanged, new UIOptionsSlider.OnSettingChanged(OnAreaLootSliderChanged));
		UIOptionsSlider slider7 = VoiceFrequency.Slider;
		slider7.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider7.OnChanged, new UIOptionsSlider.OnSettingChanged(OnVoiceFrequencyChanged));
		UIOptionsSlider slider8 = ScrollSpeed.Slider;
		slider8.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider8.OnChanged, new UIOptionsSlider.OnSettingChanged(OnScrollSpeedChanged));
		LanguageDropdown.OnDropdownOptionChanged += OnLanguageChanged;
		ResolutionDropdown.OnDropdownOptionChanged += OnResolutionChanged;
		UIOptionsSlider slider9 = QualitySlider.Slider;
		slider9.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider9.OnChanged, new UIOptionsSlider.OnSettingChanged(OnQualityChanged));
		UIOptionsSlider slider10 = FrameRateMaxSlider.Slider;
		slider10.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider10.OnChanged, new UIOptionsSlider.OnSettingChanged(OnMaxFPSChanged));
		UIOptionsSlider slider11 = OcclusionOpacitySlider.Slider;
		slider11.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider11.OnChanged, new UIOptionsSlider.OnSettingChanged(OnOcclusionOpacityChanged));
		m_VolumeSliders = new UIOptionsSliderGroup[4];
		UIOptionsVolumeSlider[] componentsInChildren = GetComponentsInChildren<UIOptionsVolumeSlider>(includeInactive: true);
		foreach (UIOptionsVolumeSlider uIOptionsVolumeSlider in componentsInChildren)
		{
			if (m_VolumeSliders[(int)uIOptionsVolumeSlider.Category] == null)
			{
				UIOptionsSliderGroup component2 = uIOptionsVolumeSlider.GetComponent<UIOptionsSliderGroup>();
				m_VolumeSliders[(int)uIOptionsVolumeSlider.Category] = component2;
				UIOptionsSlider slider12 = component2.Slider;
				slider12.OnChanged = (UIOptionsSlider.OnSettingChanged)Delegate.Combine(slider12.OnChanged, new UIOptionsSlider.OnSettingChanged(OnVolumeChanged));
			}
		}
		UIMultiSpriteImageButton acceptButton = AcceptButton;
		acceptButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(acceptButton.onClick, new UIEventListener.VoidDelegate(OnAcceptClick));
		UIMultiSpriteImageButton defControlsButton = DefControlsButton;
		defControlsButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(defControlsButton.onClick, new UIEventListener.VoidDelegate(OnRestoreDefaultControls));
		UIMultiSpriteImageButton applyResolutionButton = ApplyResolutionButton;
		applyResolutionButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(applyResolutionButton.onClick, new UIEventListener.VoidDelegate(OnApplyResolution));
	}

	private void OnRestoreDefaultControls(GameObject sender)
	{
		UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.GetText(1740)).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox boxsend)
		{
			if (result == UIMessageBox.Result.AFFIRMATIVE)
			{
				m_Controls.CopyFrom(MappedInput.DefaultMapping);
				ControlManager.Reload(MappedControl.NONE);
			}
		};
	}

	private void OnApplyResolution(GameObject sender)
	{
		m_GameMode.Option.ApplyResolution();
	}

	private void OnAcceptClick(GameObject sender)
	{
		Accepted = true;
		ApplyChangesToGame();
		HideWindow();
	}

	private void OnQuitClicked(GameObject go)
	{
		string title = "";
		string text = GUIUtils.GetText(1888);
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, title, text);
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnQuitDialogEnd));
	}

	private void OnQuitDialogEnd(UIMessageBox.Result result, UIMessageBox sender)
	{
		sender.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Remove(sender.OnDialogEnd, new UIMessageBox.OnEndDialog(OnQuitDialogEnd));
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			HideWindow();
			GameState.LoadMainMenu(fadeOut: true);
		}
	}

	private void OnLoadClicked(GameObject sender)
	{
		HideWindow();
		UISaveLoadManager.Instance.ToggleAlt();
	}

	private void OnSaveClicked(GameObject sender)
	{
		HideWindow();
		UISaveLoadManager.Instance.Toggle();
	}

	private void OnCombatTimerChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.AutoPause.CombatRoundTime = CombatTimerSetting;
	}

	private void OnAutoslowThresholdChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.AutoPause.AutoslowEnemyThreshold = AutoslowThresholdSetting;
	}

	private void OnVolumeChanged(UIOptionsSlider sender, int setting)
	{
		UIOptionsVolumeSlider component = sender.GetComponent<UIOptionsVolumeSlider>();
		if ((bool)component)
		{
			m_GameMode.Option.SetVolume(component.Category, (float)setting / 100f);
		}
	}

	private void OnTooltipDelayChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.TooltipDelay = TooltipDelaySetting;
	}

	private void OnFontSizeChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.FontScale = FontSizeSetting;
	}

	private void OnGammaSliderChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.Gamma = GammaSetting;
	}

	private void OnAreaLootSliderChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.AreaLootRange = AreaLootRangeSetting;
	}

	private void OnVoiceFrequencyChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.VoiceFrequency = VoiceFrequencySetting;
	}

	private void OnMaxFPSChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.FrameRateMax = FrameRateMaxSetting;
		if (FrameRateMaxSetting < 0)
		{
			FrameRateMaxValue.text = GUIUtils.GetText(1934);
		}
		else
		{
			FrameRateMaxValue.text = FrameRateMaxSetting.ToString();
		}
	}

	private void OnScrollSpeedChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.ScrollSpeed = ScrollSpeed.Setting;
	}

	private void OnOcclusionOpacityChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.OcclusionOpacity = OcclusionOpacitySlider.Setting;
	}

	private void OnQualityChanged(UIOptionsSlider sender, int setting)
	{
		m_GameMode.Option.Quality = setting;
	}

	private void OnLanguageChanged(object setting)
	{
		Language language = (Language)setting;
		m_GameMode.Option.LanguageName = language.Name;
	}

	private void OnResolutionChanged(object setting)
	{
		m_GameMode.Option.Resolution = (UIDropdownResolution.MyResolution)setting;
	}

	private void OnCheckChanged(GameObject sender, bool state)
	{
		for (int i = 0; i < m_Options.Length; i++)
		{
			if ((bool)m_Options[i].Checkbox && m_Options[i].Checkbox.gameObject == sender)
			{
				SetOption(m_Options[i], state);
			}
		}
		ResetControls();
	}

	private void OnSliderChanged(UIOptionsSliderGroup sender, float newSetting)
	{
	}

	public bool IsSettingControl(MappedControl control, int index)
	{
		if (m_SettingControl == control)
		{
			return m_SettingIndex == index;
		}
		return false;
	}

	public void BeginMapControl(MappedControl control, int index)
	{
		WaitToSet = true;
		m_SettingControl = control;
		m_SettingIndex = index;
		GameInput.BeginBlockAllKeys();
	}

	public void CancelMapControl()
	{
		MappedControl settingControl = m_SettingControl;
		WaitToSet = true;
		m_SettingControl = MappedControl.NONE;
		m_SettingIndex = 0;
		ControlManager.Reload(settingControl);
		GameInput.EndBlockAllKeys();
		GameInput.HandleAllKeys();
	}

	public void ClearControl(MappedControl control, int index)
	{
		List<KeyControl> list = m_Controls.Controls[(int)control];
		if (index >= 0 && index < list.Count)
		{
			list[index] = default(KeyControl);
			ControlManager.Reload(control);
		}
	}

	private void Update()
	{
		if (m_SettingControl != 0)
		{
			for (KeyCode keyCode = KeyCode.Mouse0; keyCode <= KeyCode.Mouse6; keyCode++)
			{
				if (!WaitToSet && Input.GetMouseButtonUp((int)(keyCode - 323)) && !MappedInput.MouseForbidden[(int)m_SettingControl])
				{
					TryBind(new KeyControl(keyCode));
					break;
				}
			}
		}
		WaitToSet = false;
	}

	private void OnGUI()
	{
		if (m_SettingControl == MappedControl.NONE || (Event.current.type != EventType.KeyUp && Event.current.type != EventType.MouseUp))
		{
			return;
		}
		KeyCode keyCode = Event.current.keyCode;
		if (Event.current.type == EventType.MouseUp)
		{
			if (MappedInput.MouseForbidden[(int)m_SettingControl])
			{
				GameInput.HandleAllKeys();
				WaitToSet = true;
				return;
			}
			keyCode = (KeyCode)(323 + Event.current.button);
		}
		if (MappedInput.Forbidden.Contains(keyCode))
		{
			GameInput.HandleAllKeys();
			WaitToSet = true;
			return;
		}
		KeyControl kc = new KeyControl(keyCode);
		kc.ShiftKey = (Event.current.modifiers & EventModifiers.Shift) > EventModifiers.None;
		kc.AltKey = (Event.current.modifiers & EventModifiers.Alt) > EventModifiers.None;
		kc.CtrlKey = (Event.current.modifiers & EventModifiers.Control) > EventModifiers.None;
		TryBind(kc);
		GameInput.EndBlockAllKeys();
		GameInput.HandleAllKeys();
	}

	private void TryBind(KeyControl kc)
	{
		WaitToSet = true;
		List<KeyControl> controls = m_Controls.Controls[(int)m_SettingControl];
		while (controls.Count <= m_SettingIndex)
		{
			controls.Add(default(KeyControl));
		}
		List<MappedControl> conflicts = new List<MappedControl>();
		if ((bool)Instance && Instance.Controls != null)
		{
			for (int i = 0; i < Instance.Controls.Controls.Length; i++)
			{
				if (MappedInput.IsOverlapAllowed(m_SettingControl, (MappedControl)i))
				{
					continue;
				}
				List<KeyControl> list = Instance.Controls.Controls[i];
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].Equals(kc))
					{
						conflicts.Add((MappedControl)i);
					}
				}
			}
		}
		MappedControl settingControl = m_SettingControl;
		m_SettingControl = MappedControl.NONE;
		if (conflicts.Count > 0)
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.Format(1754, kc.ToString(), string.Join(", ", conflicts.Select((MappedControl mc) => MappedInput.GetControlName(mc)).ToArray()))).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox sender)
			{
				if (result == UIMessageBox.Result.AFFIRMATIVE)
				{
					foreach (MappedControl item in conflicts)
					{
						ClearControl(item, Instance.Controls.Controls[(int)item].IndexOf(kc));
					}
					controls[m_SettingIndex] = kc;
				}
				ControlManager.Reload(MappedControl.NONE);
			};
		}
		else
		{
			controls[m_SettingIndex] = kc;
			ControlManager.Reload(settingControl);
		}
	}

	private void SetOption(UIOptionsTag tag, bool val)
	{
		switch (tag.Option)
		{
		case Option.EXPERT_MODE:
			m_GameMode.Expert = val;
			break;
		case Option.TRIAL_OF_IRON:
			m_GameMode.TrialOfIron = val;
			break;
		case Option.AUTOPAUSE:
			if (tag.Autoslow)
			{
				m_GameMode.Option.AutoPause.SetSlowEvent(tag.AutopauseSuboption, val);
			}
			else
			{
				m_GameMode.Option.AutoPause.SetEvent(tag.AutopauseSuboption, val);
			}
			break;
		case Option.AUTOPAUSE_CENTER:
			m_GameMode.Option.AutoPause.CenterOnCharacter = val;
			break;
		case Option.AUTOPAUSE_ALL:
			m_GameMode.Option.AutoPause.SetAll(val);
			break;
		case Option.AUTOPAUSE_STOP_ON_COMBAT:
			m_GameMode.Option.AutoPause.EnteringCombatStopsMovement = val;
			break;
		case Option.AUTOPAUSE_STOP_ON_SIGHTED:
			m_GameMode.Option.AutoPause.EnemySpottedStopMovement = val;
			break;
		case Option.BOOL_OPTION:
			m_GameMode.Option.SetOption(tag.BoolSuboption, val);
			break;
		case Option.DIFFICULTY:
			if (val)
			{
				m_GameMode.Option.Difficulty = tag.Difficulty;
			}
			break;
		case Option.DEPRECATED1:
			break;
		}
	}

	private UIOptionsTag TagWithOption(Option opt)
	{
		UIOptionsTag[] options = m_Options;
		foreach (UIOptionsTag uIOptionsTag in options)
		{
			if (uIOptionsTag.Option == opt)
			{
				return uIOptionsTag;
			}
		}
		return null;
	}

	private void OnDialogEnd(UIMessageBox.Result result, UIMessageBox owner)
	{
		DialogUp = false;
		switch (result)
		{
		case UIMessageBox.Result.AFFIRMATIVE:
			if (owner.CheckboxActive)
			{
				m_GameMode.Option.SetOption(GameOption.BoolOption.ALWAYS_SAVE_OPTIONS, setting: true);
			}
			ApplyChangesToGame();
			HideWindow();
			break;
		case UIMessageBox.Result.NEGATIVE:
			ReloadGameOptions();
			HideWindow();
			break;
		}
	}

	private void OnChangePage(UIMultiSpriteImageButton but)
	{
		for (int i = 0; i < m_PageButtons.Length; i++)
		{
			if (m_PageButtons[i] == but)
			{
				Pages[i].SetActive(value: true);
			}
			else
			{
				Pages[i].SetActive(value: false);
			}
		}
		UIOptionsTooltip.Hide();
	}

	public void ChangePage(int newpage)
	{
		PageButtonGroup.DoSelect(m_PageButtons[newpage].gameObject);
	}

	public void SetMenuLayout(OptionsMenuLayout layout)
	{
		switch (layout)
		{
		case OptionsMenuLayout.InGame:
			m_ValidOptionPages = OptionsPage.MENU | OptionsPage.GAME | OptionsPage.AUTOPAUSE | OptionsPage.DIFFICULTY | OptionsPage.GRAPHICS | OptionsPage.SOUND | OptionsPage.CONTROLS;
			break;
		case OptionsMenuLayout.MainMenu:
			m_ValidOptionPages = OptionsPage.GAME | OptionsPage.AUTOPAUSE | OptionsPage.DIFFICULTY | OptionsPage.GRAPHICS | OptionsPage.SOUND | OptionsPage.CONTROLS;
			break;
		case OptionsMenuLayout.Difficulty:
			m_ValidOptionPages = OptionsPage.GAME;
			break;
		}
	}

	public override bool CanShow()
	{
		return !GameState.IsLoading;
	}

	protected override void Show()
	{
		NormalSubWindow.SetActive(!base.AlternateMode);
		NewGameSubWindow.SetActive(base.AlternateMode);
		if (base.AlternateMode)
		{
			GameState.Mode.Difficulty = GameDifficulty.Easy;
			GameState.Mode.TrialOfIron = false;
			GameState.Mode.Expert = false;
		}
		SaveButton.enabled = !InGameHUD.Instance || InGameHUD.Instance.QuicksaveAllowed;
		Accepted = false;
		m_WarnedDifficulty = false;
		int num = -1;
		for (int i = 0; i < 7; i++)
		{
			bool flag = ((uint)m_ValidOptionPages & (uint)(1 << i)) == (uint)(1 << i);
			m_PageButtons[i].gameObject.SetActive(flag);
			if (num < 0 && flag)
			{
				num = i;
			}
		}
		m_PageButtonGrid.Reposition();
		PageButtonGroup.Reinitialize();
		ChangePage(num);
		ReloadGameOptions();
	}

	protected override bool Hide(bool forced)
	{
		GameInput.EndBlockAllKeys();
		SetMenuLayout(OptionsMenuLayout.InGame);
		if (DialogUp && !forced)
		{
			return false;
		}
		if (UnsavedChanges)
		{
			if (!forced && !m_WarnedDifficulty && GameState.Mode.Difficulty != m_GameMode.Difficulty && NormalSubWindow.activeSelf)
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.GetText(1819)).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox snd)
				{
					if (result == UIMessageBox.Result.AFFIRMATIVE)
					{
						m_WarnedDifficulty = true;
						Hide(forced);
					}
				};
				return false;
			}
			if (forced || (GameState.Option.GetOption(GameOption.BoolOption.ALWAYS_SAVE_OPTIONS) && m_GameMode.GetOption(GameOption.BoolOption.ALWAYS_SAVE_OPTIONS)))
			{
				ApplyChangesToGame();
			}
			else if (!NewGameSubWindow.activeSelf)
			{
				DialogUp = true;
				UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, "", GUIUtils.GetText(213), 212);
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnDialogEnd));
				return false;
			}
		}
		return base.Hide(forced);
	}

	private void ApplyChangesToGame()
	{
		GameState.Mode.CopyFrom(m_GameMode);
		GameState.Mode.Option.AutoPause.SaveOptions();
		GameState.Controls.CopyFrom(m_Controls);
		GameState.Mode.SaveToPrefs();
		GameState.Controls.SaveToPrefs();
		GameState.Mode.LoadFromPrefs();
		GameState.Controls.LoadFromPrefs();
	}

	private void ReloadGameOptions()
	{
		m_GameMode.CopyFrom(GameState.Mode);
		m_Controls.CopyFrom(GameState.Controls);
		for (MusicManager.SoundCategory soundCategory = MusicManager.SoundCategory.MASTER; soundCategory < MusicManager.SoundCategory.COUNT; soundCategory++)
		{
			if ((bool)m_VolumeSliders[(int)soundCategory])
			{
				m_VolumeSliders[(int)soundCategory].Setting = m_GameMode.Option.GetRawVolume(soundCategory) * 100f;
			}
		}
		CombatTimerSetting = m_GameMode.Option.AutoPause.CombatRoundTime;
		AutoslowThresholdSetting = m_GameMode.Option.AutoPause.AutoslowEnemyThreshold;
		TooltipDelaySetting = m_GameMode.Option.TooltipDelay;
		FontSizeSetting = m_GameMode.Option.FontScale;
		GammaSetting = m_GameMode.Option.Gamma;
		AreaLootRangeSetting = m_GameMode.Option.AreaLootRange;
		VoiceFrequencySetting = m_GameMode.Option.VoiceFrequency;
		ScrollSpeed.Setting = m_GameMode.Option.ScrollSpeed;
		OcclusionOpacitySlider.Setting = m_GameMode.Option.OcclusionOpacity;
		QualitySlider.Slider.Setting = m_GameMode.Option.Quality;
		LanguageDropdown.SelectedItem = StringTableManager.GetLanguage(m_GameMode.Option.LanguageName);
		ResolutionDropdown.SelectedItem = m_GameMode.Option.Resolution;
		FrameRateMaxSetting = m_GameMode.Option.FrameRateMax;
		ApplyChangesToGame();
		ResetControls();
	}

	private void ResetControls()
	{
		ControlManager.Reload(MappedControl.NONE);
		CombatTimerSetting = m_GameMode.Option.AutoPause.CombatRoundTime;
		AutoslowThresholdSetting = m_GameMode.Option.AutoPause.AutoslowEnemyThreshold;
		UIOptionsTag uIOptionsTag = null;
		UIOptionsTag[] options = m_Options;
		foreach (UIOptionsTag uIOptionsTag2 in options)
		{
			if (uIOptionsTag2.Checkbox == null)
			{
				continue;
			}
			bool flag = false;
			switch (uIOptionsTag2.Option)
			{
			case Option.EXPERT_MODE:
				flag = m_GameMode.Expert;
				if (GameState.Instance.ExpertMode || NewGameSubWindow.activeSelf)
				{
					uIOptionsTag2.Enable();
				}
				else
				{
					uIOptionsTag2.Disable();
				}
				break;
			case Option.TRIAL_OF_IRON:
				flag = m_GameMode.TrialOfIron;
				break;
			case Option.AUTOPAUSE:
				flag = ((!uIOptionsTag2.Autoslow) ? m_GameMode.Option.AutoPause.IsEventSet(uIOptionsTag2.AutopauseSuboption) : m_GameMode.Option.AutoPause.IsSlowEventSet(uIOptionsTag2.AutopauseSuboption));
				if (uIOptionsTag2.AutopauseSuboption == AutoPauseOptions.PauseEvent.CombatStart)
				{
					UIOptionsTag uIOptionsTag3 = TagWithOption(Option.AUTOPAUSE_STOP_ON_COMBAT);
					if (m_GameMode.Option.AutoPause.IsEventSet(AutoPauseOptions.PauseEvent.CombatStart))
					{
						uIOptionsTag3.Enable();
					}
					else
					{
						uIOptionsTag3.Disable();
					}
				}
				else if (uIOptionsTag2.AutopauseSuboption == AutoPauseOptions.PauseEvent.EnemySpotted)
				{
					UIOptionsTag uIOptionsTag4 = TagWithOption(Option.AUTOPAUSE_STOP_ON_SIGHTED);
					if (m_GameMode.Option.AutoPause.IsEventSet(AutoPauseOptions.PauseEvent.EnemySpotted))
					{
						uIOptionsTag4.Enable();
					}
					else
					{
						uIOptionsTag4.Disable();
					}
				}
				break;
			case Option.AUTOPAUSE_CENTER:
				flag = m_GameMode.Option.AutoPause.CenterOnCharacter;
				break;
			case Option.AUTOPAUSE_ALL:
				flag = m_GameMode.Option.AutoPause.IsAllSet();
				break;
			case Option.AUTOPAUSE_STOP_ON_COMBAT:
				flag = m_GameMode.Option.AutoPause.EnteringCombatStopsMovement;
				break;
			case Option.AUTOPAUSE_STOP_ON_SIGHTED:
				flag = m_GameMode.Option.AutoPause.EnemySpottedStopMovement;
				break;
			case Option.BOOL_OPTION:
				flag = m_GameMode.Option.GetOption(uIOptionsTag2.BoolSuboption);
				break;
			case Option.DIFFICULTY:
				flag = m_GameMode.Difficulty == uIOptionsTag2.Difficulty;
				if (flag)
				{
					uIOptionsTag = uIOptionsTag2;
				}
				break;
			}
			uIOptionsTag2.Checkbox.SetNoCallback(flag);
		}
		options = m_Options;
		for (int i = 0; i < options.Length; i++)
		{
			options[i].Reload(m_GameMode);
		}
		if (base.AlternateMode && uIOptionsTag != null)
		{
			uIOptionsTag.ForceHover();
		}
	}
}

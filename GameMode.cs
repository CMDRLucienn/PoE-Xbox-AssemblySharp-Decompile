using System;
using UnityEngine;

[Serializable]
public class GameMode
{
	public delegate void FontScaleChangedDelegate(float newscale);

	private bool m_Expert;

	private bool m_TrialOfIron;

	private GameDifficulty m_Difficulty;

	private DifficultyScaling.Scaler m_ActiveScalers;

	private bool[] BoolOptions = new bool[35];

	private const float MASTER_DEFAULT_VOLUME = 1f;

	private const float MUSIC_DEFAULT_VOLUME = 0.3f;

	private const float SOUND_DEFAULT_VOLUME = 1f;

	private const float VOICE_DEFAULT_VOLUME = 1f;

	private float MasterVolume = 1f;

	private float MusicVolume = 0.3f;

	private float SoundVolume = 1f;

	private float VoiceVolume = 1f;

	public float TooltipDelay = 0.5f;

	public string LanguageName = "";

	public UIDropdownResolution.MyResolution Resolution;

	public float ScrollSpeed = 1f;

	public float Gamma = 1f;

	public float AreaLootRange = 4f;

	public float VoiceFrequency = 1f;

	private float m_FontScale = 1f;

	public FontScaleChangedDelegate OnFontScaleChanged;

	public float MinZoom = 0.75f;

	public float MaxZoom = 1.5f;

	public int Quality = 1;

	public int MaxAutosaves = 3;

	public int FrameRateMax = -1;

	private const float DEFAULT_OCCLUSION_OPACITY = 0.3f;

	public float OcclusionOpacity = 0.3f;

	public MainMenuBackgroundType PreferredMainMenuBackground;

	private AutoPauseOptions m_autopause = new AutoPauseOptions();

	public bool Expert
	{
		get
		{
			return m_Expert;
		}
		set
		{
			m_Expert = value;
			if ((bool)AchievementTracker.Instance)
			{
				AchievementTracker.Instance.ForceSetTrackedStat(AchievementTracker.TrackedAchievementStat.ExpertModeOn, value ? 1 : 0);
			}
		}
	}

	public bool TrialOfIron
	{
		get
		{
			return m_TrialOfIron;
		}
		set
		{
			m_TrialOfIron = value;
			if ((bool)AchievementTracker.Instance)
			{
				AchievementTracker.Instance.ForceSetTrackedStat(AchievementTracker.TrackedAchievementStat.TrialOfIronOn, value ? 1 : 0);
			}
		}
	}

	public GameDifficulty Difficulty
	{
		get
		{
			return m_Difficulty;
		}
		set
		{
			m_Difficulty = value;
			if ((bool)AchievementTracker.Instance)
			{
				AchievementTracker.Instance.ForceSetTrackedStat(AchievementTracker.TrackedAchievementStat.PathOfTheDamnedOn, (m_Difficulty == GameDifficulty.PathOfTheDamned) ? 1 : 0);
			}
		}
	}

	public DifficultyScaling.Scaler ActiveScalers
	{
		get
		{
			return m_ActiveScalers;
		}
		set
		{
			m_ActiveScalers = value;
		}
	}

	public GameMode Option => this;

	public bool DeveloperCommentary
	{
		get
		{
			return GetOption(GameOption.BoolOption.DEVELOPER_COMMENTARY);
		}
		set
		{
			SetOption(GameOption.BoolOption.DEVELOPER_COMMENTARY, value);
		}
	}

	public bool DeathIsNotPermanent
	{
		get
		{
			return GetOption(GameOption.BoolOption.DEATH_IS_NOT_PERMANENT);
		}
		set
		{
			SetOption(GameOption.BoolOption.DEATH_IS_NOT_PERMANENT, value);
		}
	}

	public bool AoeHighlighting
	{
		get
		{
			return GetOption(GameOption.BoolOption.AOE_HIGHLIGHTING);
		}
		set
		{
			SetOption(GameOption.BoolOption.AOE_HIGHLIGHTING, value);
		}
	}

	public bool DisplayUnqualifiedInteractions
	{
		get
		{
			return GetOption(GameOption.BoolOption.DISPLAY_UNQUALIFIED_INTERACTIONS);
		}
		set
		{
			SetOption(GameOption.BoolOption.DISPLAY_UNQUALIFIED_INTERACTIONS, value);
		}
	}

	public bool DisplayInteractionQualifier
	{
		get
		{
			return GetOption(GameOption.BoolOption.DISPLAY_INTERACTION_QUALIFIER);
		}
		set
		{
			SetOption(GameOption.BoolOption.DISPLAY_INTERACTION_QUALIFIER, value);
		}
	}

	public bool DisplayPersonalityReputationIndicators
	{
		get
		{
			return GetOption(GameOption.BoolOption.DISPLAY_PERSONALITY_REPUTATION_INDICATORS);
		}
		set
		{
			SetOption(GameOption.BoolOption.DISPLAY_PERSONALITY_REPUTATION_INDICATORS, value);
		}
	}

	public bool DisplayRelativeDefenses
	{
		get
		{
			return GetOption(GameOption.BoolOption.DISPLAY_RELATIVE_DEFENSES);
		}
		set
		{
			SetOption(GameOption.BoolOption.DISPLAY_RELATIVE_DEFENSES, value);
		}
	}

	public bool DisplayQuestObjectiveTitles
	{
		get
		{
			return GetOption(GameOption.BoolOption.DISPLAY_QUEST_OBJECTIVE_TITLES);
		}
		set
		{
			SetOption(GameOption.BoolOption.DISPLAY_QUEST_OBJECTIVE_TITLES, value);
		}
	}

	public float FontScale
	{
		get
		{
			return m_FontScale;
		}
		set
		{
			if (m_FontScale != value)
			{
				m_FontScale = value;
				if (OnFontScaleChanged != null)
				{
					OnFontScaleChanged(value);
				}
			}
		}
	}

	public AutoPauseOptions AutoPause
	{
		get
		{
			return m_autopause;
		}
		set
		{
			m_autopause = value;
		}
	}

	public event Action<GameMode> OptionsReloaded;

	public bool GetOption(GameOption.BoolOption option)
	{
		if (Expert && GameOption.IsBoolOptionExpert(option))
		{
			return false;
		}
		return BoolOptions[(int)option];
	}

	public void SetOption(GameOption.BoolOption option, bool setting)
	{
		if (!Expert || !GameOption.IsBoolOptionExpert(option))
		{
			BoolOptions[(int)option] = setting;
		}
	}

	public float GetVolume(MusicManager.SoundCategory category)
	{
		return category switch
		{
			MusicManager.SoundCategory.MASTER => MasterVolume, 
			MusicManager.SoundCategory.MUSIC => MusicVolume * MasterVolume, 
			MusicManager.SoundCategory.EFFECTS => SoundVolume * MasterVolume, 
			MusicManager.SoundCategory.VOICE => VoiceVolume * MasterVolume, 
			_ => MasterVolume, 
		};
	}

	public float GetRawVolume(MusicManager.SoundCategory category)
	{
		return category switch
		{
			MusicManager.SoundCategory.MASTER => MasterVolume, 
			MusicManager.SoundCategory.MUSIC => MusicVolume, 
			MusicManager.SoundCategory.EFFECTS => SoundVolume, 
			MusicManager.SoundCategory.VOICE => VoiceVolume, 
			_ => MasterVolume, 
		};
	}

	public void SetVolume(MusicManager.SoundCategory category, float setting)
	{
		switch (category)
		{
		case MusicManager.SoundCategory.MASTER:
			MasterVolume = setting;
			break;
		case MusicManager.SoundCategory.MUSIC:
			MusicVolume = setting;
			break;
		case MusicManager.SoundCategory.EFFECTS:
			SoundVolume = setting;
			break;
		case MusicManager.SoundCategory.VOICE:
			VoiceVolume = setting;
			break;
		}
	}

	public void CopyFrom(GameMode other)
	{
		TrialOfIron = other.TrialOfIron;
		Difficulty = other.Difficulty;
		ActiveScalers = other.ActiveScalers;
		Expert = other.Expert;
		if (BoolOptions.Length < other.BoolOptions.Length)
		{
			bool[] array = new bool[other.BoolOptions.Length];
			BoolOptions.CopyTo(array, 0);
		}
		for (int i = 0; i < other.BoolOptions.Length; i++)
		{
			if (!Expert || !GameOption.IsBoolOptionExpert((GameOption.BoolOption)i))
			{
				BoolOptions[i] = other.BoolOptions[i];
			}
		}
		MasterVolume = other.MasterVolume;
		MusicVolume = other.MusicVolume;
		SoundVolume = other.SoundVolume;
		VoiceVolume = other.VoiceVolume;
		TooltipDelay = other.TooltipDelay;
		FontScale = other.FontScale;
		ScrollSpeed = other.ScrollSpeed;
		Gamma = other.Gamma;
		AreaLootRange = other.AreaLootRange;
		VoiceFrequency = other.VoiceFrequency;
		LanguageName = other.LanguageName;
		MinZoom = other.MinZoom;
		MaxZoom = other.MaxZoom;
		Quality = other.Quality;
		MaxAutosaves = other.MaxAutosaves;
		FrameRateMax = other.FrameRateMax;
		OcclusionOpacity = other.OcclusionOpacity;
		Resolution = new UIDropdownResolution.MyResolution(other.Resolution);
		PreferredMainMenuBackground = other.PreferredMainMenuBackground;
		AutoPause.CopyFrom(other.AutoPause);
	}

	public bool Matches(GameMode other)
	{
		if (BoolOptions.Length != other.BoolOptions.Length)
		{
			return false;
		}
		for (int i = 0; i < BoolOptions.Length; i++)
		{
			if ((!Expert || !GameOption.IsBoolOptionExpert((GameOption.BoolOption)i)) && BoolOptions[i] != other.BoolOptions[i])
			{
				return false;
			}
		}
		if (TrialOfIron == other.TrialOfIron && Difficulty == other.Difficulty && ActiveScalers == other.ActiveScalers && Expert == other.Expert && other.MasterVolume == MasterVolume && other.MusicVolume == MusicVolume && other.SoundVolume == SoundVolume && other.VoiceVolume == VoiceVolume && other.TooltipDelay == TooltipDelay && other.FontScale == FontScale && other.ScrollSpeed == ScrollSpeed && other.Gamma == Gamma && other.VoiceFrequency == VoiceFrequency && other.AreaLootRange == AreaLootRange && other.LanguageName.Equals(LanguageName) && other.MinZoom == MinZoom && other.MaxZoom == MaxZoom && other.Quality == Quality && other.MaxAutosaves == MaxAutosaves && other.Resolution.Equals(Resolution) && AutoPause.Matches(other.AutoPause) && other.FrameRateMax.Equals(FrameRateMax) && other.PreferredMainMenuBackground == PreferredMainMenuBackground)
		{
			return other.OcclusionOpacity.Equals(OcclusionOpacity);
		}
		return false;
	}

	public void ApplyResolution()
	{
		Resolution desiredResolution = default(Resolution);
		desiredResolution.width = Resolution.width;
		desiredResolution.height = Resolution.height;
		desiredResolution.refreshRate = Resolution.refreshRate;
		ResolutionController.DisplayModes desiredDisplayMode = (GetOption(GameOption.BoolOption.FULLSCREEN) ? ResolutionController.DisplayModes.Fullscreen : ResolutionController.DisplayModes.Windowed);
		if ((bool)ResolutionController.Instance)
		{
			ResolutionController.Instance.TryChangeResolutionAndDisplayMode(desiredResolution, desiredDisplayMode);
		}
	}

	public void LoadFromPrefs()
	{
		for (int i = 0; i < 35; i++)
		{
			bool flag = GameOption.BoolOptionDefault((GameOption.BoolOption)i);
			GameOption.BoolOption boolOption = (GameOption.BoolOption)i;
			bool flag2 = PlayerPrefs.GetInt(boolOption.ToString(), flag ? 1 : 0) > 0;
			BoolOptions[i] = flag2;
		}
		MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
		MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
		SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
		VoiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
		TooltipDelay = PlayerPrefs.GetFloat("TooltipDelay", 0.5f);
		ScrollSpeed = PlayerPrefs.GetFloat("ScrollSpeed", 1f);
		Gamma = PlayerPrefs.GetFloat("Gamma", 1f);
		AreaLootRange = PlayerPrefs.GetFloat("AreaLootRange", 4f);
		VoiceFrequency = PlayerPrefs.GetFloat("VoiceFrequency", 1f);
		FontScale = PlayerPrefs.GetFloat("FontScale", 1f);
		MinZoom = PlayerPrefs.GetFloat("NewMinZoom", 0.75f);
		MaxZoom = PlayerPrefs.GetFloat("NewMaxZoom", 1.5f);
		Quality = PlayerPrefs.GetInt("Quality", 1);
		MaxAutosaves = PlayerPrefs.GetInt("MaxAutosaves", 3);
		FrameRateMax = PlayerPrefs.GetInt("MaxFrameRate", -1);
		OcclusionOpacity = PlayerPrefs.GetFloat("OcclusionOpacity", 0.3f);
		LanguageName = PlayerPrefs.GetString("LanguageName", StringTableManager.CurrentLanguage.Name);
		PreferredMainMenuBackground = (MainMenuBackgroundType)PlayerPrefs.GetInt("PreferredMainMenuBackground", 0);
		AutoPause.Initialize();
		BigHeads.Enabled = GameState.Option.GetOption(GameOption.BoolOption.BIG_HEADS);
		Resolution = new UIDropdownResolution.MyResolution(PlayerPrefs.GetString("Resolution", new UIDropdownResolution.MyResolution(Screen.width, Screen.height, Screen.currentResolution.refreshRate).SerialString()));
		ApplyResolution();
		QualitySettings.antiAliasing = 0;
		PE_GameRender.SetAntiAliasing(Quality > 0);
		Application.targetFrameRate = FrameRateMax;
		if (StringTableManager.CurrentLanguage.Name != GameState.Option.LanguageName)
		{
			StringTableManager.SetCurrentLanguageByName(GameState.Option.LanguageName);
		}
		if (GameState.OnDifficultyChanged != null)
		{
			GameState.OnDifficultyChanged(GameState.Mode.Difficulty);
		}
		QualitySettings.vSyncCount = (GetOption(GameOption.BoolOption.VSYNC) ? 1 : 0);
		UICamera.tooltipDelay = TooltipDelay;
		WinCursor.Clip(state: true);
		Shader.SetGlobalFloat("_OcclusionOpacityStrength", GameState.Option.OcclusionOpacity);
		if (this.OptionsReloaded != null)
		{
			this.OptionsReloaded(this);
		}
	}

	public void SaveToPrefs()
	{
		for (int i = 0; i < 35; i++)
		{
			GameOption.BoolOption boolOption = (GameOption.BoolOption)i;
			PlayerPrefs.SetInt(boolOption.ToString(), BoolOptions[i] ? 1 : 0);
		}
		PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
		PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
		PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
		PlayerPrefs.SetFloat("VoiceVolume", VoiceVolume);
		PlayerPrefs.SetFloat("TooltipDelay", TooltipDelay);
		PlayerPrefs.SetFloat("FontScale", FontScale);
		PlayerPrefs.SetFloat("NewMinZoom", MinZoom);
		PlayerPrefs.SetFloat("NewMaxZoom", MaxZoom);
		PlayerPrefs.SetInt("Quality", Quality);
		PlayerPrefs.SetInt("MaxFrameRate", FrameRateMax);
		PlayerPrefs.SetFloat("OcclusionOpacity", OcclusionOpacity);
		PlayerPrefs.SetFloat("ScrollSpeed", ScrollSpeed);
		PlayerPrefs.SetFloat("Gamma", Gamma);
		PlayerPrefs.SetFloat("AreaLootRange", AreaLootRange);
		PlayerPrefs.SetFloat("VoiceFrequency", VoiceFrequency);
		PlayerPrefs.SetString("LanguageName", LanguageName);
		PlayerPrefs.SetString("Resolution", Resolution.SerialString());
		PlayerPrefs.SetInt("PreferredMainMenuBackground", (int)PreferredMainMenuBackground);
		AutoPause.SaveOptions();
		PlayerPrefs.Save();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AI.Player;
using OEICommon;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
	public delegate void DifficultyDelegate(GameDifficulty difficulty);

	public enum ResetStyle
	{
		NewGame,
		LoadedGame
	}

	public static DifficultyDelegate OnDifficultyChanged;

	public static Player s_playerCharacter = null;

	private static Stronghold m_stronghold;

	public static GameMode Mode = new GameMode();

	public static ControlMapping Controls = MappedInput.DefaultMapping.Copy();

	[Persistent]
	public bool RetroactiveSpellMasteryChecked;

	private int m_AutosaveCycleNumber;

	[HideInInspector]
	public static bool ApplicationIsFocused = true;

	[Persistent]
	private static bool s_isInCombat = false;

	private static int s_noSaveInCombatCountdown = 0;

	private static bool s_isInTrapTriggeredCombat = false;

	private static float s_outOfCombatTimer = 0f;

	private static float s_inCombatTimer = 0f;

	private static float s_combatPauseTimer = 0f;

	private static float s_deadTimer = 0f;

	private static bool s_gameOver = false;

	private static bool s_firstLoad = true;

	private static bool s_inRestMode = false;

	public static bool ShowDebug = false;

	public TextAsset TrialOfIronReadme;

	private static List<UnityEngine.Object> m_persistAcrossSceneLoadObjects = new List<UnityEngine.Object>();

	private bool m_FirstUpdate = true;

	public static Stronghold Stronghold
	{
		get
		{
			if (!m_stronghold)
			{
				m_stronghold = UnityEngine.Object.FindObjectOfType<Stronghold>();
			}
			return m_stronghold;
		}
	}

	public static GameObject LastPersonToUseScriptedInteraction { get; set; }

	public static GameMode Option => Mode;

	public bool IsDifficultyPotd => Difficulty == GameDifficulty.PathOfTheDamned;

	public bool IsDifficultyStoryTime => Difficulty == GameDifficulty.StoryTime;

	[Persistent]
	public GameDifficulty Difficulty
	{
		get
		{
			return Mode.Difficulty;
		}
		set
		{
			Mode.Difficulty = value;
		}
	}

	[Persistent]
	public DifficultyScaling.Scaler ActiveScalers
	{
		get
		{
			return Mode.ActiveScalers;
		}
		set
		{
			Mode.ActiveScalers = value;
		}
	}

	[Persistent]
	public bool TrialOfIron
	{
		get
		{
			return Mode.TrialOfIron;
		}
		set
		{
			Mode.TrialOfIron = value;
		}
	}

	[Persistent]
	public bool ExpertMode
	{
		get
		{
			return Mode.Expert;
		}
		set
		{
			Mode.Expert = value;
		}
	}

	[Persistent]
	public bool UiWorldMapTutorialFinished { get; set; }

	[Persistent]
	public bool HasNotifiedPX2Installation { get; set; }

	public static bool LoadedGame { get; set; }

	public static bool NewGame { get; set; }

	public static bool GameComplete { get; set; }

	[Persistent]
	public bool CheatsEnabled { get; set; }

	[Persistent]
	public bool IgnoreSpellLimits { get; set; }

	[Persistent]
	public bool IgnoreInGrimoire { get; set; }

	[Persistent]
	public bool HasEnteredPX1 { get; set; }

	[Persistent]
	public bool HasEnteredPX2 { get; set; }

	public static bool IsRestoredLevel { get; set; }

	public static string LoadedFileName { get; set; }

	public static bool IsLoading { get; set; }

	public static bool IsInGameSession => s_playerCharacter != null;

	public static bool ForceCombatMode { get; set; }

	public static bool PlayerSafeMode { get; set; }

	public static bool PartyDead { get; set; }

	public static int NumSceneLoads { get; set; }

	public static bool IsWindows32Bit { get; private set; }

	[Persistent]
	public int AutosaveCycleNumber
	{
		get
		{
			m_AutosaveCycleNumber %= Option.MaxAutosaves;
			return m_AutosaveCycleNumber;
		}
		set
		{
			m_AutosaveCycleNumber = value;
		}
	}

	public static string ApplicationLoadedLevelName { get; set; }

	public MapData CurrentNextMap { get; set; }

	public MapData CurrentMap { get; set; }

	public bool CouldAccessStashOnLastMap { get; private set; }

	public bool CurrentMapIsStronghold
	{
		get
		{
			if (CurrentMap != null)
			{
				return CurrentMap.IsStronghold;
			}
			return false;
		}
	}

	public static GameState Instance { get; private set; }

	public static bool InCombat => s_isInCombat;

	public static bool IsCombatWaitingToEnd => s_outOfCombatTimer < CharacterStats.StaminaRechargeDelay;

	public static bool CannotSaveBecauseInCombat
	{
		get
		{
			if (!s_isInCombat)
			{
				return s_noSaveInCombatCountdown > 0;
			}
			return true;
		}
	}

	public static bool IsInTrapTriggeredCombat => s_isInTrapTriggeredCombat;

	public static float InCombatDuration => s_inCombatTimer;

	public static bool GameOver
	{
		get
		{
			return s_gameOver;
		}
		set
		{
			s_gameOver = value;
		}
	}

	public static bool CutsceneAllowed
	{
		get
		{
			if (!PartyDead)
			{
				return !GameOver;
			}
			return false;
		}
	}

	public static bool Paused
	{
		get
		{
			if ((bool)TimeController.Instance)
			{
				return TimeController.Instance.Paused;
			}
			return false;
		}
	}

	public static event EventHandler OnCombatStart;

	public static event EventHandler OnCombatEnd;

	public static event EventHandler OnLevelUnload;

	public static event EventHandler OnLevelLoaded;

	public static event EventHandler OnResting;

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
		if (!SceneIsTransitionScene(scene))
		{
			ApplicationLoadedLevelName = SceneManager.GetActiveScene().name;
			if ((bool)TimeController.Instance)
			{
				TimeController.Instance.Paused = false;
			}
		}
	}

	public static bool CurrentSceneIsTransitionScene()
	{
		return SceneIsTransitionScene(SceneManager.GetActiveScene());
	}

	public static bool SceneIsTransitionScene(Scene scene)
	{
		return scene.name.ToLower() == "oei_scene_transition";
	}

	public static bool CurrentSceneIsStartingScene()
	{
		return SceneIsStartingScene(SceneManager.GetActiveScene());
	}

	public static bool SceneIsStartingScene(Scene scene)
	{
		return scene.name.ToLower() == "companyintro";
	}

	public static string GetDebugOutput()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("-- Combat State Debug --");
		stringBuilder.AppendLine("In Combat: " + InCombat);
		stringBuilder.AppendLine("Forced: " + ForceCombatMode);
		stringBuilder.AppendLine("Active Combatant: " + GetActiveCombatant(enemyOnly: false));
		stringBuilder.AppendLine("Timers - In:" + s_inCombatTimer.ToString("#0.0") + " / Pause:" + s_combatPauseTimer.ToString("#0.0") + " / Out:" + s_outOfCombatTimer.ToString("#0.0"));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("-- Game State Debug --");
		stringBuilder.AppendLine("Party Dead: " + PartyDead);
		stringBuilder.AppendLine("Force Combat Mode: " + ForceCombatMode);
		stringBuilder.AppendLine("Is Restored Level: " + IsRestoredLevel);
		stringBuilder.AppendLine("Game Over: " + GameOver);
		stringBuilder.AppendLine("Loaded Game: " + LoadedGame);
		stringBuilder.AppendLine("PartyMemberAI.SafeEnableDisable: " + PartyMemberAI.SafeEnableDisable);
		stringBuilder.AppendLine("Player Safe Mode: " + PlayerSafeMode);
		stringBuilder.AppendLine("GameInput.DisableInput: " + GameInput.DisableInput);
		stringBuilder.AppendLine("UICamera.DisableSelectionInput: " + UICamera.DisableSelectionInput);
		return stringBuilder.ToString();
	}

	private static void CleanupNullsInPersistAcrossSceneLoadList()
	{
		m_persistAcrossSceneLoadObjects.RemoveAll((UnityEngine.Object obj) => obj == null);
	}

	public static void DestroyTrackedObject(UnityEngine.Object obj)
	{
		if (m_persistAcrossSceneLoadObjects.Contains(obj))
		{
			m_persistAcrossSceneLoadObjects.Remove(obj);
		}
		GameUtilities.Destroy(obj);
	}

	public static void DestroyTrackedObjectImmediate(UnityEngine.Object obj)
	{
		if (m_persistAcrossSceneLoadObjects.Contains(obj))
		{
			m_persistAcrossSceneLoadObjects.Remove(obj);
		}
		GameUtilities.DestroyImmediate(obj);
	}

	public static void MarkTrackedObjectForDelayedDestroy(UnityEngine.Object obj)
	{
		if (m_persistAcrossSceneLoadObjects.Contains(obj))
		{
			Persistence persistence = null;
			if (obj is GameObject)
			{
				persistence = (obj as GameObject).GetComponent<Persistence>();
			}
			else if (obj is MonoBehaviour)
			{
				persistence = (obj as MonoBehaviour).GetComponent<Persistence>();
			}
			if ((bool)persistence)
			{
				persistence.UnloadsBetweenLevels = true;
			}
		}
	}

	public static void PerformTrackedObjectDelayedDestroy()
	{
		for (int num = m_persistAcrossSceneLoadObjects.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object @object = m_persistAcrossSceneLoadObjects[num];
			if (@object != null)
			{
				Persistence persistence = null;
				if (@object is GameObject)
				{
					persistence = (@object as GameObject).GetComponent<Persistence>();
				}
				else if (@object is MonoBehaviour)
				{
					persistence = (@object as MonoBehaviour).GetComponent<Persistence>();
				}
				if ((bool)persistence && persistence.UnloadsBetweenLevels)
				{
					m_persistAcrossSceneLoadObjects.RemoveAt(num);
					GameUtilities.Destroy(@object);
				}
			}
		}
	}

	public static void PersistAcrossSceneLoadsTracked(UnityEngine.Object obj)
	{
		CleanupNullsInPersistAcrossSceneLoadList();
		if (m_persistAcrossSceneLoadObjects.Contains(obj))
		{
			return;
		}
		m_persistAcrossSceneLoadObjects.Add(obj);
		Persistence persistence = null;
		if (obj is GameObject)
		{
			if (!(obj as GameObject).transform.parent)
			{
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			persistence = (obj as GameObject).GetComponent<Persistence>();
		}
		else if (obj is MonoBehaviour)
		{
			if (!(obj as MonoBehaviour).transform.parent)
			{
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			persistence = (obj as MonoBehaviour).GetComponent<Persistence>();
		}
		if ((bool)persistence)
		{
			persistence.UnloadsBetweenLevels = false;
		}
	}

	public static void PersistAcrossSceneLoadsUntracked(UnityEngine.Object obj)
	{
		Persistence persistence = null;
		if (obj is GameObject)
		{
			if (!(obj as GameObject).transform.parent)
			{
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			persistence = (obj as GameObject).GetComponent<Persistence>();
		}
		else if (obj is MonoBehaviour)
		{
			if (!(obj as MonoBehaviour).transform.parent)
			{
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			persistence = (obj as MonoBehaviour).GetComponent<Persistence>();
		}
		if ((bool)persistence)
		{
			persistence.UnloadsBetweenLevels = false;
		}
	}

	public static void CleanupPersistAcrossSceneLoadObjects()
	{
		InstanceID.ResetActiveList();
		CleanupNullsInPersistAcrossSceneLoadList();
		foreach (UnityEngine.Object persistAcrossSceneLoadObject in m_persistAcrossSceneLoadObjects)
		{
			if ((bool)persistAcrossSceneLoadObject)
			{
				GameObject gameObject = persistAcrossSceneLoadObject as GameObject;
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: false);
				}
				GameUtilities.Destroy(persistAcrossSceneLoadObject);
			}
		}
		m_persistAcrossSceneLoadObjects.Clear();
	}

	public static void CleanupPersistAcrossSceneLoadObjectsOfType(Type destroyType)
	{
		CleanupNullsInPersistAcrossSceneLoadList();
		foreach (UnityEngine.Object persistAcrossSceneLoadObject in m_persistAcrossSceneLoadObjects)
		{
			if ((bool)persistAcrossSceneLoadObject)
			{
				GameObject gameObject = persistAcrossSceneLoadObject as GameObject;
				if ((bool)gameObject && gameObject.GetComponent(destroyType) != null)
				{
					gameObject.SetActive(value: false);
					GameUtilities.Destroy(persistAcrossSceneLoadObject);
				}
			}
		}
		CleanupNullsInPersistAcrossSceneLoadList();
	}

	private void OnApplicationQuit()
	{
		if (CanTrialOfIronQuitSave())
		{
			TrialOfIronSave();
		}
		SaveGameInfo.CancelRunningThreads();
	}

	private void OnApplicationFocus(bool status)
	{
		bool applicationIsFocused = ApplicationIsFocused;
		WinCursor.Clip(status);
		ApplicationIsFocused = status;
		if (!applicationIsFocused && status && Paused && ((ConversationManager.Instance != null && ConversationManager.Instance.IsConversationOrSIRunning()) || Cutscene.CutsceneActive))
		{
			TimeController.Instance.Paused = false;
		}
	}

	private static void LoadToMainMenuOnFadeEnd()
	{
		GameInput.DisableInput = false;
		UICamera.DisableSelectionInput = false;
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadToMainMenuOnFadeEnd));
		Debug.Log("\n");
		Debug.Log("------- LOAD TO MAIN MENU --------\n\n");
		SceneManager.LoadScene("MainMenu");
	}

	public static void LoadMainMenu(bool fadeOut)
	{
		if (CanTrialOfIronQuitSave())
		{
			TrialOfIronSave();
		}
		if (fadeOut)
		{
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 0.35f, AudioFadeMode.MusicAndFx);
			FadeManager instance = FadeManager.Instance;
			instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Combine(instance.OnFadeEnded, new FadeManager.OnFadeEnd(LoadToMainMenuOnFadeEnd));
			GameInput.DisableInput = true;
			UICamera.DisableSelectionInput = true;
		}
		else
		{
			Debug.Log("\n");
			Debug.Log("------- LOAD TO MAIN MENU --------\n\n");
			SceneManager.LoadScene("MainMenu");
		}
	}

	public static void Autosave()
	{
		if (Mode.TrialOfIron)
		{
			TrialOfIronSave();
		}
		else if (GameResources.SaveGame(SaveGameInfo.GetAutosaveFileName()))
		{
			GameResources.DeleteSavedGame(SaveGameInfo.GetOldAutosaveFileName());
		}
		Instance.AutosaveCycleNumber++;
	}

	public static bool CanTrialOfIronQuitSave()
	{
		if ((bool)InGameHUD.Instance && InGameHUD.Instance.QuicksaveAllowed && Mode != null && Mode.TrialOfIron)
		{
			return !s_gameOver;
		}
		return false;
	}

	public static void TrialOfIronSave()
	{
		string loadedFileName = LoadedFileName;
		string saveFileName = SaveGameInfo.GetSaveFileName();
		if (GameResources.SaveGame(saveFileName))
		{
			GameResources.WriteTrialOfIronReadme();
			if (!string.IsNullOrEmpty(loadedFileName) && !saveFileName.Equals(loadedFileName))
			{
				GameResources.DeleteSavedGame(loadedFileName);
			}
		}
	}

	public static void TrialOfIronDelete()
	{
		if (!string.IsNullOrEmpty(LoadedFileName) && Mode.TrialOfIron)
		{
			GameResources.DeleteSavedGame(LoadedFileName);
			LoadedFileName = "";
		}
	}

	public static void TriggerRestMode()
	{
		s_inRestMode = true;
	}

	public void Reset(ResetStyle style)
	{
		if ((bool)ConversationManager.Instance)
		{
			ConversationManager.Instance.KillAll();
		}
		PartyDead = false;
		ForceCombatMode = false;
		PlayerSafeMode = false;
		CheatsEnabled = false;
		s_isInCombat = false;
		s_noSaveInCombatCountdown = 0;
		s_isInTrapTriggeredCombat = false;
		s_outOfCombatTimer = 0f;
		s_inCombatTimer = 0f;
		s_combatPauseTimer = 0f;
		s_deadTimer = 0f;
		s_gameOver = false;
		s_playerCharacter = null;
		m_stronghold = null;
		LoadedGame = false;
		NewGame = false;
		GameComplete = false;
		IsRestoredLevel = false;
		LoadedFileName = "";
		UiWorldMapTutorialFinished = false;
		NumSceneLoads = 0;
		IgnoreSpellLimits = false;
		IgnoreInGrimoire = false;
		HasEnteredPX1 = false;
		HasEnteredPX2 = false;
		RetroactiveSpellMasteryChecked = false;
		ActiveScalers = (DifficultyScaling.Scaler)0;
		if (style == ResetStyle.NewGame)
		{
			s_firstLoad = true;
			PersistenceManager.ClearTempData();
		}
		else
		{
			s_firstLoad = false;
		}
		GameState.OnResting = null;
		OnDifficultyChanged = null;
		GameInput.DisableInput = false;
		GameInput.EndBlockAllKeys();
		UICamera.DisableSelectionInput = false;
		UIScreenEdgeBlocker.Reset();
		RestZone.Reset();
		BigHeads.Reset();
		Team.RemoveAllTeams();
		Faction.ClearPlayerTeam();
		PartyMemberAI.Reset();
		if ((bool)WorldTime.Instance)
		{
			WorldTime.Instance.Reset();
		}
		CharacterStats.ResetStaticData();
		Cutscene.ForceEndAllCutscenes(callEndScripts: false);
		if ((bool)UIWindowManager.Instance)
		{
			UIWindowManager.Instance.CloseAllWindows();
		}
		if ((bool)InGameHUD.Instance)
		{
			InGameHUD.Instance.ForceShowHUD();
		}
		if ((bool)UIDisembodiedBark.Instance)
		{
			UIDisembodiedBark.Instance.Reset();
		}
	}

	private void DrawDebugInfo()
	{
		UIDebug.Instance.SetText("GameState Debug", GetDebugOutput(), Color.cyan);
		UIDebug.Instance.SetTextPosition("GameState Debug", 0.95f, 0.95f, UIWidget.Pivot.TopRight);
	}

	private void Awake()
	{
		IsWindows32Bit = !Utils.IsCurrentProcess64Bit();
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GameState' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		StringTableManager.Init();
		s_gameOver = false;
		PartyDead = false;
		IsLoading = false;
		Controls.LoadFromPrefs();
		Controls.Restored();
		if (Application.isPlaying)
		{
			SaveGameInfo.CacheSaveGameInfo();
		}
		ApplicationLoadedLevelName = SceneManager.GetActiveScene().name;
		Debug.Log("\n");
		Debug.Log("---- GAME BUILD VERSION: " + buildnum.BUILD_NUMBER + " ----\n");
		if (IsWindows32Bit)
		{
			Debug.Log("Game is running Windows 32 Bit\n");
		}
		else
		{
			Debug.Log("Game is not running Windows 32 Bit\n");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
		GameResources.ClearPrefabReferences();
		QuestManager.UnloadPreloadedQuestData();
		UIDynamicFontSize.Cleanup();
		StringTableManager.Cleanup();
		SaveGameInfo.CleanCache();
		ComponentUtils.Cleanup();
	}

	private void Update()
	{
		if (m_FirstUpdate)
		{
			m_FirstUpdate = false;
			Mode.LoadFromPrefs();
		}
		if (ShowDebug)
		{
			DrawDebugInfo();
		}
		if (ScriptEvent.DisplayRecentScripts)
		{
			ScriptEvent.DrawScriptHistory();
		}
		if (!ApplicationIsFocused && Screen.fullScreen && TimeController.Instance != null && (!UICharacterCreationManager.Instance || !UICharacterCreationManager.Instance.WindowActive()))
		{
			TimeController.Instance.SafePaused = true;
		}
		UpdateIsInCombatAndGameOver();
		PartyMemberAI.UpdateEnemySpottedTimer();
		GameUtilities.UpdateFadingEffects();
		if (Screen.fullScreen != Option.GetOption(GameOption.BoolOption.FULLSCREEN))
		{
			Option.SetOption(GameOption.BoolOption.FULLSCREEN, Screen.fullScreen);
			if ((bool)UIOptionsManager.Instance)
			{
				UIOptionsManager.Instance.ForceUpdateFullscreen();
			}
		}
		if (!Conditionals.CommandLineArg("e3"))
		{
			return;
		}
		if (GameInput.GetKeyDown(KeyCode.F9))
		{
			InGameHUD.Instance.ShowHUD = false;
			if (FogOfWar.Instance != null)
			{
				FogOfWar.Instance.QueueDisable();
			}
		}
		if (GameInput.GetKeyDown(KeyCode.F10) && FogOfWar.Instance != null)
		{
			Scripts.ReturnToMainMenu();
		}
	}

	public void LateUpdate()
	{
		Stealth.UpdateStaticLogic();
	}

	public void ForceIsInTrapTriggeredCombat()
	{
		if (!s_isInCombat)
		{
			s_outOfCombatTimer = CharacterStats.StaminaRechargeDelay;
			s_isInCombat = true;
			s_isInTrapTriggeredCombat = true;
		}
	}

	public static AIController GetActiveCombatant(bool enemyOnly)
	{
		for (int i = 0; i < Faction.ActiveFactionComponents.Count; i++)
		{
			Faction faction = Faction.ActiveFactionComponents[i];
			if (faction == null || !faction.gameObject.activeInHierarchy)
			{
				continue;
			}
			AIController aIController = GameUtilities.FindActiveAIController(faction.gameObject);
			CharacterStats component = faction.GetComponent<CharacterStats>();
			if ((bool)component && component.HasFactionSwapEffect() && !PartyHelper.IsPartyMember(faction.gameObject))
			{
				return aIController;
			}
			if ((bool)aIController && aIController.InCombat)
			{
				if (!enemyOnly && aIController is PartyMemberAI)
				{
					return aIController;
				}
				if (aIController.MustDieForCombatToEnd && faction.RelationshipToPlayer == Faction.Relationship.Hostile)
				{
					return aIController;
				}
				if (aIController.IsConfused)
				{
					return aIController;
				}
				PartyMemberAI component2 = ComponentUtils.GetComponent<PartyMemberAI>(aIController.StateManager.CurrentTarget);
				if ((bool)component2 && component2.enabled)
				{
					return aIController;
				}
			}
		}
		return null;
	}

	private void UpdateIsInCombatAndGameOver()
	{
		if (ForceCombatMode)
		{
			s_isInCombat = true;
			Stealth.GlobalSetInStealthMode(inStealth: false);
		}
		else
		{
			if (s_firstLoad)
			{
				return;
			}
			if (s_inRestMode)
			{
				if (GameState.OnResting != null)
				{
					GameState.OnResting(null, EventArgs.Empty);
				}
				s_inRestMode = false;
			}
			else
			{
				if (Paused || s_playerCharacter == null)
				{
					return;
				}
				if (s_outOfCombatTimer > 0f)
				{
					s_outOfCombatTimer -= Time.deltaTime;
				}
				if (s_isInCombat && s_combatPauseTimer > 0f)
				{
					s_combatPauseTimer -= Time.deltaTime;
				}
				if (s_isInCombat)
				{
					s_inCombatTimer += Time.deltaTime;
				}
				if (s_noSaveInCombatCountdown > 0)
				{
					s_noSaveInCombatCountdown--;
				}
				GameObject target = null;
				bool flag = s_isInCombat;
				bool flag2 = false;
				if (Faction.ActiveFactionComponents.Count == 0)
				{
					return;
				}
				AIController activeCombatant = GetActiveCombatant(enemyOnly: false);
				if ((bool)activeCombatant)
				{
					target = activeCombatant.CurrentTarget;
					s_isInCombat = true;
					flag2 = true;
					s_outOfCombatTimer = CharacterStats.StaminaRechargeDelay;
				}
				s_isInCombat = s_outOfCombatTimer > 0f;
				if (s_isInCombat && s_combatPauseTimer <= 0f)
				{
					AutoPause(AutoPauseOptions.PauseEvent.CombatTimer, null, null);
					s_combatPauseTimer = Option.AutoPause.CombatRoundTime;
				}
				if (s_isInCombat != flag || (flag2 && s_isInTrapTriggeredCombat))
				{
					s_inCombatTimer = 0f;
					if (s_isInCombat)
					{
						s_isInTrapTriggeredCombat = false;
						if (!s_isInTrapTriggeredCombat)
						{
							TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.COMBAT_START);
						}
						if (GameState.OnCombatStart != null)
						{
							GameState.OnCombatStart(null, EventArgs.Empty);
						}
						if (!s_isInTrapTriggeredCombat)
						{
							AutoPause(AutoPauseOptions.PauseEvent.CombatStart, target, null);
						}
					}
					else
					{
						s_combatPauseTimer = 0f;
						s_noSaveInCombatCountdown = 10;
						if (GameState.OnCombatEnd != null)
						{
							GameState.OnCombatEnd(null, EventArgs.Empty);
						}
						s_isInTrapTriggeredCombat = false;
					}
				}
				if (!s_gameOver && PartyDead)
				{
					s_gameOver = true;
					s_deadTimer = 2f;
				}
				if (s_gameOver && s_deadTimer > 0f)
				{
					s_deadTimer -= Time.deltaTime;
					if (s_deadTimer <= 0f)
					{
						s_deadTimer = 0f;
						UIDeathManager.Instance.ShowWindow();
					}
				}
			}
		}
	}

	public static void AutoPause(AutoPauseOptions.PauseEvent evt, GameObject target, GameObject triggerer, GenericAbility ability = null)
	{
		if (PlayerSafeMode || GameInput.DisableInput || (evt != AutoPauseOptions.PauseEvent.CombatTimer && evt != AutoPauseOptions.PauseEvent.CombatStart && (target == null || (evt != AutoPauseOptions.PauseEvent.EnemySpotted && evt != AutoPauseOptions.PauseEvent.SpellCast && target.GetComponent<PartyMemberAI>() == null))))
		{
			return;
		}
		if (Option.AutoPause.IsSlowEventSet(evt))
		{
			TimeController.Instance.Slow = true;
		}
		if (!Option.AutoPause.IsEventSet(evt))
		{
			return;
		}
		if (!Paused && Option.AutoPause.CenterOnCharacter)
		{
			CameraControl component = Camera.main.GetComponent<CameraControl>();
			if (component != null && target != null)
			{
				Vector3 vector = target.transform.position;
				if ((bool)triggerer)
				{
					Vector3 vector2 = triggerer.transform.position - vector;
					float num = vector2.magnitude * 0.85f;
					vector2.Normalize();
					vector = vector2 * num + target.transform.position;
				}
				component.FocusOnPoint(vector, 0.25f);
			}
		}
		if ((evt == AutoPauseOptions.PauseEvent.CombatStart && Option.AutoPause.EnteringCombatStopsMovement) || (evt == AutoPauseOptions.PauseEvent.EnemySpotted && Option.AutoPause.EnemySpottedStopMovement))
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if ((bool)partyMemberAI)
				{
					partyMemberAI.StateManager.PopStates(typeof(Move));
				}
			}
		}
		TimeController.Instance.SafePaused = true;
		Console.AddMessage(AutoPauseOptions.GetResponseString(evt, target, ability), Color.red);
	}

	public static float PartyAverageStealthValue()
	{
		float num = 0f;
		int num2 = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if ((bool)component)
				{
					num += component.StealthValue;
					num2++;
				}
			}
		}
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		return num;
	}

	public static void BeginLevelUnload(string nextLevel)
	{
		IsLoading = true;
		PerformTrackedObjectDelayedDestroy();
		Debug.Log("\n");
		Debug.Log("------- BEGIN LEVEL LOAD INITIATED --------        Pending level = " + nextLevel + ".");
		if (GameState.OnLevelUnload != null)
		{
			GameState.OnLevelUnload(nextLevel, EventArgs.Empty);
		}
	}

	public static void ChangeLevel(string level)
	{
		List<string> list = new List<string>();
		list.AddRange(Enum.GetNames(typeof(MapType)));
		int num = list.IndexOf(level);
		if (num >= 0)
		{
			ChangeLevel((MapType)Enum.GetValues(typeof(MapType)).GetValue(num));
		}
	}

	public static void ChangeLevel(MapType level)
	{
		if (level == MapType.Map)
		{
			Debug.LogError("Tried to transition to bad map. Did you forget to set a map in a dropdown?");
		}
		else
		{
			ChangeLevel(WorldMap.Instance.LoadedMaps[(int)level]);
		}
	}

	public static void ChangeLevel(MapData map)
	{
		try
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			foreach (PartyMemberAI partyMemberAI in partyMembers)
			{
				if (!(partyMemberAI == null))
				{
					if (partyMemberAI.StateManager != null)
					{
						partyMemberAI.StateManager.CurrentState?.StopMover();
						partyMemberAI.StateManager.AbortStateStack();
					}
					Stealth component = partyMemberAI.GetComponent<Stealth>();
					if ((bool)component)
					{
						component.ClearAllSuspicion();
					}
				}
			}
			StartPoint.s_ChosenStartPoint = null;
			BeginLevelUnload(map.SceneName);
			ConditionalToggleManager.Instance.ResetBetweenSceneLoads();
			PersistenceManager.SaveGame();
			FogOfWar.Save();
			IsRestoredLevel = File.Exists(PersistenceManager.GetLevelFilePath(map.SceneName));
			if ((bool)GameUtilities.Instance)
			{
				bool loadFromSaveFile = false;
				GameUtilities.Instance.StartCoroutine(GameResources.LoadScene(map.SceneName, loadFromSaveFile));
			}
			Instance.CurrentNextMap = map;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void ReturnToMainMenuFromError()
	{
		DestroyOnLevelLoad[] array = UnityEngine.Object.FindObjectsOfType<DestroyOnLevelLoad>();
		if (array != null)
		{
			DestroyOnLevelLoad[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].InGameLevelOnly = false;
			}
		}
		Debug.Log("\n");
		Debug.Log("------- LOAD TO MAIN MENU --------\n\n");
		UIMainMenuManager.s_ReturningToMainMenuFromError = true;
		SceneManager.LoadScene("MainMenu");
	}

	public static void LoadLevel(MapType level)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				partyMemberAI.StateManager.AbortStateStack();
			}
		}
		LoadLevel(WorldMap.Instance.LoadedMaps[(int)level]);
	}

	public static void LoadLevel(string level)
	{
		MapData mapData = null;
		mapData = ((!(WorldMap.Instance != null)) ? WorldMap.Maps.GetMap(level) : WorldMap.Instance.GetMap(level));
		if (mapData != null)
		{
			LoadLevel(mapData);
		}
		else
		{
			Debug.LogError("Unable to find a map named " + level + " ensure it is contained in the AllMaps list.");
		}
	}

	public static void LoadLevel(MapData map)
	{
		StartPoint.s_ChosenStartPoint = null;
		if ((bool)ConditionalToggleManager.Instance)
		{
			ConditionalToggleManager.Instance.ResetBetweenSceneLoads();
		}
		Instance.CurrentNextMap = map;
		s_firstLoad = false;
		IsRestoredLevel = File.Exists(PersistenceManager.GetLevelFilePath(map.SceneName));
		bool loadFromSaveFile = true;
		Instance.StartCoroutine(GameResources.LoadScene(map.SceneName, loadFromSaveFile));
	}

	public void ProcessPersistenceData()
	{
		MapData currentMap = CurrentMap;
		CurrentMap = CurrentNextMap;
		if (CurrentMap == null || CurrentMap.DisplayName.StringID < 0)
		{
			CurrentMap = WorldMap.Instance.GetMap(SceneManager.GetActiveScene().name);
		}
		if (currentMap != null)
		{
			CouldAccessStashOnLastMap = currentMap.GetCanAccessStash();
		}
		else
		{
			CouldAccessStashOnLastMap = true;
		}
		try
		{
			if (s_firstLoad)
			{
				PersistenceManager.ClearTempData();
				PersistenceManager.LevelLoaded();
				s_firstLoad = false;
			}
			else
			{
				PersistenceManager.LevelLoaded();
				FogOfWar.Load();
			}
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
			ReturnToMainMenuFromError();
		}
	}

	public void FinalizeLevelLoad()
	{
		try
		{
			if (CurrentMap != null && !CurrentMap.HasBeenVisited && (bool)BonusXpManager.Instance && CurrentMap.GivesExplorationXp)
			{
				CurrentMap.HasBeenVisited = true;
				int num = 0;
				if (BonusXpManager.Instance != null)
				{
					num = BonusXpManager.Instance.MapExplorationXp;
				}
				Console.AddMessage("[" + NGUITools.EncodeColor(Color.yellow) + "]" + Console.Format(GUIUtils.GetTextWithLinks(1633), CurrentMap.DisplayName, num * PartyHelper.NumPartyMembers));
				PartyHelper.AssignXPToParty(num, printMessage: false);
			}
			if (GameState.OnLevelLoaded != null)
			{
				GameState.OnLevelLoaded(SceneManager.GetActiveScene().name, EventArgs.Empty);
			}
			if (NewGame)
			{
				if (Difficulty == GameDifficulty.Easy)
				{
					Option.AutoPause.SetSlowEvent(AutoPauseOptions.PauseEvent.CombatStart, isActive: true);
				}
				HasNotifiedPX2Installation = true;
			}
			ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnLevelLoaded);
			IsLoading = false;
			if (s_playerCharacter != null && !LoadedGame && !NewGame && NumSceneLoads > 0)
			{
				if ((bool)FogOfWar.Instance)
				{
					FogOfWar.Instance.WaitForFogUpdate();
				}
				Autosave();
			}
			NewGame = false;
			if (CurrentMap != null && CouldAccessStashOnLastMap != CurrentMap.GetCanAccessStash() && !Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH))
			{
				if (CurrentMap.GetCanAccessStash())
				{
					UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1565), Color.white);
				}
				else
				{
					UISystemMessager.Instance.PostMessage(GUIUtils.GetText(1566), Color.white);
				}
			}
			_ = NumSceneLoads;
			NumSceneLoads++;
			FatigueCamera.CreateCamera();
			GammaCamera.CreateCamera();
			WinCursor.Clip(state: true);
			if (CurrentMap != null)
			{
				TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.ENTERED_MAP);
				trigger.Map = CurrentMap.SceneName;
				TutorialManager.STriggerTutorialsOfType(trigger);
			}
			if (CurrentMap != null && CurrentMap.IsValidOnMap("px1"))
			{
				HasEnteredPX1 = true;
				if (GameGlobalVariables.HasStartedPX2())
				{
					HasEnteredPX2 = true;
				}
			}
			_ = CurrentMap;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			ReturnToMainMenuFromError();
		}
		if (!RetroactiveSpellMasteryChecked)
		{
			for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
			{
				if (!(PartyMemberAI.PartyMembers[i] == null))
				{
					CharacterStats component = PartyMemberAI.PartyMembers[i].GetComponent<CharacterStats>();
					if ((bool)component && component.MaxMasteredAbilitiesAllowed() > component.GetNumMasteredAbilities())
					{
						UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(2252), GUIUtils.GetText(2303));
						break;
					}
				}
			}
			RetroactiveSpellMasteryChecked = true;
		}
		if (GameUtilities.HasPX2() && LoadedGame)
		{
			if (GameGlobalVariables.HasFinishedPX1())
			{
				QuestManager.Instance.StartPX2Umbrella();
			}
			else if (!HasNotifiedPX2Installation)
			{
				UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(2438));
				HasNotifiedPX2Installation = true;
			}
		}
	}

	public void UnsetLoadedGameFlags()
	{
		LoadedGame = false;
		IsRestoredLevel = false;
	}
}

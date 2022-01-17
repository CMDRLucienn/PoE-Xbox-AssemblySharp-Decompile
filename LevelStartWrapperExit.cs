using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStartWrapperExit : MonoBehaviour
{
	private enum LevelStartPhase
	{
		LSP_NOT_STARTED,
		LSP_AWAIT_UI_LOAD,
		LSP_PROCESS_PERSISTENCE_DATA,
		LSP_FINALIZE_LEVEL_LOAD,
		LSP_AWAIT_RESTORED_CALLBACKS,
		LSP_DONE
	}

	private static LevelStartWrapperExit s_instance;

	private LevelStartPhase m_levelStartPhase;

	private int m_updateCountdown;

	private bool m_hudCreated;

	private void Awake()
	{
		LevelStartWrapperEnter.WriteLineToLog("AWAKE: " + GetType().ToString() + ", GameObject = " + base.gameObject);
	}

	private void Start()
	{
		LevelStartWrapperEnter.WriteLineToLog("START: " + GetType().ToString() + ", GameObject = " + base.gameObject);
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (s_instance == null)
		{
			LevelStartWrapperEnter.WriteLineToLog("UPDATE: PRIMARY LEVELSTARTWRAPPEREXIT OWNED BY: " + base.gameObject);
			s_instance = this;
			if (GameState.IsLoading)
			{
				m_updateCountdown = 5;
			}
			m_levelStartPhase = LevelStartPhase.LSP_AWAIT_UI_LOAD;
		}
		if (s_instance != this)
		{
			return;
		}
		if (m_levelStartPhase == LevelStartPhase.LSP_AWAIT_UI_LOAD)
		{
			if (m_updateCountdown <= 0)
			{
				if (!m_hudCreated)
				{
					InGameUILayout instance = InGameUILayout.Instance;
					UIRoot firstUIRoot = UIRoot.GetFirstUIRoot();
					if (instance == null || firstUIRoot.GetComponentInChildren<UIWindowManager>() == null)
					{
						SceneManager.LoadScene("HUD", LoadSceneMode.Additive);
						firstUIRoot = UIRoot.GetFirstUIRoot();
						GameState.PersistAcrossSceneLoadsTracked(firstUIRoot);
						if (!HudEngagementManager.Instance)
						{
							GameObject obj = new GameObject("HudEngagementManager");
							obj.AddComponent<HudEngagementManager>();
							GameState.PersistAcrossSceneLoadsTracked(obj);
						}
					}
					m_hudCreated = true;
				}
				if (UIWindowManager.s_windowsInitialized)
				{
					LevelStartWrapperEnter.WriteLineToLog("LEVEL START PROCESS: WINDOWS INITIALIZED.");
					m_levelStartPhase = LevelStartPhase.LSP_PROCESS_PERSISTENCE_DATA;
					if (GameState.IsLoading)
					{
						m_updateCountdown = 5;
					}
				}
			}
			else
			{
				m_updateCountdown--;
			}
		}
		else if (m_levelStartPhase == LevelStartPhase.LSP_PROCESS_PERSISTENCE_DATA)
		{
			if (m_updateCountdown <= 0)
			{
				LevelStartWrapperEnter.WriteLineToLog("LEVEL START PROCESS: APPLYING PERSISTENCE DATA.");
				GameState.Instance.ProcessPersistenceData();
				m_levelStartPhase = LevelStartPhase.LSP_FINALIZE_LEVEL_LOAD;
				m_updateCountdown = 3;
			}
			else
			{
				m_updateCountdown--;
			}
		}
		else if (m_levelStartPhase == LevelStartPhase.LSP_FINALIZE_LEVEL_LOAD)
		{
			if (m_updateCountdown <= 0)
			{
				LevelStartWrapperEnter.WriteLineToLog("LEVEL START PROCESS: FINALIZING LEVEL LOAD.");
				GameState.Instance.FinalizeLevelLoad();
				m_levelStartPhase = LevelStartPhase.LSP_AWAIT_RESTORED_CALLBACKS;
				m_updateCountdown = 3;
			}
			else
			{
				m_updateCountdown--;
			}
		}
		else if (m_levelStartPhase == LevelStartPhase.LSP_AWAIT_RESTORED_CALLBACKS)
		{
			if (m_updateCountdown <= 0)
			{
				GameState.Instance.UnsetLoadedGameFlags();
				GameResources.RemoveNullEntriesFromPrefabCache();
				m_levelStartPhase = LevelStartPhase.LSP_DONE;
			}
			else
			{
				m_updateCountdown--;
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OEIFormats.FlowCharts;
using OEIFormats.FlowCharts.Quests;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
	private class QueuedEventTriggeredChange
	{
		public string QuestName { get; set; }

		public int ID { get; set; }

		public bool State { get; set; }
	}

	public delegate void QuestDelegate(Quest quest);

	public delegate void QuestObjectiveDelegate(Quest quest, ObjectiveNode node);

	public delegate void QuestAddendumDelegate(Quest quest, int addendumId);

	[Serializable]
	public class QuestTimestamps
	{
		[SerializeField]
		public EternityDateTime startTime { get; set; }

		[SerializeField]
		public EternityDateTime[] objectiveTimestamps { get; set; }

		[SerializeField]
		public EternityDateTime[] addendumTimestamps { get; set; }

		public QuestTimestamps()
		{
		}

		public QuestTimestamps(Quest q)
		{
			objectiveTimestamps = new EternityDateTime[q.GetHighestNodeID() + 1];
			addendumTimestamps = new EternityDateTime[q.GetHighestAddendumID() + 1];
		}

		public void SetObjective(int nodeid)
		{
			if (nodeid < objectiveTimestamps.Length)
			{
				objectiveTimestamps[nodeid] = new EternityDateTime(WorldTime.Instance.CurrentTime);
			}
		}

		public void SetAddendum(int addendumid)
		{
			if (addendumid < addendumTimestamps.Length)
			{
				addendumTimestamps[addendumid] = new EternityDateTime(WorldTime.Instance.CurrentTime);
			}
		}

		public EternityDateTime GetObjective(int nodeid)
		{
			if (nodeid < objectiveTimestamps.Length)
			{
				return objectiveTimestamps[nodeid];
			}
			return null;
		}

		public EternityDateTime GetAddendum(int addendumid)
		{
			if (addendumid < addendumTimestamps.Length)
			{
				return addendumTimestamps[addendumid];
			}
			return null;
		}
	}

	[Serializable]
	public class QuestTracker
	{
		public BitArray TriggeredEvents { get; set; }

		public BitArray VisitedStates { get; set; }

		public BitArray FailedStates { get; set; }

		public BitArray TriggeredAddendums { get; set; }

		public int EndState { get; set; }

		public bool Failed { get; set; }

		public QuestTracker()
		{
		}

		public QuestTracker(int eventBits, int stateBits, int addendumBits)
		{
			TriggeredEvents = new BitArray(eventBits, defaultValue: false);
			VisitedStates = new BitArray(stateBits, defaultValue: false);
			FailedStates = new BitArray(stateBits, defaultValue: false);
			TriggeredAddendums = new BitArray(addendumBits, defaultValue: false);
			EndState = -1;
		}

		public void EnsureSizesAreLargeEnough(int eventBits, int stateBits, int addendumBits)
		{
			ValidateBitArrays(eventBits, stateBits, addendumBits);
			if (TriggeredEvents.Count <= eventBits)
			{
				TriggeredEvents.Length = eventBits + 1;
			}
			if (VisitedStates.Count <= stateBits)
			{
				VisitedStates.Length = stateBits + 1;
			}
			if (FailedStates.Count <= stateBits)
			{
				FailedStates.Length = stateBits + 1;
			}
			if (TriggeredAddendums.Count <= addendumBits)
			{
				TriggeredAddendums.Length = addendumBits + 1;
			}
		}

		private void ValidateBitArrays(int eventBits, int stateBits, int addendumBits)
		{
			if (TriggeredEvents == null)
			{
				TriggeredEvents = new BitArray(eventBits, defaultValue: false);
			}
			if (VisitedStates == null)
			{
				VisitedStates = new BitArray(stateBits, defaultValue: false);
			}
			if (FailedStates == null)
			{
				FailedStates = new BitArray(stateBits, defaultValue: false);
			}
			if (TriggeredAddendums == null)
			{
				TriggeredAddendums = new BitArray(addendumBits, defaultValue: false);
			}
		}
	}

	private List<QueuedEventTriggeredChange> m_queuedEventChanges = new List<QueuedEventTriggeredChange>();

	private List<QueuedEventTriggeredChange> m_queuedAddendumChanges = new List<QueuedEventTriggeredChange>();

	[Tooltip("The display name of the main questline heading in the journal.")]
	public GUIDatabaseString QuestlineName;

	[Tooltip("The display name of the PX1 main questline heading.")]
	public GUIDatabaseString PX1_QuestlineName;

	[Tooltip("The display name of the PX2 main questline heading.")]
	public GUIDatabaseString PX2_QuestlineName;

	[Tooltip("The quest to start to lead the player to PX1.")]
	[QuestPath]
	public string PX1_QuestHook;

	[Tooltip("The quest to start to lead the player to PX2.")]
	[QuestPath]
	public string PX2_QuestHook;

	private static bool s_isInitialized = false;

	private const string QUEST_PATH = "/data/quests/";

	private const string QUEST_FILE_EXTENSION = ".quest";

	private List<KeyValuePair<Quest, int>> NodesToStamp = new List<KeyValuePair<Quest, int>>();

	private List<KeyValuePair<Quest, int>> AddendumsToStamp = new List<KeyValuePair<Quest, int>>();

	private ExperienceTable m_experienceTable = new ExperienceTable();

	private List<QuestSerializerPacket> m_serializedQuestData = new List<QuestSerializerPacket>();

	private Quest m_UpdatedQuest;

	[Persistent]
	private string m_LastUpdatedQuestFilename;

	private object m_UpdatedQuestMonitor = new object();

	private static List<Tuple<Quest, ObjectiveNode>> ObjectivesToComplete = new List<Tuple<Quest, ObjectiveNode>>();

	private static List<Quest> QuestsToComplete = new List<Quest>();

	public QuestDelegate OnQuestStarted;

	public QuestObjectiveDelegate OnQuestUpdated;

	public QuestDelegate OnQuestCompleted;

	public QuestDelegate OnQuestFailed;

	public QuestAddendumDelegate OnQuestAddendum;

	public static bool StartingQuest = false;

	public static QuestManager Instance { get; private set; }

	public static QuestManager ToolInstance => Instance;

	public static bool IsInitialized => s_isInitialized;

	[Persistent]
	private Dictionary<string, QuestTimestamps> Timestamps { get; set; }

	private static Dictionary<string, Quest> s_PreLoadedQuests { get; set; }

	private Dictionary<string, Quest> LoadedQuests { get; set; }

	private Dictionary<string, Quest> ActiveQuests { get; set; }

	[Persistent(Persistent.ConversionType.Binary)]
	public List<QuestSerializerPacket> SerializedActiveQuests
	{
		get
		{
			m_serializedQuestData.Clear();
			foreach (Quest value in ActiveQuests.Values)
			{
				QuestSerializerPacket item = new QuestSerializerPacket(value.Filename, value.ActiveStates, value.QuestDescriptionID);
				m_serializedQuestData.Add(item);
			}
			return m_serializedQuestData;
		}
		set
		{
			m_serializedQuestData = value;
		}
	}

	[Persistent]
	public List<ExperienceSerializerPacket> SerializedExperienceData
	{
		get
		{
			return m_experienceTable.SerializedList;
		}
		set
		{
			m_experienceTable.SerializedList = value;
		}
	}

	[Persistent(Persistent.ConversionType.Binary)]
	private Dictionary<string, QuestTracker> QuestTrackers { get; set; }

	public Quest LastUpdatedQuest
	{
		get
		{
			if (!string.IsNullOrEmpty(m_LastUpdatedQuestFilename) && LoadedQuests.ContainsKey(m_LastUpdatedQuestFilename))
			{
				return LoadedQuests[m_LastUpdatedQuestFilename];
			}
			return null;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'QuestManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		LoadedQuests = new Dictionary<string, Quest>(StringComparer.OrdinalIgnoreCase);
		ActiveQuests = new Dictionary<string, Quest>(StringComparer.OrdinalIgnoreCase);
		QuestTrackers = new Dictionary<string, QuestTracker>(StringComparer.OrdinalIgnoreCase);
		m_queuedEventChanges.Clear();
		m_queuedAddendumChanges.Clear();
		OutputQuestsToFile();
		s_isInitialized = true;
	}

	public static void LoadQuestData()
	{
		if (s_PreLoadedQuests == null)
		{
			s_PreLoadedQuests = new Dictionary<string, Quest>(StringComparer.OrdinalIgnoreCase);
			string[] files = Directory.GetFiles(Application.dataPath + "/data/quests/", "*.quest", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string text = files[i].Replace('\\', '/');
				text = text.Remove(0, Application.dataPath.Length + 1).ToLower();
				Quest quest = ScriptableObject.CreateInstance<Quest>();
				quest.Load(text);
				s_PreLoadedQuests.Add(text, quest);
			}
		}
	}

	public static void UnloadPreloadedQuestData()
	{
		if (s_PreLoadedQuests == null)
		{
			return;
		}
		foreach (Quest value in s_PreLoadedQuests.Values)
		{
			if (value != null)
			{
				GameUtilities.Destroy(value);
			}
		}
		s_PreLoadedQuests.Clear();
	}

	private void Start()
	{
		LoadQuestData();
		if (Timestamps == null)
		{
			Timestamps = new Dictionary<string, QuestTimestamps>();
		}
		m_experienceTable.InitializeExperienceTable(32);
		foreach (KeyValuePair<string, Quest> s_PreLoadedQuest in s_PreLoadedQuests)
		{
			Quest quest = ComponentUtils.CopyScriptableObject(s_PreLoadedQuest.Value);
			quest.ActiveStates = new List<int>();
			LoadedQuests.Add(quest.Filename, quest);
			m_experienceTable.RegisterQuest(quest);
			VerifyTrackerKeys(quest);
		}
		OutputQuestsToFile();
		GameResources.OnPreloadGame += GameResources_OnPreloadGame;
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void GameResources_OnPreloadGame()
	{
		ActiveQuests = new Dictionary<string, Quest>(StringComparer.OrdinalIgnoreCase);
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(PX1_QuestHook) && GameUtilities.HasPX1() && GlobalVariables.Instance.GetVariable("n_Ruins_Quest_Stage") == 4 && !IsQuestActive(PX1_QuestHook))
		{
			StartQuest(PX1_QuestHook, null);
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(873), GUIUtils.GetText(2115));
		}
	}

	public bool StartPX2Umbrella()
	{
		Quest questByPath = GetQuestByPath(PX2_QuestHook);
		if (questByPath != null && GameUtilities.HasPX2() && !questByPath.IsStarted())
		{
			GameState.Instance.HasNotifiedPX2Installation = true;
			StartQuest(PX2_QuestHook, null);
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, "", GUIUtils.GetText(2439));
			return true;
		}
		return false;
	}

	private void VerifyTrackerKeys(Quest quest)
	{
		if (!(quest == null))
		{
			int highestEventID = quest.GetHighestEventID();
			int highestNodeID = quest.GetHighestNodeID();
			int highestAddendumID = quest.GetHighestAddendumID();
			if (!QuestTrackers.ContainsKey(quest.Filename))
			{
				QuestTrackers.Add(quest.Filename, new QuestTracker(highestEventID + 1, highestNodeID + 1, highestAddendumID + 1));
			}
			else
			{
				QuestTrackers[quest.Filename].EnsureSizesAreLargeEnough(highestEventID, highestNodeID, highestAddendumID);
			}
			if (!Timestamps.ContainsKey(quest.Filename))
			{
				Timestamps.Add(quest.Filename, new QuestTimestamps(quest));
			}
		}
	}

	private void OnQuestRestored(Quest quest)
	{
		VerifyTrackerKeys(quest);
	}

	public void Restored()
	{
		foreach (KeyValuePair<string, Quest> loadedQuest in LoadedQuests)
		{
			loadedQuest.Value.ActiveStates = new List<int>();
			OnQuestRestored(loadedQuest.Value);
		}
		foreach (QuestSerializerPacket serializedQuestDatum in m_serializedQuestData)
		{
			if (LoadedQuests.ContainsKey(serializedQuestDatum.Name))
			{
				if (!ActiveQuests.ContainsKey(serializedQuestDatum.Name))
				{
					ActiveQuests.Add(serializedQuestDatum.Name, LoadedQuests[serializedQuestDatum.Name]);
				}
				ActiveQuests[serializedQuestDatum.Name].ActiveStates = serializedQuestDatum.ActiveStates;
				ActiveQuests[serializedQuestDatum.Name].QuestDescriptionID = serializedQuestDatum.QuestDescriptionID;
			}
		}
		m_experienceTable.Restored(LoadedQuests);
		OutputQuestsToFile();
	}

	public void Update()
	{
		lock (m_UpdatedQuestMonitor)
		{
			if ((bool)m_UpdatedQuest)
			{
				UIJournalManager.Instance.HintShowQuest(m_UpdatedQuest);
				m_UpdatedQuest = null;
				if (GameState.Instance.CurrentMap.SceneName != "AR_0701_Encampment")
				{
					TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.JOURNAL_UPDATED_NOT_ENCAMPMENT);
				}
			}
		}
		lock (ObjectivesToComplete)
		{
			if (ObjectivesToComplete.Count > 0)
			{
				foreach (Tuple<Quest, ObjectiveNode> item in ObjectivesToComplete)
				{
					m_experienceTable.CompleteQuestObjective(item.First, item.Second);
				}
				ObjectivesToComplete.Clear();
			}
		}
		lock (QuestsToComplete)
		{
			if (QuestsToComplete.Count > 0)
			{
				for (int i = 0; i < QuestsToComplete.Count; i++)
				{
					m_experienceTable.CompleteQuest(QuestsToComplete[i]);
				}
				QuestsToComplete.Clear();
			}
		}
		lock (NodesToStamp)
		{
			if (NodesToStamp.Count > 0)
			{
				foreach (KeyValuePair<Quest, int> item2 in NodesToStamp)
				{
					Timestamps[item2.Key.Filename].SetObjective(item2.Value);
				}
				NodesToStamp.Clear();
			}
		}
		lock (AddendumsToStamp)
		{
			if (AddendumsToStamp.Count > 0)
			{
				foreach (KeyValuePair<Quest, int> item3 in AddendumsToStamp)
				{
					Timestamps[item3.Key.Filename].SetAddendum(item3.Value);
				}
				AddendumsToStamp.Clear();
			}
		}
		lock (m_queuedEventChanges)
		{
			if (m_queuedEventChanges.Count > 0)
			{
				foreach (QueuedEventTriggeredChange queuedEventChange in m_queuedEventChanges)
				{
					SetEventTriggeredState(queuedEventChange.QuestName, queuedEventChange.ID, queuedEventChange.State);
					AdvanceQuestFromStart(queuedEventChange.QuestName);
				}
				m_queuedEventChanges.Clear();
			}
		}
		lock (m_queuedAddendumChanges)
		{
			if (m_queuedAddendumChanges.Count <= 0)
			{
				return;
			}
			foreach (QueuedEventTriggeredChange queuedAddendumChange in m_queuedAddendumChanges)
			{
				SetQuestAddendumState(queuedAddendumChange.QuestName, queuedAddendumChange.ID, queuedAddendumChange.State);
			}
			m_queuedAddendumChanges.Clear();
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if (LoadedQuests != null)
		{
			foreach (Quest value in LoadedQuests.Values)
			{
				if (value != null)
				{
					GameUtilities.Destroy(value);
				}
			}
		}
		LoadedQuests.Clear();
		GameResources.OnPreloadGame -= GameResources_OnPreloadGame;
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void StartQuest(string questName, GameObject owner)
	{
		StartQuest(questName, 0, owner);
	}

	public void StartQuest(string questName, int questDescriptionID, GameObject owner)
	{
		if (string.IsNullOrEmpty(questName))
		{
			Debug.LogWarning("Attempting to start an empty quest.");
			return;
		}
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (ActiveQuests.ContainsKey(questName))
		{
			Debug.LogWarning("Quest is already active " + questName + ".");
		}
		else if (!LoadedQuests.ContainsKey(questName))
		{
			Debug.LogWarning("Quest does not exist " + questName + ".");
		}
		else if (!LoadedQuests[questName].IsQuestFailed())
		{
			ActiveQuests.Add(questName, LoadedQuests[questName]);
			LoadedQuests[questName].StartQuest(questDescriptionID);
			Timestamps[questName].startTime = new EternityDateTime(WorldTime.Instance.CurrentTime);
			NotifyQuestStarted(LoadedQuests[questName]);
		}
	}

	public void AdvanceQuest(string questName)
	{
		AdvanceQuest(questName, force: false);
	}

	public void AdvanceQuest(string questName, bool force)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (ActiveQuests.ContainsKey(questName))
		{
			ActiveQuests[questName].Advance(force);
		}
	}

	public void AdvanceQuestFromStart(string questName)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			int questDescriptionID = LoadedQuests[questName].QuestDescriptionID;
			QuestTrackers[questName].VisitedStates.SetAll(value: false);
			QuestTrackers[questName].FailedStates.SetAll(value: false);
			LoadedQuests[questName].ClearActiveStates();
			EndQuest(questName);
			StartQuest(questName, questDescriptionID, null);
		}
	}

	public void EndQuest(string questName)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (ActiveQuests.ContainsKey(questName))
		{
			ActiveQuests.Remove(questName);
		}
	}

	public bool IsQuestStateActive(string questName, int nodeID)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (ActiveQuests != null && ActiveQuests.ContainsKey(questName))
		{
			return ActiveQuests[questName].IsQuestStateActive(nodeID);
		}
		return false;
	}

	public bool IsQuestActive(string questName)
	{
		return ActiveQuests.ContainsKey(questName);
	}

	public List<Quest> GetActiveQuestList()
	{
		return new List<Quest>(ActiveQuests.Values);
	}

	public List<Quest> GetCompletedQuestList()
	{
		List<Quest> list = new List<Quest>();
		foreach (Quest value in LoadedQuests.Values)
		{
			if (value.IsComplete())
			{
				list.Add(value);
			}
		}
		return list;
	}

	public List<Quest> GetIncompleteQuests(QuestType type)
	{
		List<Quest> list = new List<Quest>();
		foreach (Quest value in LoadedQuests.Values)
		{
			if (value != null && value.GetQuestType() == type && value.IsStarted() && !value.IsComplete() && !value.IsQuestFailed())
			{
				list.Add(value);
			}
		}
		return list;
	}

	public List<Quest> GetCompleteQuests(QuestType type)
	{
		List<Quest> list = new List<Quest>();
		foreach (Quest value in LoadedQuests.Values)
		{
			if (value != null && value.GetQuestType() == type && value.IsComplete() && !value.IsQuestFailed())
			{
				list.Add(value);
			}
		}
		return list;
	}

	public void SetEventTriggered(Quest quest, int eventID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			QuestTrackers[quest.Filename].TriggeredEvents.Set(eventID, value: true);
			AdvanceQuest(quest.Filename);
		}
	}

	public void QueueEventTriggeredState(string questName, int id, bool state)
	{
		QueuedEventTriggeredChange queuedEventTriggeredChange = new QueuedEventTriggeredChange();
		queuedEventTriggeredChange.QuestName = questName;
		queuedEventTriggeredChange.ID = id;
		queuedEventTriggeredChange.State = state;
		lock (m_queuedEventChanges)
		{
			m_queuedEventChanges.Add(queuedEventTriggeredChange);
		}
	}

	public void SetEventTriggeredState(string questName, int eventID, bool state)
	{
		if (LoadedQuests.ContainsKey(questName) && QuestTrackers.ContainsKey(questName))
		{
			QuestTrackers[questName].TriggeredEvents.Set(eventID, state);
		}
	}

	public bool IsEventTriggered(Quest quest, int eventID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].TriggeredEvents.Get(eventID);
		}
		return false;
	}

	public bool IsEventTriggered(string questName, int eventID)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			return IsEventTriggered(LoadedQuests[questName], eventID);
		}
		return false;
	}

	public void SetStateVisited(Quest quest, int nodeID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			QuestTrackers[quest.Filename].VisitedStates.Set(nodeID, value: true);
		}
		if (Timestamps.ContainsKey(quest.Filename))
		{
			lock (NodesToStamp)
			{
				NodesToStamp.Add(new KeyValuePair<Quest, int>(quest, nodeID));
			}
		}
	}

	public bool IsStateVisited(Quest quest, int nodeID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].VisitedStates.Get(nodeID);
		}
		return false;
	}

	public void FailState(Quest quest, int nodeID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			QuestTrackers[quest.Filename].FailedStates.Set(nodeID, value: true);
		}
	}

	public bool IsStateFailed(Quest quest, int nodeID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].FailedStates.Get(nodeID);
		}
		return false;
	}

	public EternityDateTime GetStateTimestamp(Quest quest, int nodeID)
	{
		return Timestamps[quest.Filename].GetObjective(nodeID);
	}

	public EternityDateTime GetQuestStartTime(Quest quest)
	{
		if (Timestamps.ContainsKey(quest.Filename))
		{
			return Timestamps[quest.Filename].startTime;
		}
		return new EternityDateTime(0);
	}

	public void SetQuestAlternateDescription(string questName, int questDescriptionID)
	{
		if (string.IsNullOrEmpty(questName))
		{
			Debug.LogWarning("Attempting to modify an empty quest.");
			return;
		}
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			LoadedQuests[questName].QuestDescriptionID = questDescriptionID;
		}
	}

	public EternityDateTime GetAddendumTimestamp(Quest quest, int nodeID)
	{
		return Timestamps[quest.Filename].GetAddendum(nodeID);
	}

	public void TriggerQuestAddendum(string questName, int addendumID)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			TriggerQuestAddendum(LoadedQuests[questName], addendumID);
		}
	}

	public void TriggerQuestAddendum(Quest quest, int addendumID)
	{
		if (GetEndStateID(quest) != -1 || !QuestTrackers.ContainsKey(quest.Filename) || IsAddendumTriggered(quest, addendumID))
		{
			return;
		}
		QuestTracker questTracker = QuestTrackers[quest.Filename];
		if (quest.IsStarted() && IsStateVisited(quest, quest.GetAddendumNode(addendumID)))
		{
			NotifyQuestAddendum(quest, addendumID);
		}
		questTracker.TriggeredAddendums.Set(addendumID, value: true);
		if (Timestamps.ContainsKey(quest.Filename))
		{
			lock (AddendumsToStamp)
			{
				AddendumsToStamp.Add(new KeyValuePair<Quest, int>(quest, addendumID));
			}
		}
	}

	public void QueueAddendumState(string questName, int id, bool state)
	{
		QueuedEventTriggeredChange queuedEventTriggeredChange = new QueuedEventTriggeredChange();
		queuedEventTriggeredChange.QuestName = questName;
		queuedEventTriggeredChange.ID = id;
		queuedEventTriggeredChange.State = state;
		lock (m_queuedAddendumChanges)
		{
			m_queuedAddendumChanges.Add(queuedEventTriggeredChange);
		}
	}

	public void UntriggerQuestAddendum(string questName, int addendumID)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			UntriggerQuestAddendum(LoadedQuests[questName], addendumID);
		}
	}

	public void UntriggerQuestAddendum(Quest quest, int addendumID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename) && IsAddendumTriggered(quest, addendumID))
		{
			QuestTrackers[quest.Filename].TriggeredAddendums.Set(addendumID, value: false);
		}
	}

	public bool IsAddendumTriggered(string questName, int addendumID)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			return IsAddendumTriggered(LoadedQuests[questName], addendumID);
		}
		return false;
	}

	public bool IsAddendumTriggered(Quest quest, int addendumID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].TriggeredAddendums.Get(addendumID);
		}
		return false;
	}

	public void SetQuestAddendumState(string questName, int addendumID, bool state)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (state)
		{
			TriggerQuestAddendum(LoadedQuests[questName], addendumID);
		}
		else
		{
			UntriggerQuestAddendum(LoadedQuests[questName], addendumID);
		}
	}

	public void TriggerQuestEndState(string questName, int endStateID, bool failed)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (Timestamps.ContainsKey(questName))
		{
			QuestTimestamps questTimestamps = Timestamps[questName];
			if (questTimestamps.startTime == null)
			{
				questTimestamps.startTime = new EternityDateTime(WorldTime.Instance.CurrentTime);
			}
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			TriggerQuestEndState(LoadedQuests[questName], endStateID, failed);
		}
	}

	public void TriggerQuestEndState(Quest quest, int endStateID, bool failed)
	{
		if (!QuestTrackers.ContainsKey(quest.Filename))
		{
			return;
		}
		bool flag = quest.IsComplete();
		bool failed2 = QuestTrackers[quest.Filename].Failed;
		QuestTrackers[quest.Filename].EndState = endStateID;
		QuestTrackers[quest.Filename].Failed = failed;
		if (failed)
		{
			if (!failed2)
			{
				NotifyQuestFailed(quest);
			}
		}
		else if (!flag)
		{
			MarkQuestCompleted(quest);
			NotifyQuestComplete(quest);
		}
	}

	public bool IsQuestFailed(string questName)
	{
		Quest questByPath = GetQuestByPath(questName);
		if ((bool)questByPath)
		{
			return IsQuestFailed(questByPath);
		}
		return false;
	}

	public bool IsQuestFailed(Quest quest)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].Failed;
		}
		return false;
	}

	public bool IsEndStateTriggered(string questName, int endStateID)
	{
		Quest questByPath = GetQuestByPath(questName);
		if ((bool)questByPath)
		{
			return IsEndStateTriggered(questByPath, endStateID);
		}
		return false;
	}

	public bool IsEndStateTriggered(Quest quest, int endStateID)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].EndState == endStateID;
		}
		return false;
	}

	public int GetEndStateID(string questName)
	{
		Quest questByPath = GetQuestByPath(questName);
		if ((bool)questByPath)
		{
			return GetEndStateID(questByPath);
		}
		return -1;
	}

	private Quest GetQuestByPath(string questName)
	{
		questName = questName.Replace('\\', '/').ToLower();
		if (questName.Contains("assets"))
		{
			questName = questName.Remove(0, "assets/".Length);
		}
		if (LoadedQuests.ContainsKey(questName))
		{
			return LoadedQuests[questName];
		}
		return null;
	}

	public int GetEndStateID(Quest quest)
	{
		if (QuestTrackers.ContainsKey(quest.Filename))
		{
			return QuestTrackers[quest.Filename].EndState;
		}
		return -1;
	}

	public void TriggerTalkEvent(string conversationFilename, int nodeID)
	{
		conversationFilename = conversationFilename.Replace('\\', '/').ToLower();
		if (conversationFilename.Contains("assets"))
		{
			conversationFilename = conversationFilename.Remove(0, "assets/".Length);
		}
		foreach (Quest value in LoadedQuests.Values)
		{
			foreach (QuestEvent @event in value.QuestData.Events)
			{
				if (@event.EventType == QuestEventType.Talk)
				{
					QuestEventTalk obj = @event as QuestEventTalk;
					string text = obj.Conversation.Replace('\\', '/').ToLower();
					if (text.Contains("assets"))
					{
						text = text.Remove(0, "assets/".Length);
					}
					if (obj.NodeID == nodeID && string.Compare(text, conversationFilename, ignoreCase: true) == 0)
					{
						SetEventTriggered(value, @event.EventID);
					}
				}
			}
		}
	}

	public void TriggerGlobalVariableEvent(string globalVariableName, int variableValue)
	{
		if (LoadedQuests == null)
		{
			Debug.LogWarning("LoadedQuests is not ready for TriggerGlobalVariableEvent in QuestManager!");
			return;
		}
		foreach (Quest value in LoadedQuests.Values)
		{
			foreach (QuestEvent @event in value.QuestData.Events)
			{
				if (@event.EventType == QuestEventType.GlobalVariable)
				{
					QuestEventGlobalVariable questEventGlobalVariable = @event as QuestEventGlobalVariable;
					int num = questEventGlobalVariable.VariableValue;
					if (!string.IsNullOrEmpty(questEventGlobalVariable.ExternalVariableValue))
					{
						num = GlobalVariables.Instance.GetVariable(questEventGlobalVariable.ExternalVariableValue);
					}
					if (num == variableValue && string.Compare(questEventGlobalVariable.VariableName, globalVariableName, ignoreCase: true) == 0)
					{
						SetEventTriggered(value, @event.EventID);
					}
				}
			}
		}
	}

	public static void MarkObjectiveCompleted(Quest quest, ObjectiveNode node)
	{
		lock (ObjectivesToComplete)
		{
			ObjectivesToComplete.Add(new Tuple<Quest, ObjectiveNode>(quest, node));
		}
	}

	public static void MarkQuestCompleted(Quest quest)
	{
		lock (QuestsToComplete)
		{
			QuestsToComplete.Add(quest);
		}
	}

	public List<string> FindLoadedQuests(string search)
	{
		List<string> list = new List<string>();
		foreach (Quest value in LoadedQuests.Values)
		{
			if (Path.GetFileNameWithoutExtension(value.Filename).Contains(search))
			{
				list.Add(value.Filename);
			}
		}
		return list;
	}

	public List<string> GetLoadedQuestPaths()
	{
		List<string> list = new List<string>();
		list.AddRange(LoadedQuests.Keys);
		return list;
	}

	public void NotifyQuestStarted(Quest quest)
	{
		PostQuestMessage(quest, 95);
		if (OnQuestStarted != null)
		{
			OnQuestStarted(quest);
		}
	}

	public void NotifyQuestUpdated(Quest quest, ObjectiveNode data)
	{
		if (!StartingQuest)
		{
			PostQuestMessage(quest, 96);
		}
		NotifyQuestUpdatedNoLog(quest, data);
	}

	public void NotifyQuestUpdatedNoLog(Quest quest, ObjectiveNode data)
	{
		if (OnQuestUpdated != null && quest.IsStarted())
		{
			OnQuestUpdated(quest, data);
		}
	}

	public void NotifyQuestComplete(Quest quest)
	{
		PostQuestMessage(quest, 97);
		if (OnQuestCompleted != null)
		{
			OnQuestCompleted(quest);
		}
	}

	public void NotifyQuestFailed(Quest quest)
	{
		PostQuestMessage(quest, 261);
		if (OnQuestFailed != null)
		{
			OnQuestFailed(quest);
		}
	}

	public void NotifyQuestAddendum(Quest quest, int addendumID)
	{
		PostQuestMessage(quest, 96);
		if (OnQuestAddendum != null)
		{
			OnQuestAddendum(quest, addendumID);
		}
	}

	private void PostQuestMessage(Quest quest, int stringId)
	{
		lock (m_UpdatedQuestMonitor)
		{
			m_UpdatedQuest = quest;
		}
		if (!quest.IsComplete())
		{
			m_LastUpdatedQuestFilename = quest.Filename;
		}
		string text = ((quest.GetQuestType() == QuestType.Task) ? GUIUtils.GetText(116) : GUIUtils.GetText(118));
		if (GameState.Option.DisplayQuestObjectiveTitles)
		{
			Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(stringId), text, quest.GetQuestTitle()), Color.green);
		}
		else
		{
			Console.AddMessage(GUIUtils.GetTextWithLinks(94), Color.green);
		}
	}

	public void OutputQuestsToFile()
	{
	}

	public void OutputQuestsToFile(string filename)
	{
	}

	public void ImportQuestsFromFile(string filename)
	{
	}
}

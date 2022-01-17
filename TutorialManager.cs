using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	public enum TriggerType
	{
		NONE,
		ITEM_COLLECTED,
		ENTERED_MAP,
		UIWINDOW_OPENED,
		UIWINDOW_CLOSED,
		STORE_SCREEN_OPENED,
		STRONGHOLD_SCREEN_OPENED
	}

	public enum ExclusiveTriggerType
	{
		NONE,
		CLICK_FRIENDLY,
		CONVERSATION_AFTER_INTRO,
		CONVERSATION_STAT_OPTION,
		DEPRECATED_ANY_TRIGGER,
		JOURNAL_UPDATED_NOT_ENCAMPMENT,
		COMBAT_START,
		PARTYMEM_ATTACK_ROLL,
		PARTYMEM_GETS_MIN_DAMAGE,
		PARTYMEM_GETS_CRIT,
		PARTYMEM_GETS_DEFENSE_TOO_HIGH,
		PARTYMEM_TAKES_DAMAGE,
		PARTYMEM_KNOCKED_OUT,
		USED_PER_REST_ENCOUNTER,
		PARTYMEM_LOW_HEALTH,
		SCOUTING_DETECTION_BEGIN,
		WIZARD_SPELL_LEARNED,
		GRIMOIRE_LOOTED,
		INVENTORY_CLOSED_WITH_WEAPON_SET,
		MULTIPLE_CHARACTERS_SELECTED,
		PARTY_MEMBER_DISMISSED,
		LEVEL_UP_AVAILABLE,
		DISPOSITION_GAINED,
		REPUTATION_GAINED,
		LEVEL_UP_COMPLETE,
		PARTYMEM_GETS_ENGAGED,
		PARTYMEM_GETS_FATIGUED,
		STRONGHOLD_UNLOCKED,
		COUNT
	}

	[Serializable]
	public struct TutorialTrigger
	{
		public TriggerType Type;

		[Tooltip("If the TriggerType requires a map, the map to check against.")]
		public string Map;

		[Tooltip("If the TriggerType requires an item, the item's prefab name.")]
		public string ItemName;

		public UIHudWindow.WindowType WindowType;

		public UIStorePageType StoreTab;

		public Stronghold.WindowPane StrongholdTab;

		public TutorialTrigger(TriggerType type)
		{
			Type = type;
			Map = "";
			ItemName = "";
			WindowType = UIHudWindow.WindowType.Area_Map;
			StoreTab = UIStorePageType.Inn;
			StrongholdTab = Stronghold.WindowPane.Status;
		}

		public override bool Equals(object obj)
		{
			if (obj is TutorialTrigger tutorialTrigger)
			{
				if (Type != tutorialTrigger.Type)
				{
					return false;
				}
				if (Type == TriggerType.ENTERED_MAP)
				{
					return Map == tutorialTrigger.Map;
				}
				if (Type == TriggerType.ITEM_COLLECTED)
				{
					return ItemName == tutorialTrigger.ItemName;
				}
				if (Type == TriggerType.UIWINDOW_CLOSED || Type == TriggerType.UIWINDOW_OPENED)
				{
					return WindowType == tutorialTrigger.WindowType;
				}
				if (Type == TriggerType.STORE_SCREEN_OPENED)
				{
					return StoreTab == tutorialTrigger.StoreTab;
				}
				if (Type == TriggerType.STRONGHOLD_SCREEN_OPENED)
				{
					return StrongholdTab == tutorialTrigger.StrongholdTab;
				}
				return true;
			}
			return false;
		}

		public static bool operator ==(TutorialTrigger trigger1, TutorialTrigger trigger2)
		{
			if (trigger1.Type != trigger2.Type)
			{
				return false;
			}
			if (trigger1.Type == TriggerType.ENTERED_MAP)
			{
				return trigger1.Map == trigger2.Map;
			}
			if (trigger1.Type == TriggerType.ITEM_COLLECTED)
			{
				return trigger1.ItemName == trigger2.ItemName;
			}
			if (trigger1.Type == TriggerType.UIWINDOW_CLOSED || trigger1.Type == TriggerType.UIWINDOW_OPENED)
			{
				return trigger1.WindowType == trigger2.WindowType;
			}
			if (trigger1.Type == TriggerType.STORE_SCREEN_OPENED)
			{
				return trigger1.StoreTab == trigger2.StoreTab;
			}
			if (trigger1.Type == TriggerType.STRONGHOLD_SCREEN_OPENED)
			{
				return trigger1.StrongholdTab == trigger2.StrongholdTab;
			}
			return true;
		}

		public static bool operator !=(TutorialTrigger trigger1, TutorialTrigger trigger2)
		{
			if (trigger1.Type != trigger2.Type)
			{
				return true;
			}
			if (trigger1.Type == TriggerType.ENTERED_MAP)
			{
				return !(trigger1.Map == trigger2.Map);
			}
			if (trigger1.Type == TriggerType.ITEM_COLLECTED)
			{
				return !(trigger1.ItemName == trigger2.ItemName);
			}
			if (trigger1.Type == TriggerType.UIWINDOW_CLOSED || trigger1.Type == TriggerType.UIWINDOW_OPENED)
			{
				return trigger1.WindowType != trigger2.WindowType;
			}
			if (trigger1.Type == TriggerType.STORE_SCREEN_OPENED)
			{
				return trigger1.StoreTab != trigger2.StoreTab;
			}
			if (trigger1.Type == TriggerType.STRONGHOLD_SCREEN_OPENED)
			{
				return trigger1.StrongholdTab != trigger2.StrongholdTab;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode();
		}

		public override string ToString()
		{
			string text = "{" + Type;
			if (Type == TriggerType.ENTERED_MAP)
			{
				text = text + ", " + Map;
			}
			else if (Type == TriggerType.ITEM_COLLECTED)
			{
				text = text + ", " + ItemName;
			}
			else if (Type == TriggerType.UIWINDOW_CLOSED || Type == TriggerType.UIWINDOW_OPENED)
			{
				text = text + ", " + WindowType;
			}
			else if (Type == TriggerType.STORE_SCREEN_OPENED)
			{
				text = text + ", " + StoreTab;
			}
			else if (Type == TriggerType.STRONGHOLD_SCREEN_OPENED)
			{
				text = text + ", " + StrongholdTab;
			}
			return text + "}";
		}
	}

	[Serializable]
	public class TutorialItem
	{
		[Tooltip("Design note used to identify this tutorial in the editor only.")]
		public string Note;

		public TutorialDatabaseString Text;

		[Tooltip("Does this tutorial pause the game when triggered?")]
		public bool Pauses;

		[Tooltip("The index of a tutorial that should succeed this one.")]
		public int FollowedBy = -1;

		public ExclusiveTriggerType ExAutoTrigger;

		[Tooltip("Things that should automatically trigger this tutorial. Multiple entries are treated as an OR.")]
		public TutorialTrigger[] AutoTriggers;

		[Tooltip("If set, this tutorial will not show as long as other tutorials are showing.")]
		public bool Soft;

		[Tooltip("If set, this tutorial will show even if tutorials are turned off.")]
		public bool ShowEvenIfDisabled;
	}

	public float AutoCloseCPM = 900f;

	public float AutoCloseBuffer = 4f;

	public TutorialList TutorialList;

	private int[] m_ExclusiveTriggeredTutorials;

	[Persistent]
	private bool[] m_TutorialsPlayed;

	public static TutorialManager Instance { get; private set; }

	public static bool AllowTutorialsNow
	{
		get
		{
			if (!Cutscene.CutsceneActive && !GameState.IsLoading)
			{
				return !UIInterstitialManager.Instance.IsVisible;
			}
			return false;
		}
	}

	public TutorialItem[] Tutorials => TutorialList.Tutorials;

	[Persistent]
	public bool TutorialsAreMinimized { get; set; }

	public float GetAutoCloseTime(string str)
	{
		return AutoCloseBuffer + (float)str.Length / (AutoCloseCPM / 60f);
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'TutorialManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		m_ExclusiveTriggeredTutorials = new int[28];
		for (int i = 0; i < m_ExclusiveTriggeredTutorials.Length; i++)
		{
			m_ExclusiveTriggeredTutorials[i] = -1;
		}
		for (int j = 0; j < Tutorials.Length; j++)
		{
			if (Tutorials[j].ExAutoTrigger != 0)
			{
				m_ExclusiveTriggeredTutorials[(int)Tutorials[j].ExAutoTrigger] = j;
			}
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static bool STriggerTutorial(int index)
	{
		if ((bool)Instance)
		{
			return Instance.TriggerTutorial(index);
		}
		return false;
	}

	public bool TriggerTutorial(int index)
	{
		if (index >= Tutorials.Length || index < 0)
		{
			return false;
		}
		if (Conditionals.s_TestCommandLineArgs.Contains("bb"))
		{
			return false;
		}
		if (!GameState.Option.GetOption(GameOption.BoolOption.SHOW_TUTORIALS) && !Tutorials[index].ShowEvenIfDisabled)
		{
			return false;
		}
		if ((bool)UITutorialBox.Instance && UITutorialBox.Instance.Visible && Tutorials[index].Soft)
		{
			return false;
		}
		if (!AllowTutorialsNow)
		{
			return false;
		}
		if (m_TutorialsPlayed == null)
		{
			m_TutorialsPlayed = new bool[Tutorials.Length];
		}
		else if (m_TutorialsPlayed.Length < Tutorials.Length)
		{
			bool[] array = new bool[Tutorials.Length];
			m_TutorialsPlayed.CopyTo(array, 0);
			m_TutorialsPlayed = array;
		}
		if (!m_TutorialsPlayed[index])
		{
			if ((bool)UITutorialBox.Instance)
			{
				m_TutorialsPlayed[index] = true;
				Console.AddMessage(Tutorials[index].Text.GetText(), Console.ConsoleState.DialogueBig);
				UITutorialBox.Instance.ShowTutorial(index);
				return true;
			}
			Debug.LogError("Could not find UITutorialBox element.");
		}
		return false;
	}

	public static void STriggerTutorialsOfTypeFast(ExclusiveTriggerType type)
	{
		if ((bool)Instance)
		{
			Instance.TriggerTutorialsOfTypeFast(type);
		}
	}

	public static void STriggerTutorialsOfType(TutorialTrigger trigger)
	{
		if ((bool)Instance)
		{
			Instance.TriggerTutorialsOfType(trigger);
		}
	}

	public void TriggerTutorialsOfTypeFast(ExclusiveTriggerType type)
	{
		TriggerTutorial(m_ExclusiveTriggeredTutorials[(int)type]);
	}

	public void TriggerTutorialsOfType(TutorialTrigger trigger)
	{
		bool flag = false;
		for (int i = 0; i < Tutorials.Length; i++)
		{
			if (Tutorials[i].AutoTriggers == null)
			{
				continue;
			}
			for (int j = 0; j < Tutorials[i].AutoTriggers.Length; j++)
			{
				if (Tutorials[i].AutoTriggers[j] == trigger)
				{
					if (flag)
					{
						Debug.LogError("Tutorial Trigger matched multiple items: " + trigger);
						return;
					}
					TriggerTutorial(i);
					flag = true;
				}
			}
		}
	}
}

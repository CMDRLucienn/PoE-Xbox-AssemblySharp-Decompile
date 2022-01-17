using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementTracker : MonoBehaviour
{
	public enum TrackedAchievementStat
	{
		CompletedGame,
		CompletedAct1,
		CompletedAct2,
		CompletedAct3,
		NumBaseGamePrimaryCompanionsGained,
		NumAdventuresCreated,
		NumPartyMemberKnockouts,
		ExpertModeOn,
		TrialOfIronOn,
		PathOfTheDamnedOn,
		NumUniqueEnchantmentsCreated,
		NumUniqueFoodItemsCreated,
		NumUniqueScrollsCreated,
		NumUniquePotionsCreated,
		NumTrapItemsUsed,
		NumRestsUsed,
		NumEnemiesKilled,
		NumStrongholdUpgrades,
		NumBaseGameDragonsKilled,
		NumUniqueBaseGameMapsVisited,
		NumDispositionsAtLevel,
		NumGodsAppeased,
		BackedGame,
		NumLevelsOfOdNua,
		NumUniquePX1MapsVisited,
		NumPX1BountysCompleted,
		NumPX1PrimaryCompanionsGained,
		NumPX1DragonsKilled,
		NumSoulboundWeaponsFullyUnlocked,
		PX1DoorOfDurgansBatteryOpened,
		PX1RestartedWhiteForge,
		PX1CompletedSiegeofCragholdt,
		NumUniqueStrongholdAdventureTypesCompleted,
		DefendedPositionAsStrongholdMaster,
		NumPX2PrimaryCompanionsGained,
		NumWeaponOrShieldsLegendaryEnchanted,
		NumArmorsLegendaryEnchanted,
		PX2DefeatedMenaceOfMowrghekIen,
		PX2ReachedReliquaryInAbbey,
		PX2StoppedThreatSeenInDreams,
		NumPX2DragonsKilled,
		NumArchmagesKilled,
		NumPX2BountiesCompleted,
		Count
	}

	public enum AchievementCheckFlags
	{
		OnValueChange,
		EndGameOnly
	}

	public enum ComparisonOperator
	{
		Equals,
		GreaterThan,
		GreaterThanEqual,
		LessThan,
		LessThanEqual,
		NotEqual
	}

	public enum LogicalOperator
	{
		And,
		Or
	}

	[Serializable]
	public class AchievementCondition
	{
		[Tooltip("Which tracked stat are we comparing the value on?")]
		public TrackedAchievementStat AchievementStat;

		[Tooltip("Which comparison operator are we using on the achievement stat?")]
		public ComparisonOperator CompareOperator;

		[Tooltip("The value you are using to compare against the achievement stat you have selected.")]
		public int CompareValue;
	}

	[Serializable]
	public class Achievement
	{
		[Tooltip("The readable string version of this achievement.")]
		public string AchievementName;

		[Tooltip("The string id of the achievement sent to Steam/Xbox/etc. Must match what those APIs are looking for.")]
		public string AchievementAPIName;

		[Tooltip("When should this achievement be checked to see if it should be awarded?")]
		public AchievementCheckFlags WhenToCheckFlags;

		[Tooltip("If you have more than one condition for this achievement, specify if you want to AND or OR them all together.")]
		public LogicalOperator ConditionLogicalOperator;

		public AchievementCondition[] Conditions;
	}

	public Achievement[] Achievements;

	[Persistent]
	private int[] m_trackedAchievementStatCount = new int[43];

	[Persistent]
	private Dictionary<TrackedAchievementStat, List<string>> m_trackedUniqueValues = new Dictionary<TrackedAchievementStat, List<string>>();

	[Persistent]
	private List<string> m_completedAchievements = new List<string>();

	[Persistent]
	private bool m_disableAchievements;

	private bool mShowDebugInfo;

	public static AchievementTracker Instance { get; private set; }

	public bool DisableAchievements
	{
		get
		{
			return m_disableAchievements;
		}
		set
		{
			if (!m_disableAchievements)
			{
				m_disableAchievements = value;
			}
		}
	}

	public bool ShowDebugInfo
	{
		get
		{
			return mShowDebugInfo;
		}
		set
		{
			if (!value)
			{
				UIDebug.Instance.RemoveText("AchievementDebug");
				UIDebug.Instance.RemoveText("UniqueAchievementDebug");
				UIDebug.Instance.RemoveText("UnlockedAchievementsDebug");
			}
			mShowDebugInfo = value;
		}
	}

	private int[] GetTrackedAchievementStatCountArray()
	{
		if (m_trackedAchievementStatCount == null)
		{
			m_trackedAchievementStatCount = new int[43];
		}
		if (m_trackedAchievementStatCount.Length != 43)
		{
			Array.Resize(ref m_trackedAchievementStatCount, 43);
		}
		return m_trackedAchievementStatCount;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'AchievementTracker' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void PoEFixUpAchievements()
	{
		if ((bool)GlobalVariables.Instance)
		{
			ForceSetTrackedStat(TrackedAchievementStat.NumBaseGameDragonsKilled, ((GlobalVariables.Instance.GetVariable("n_Dragon_State") == 2) ? 1 : 0) + ((GlobalVariables.Instance.GetVariable("n_Nest_dragon_State") == 1) ? 1 : 0));
			ForceSetTrackedStat(TrackedAchievementStat.NumPX2DragonsKilled, (GlobalVariables.Instance.GetVariable("n_Bog_Quest_main") == 6) ? 2 : 0);
			ForceSetTrackedStat(TrackedAchievementStat.NumArchmagesKilled, ((GlobalVariables.Instance.GetVariable("n_Bog_Quest_main") == 6) ? 1 : 0) + ((GlobalVariables.Instance.GetVariable("b_Concelhaut_Dead") >= 1) ? 1 : 0));
		}
		if ((bool)GlobalVariables.Instance && GetAchievementStatValue(TrackedAchievementStat.NumPX1BountysCompleted) == 0)
		{
			int num = 0;
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Ogre_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Xaurip_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Fighter_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Forest_Lurker_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Fampyr_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Troll_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Chanter_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Ranger_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Paladin_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Menpwgra_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Rogue_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Priest_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Wizard_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Cipher_Stage") == 3) ? 1 : 0);
			num += ((GlobalVariables.Instance.GetVariable("n_Boss_Vithrack_Stage") == 3) ? 1 : 0);
			ForceSetTrackedStat(TrackedAchievementStat.NumPX1BountysCompleted, num);
		}
		if ((bool)GlobalVariables.Instance && GetAchievementStatValue(TrackedAchievementStat.NumPX2BountiesCompleted) == 0)
		{
			int num2 = 0;
			num2 += ((GlobalVariables.Instance.GetVariable("n_Boss_Barbarian_Stage") == 3) ? 1 : 0);
			num2 += ((GlobalVariables.Instance.GetVariable("n_Boss_Deadfire_Stage") == 3) ? 1 : 0);
			num2 += ((GlobalVariables.Instance.GetVariable("n_Boss_Lagufaeth_Stage") == 3) ? 1 : 0);
			num2 += ((GlobalVariables.Instance.GetVariable("n_Terror_Stage") == 3) ? 1 : 0);
			ForceSetTrackedStat(TrackedAchievementStat.NumPX2BountiesCompleted, num2);
		}
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		string text = (string)sender;
		if (text.StartsWith("PX1"))
		{
			TrackAndIncrementIfUnique(TrackedAchievementStat.NumUniquePX1MapsVisited, text);
		}
		else
		{
			TrackAndIncrementIfUnique(TrackedAchievementStat.NumUniqueBaseGameMapsVisited, text);
		}
		if (GameState.NumSceneLoads == 0)
		{
			PoEFixUpAchievements();
			ResignalCompletedAchievements();
			CheckAchievementRequirements();
		}
	}

	public string GetAchievementDebugOutput()
	{
		return "-- Achievement Tracker Debug --\n" + GetAchievementDebugStatValues() + "\n" + GetAchievementDebugUniqueValues() + "\n" + GetAchievementDebugUnlockedAchievements();
	}

	private int GetAchievementStatValue(TrackedAchievementStat stat)
	{
		return GetTrackedAchievementStatCountArray()[(int)stat];
	}

	private string GetAchievementDebugStatValues()
	{
		string text = "Achievement Stat Values:\n";
		for (int i = 0; i < 43; i++)
		{
			object[] obj = new object[5] { text, null, null, null, null };
			TrackedAchievementStat trackedAchievementStat = (TrackedAchievementStat)i;
			obj[1] = trackedAchievementStat.ToString();
			obj[2] = ": ";
			obj[3] = GetTrackedAchievementStatCountArray()[i];
			obj[4] = "\n";
			text = string.Concat(obj);
		}
		return text;
	}

	private string GetAchievementDebugUniqueValues()
	{
		string text = "Achievement Unique Values:\n";
		foreach (KeyValuePair<TrackedAchievementStat, List<string>> trackedUniqueValue in m_trackedUniqueValues)
		{
			text = text + trackedUniqueValue.Key.ToString() + ": ";
			int num = 3;
			foreach (string item in trackedUniqueValue.Value)
			{
				text = text + item + ", ";
				num--;
				if (num == 0)
				{
					text += "\n";
					num = 3;
				}
			}
			text += "\n";
		}
		return text;
	}

	private string GetAchievementDebugUnlockedAchievements()
	{
		string text = "Unlocked Achievements:\n";
		foreach (string completedAchievement in m_completedAchievements)
		{
			text = text + completedAchievement + "\n";
		}
		return text;
	}

	private void DisplayDebugInfo()
	{
		UIDebug.Instance.SetText("AchievementDebug", GetAchievementDebugStatValues(), Color.green);
		UIDebug.Instance.SetTextPosition("AchievementDebug", 0.025f, 0.975f, UIWidget.Pivot.TopLeft);
		UIDebug.Instance.SetText("UniqueAchievementDebug", GetAchievementDebugUniqueValues(), Color.green);
		UIDebug.Instance.SetTextPosition("UniqueAchievementDebug", 0.975f, 0.975f, UIWidget.Pivot.TopRight);
		UIDebug.Instance.SetText("UnlockedAchievementsDebug", GetAchievementDebugUnlockedAchievements(), Color.green);
		UIDebug.Instance.SetTextPosition("UnlockedAchievementsDebug", 0.025f, 0.1f, UIWidget.Pivot.BottomLeft);
	}

	private void OnDisable()
	{
		if (ShowDebugInfo && (bool)UIDebug.Instance)
		{
			UIDebug.Instance.RemoveText("AchievementDebug");
			UIDebug.Instance.RemoveText("UniqueAchievementDebug");
			UIDebug.Instance.RemoveText("UnlockedAchievementsDebug");
		}
	}

	private void Update()
	{
		if (ShowDebugInfo)
		{
			DisplayDebugInfo();
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void TrackAndIncrementIfUnique(TrackedAchievementStat stat, string value)
	{
		if (m_trackedUniqueValues.TryGetValue(stat, out var value2))
		{
			if (value2.Contains(value))
			{
				return;
			}
		}
		else
		{
			value2 = new List<string>();
			m_trackedUniqueValues.Add(stat, value2);
		}
		value2.Add(value);
		IncrementTrackedStat(stat);
	}

	public void IncrementTrackedStat(TrackedAchievementStat stat)
	{
		GetTrackedAchievementStatCountArray()[(int)stat]++;
		CheckAchievementRequirements();
		if (stat == TrackedAchievementStat.CompletedGame)
		{
			CheckEndGameAchievementRequirements();
		}
	}

	public void DecrementTrackedStat(TrackedAchievementStat stat)
	{
		GetTrackedAchievementStatCountArray()[(int)stat]--;
		CheckAchievementRequirements();
	}

	public void ForceSetTrackedStat(TrackedAchievementStat stat, int value)
	{
		GetTrackedAchievementStatCountArray()[(int)stat] = value;
		CheckAchievementRequirements();
		if (stat == TrackedAchievementStat.CompletedGame && value > 0)
		{
			CheckEndGameAchievementRequirements();
		}
	}

	private bool EqualityCompare(TrackedAchievementStat stat, ComparisonOperator compareOperator, int value)
	{
		return compareOperator switch
		{
			ComparisonOperator.Equals => GetTrackedAchievementStatCountArray()[(int)stat] == value, 
			ComparisonOperator.GreaterThan => GetTrackedAchievementStatCountArray()[(int)stat] > value, 
			ComparisonOperator.GreaterThanEqual => GetTrackedAchievementStatCountArray()[(int)stat] >= value, 
			ComparisonOperator.LessThan => GetTrackedAchievementStatCountArray()[(int)stat] < value, 
			ComparisonOperator.LessThanEqual => GetTrackedAchievementStatCountArray()[(int)stat] <= value, 
			ComparisonOperator.NotEqual => GetTrackedAchievementStatCountArray()[(int)stat] != value, 
			_ => false, 
		};
	}

	private void CheckAchievement(Achievement achievement)
	{
		if (m_completedAchievements.Contains(achievement.AchievementName))
		{
			return;
		}
		bool flag = achievement.WhenToCheckFlags == AchievementCheckFlags.OnValueChange;
		float num = ((flag && achievement.ConditionLogicalOperator == LogicalOperator.And) ? 1f : 0f);
		bool flag2 = false;
		AchievementCondition[] conditions = achievement.Conditions;
		foreach (AchievementCondition achievementCondition in conditions)
		{
			bool flag3 = EqualityCompare(achievementCondition.AchievementStat, achievementCondition.CompareOperator, achievementCondition.CompareValue);
			flag2 = flag3;
			if (!flag3 && flag)
			{
				float num2 = (float)GetTrackedAchievementStatCountArray()[(int)achievementCondition.AchievementStat] / (float)achievementCondition.CompareValue;
				Debug.LogError("---------GAMEPASS: Update achievement : achievement.AchievementName = " + achievement.AchievementName);
				int num3 = GetTrackedAchievementStatCountArray()[(int)achievementCondition.AchievementStat];
				Debug.LogError("---------GAMEPASS: Update achievement : condition.AchievementStat = " + num3 + " condition.CompareValue: " + achievementCondition.CompareValue);
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				num = ((achievement.ConditionLogicalOperator != 0) ? Mathf.Max(num, num2) : (num * num2));
			}
			if ((flag3 && achievement.ConditionLogicalOperator == LogicalOperator.Or) || (!flag3 && achievement.ConditionLogicalOperator == LogicalOperator.And))
			{
				break;
			}
		}
		if (flag2)
		{
			if (!DisableAchievements)
			{
				SetAchievement(achievement.AchievementAPIName);
			}
			m_completedAchievements.Add(achievement.AchievementName);
		}
		else if (flag && num > 0f)
		{
			uint num4 = (uint)(num * 100f);
			if (num4 > 100)
			{
				num4 = 100u;
			}
			if (!DisableAchievements)
			{
				SetAchievementProgress(achievement.AchievementAPIName, num4);
			}
		}
	}

	public void CheckEndGameAchievementRequirements()
	{
		Achievement[] achievements = Achievements;
		foreach (Achievement achievement in achievements)
		{
			if (achievement.WhenToCheckFlags == AchievementCheckFlags.EndGameOnly)
			{
				CheckAchievement(achievement);
			}
		}
	}

	private void CheckAchievementRequirements()
	{
		Achievement[] achievements = Achievements;
		foreach (Achievement achievement in achievements)
		{
			if (achievement.WhenToCheckFlags != AchievementCheckFlags.EndGameOnly)
			{
				CheckAchievement(achievement);
			}
		}
	}

	private void ResignalCompletedAchievements()
	{
		Achievement[] achievements = Achievements;
		foreach (Achievement achievement in achievements)
		{
			if (m_completedAchievements.Contains(achievement.AchievementName) && !DisableAchievements)
			{
				SetAchievement(achievement.AchievementAPIName);
			}
		}
	}

	public void SetAchievement(string achievementTag)
	{
		GamePassManager.SetAchievement(achievementTag);
	}

	public void ClearAchievement(string achievementTag)
	{
		GamePassManager.ClearAchievement(achievementTag);
	}

	public void SetAchievementProgress(string achievementTag, uint currentProgress)
	{
		GamePassManager.SetAchievementProgress(achievementTag, currentProgress);
	}
}

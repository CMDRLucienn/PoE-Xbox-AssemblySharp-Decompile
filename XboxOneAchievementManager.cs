using System.Collections.Generic;
using UnityEngine;
using XGamingRuntime;

public class XboxOneAchievementManager : IAchievementHandler
{
	private static readonly Dictionary<string, string> m_NameToIdentifier = new Dictionary<string, string>
	{
		{ "Completed Act I", "pe-completed-act-i" },
		{ "Completed Act II", "pe-completed-act-ii" },
		{ "Completed Act III", "pe-completed-act-iii" },
		{ "Won the Game!!!", "pe-won-the-game" },
		{ "The Watcher With Eight Friends", "pe-got-all-the-companions" },
		{ "Enchanter", "pe-enchanter" },
		{ "Chef", "pe-chef" },
		{ "Scribe", "pe-scribe" },
		{ "Alchemist", "pe-alchemist" },
		{ "Trappy", "pe-trappy" },
		{ "Relative Pacifism", "pe-relative-pacifism" },
		{ "Super Murderer", "pe-super-murderer" },
		{ "5 Upgrades in Stronghold", "pe-5-upgrades-in-stronghold" },
		{ "All Upgrades in Stronghold", "pe-all-upgrades-in-stronghold" },
		{ "From the Clouds to the Depths", "pe-kill-all-the-dragons" },
		{ "Appease All of the Gods", "pe-appease-all-of-the-gods" },
		{ "Explorer", "pe-explorer" },
		{ "Disposition", "pe-disposition" },
		{ "The Storied Adventurer", "pe-the-storied-adventurer" },
		{ "The Heir of Caed Nua", "pe-the-heir-of-caed-nua" },
		{ "Herald of the Old Flame", "px1-herald-of-the-old-flame" },
		{ "Terror of the White March", "px1-terror-of-the-white-march" },
		{ "The Siege of Crägholdt", "px1-the-siege-of-cragholdt" },
		{ "Among the Moss and Peat", "px2-among-the-moss-and-peat" },
		{ "A Voice from the Deep", "px2-a-voice-from-the-deep" },
		{ "Called to their Labor", "px2-called-to-their-labor" }
	};

	private Dictionary<string, string> m_NameToId = new Dictionary<string, string>();

	private Dictionary<string, string> m_Achievements = new Dictionary<string, string>();

	private static bool m_Initialized = false;

	public void Update()
	{
	}

	public bool IsInitialized()
	{
		return m_Initialized;
	}

	public void Initialize()
	{
		Debug.Log("---------GAMEPASS: XboxOneAchievementManager Initialize");
		m_Initialized = true;
		FetchAchievements();
	}

	public void FetchAchievements()
	{
		SDK.XBL.XblAchievementsGetAchievementsForTitleIdAsync(GamePassManager.Instance.ContextHandle, GamePassManager.Instance.UserID, GamePassManager.Instance.TitleId, XblAchievementType.All, unlockedOnly: false, XblAchievementOrderBy.DefaultOrder, 0u, 10000u, delegate(int hresult, XblAchievementsResultHandle result)
		{
			GamePassManager.LogHR("---------GAMEPASS: Fetching achievements completed", hresult);
			if (hresult >= 0)
			{
				XblAchievement[] achievements;
				int num = SDK.XBL.XblAchievementsResultGetAchievements(result, out achievements);
				GamePassManager.LogHR("---------GAMEPASS: Extracted achievements result :", num);
				if (num >= 0)
				{
					Debug.Log($"---------GAMEPASS: {achievements.Length} achievements extracted");
					XblAchievement[] array = achievements;
					foreach (XblAchievement xblAchievement in array)
					{
						if (m_NameToIdentifier.TryGetValue(xblAchievement.Name, out var value))
						{
							if (xblAchievement.ProgressState == XblAchievementProgressState.Achieved)
							{
								m_Achievements.Add(value, xblAchievement.Id);
							}
							m_NameToId.Add(value, xblAchievement.Id);
						}
						else
						{
							Debug.LogError("---------GAMEPASS: fetched achievement " + xblAchievement.Name + " doesn't exist");
						}
					}
				}
			}
		});
	}

	public void Reinitialize()
	{
		m_Initialized = false;
		Initialize();
	}

	public bool AchievementsAvailable()
	{
		return IsInitialized();
	}

	public void UnlockAllAchievements()
	{
		foreach (string value in m_NameToId.Values)
		{
			AwardAchievement(value);
		}
	}

	public void ResetAchievementUnlocks()
	{
		Debug.LogError("---------GAMEPASS: XboxOneAchievementManager.ResetAchievementUnlocks : It is not possible to reset achievements on Game Pass");
	}

	public bool CanAwardAchievement(string achievementKey)
	{
		if (AchievementsAvailable() && !IsAchievementUnlocked(achievementKey))
		{
			return AchievementExists(achievementKey);
		}
		return false;
	}

	public bool IsAchievementUnlocked(string achievementKey)
	{
		return m_Achievements.ContainsKey(achievementKey);
	}

	public bool AchievementExists(string achievementKey)
	{
		foreach (string key in m_NameToId.Keys)
		{
			if (key == achievementKey)
			{
				return true;
			}
		}
		return false;
	}

	public void AwardAchievement(string achievementKey)
	{
		Debug.Log("---------GAMEPASS: Award achievement : name = " + achievementKey);
		if (CanAwardAchievement(achievementKey))
		{
			m_NameToId.TryGetValue(achievementKey, out var value);
			SDK.XBL.XblAchievementsUpdateAchievementAsync(GamePassManager.Instance.ContextHandle, GamePassManager.Instance.UserID, GamePassManager.Instance.TitleId, GamePassManager.Instance.PrimaryServiceConfigID, value, 100u, delegate(int achievementUpdateResult)
			{
				Debug.Log("---------GAMEPASS: AwardAchievement: Get achievements completed : hr = " + achievementUpdateResult.ToString("X8"));
				if (achievementUpdateResult >= 0)
				{
					if (m_NameToId.TryGetValue(achievementKey, out var value2))
					{
						Debug.Log("---------GAMEPASS: Got achievements \"" + achievementKey + "\" unlocked");
						m_Achievements.Add(achievementKey, value2);
					}
					else
					{
						Debug.LogError("-------- - GAMEPASS: Got achievement \"" + achievementKey + "\" from xbox services but no relevant identifier was mapped to that name");
					}
				}
			});
		}
		else if (!AchievementsAvailable())
		{
			Debug.LogError("---------GAMEPASS: AwardAchievement : Achievements are not available when trying to unlock: " + achievementKey);
		}
		else if (IsAchievementUnlocked(achievementKey))
		{
			Debug.Log("---------GAMEPASS: AwardAchievement : Achievement " + achievementKey + " was triggered but it is already unlocked");
		}
		else if (AchievementExists(achievementKey))
		{
			Debug.LogError("---------GAMEPASS: AwardAchievement : Could not unlock achievement that exists: " + achievementKey);
		}
		else
		{
			Debug.LogWarning("---------GAMEPASS: AwardAchievement : Cannot award achievement: " + achievementKey + " because it doesn't exist!");
		}
	}

	public void UpdateAchievement(string achievementKey, uint currentProgress)
	{
		Debug.LogError("---------GAMEPASS: Update achievement : name = " + achievementKey + " progress: " + currentProgress);
		if (CanAwardAchievement(achievementKey))
		{
			m_NameToId.TryGetValue(achievementKey, out var value);
			SDK.XBL.XblAchievementsUpdateAchievementAsync(GamePassManager.Instance.ContextHandle, GamePassManager.Instance.UserID, GamePassManager.Instance.TitleId, GamePassManager.Instance.PrimaryServiceConfigID, value, currentProgress, delegate(int achievementUpdateResult)
			{
				Debug.LogError("---------GAMEPASS: UpdateAchievement: Get achievements completed : hr = " + achievementUpdateResult.ToString("X8"));
				if (achievementUpdateResult >= 0)
				{
					if (m_NameToId.TryGetValue(achievementKey, out var _))
					{
						Debug.LogError("---------GAMEPASS: UpdateAchievement: Got achievements \"" + achievementKey + "\" updated");
					}
					else
					{
						Debug.LogError("-------- - GAMEPASS: UpdateAchievement: Got achievement \"" + achievementKey + "\" from xbox services but no relevant identifier was mapped to that name");
					}
				}
			});
		}
		else if (!AchievementsAvailable())
		{
			Debug.LogError("---------GAMEPASS: UpdateAchievement : Achievements are not available when trying to update: " + achievementKey);
		}
		else if (IsAchievementUnlocked(achievementKey))
		{
			Debug.Log("---------GAMEPASS: UpdateAchievement : Achievement " + achievementKey + " was updated but it is already unlocked");
		}
		else if (AchievementExists(achievementKey))
		{
			Debug.LogError("---------GAMEPASS: UpdateAchievement : Could not update achievement that exists: " + achievementKey);
		}
		else
		{
			Debug.LogWarning("---------GAMEPASS: UpdateAchievement : Cannot update achievement: " + achievementKey + " because it doesn't exist!");
		}
	}
}

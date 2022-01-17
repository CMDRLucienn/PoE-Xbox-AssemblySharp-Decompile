using UnityEngine;
using XGamingRuntime;

internal class XboxOneHeroStatsWrapper
{
	private float m_TotalDamageDone;

	private float m_MaxSingleTargetDamage;

	private float m_DamageTaken;

	private int m_CriticalHits;

	private int m_Hits;

	private int m_EnemiesDefeated;

	public void Initialize()
	{
	}

	public void UpdateStat(string eventName, string statName, string value)
	{
		int num = SDK.XBL.XblEventsWriteInGameEvent(GamePassManager.Instance.ContextHandle, eventName, "{}", "{\"Value\":" + value + ", \"HeroStatIdentifier\":\"" + statName + "\"}");
		Debug.Log("GAMEPASS: UpdateIntStat: " + statName + " with " + value + " XblEventsWriteInGameEvent " + num.ToString("X8"));
	}

	public void Update()
	{
		bool flag = false;
		if (RecordKeeper.Instance == null)
		{
			return;
		}
		PartyMemberStats[] allPeopleStats = RecordKeeper.Instance.GetAllPeopleStats();
		if (allPeopleStats == null)
		{
			return;
		}
		for (int i = 0; i < allPeopleStats.Length; i++)
		{
			if (allPeopleStats[i].TotalDamageDone > m_TotalDamageDone)
			{
				flag = true;
				m_TotalDamageDone = allPeopleStats[i].TotalDamageDone;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateFloatHeroStat", "most-damage-done", m_TotalDamageDone.ToString());
		}
		flag = false;
		for (int j = 0; j < allPeopleStats.Length; j++)
		{
			if (allPeopleStats[j].MaxSingleTargetDamage > m_MaxSingleTargetDamage)
			{
				flag = true;
				m_MaxSingleTargetDamage = allPeopleStats[j].MaxSingleTargetDamage;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateFloatHeroStat", "highest-single-target-damage", m_MaxSingleTargetDamage.ToString());
		}
		flag = false;
		for (int k = 0; k < allPeopleStats.Length; k++)
		{
			if (allPeopleStats[k].DamageTaken > m_DamageTaken)
			{
				flag = true;
				m_DamageTaken = allPeopleStats[k].DamageTaken;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateFloatHeroStat", "most-damage-taken", m_DamageTaken.ToString());
		}
		flag = false;
		for (int l = 0; l < allPeopleStats.Length; l++)
		{
			if (allPeopleStats[l].CriticalHits > m_CriticalHits)
			{
				flag = true;
				m_CriticalHits = allPeopleStats[l].CriticalHits;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateIntHeroStat", "critical-hits", m_CriticalHits.ToString());
		}
		flag = false;
		for (int m = 0; m < allPeopleStats.Length; m++)
		{
			if (allPeopleStats[m].TotalHits > m_Hits)
			{
				flag = true;
				m_Hits = allPeopleStats[m].TotalHits;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateIntHeroStat", "hits", m_Hits.ToString());
		}
		flag = false;
		for (int n = 0; n < allPeopleStats.Length; n++)
		{
			if (allPeopleStats[n].EnemiesDefeated > m_EnemiesDefeated)
			{
				flag = true;
				m_EnemiesDefeated = allPeopleStats[n].EnemiesDefeated;
			}
		}
		if (flag)
		{
			UpdateStat("UpdateIntHeroStat", "enemies-defeated", m_EnemiesDefeated.ToString());
		}
		flag = false;
	}
}

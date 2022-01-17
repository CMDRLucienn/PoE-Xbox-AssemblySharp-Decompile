using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RecordKeeper : MonoBehaviour
{
	private PartyMemberStats[] m_AllPeopleStats;

	private PartyMemberStats[] m_AllCompanionsStats;

	public static RecordKeeper Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'RecordKeeper' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	public PartyMemberStats[] GetAllPeopleStats()
	{
		return m_AllPeopleStats;
	}

	private void Start()
	{
		FindCompanions();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void FindCompanions()
	{
		IEnumerable<PartyMemberAI> source = PartyMemberAI.OnlyPrimaryPartyMembers.Where((PartyMemberAI pai) => !pai.GetComponent<Player>());
		m_AllPeopleStats = (from pai in PartyMemberAI.OnlyPrimaryPartyMembers
			select pai.GetComponent<PartyMemberStats>() into s
			where s
			select s).ToArray();
		m_AllCompanionsStats = (from pai in source
			select pai.GetComponent<PartyMemberStats>() into s
			where s
			select s).ToArray();
	}

	public string GetStatValue(RecordAggregator.PersonalStat stat, PartyMemberAI selected)
	{
		PartyMemberStats component = selected.GetComponent<PartyMemberStats>();
		if (!component)
		{
			return "";
		}
		switch (stat)
		{
		case RecordAggregator.PersonalStat.TIME_IN_PARTY:
			return component.TimeInParty.ToString();
		case RecordAggregator.PersonalStat.PERSONAL_TIME_IN_COMBAT:
			return component.TimeInCombat.ToString();
		case RecordAggregator.PersonalStat.TOTAL_ENEMIES_DEFEATED:
			return component.EnemiesDefeated.ToString();
		case RecordAggregator.PersonalStat.MOST_POWERFUL_ENEMY_DEFEATED:
			if (component.MostPowerfulEnemyName == null)
			{
				return GUIUtils.GetText(343);
			}
			return component.MostPowerfulEnemyName.GetText(component.MostPowerfulEnemyGender);
		case RecordAggregator.PersonalStat.TOTAL_DAMAGE_DONE:
			return component.TotalDamageDone.ToString("#0.0");
		case RecordAggregator.PersonalStat.HIGHEST_SINGLE_TARGET_DAMAGE_HIT:
			return component.MaxSingleTargetDamage.ToString("#0.0");
		case RecordAggregator.PersonalStat.TOTAL_CRITICAL_HITS:
			return component.CriticalHits.ToString();
		case RecordAggregator.PersonalStat.TOTAL_HITS:
			return component.TotalHits.ToString();
		case RecordAggregator.PersonalStat.TOTAL_DAMAGE_TAKEN:
			return component.DamageTaken.ToString("#0.0");
		case RecordAggregator.PersonalStat.TOTAL_TIMES_KNOCKED_OUT:
			return component.TimesKOed.ToString();
		default:
			return "";
		}
	}

	public string GetStatValue(RecordAggregator.PartyStat stat)
	{
		return stat switch
		{
			RecordAggregator.PartyStat.TOTAL_ENEMIES_DEFEATED => BestiaryManager.Instance.GetTotalKills().ToString(), 
			RecordAggregator.PartyStat.MOST_POWERFUL_ENEMY_DEFEATED => StringOverMultipleValues((PartyMemberStats m) => m.MostPowerfulEnemyLevel, from pm in m_AllPeopleStats
				where pm.MostPowerfulEnemyLevel > 0
				orderby pm.MostPowerfulEnemyLevel descending
				select pm, (PartyMemberStats m) => m.MostPowerfulEnemyName.GetText(m.MostPowerfulEnemyGender), 0), 
			RecordAggregator.PartyStat.MOST_TIME_IN_PARTY => StringOverFirstValue((PartyMemberStats m) => m.TimeInParty, m_AllCompanionsStats.OrderByDescending((PartyMemberStats pm) => pm.TimeInParty), (EternityTimeInterval m) => m.ToString(), new EternityTimeInterval(), allowTies: false), 
			RecordAggregator.PartyStat.MOST_ENEMIES_DEFEATED => StringOverFirstValue((PartyMemberStats m) => m.EnemiesDefeated, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.EnemiesDefeated), (int m) => m.ToString(), 0), 
			RecordAggregator.PartyStat.MOST_TOTAL_DAMAGE_DONE => StringOverFirstValue((PartyMemberStats m) => m.TotalDamageDone, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.TotalDamageDone), (float m) => m.ToString("#0"), 0f), 
			RecordAggregator.PartyStat.HIGHEST_SINGLE_TARGET_DAMAGE_HIT => StringOverFirstValue((PartyMemberStats m) => m.MaxSingleTargetDamage, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.MaxSingleTargetDamage), (float m) => m.ToString("#0"), 0f), 
			RecordAggregator.PartyStat.MOST_CRITICAL_HITS => StringOverFirstValue((PartyMemberStats m) => m.CriticalHits, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.CriticalHits), (int m) => m.ToString(), 0), 
			RecordAggregator.PartyStat.MOST_HITS => StringOverFirstValue((PartyMemberStats m) => m.TotalHits, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.TotalHits), (int m) => m.ToString(), 0), 
			RecordAggregator.PartyStat.MOST_DAMAGE_TAKEN => StringOverFirstValue((PartyMemberStats m) => m.DamageTaken, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.DamageTaken), (float m) => m.ToString("#0"), 0f), 
			RecordAggregator.PartyStat.MOST_TIMES_KNOCKED_OUT => StringOverFirstValue((PartyMemberStats m) => m.TimesKOed, m_AllPeopleStats.OrderByDescending((PartyMemberStats pm) => pm.TimesKOed), (int m) => m.ToString(), 0), 
			_ => "", 
		};
	}

	private string StringOverFirstValue<T>(Func<PartyMemberStats, T> value, IEnumerable<PartyMemberStats> from, Func<T, string> valueTostring, T nullVal)
	{
		return StringOverFirstValue(value, from, valueTostring, nullVal, allowTies: true);
	}

	private string StringOverFirstValue<T>(Func<PartyMemberStats, T> value, IEnumerable<PartyMemberStats> from, Func<T, string> valueTostring, T nullVal, bool allowTies)
	{
		if (!from.Any())
		{
			return GUIUtils.GetText(343);
		}
		T rval = value(from.First());
		IEnumerable<PartyMemberStats> enumerable = from.Where((PartyMemberStats m) => value(m).Equals(rval));
		if (!enumerable.Any() || rval.Equals(nullVal) || (!allowTies && enumerable.Count() > 1))
		{
			return GUIUtils.GetText(343);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PartyMemberStats item in enumerable)
		{
			stringBuilder.Append(CharacterStats.Name(item.gameObject));
			stringBuilder.Append(GUIUtils.Comma());
		}
		if (stringBuilder.Length >= GUIUtils.Comma().Length)
		{
			stringBuilder.Remove(stringBuilder.Length - GUIUtils.Comma().Length);
		}
		stringBuilder.AppendGuiFormat(1731, valueTostring(rval));
		return stringBuilder.ToString();
	}

	private string StringOverMultipleValues<T>(Func<PartyMemberStats, T> value, IEnumerable<PartyMemberStats> from, Func<PartyMemberStats, string> getstring, T nullVal)
	{
		if (!from.Any())
		{
			return GUIUtils.GetText(343);
		}
		T rval = value(from.First());
		IEnumerable<PartyMemberStats> enumerable = from.Where((PartyMemberStats m) => value(m).Equals(rval));
		if (!enumerable.Any() || rval.Equals(nullVal))
		{
			return GUIUtils.GetText(343);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PartyMemberStats item in enumerable)
		{
			stringBuilder.Append(CharacterStats.Name(item));
			stringBuilder.AppendGuiFormat(1731, getstring(item));
			stringBuilder.Append(GUIUtils.Comma());
		}
		if (stringBuilder.Length >= GUIUtils.Comma().Length)
		{
			stringBuilder.Remove(stringBuilder.Length - GUIUtils.Comma().Length);
		}
		return stringBuilder.ToString();
	}
}

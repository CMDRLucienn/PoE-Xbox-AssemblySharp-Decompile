using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Team : ScriptableObject, IComparer<Team>, IComparable<Team>
{
	public static int PlayerTeamHash = Animator.StringToHash("player");

	protected static Dictionary<string, Team> s_loadedTeams = new Dictionary<string, Team>();

	[FormerlySerializedAs("ScriptTag")]
	public string m_scriptTag;

	public Faction.Relationship DefaultRelationship;

	public Team[] hostileList;

	public Team[] neutralList;

	public Team[] friendlyList;

	public FactionName GameFaction;

	public Reputation.ChangeStrength InjuredReputationChange;

	public Reputation.ChangeStrength MurderedReputationChange;

	private int m_scriptTagHash;

	private bool m_isRestored;

	public HashSet<Team> HostileTeamSet = new HashSet<Team>();

	public HashSet<Team> NeutralTeamSet = new HashSet<Team>();

	public HashSet<Team> FriendlyTeamSet = new HashSet<Team>();

	public string ScriptTag
	{
		get
		{
			return m_scriptTag;
		}
		set
		{
			m_scriptTag = value.ToLower();
			m_scriptTagHash = Animator.StringToHash(m_scriptTag);
		}
	}

	public int ScriptTagHash
	{
		get
		{
			if (m_scriptTagHash == 0)
			{
				if (string.IsNullOrEmpty(ScriptTag))
				{
					ScriptTag = base.name.ToLower();
				}
				else
				{
					ScriptTag = ScriptTag.ToLower();
				}
				m_scriptTagHash = Animator.StringToHash(ScriptTag);
				if (m_scriptTagHash == 0)
				{
					Debug.LogError("Calculated hash for Script Tag " + ScriptTag + " is 0. Please use a different Script Tag (even if it's only one character different).");
				}
			}
			return m_scriptTagHash;
		}
	}

	public bool RestoredTeam
	{
		get
		{
			return true;
		}
		set
		{
			m_isRestored = value;
		}
	}

	public string Tag
	{
		get
		{
			return ScriptTag;
		}
		set
		{
			ScriptTag = value;
		}
	}

	[Obsolete("Use HostileTeamSet instead.")]
	public List<Team> HostileTeams
	{
		get
		{
			return HostileTeamSet.ToList();
		}
		set
		{
			HostileTeamSet.AddRange(value);
		}
	}

	[Obsolete("Use NeutralTeamSet instead.")]
	public List<Team> NeutralTeams
	{
		get
		{
			return NeutralTeamSet.ToList();
		}
		set
		{
			NeutralTeamSet.AddRange(value);
		}
	}

	[Obsolete("Use FriendlyTeamSet instead.")]
	public List<Team> FriendlyTeams
	{
		get
		{
			return FriendlyTeamSet.ToList();
		}
		set
		{
			FriendlyTeamSet.AddRange(value);
		}
	}

	public static Team Create()
	{
		Team team = Resources.Load<Team>("Data/Lists/Empty");
		if (team == null)
		{
			Debug.LogError("No empty.asset team prefab found at resources/data/lists!");
			return null;
		}
		return UnityEngine.Object.Instantiate(team);
	}

	public static void RemoveAllTeams()
	{
		s_loadedTeams.Clear();
	}

	private void CleanUpNullEntries(HashSet<Team> list)
	{
	}

	public Team Register()
	{
		if (string.IsNullOrEmpty(ScriptTag))
		{
			ScriptTag = base.name;
		}
		ScriptTag = ScriptTag.ToLower();
		Team team = null;
		if (s_loadedTeams.ContainsKey(ScriptTag))
		{
			team = s_loadedTeams[ScriptTag];
		}
		else
		{
			team = UnityEngine.Object.Instantiate(this);
			if (hostileList != null)
			{
				team.HostileTeamSet.AddRange(hostileList);
				CleanUpNullEntries(team.HostileTeamSet);
			}
			if (neutralList != null)
			{
				team.NeutralTeamSet.AddRange(neutralList);
				CleanUpNullEntries(team.NeutralTeamSet);
			}
			if (friendlyList != null)
			{
				team.FriendlyTeamSet.AddRange(friendlyList);
				CleanUpNullEntries(team.FriendlyTeamSet);
			}
			s_loadedTeams.Add(ScriptTag, team);
		}
		if (m_isRestored && GameState.IsRestoredLevel && team != this)
		{
			team.HostileTeamSet.Clear();
			team.NeutralTeamSet.Clear();
			team.FriendlyTeamSet.Clear();
			team.HostileTeamSet.AddRange(HostileTeamSet);
			team.NeutralTeamSet.AddRange(NeutralTeamSet);
			team.FriendlyTeamSet.AddRange(FriendlyTeamSet);
			CleanUpNullEntries(team.HostileTeamSet);
			CleanUpNullEntries(team.NeutralTeamSet);
			CleanUpNullEntries(team.FriendlyTeamSet);
		}
		return team;
	}

	public Faction.Relationship GetRelationship(Team other)
	{
		if (other == this)
		{
			return Faction.Relationship.Friendly;
		}
		if (other == null)
		{
			return DefaultRelationship;
		}
		if (HostileTeamSet.Contains(other))
		{
			return Faction.Relationship.Hostile;
		}
		if (FriendlyTeamSet.Contains(other))
		{
			return Faction.Relationship.Friendly;
		}
		if (NeutralTeamSet.Contains(other))
		{
			return Faction.Relationship.Neutral;
		}
		return DefaultRelationship;
	}

	public void SetRelationship(Team other, Faction.Relationship newRelation, bool mutual)
	{
		switch (GetRelationship(other))
		{
		case Faction.Relationship.Hostile:
			HostileTeamSet.Remove(other);
			CleanUpNullEntries(HostileTeamSet);
			break;
		case Faction.Relationship.Neutral:
			NeutralTeamSet.Remove(other);
			CleanUpNullEntries(NeutralTeamSet);
			break;
		case Faction.Relationship.Friendly:
			FriendlyTeamSet.Remove(other);
			CleanUpNullEntries(FriendlyTeamSet);
			break;
		}
		switch (newRelation)
		{
		case Faction.Relationship.Hostile:
			HostileTeamSet.Add(other);
			CleanUpNullEntries(HostileTeamSet);
			break;
		case Faction.Relationship.Neutral:
			NeutralTeamSet.Add(other);
			CleanUpNullEntries(NeutralTeamSet);
			break;
		case Faction.Relationship.Friendly:
			FriendlyTeamSet.Add(other);
			CleanUpNullEntries(FriendlyTeamSet);
			break;
		}
		if (mutual && other != null)
		{
			other.SetRelationship(this, newRelation, mutual: false);
		}
	}

	public static Team GetTeamByTag(string tag)
	{
		if (!s_loadedTeams.ContainsKey(tag.ToLower()))
		{
			return null;
		}
		return s_loadedTeams[tag.ToLower()];
	}

	public int Compare(Team x, Team y)
	{
		return x.ScriptTagHash.CompareTo(y.ScriptTagHash);
	}

	int IComparable<Team>.CompareTo(Team x)
	{
		return x.ScriptTagHash.CompareTo(ScriptTagHash);
	}

	public override bool Equals(object o)
	{
		Team team = o as Team;
		if ((bool)team)
		{
			return ScriptTagHash == team.ScriptTagHash;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ScriptTagHash;
	}
}

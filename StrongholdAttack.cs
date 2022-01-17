using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class StrongholdAttack
{
	public string Tag = string.Empty;

	public DatabaseString Description = new GUIDatabaseString();

	public DatabaseString Name = new GUIDatabaseString();

	public float Weight = 1f;

	public int MinPlayerLevel = 1;

	public int MaxPlayerLevel = 20;

	public int MinSecurity;

	public int MaxSecurity = 9999;

	public int MinPrestige;

	public int MaxPrestige = 9999;

	[GlobalVariableString]
	[Tooltip("If set, this variable must be set to non-zero or the attack will not be picked.")]
	public string PrerequisiteGlobalVariableName;

	public FactionName FactionID;

	public int MinNegativeFactionRank;

	public MapType LinkedScene;

	public StartPoint.PointLocation StartPoint;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 before the map is loaded (so the encounter can be triggered).")]
	public string EncounterGlobalVariableName;

	[GlobalVariableString]
	[Tooltip("This global is set to 1 when the attack is scheduled to occur.")]
	public string ScheduledGlobalVariableName;

	public bool IsValid(Player player, Stronghold stronghold)
	{
		int security = stronghold.GetSecurity();
		int prestige = stronghold.GetPrestige();
		if (security < MinSecurity)
		{
			return false;
		}
		if (security > MaxSecurity)
		{
			return false;
		}
		if (prestige < MinPrestige)
		{
			return false;
		}
		if (prestige > MaxPrestige)
		{
			return false;
		}
		CharacterStats component = player.GetComponent<CharacterStats>();
		if (component == null || component.Level < MinPlayerLevel || component.Level > MaxPlayerLevel)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(PrerequisiteGlobalVariableName) && GlobalVariables.Instance.GetVariable(PrerequisiteGlobalVariableName) == 0)
		{
			return false;
		}
		if (FactionID != 0)
		{
			Reputation reputation = ReputationManager.Instance.GetReputation(FactionID);
			if (reputation != null && reputation.BadRank < MinNegativeFactionRank)
			{
				return false;
			}
		}
		return true;
	}

	public void AutoResolve(Stronghold stronghold)
	{
		int num = OEIRandom.Range(-5, 5);
		int num2 = OEIRandom.Range(1, 6) + num + 2;
		if (num2 <= 0)
		{
			stronghold.LogTimeEvent(Stronghold.Format(13, Name.GetText()).CapitalizeFirst(), Stronghold.NotificationType.Negative);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(Stronghold.Format(11, Name.GetText()).CapitalizeFirst());
		SortedList<int, List<Stronghold.Damageables>> sortedList = stronghold.GatherDamageables(num2);
		while (num2 > 0 && sortedList != null && sortedList.Count > 0)
		{
			for (int i = 0; i < sortedList.Count; i++)
			{
				int num3 = sortedList.Keys[i];
				if (num3 > num2)
				{
					sortedList.Clear();
					break;
				}
				bool flag = false;
				if (i + 1 < sortedList.Count && sortedList.Keys[i + 1] <= num2)
				{
					flag = true;
				}
				List<Stronghold.Damageables> list = sortedList.Values[i];
				int num4 = list.Count;
				if (flag)
				{
					num4++;
				}
				int num5 = OEIRandom.Index(num4);
				if (!flag || num5 != num4 - 1)
				{
					Stronghold.Damageables damageables = list[num5];
					if (damageables.isUpgrade)
					{
						stronghold.DestroyUpgrade(damageables.upgrade.UpgradeType);
						stringBuilder.Append(" " + Stronghold.Format(14, damageables.upgrade.Name.GetText()));
					}
					else
					{
						stronghold.DismissHireling(damageables.hireling);
						stringBuilder.Append(" " + StrongholdUtils.Format(CharacterStats.GetGender(damageables.hireling.HirelingPrefab), 15, damageables.hireling.Name));
					}
					num2 -= num3;
					list.RemoveAt(num5);
					if (list.Count == 0)
					{
						sortedList.RemoveAt(i);
					}
					break;
				}
			}
		}
		if (num2 > 0)
		{
			int num6 = 100 * num2;
			stronghold.Debt += num6;
			stringBuilder.Append(" " + Stronghold.Format(16, GUIUtils.Format(466, num6)));
		}
		stronghold.FlipTiles();
		stronghold.LogTimeEvent(stringBuilder.ToString(), Stronghold.NotificationType.Negative);
	}

	public void ManualResolve(Stronghold stronghold)
	{
		if (LinkedScene == MapType.Map)
		{
			Debug.LogError("StrongholdAttack has no linked scene set. The attack could not be launched.");
		}
		else if (!string.IsNullOrEmpty(EncounterGlobalVariableName))
		{
			GlobalVariables.Instance.SetVariable(EncounterGlobalVariableName, 1);
			GlobalVariables.Instance.SetVariable(stronghold.AttackGlobalVariableName, 1);
			GameState.s_playerCharacter.StartPointLink = StartPoint;
			GameState.LoadedGame = false;
			GameState.ChangeLevel(LinkedScene);
		}
	}
}

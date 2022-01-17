using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Abilities/SummonRandom")]
public class SummonRandom : Summon
{
	public int MaxSummons = 1;

	private void PerformOneSummoning(Vector3 location, Faction ownerFaction)
	{
		if (SummonList.Count > 0)
		{
			List<CharacterStats> list = new List<CharacterStats>();
			foreach (CharacterStats summon in SummonList)
			{
				if (summon != null)
				{
					list.Add(summon);
				}
			}
			if (list.Count > 0)
			{
				int index = OEIRandom.Index(list.Count);
				SummonCreature(list[index], location, ownerFaction);
				return;
			}
		}
		if (SummonFileList.Count <= 0)
		{
			return;
		}
		List<string> list2 = new List<string>();
		foreach (string summonFile in SummonFileList)
		{
			if (!string.IsNullOrEmpty(summonFile))
			{
				list2.Add(summonFile);
			}
		}
		int index2 = OEIRandom.Index(list2.Count);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(list2[index2]);
		if (GameResources.IsPrefabLoaded(fileNameWithoutExtension))
		{
			CharacterStats characterStats = GameResources.LoadPrefab<CharacterStats>(fileNameWithoutExtension, instantiate: false);
			if (!(characterStats == null))
			{
				SummonCreature(characterStats, location, ownerFaction);
			}
			return;
		}
		AssetBundleRequest assetBundleRequest = GameResources.LoadPrefabAsync<CharacterStats>(fileNameWithoutExtension);
		if (assetBundleRequest != null)
		{
			SummonAsyncLoadRequest item = new SummonAsyncLoadRequest(assetBundleRequest, fileNameWithoutExtension, location, ownerFaction);
			m_summonAsyncLoadRequests.Add(item);
		}
	}

	protected override void PerformSummoning(Vector3 location, Faction ownerFaction)
	{
		if (MaxSummons <= 0)
		{
			return;
		}
		if (MaxSummons == 1)
		{
			PerformOneSummoning(location, ownerFaction);
			return;
		}
		int num = 0;
		if (SummonList.Count > 0)
		{
			IList<CharacterStats> list = SummonList.Shuffle();
			for (int i = 0; i < list.Count && i < MaxSummons; i++)
			{
				if (list[i] != null)
				{
					SummonCreature(list[i], location, ownerFaction);
					num++;
				}
			}
		}
		else if (SummonFileList.Count > 0)
		{
			IList<string> list2 = SummonFileList.Shuffle();
			for (int j = 0; j < list2.Count && j < MaxSummons; j++)
			{
				if (string.IsNullOrEmpty(list2[j]))
				{
					continue;
				}
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(list2[j]);
				if (GameResources.IsPrefabLoaded(fileNameWithoutExtension))
				{
					CharacterStats characterStats = GameResources.LoadPrefab<CharacterStats>(fileNameWithoutExtension, instantiate: false);
					if (characterStats != null)
					{
						SummonCreature(characterStats, location, ownerFaction);
						num++;
					}
					continue;
				}
				AssetBundleRequest assetBundleRequest = GameResources.LoadPrefabAsync<CharacterStats>(fileNameWithoutExtension);
				if (assetBundleRequest != null)
				{
					SummonAsyncLoadRequest item = new SummonAsyncLoadRequest(assetBundleRequest, fileNameWithoutExtension, location, ownerFaction);
					m_summonAsyncLoadRequests.Add(item);
					num++;
				}
			}
		}
		if (num == 0)
		{
			PerformOneSummoning(location, ownerFaction);
		}
	}

	public override void AddSummonEffects(StringEffects stringEffects, GameObject character)
	{
		int maxSummons = MaxSummons;
		int num = MaxSummons;
		if (SummonList.Count > 0)
		{
			for (int i = 0; i < SummonList.Count; i++)
			{
				if (!SummonList[i])
				{
					num--;
				}
			}
		}
		else if (SummonFileList.Count > 0)
		{
			for (int j = 0; j < SummonFileList.Count; j++)
			{
				if (string.IsNullOrEmpty(SummonFileList[j]))
				{
					num--;
				}
			}
		}
		string text = ((maxSummons != num) ? GUIUtils.Format(445, num, maxSummons) : maxSummons.ToString());
		AddStringEffect(GUIUtils.Format(2148, text), GetSummonListString(character), hostile: false, stringEffects);
	}
}

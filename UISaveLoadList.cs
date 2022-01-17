using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UISaveLoadList : MonoBehaviour
{
	public UISaveLoadPlaythroughGroup RootGroup;

	public GameObject NoSavesDisplay;

	public GameObject BottomPadder;

	private List<UISaveLoadPlaythroughGroup> m_Playthroughs = new List<UISaveLoadPlaythroughGroup>();

	public void Reload()
	{
		if (m_Playthroughs != null)
		{
			foreach (UISaveLoadPlaythroughGroup playthrough in m_Playthroughs)
			{
				GameUtilities.Destroy(playthrough.gameObject);
			}
		}
		m_Playthroughs = new List<UISaveLoadPlaythroughGroup>();
		IEnumerable<SaveGameInfo> enumerable = SaveGameInfo.CachedSaveGameInfo.Where((SaveGameInfo sgi) => sgi != null);
		RootGroup.gameObject.SetActive(value: false);
		UISaveLoadPlaythroughGroup uISaveLoadPlaythroughGroup = null;
		bool flag = false;
		if (enumerable != null && enumerable.Any())
		{
			bool flag2 = true;
			foreach (SaveGameInfo item in enumerable)
			{
				if (flag2)
				{
					uISaveLoadPlaythroughGroup = NGUITools.AddChild(RootGroup.transform.parent.gameObject, RootGroup.gameObject).GetComponent<UISaveLoadPlaythroughGroup>();
					uISaveLoadPlaythroughGroup.gameObject.SetActive(value: true);
					uISaveLoadPlaythroughGroup.AddSaveFile(item);
					m_Playthroughs.Add(uISaveLoadPlaythroughGroup);
					flag2 = false;
					continue;
				}
				flag = false;
				foreach (UISaveLoadPlaythroughGroup playthrough2 in m_Playthroughs)
				{
					if (playthrough2.ClaimsSaveFile(item))
					{
						playthrough2.AddSaveFile(item);
						flag = true;
					}
				}
				if (!flag)
				{
					uISaveLoadPlaythroughGroup = NGUITools.AddChild(RootGroup.transform.parent.gameObject, RootGroup.gameObject).GetComponent<UISaveLoadPlaythroughGroup>();
					uISaveLoadPlaythroughGroup.gameObject.SetActive(value: true);
					uISaveLoadPlaythroughGroup.AddSaveFile(item);
					m_Playthroughs.Add(uISaveLoadPlaythroughGroup);
				}
			}
		}
		if (UISaveLoadManager.Instance.SaveMode)
		{
			flag = false;
			foreach (UISaveLoadPlaythroughGroup playthrough3 in m_Playthroughs)
			{
				if (playthrough3.ClaimsSession(GameState.s_playerCharacter.SessionID))
				{
					uISaveLoadPlaythroughGroup = playthrough3;
					if (!GameState.Mode.TrialOfIron)
					{
						playthrough3.AddNewSaveFile(GameState.s_playerCharacter.SessionID);
					}
					flag = true;
				}
			}
			if (!flag)
			{
				uISaveLoadPlaythroughGroup = NGUITools.AddChild(RootGroup.transform.parent.gameObject, RootGroup.gameObject).GetComponent<UISaveLoadPlaythroughGroup>();
				uISaveLoadPlaythroughGroup.gameObject.SetActive(value: true);
				uISaveLoadPlaythroughGroup.AddNewSaveFile(GameState.s_playerCharacter.SessionID);
				m_Playthroughs.Add(uISaveLoadPlaythroughGroup);
			}
		}
		m_Playthroughs.Sort((UISaveLoadPlaythroughGroup x, UISaveLoadPlaythroughGroup y) => DateTime.Compare(y.MostRecentSave, x.MostRecentSave));
		if ((bool)GameState.s_playerCharacter)
		{
			foreach (UISaveLoadPlaythroughGroup playthrough4 in m_Playthroughs)
			{
				if (playthrough4.ClaimsSession(GameState.s_playerCharacter.SessionID))
				{
					playthrough4.Collapsed = false;
					break;
				}
			}
		}
		else if (m_Playthroughs.Count > 0)
		{
			m_Playthroughs[0].Collapsed = false;
		}
		Reposition();
	}

	public void Reposition()
	{
		int num = 0;
		foreach (UISaveLoadPlaythroughGroup playthrough in m_Playthroughs)
		{
			playthrough.transform.localPosition = new Vector3(playthrough.transform.localPosition.x, num, playthrough.transform.localPosition.z);
			num -= playthrough.Height;
		}
		BottomPadder.transform.localPosition = new Vector3(BottomPadder.transform.localPosition.x, num, BottomPadder.transform.localPosition.z);
		NoSavesDisplay.gameObject.SetActive(m_Playthroughs.Count == 0);
	}

	public void RemoveGroup(UISaveLoadPlaythroughGroup grp)
	{
		m_Playthroughs.Remove(grp);
		GameUtilities.Destroy(grp.gameObject);
		Reposition();
	}
}

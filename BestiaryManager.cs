using System.Collections.Generic;
using UnityEngine;

public class BestiaryManager : MonoBehaviour
{
	[Persistent]
	private int[] m_KillCounts = new int[0];

	[Persistent]
	private int m_TotalKills;

	public BestiaryReferenceList ReferenceList;

	public const float DefaultRevealPoint = 0f;

	private List<BestiaryReference> m_ListWarned = new List<BestiaryReference>();

	public static BestiaryManager Instance { get; private set; }

	public int[] GetKillCounts()
	{
		return m_KillCounts;
	}

	public int GetTotalKills()
	{
		return m_TotalKills;
	}

	public BestiaryReference GetReferenceByIndex(int index)
	{
		return ReferenceList.Prefabs[index];
	}

	public bool GetEntryVisible(BestiaryReference form)
	{
		if (GetKillCount(form) == 0)
		{
			return false;
		}
		if ((ProductConfiguration.ActivePackage & form.Package) == 0)
		{
			return false;
		}
		return true;
	}

	public int GetKillCount(BestiaryReference form)
	{
		int num = IndexOf(form);
		if (num >= 0)
		{
			return m_KillCounts[num];
		}
		return 0;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'BestiaryManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void Start()
	{
		ResizeKillList();
		ReferenceList.Initialize();
	}

	public void Restored()
	{
		ResizeKillList();
	}

	private void ResizeKillList()
	{
		if (m_KillCounts.Length < ReferenceList.Prefabs.Length)
		{
			int[] array = new int[ReferenceList.Prefabs.Length];
			m_KillCounts.CopyTo(array, 0);
			m_KillCounts = array;
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

	public List<BestiaryReference> GetChildren(string tag)
	{
		List<BestiaryReference> list = new List<BestiaryReference>();
		for (int i = 0; i < ReferenceList.Prefabs.Length; i++)
		{
			if (ReferenceList.Prefabs[i].ParentTag == tag)
			{
				list.Add(ReferenceList.Prefabs[i]);
			}
		}
		return list;
	}

	public void RecordKill(BestiaryReference form)
	{
		m_TotalKills++;
		if (!form)
		{
			return;
		}
		float killProportion = GetKillProportion(form);
		int num = IndexOf(form);
		if (num >= 0)
		{
			m_KillCounts[num]++;
		}
		bool flag = killProportion == 0f;
		if (!flag)
		{
			for (int i = 0; i < 26; i++)
			{
				if (!ReferenceList.DefaultReveal.CanSeeStat(form.RevealOverrides, (IndexableStat)i, killProportion) && ReferenceList.DefaultReveal.CanSeeStat(form.RevealOverrides, (IndexableStat)i, GetKillProportion(form)))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Console.ConsoleMessage consoleMessage;
		if (form.TotalExperienceGained == 0)
		{
			consoleMessage = new Console.ConsoleMessage(GUIUtils.FormatWithLinks(1577, CharacterStats.Name(form)), Console.ConsoleState.Combat, Color.green);
		}
		else
		{
			int numUnlocks = ReferenceList.DefaultReveal.GetNumUnlocks(form.RevealOverrides, form.KillsToMaster);
			int numUnlocksAchieved = ReferenceList.DefaultReveal.GetNumUnlocksAchieved(form.RevealOverrides, GetKillProportion(form), form.KillsToMaster);
			int num2 = Mathf.FloorToInt((float)numUnlocksAchieved / (float)numUnlocks * 100f);
			int num3 = form.TotalExperienceGained / numUnlocks;
			int num4 = num3 * numUnlocksAchieved;
			if (num2 == 100)
			{
				int num5 = form.TotalExperienceGained % numUnlocks;
				num3 += num5;
				num4 += num5;
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.QuestComplete);
			}
			Color green = Color.green;
			consoleMessage = new Console.ConsoleMessage(GUIUtils.FormatWithLinks(1630, CharacterStats.Name(form), num2, num4 * PartyHelper.NumPartyMembers, form.TotalExperienceGained * PartyHelper.NumPartyMembers), Console.ConsoleState.Combat, green);
			PartyHelper.AssignXPToParty(num3);
		}
		consoleMessage.OnClickCallback = delegate
		{
			UIJournalManager.Instance.ShowBestiaryReference(form);
		};
		Console.Instance.AddMessage(consoleMessage);
	}

	public bool CanSeeStat(BestiaryReference form, IndexableStat stat)
	{
		if ((bool)form)
		{
			return ReferenceList.DefaultReveal.CanSeeStat(form.RevealOverrides, stat, GetKillProportion(form));
		}
		return false;
	}

	private float GetKillProportion(BestiaryReference form)
	{
		int num = IndexOf(form);
		if (num >= 0)
		{
			return (float)m_KillCounts[num] / (float)form.KillsToMaster;
		}
		return -1f;
	}

	private int IndexOf(BestiaryReference form)
	{
		if (!form)
		{
			return -1;
		}
		for (int i = 0; i < ReferenceList.Prefabs.Length; i++)
		{
			if (ReferenceList.Prefabs[i].name == form.name)
			{
				return i;
			}
		}
		if (!m_ListWarned.Contains(form))
		{
			m_ListWarned.Add(form);
			UIDebug.Instance.LogOnceOnlyWarning("Bestiary creature prefab '" + form.name + "' isn't entered in the BestiaryReferences list.", UIDebug.Department.Design, 10f);
		}
		return -1;
	}

	public void CheatMaxAll()
	{
		for (int i = 0; i < m_KillCounts.Length; i++)
		{
			m_KillCounts[i] = ReferenceList.Prefabs[i].KillsToMaster;
		}
	}
}

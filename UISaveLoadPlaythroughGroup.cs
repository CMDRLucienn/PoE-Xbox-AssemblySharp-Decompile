using System;
using System.Collections.Generic;
using UnityEngine;

public class UISaveLoadPlaythroughGroup : MonoBehaviour
{
	public UILabel NameLabel;

	public UISprite ExpandArrow;

	public UIGrid SaveBarGrid;

	public GameObject ExpandHitbox;

	public UISaveLoadSave RootSave;

	private List<UISaveLoadSave> m_SaveBars = new List<UISaveLoadSave>();

	public DateTime MostRecentSave;

	private Guid m_LoadedSessionId;

	private bool m_Collapsed = true;

	public int AdditionalHeight;

	public bool Collapsed
	{
		get
		{
			return m_Collapsed;
		}
		set
		{
			m_Collapsed = value;
			foreach (UISaveLoadSave saveBar in m_SaveBars)
			{
				saveBar.gameObject.SetActive(!m_Collapsed);
			}
			SaveBarGrid.Reposition();
			if (m_Collapsed)
			{
				ExpandArrow.spriteName = "goldArrowRight";
			}
			else
			{
				ExpandArrow.spriteName = "goldArrowDown";
			}
			ExpandArrow.MakePixelPerfect();
			UISaveLoadManager.Instance.SaveList.Reposition();
		}
	}

	public int Height
	{
		get
		{
			int num = AdditionalHeight;
			if (!m_Collapsed)
			{
				foreach (UISaveLoadSave saveBar in m_SaveBars)
				{
					num += saveBar.Height;
				}
				return num;
			}
			return num;
		}
	}

	private void Awake()
	{
		RootSave.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		Collapsed = Collapsed;
		UIEventListener uIEventListener = UIEventListener.Get(ExpandHitbox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnExpandClick));
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnExpandClick(GameObject go)
	{
		Collapsed = !Collapsed;
	}

	public bool ClaimsSaveFile(SaveGameInfo info)
	{
		return info.SessionID == m_LoadedSessionId;
	}

	public bool ClaimsSession(Guid sessionId)
	{
		return m_LoadedSessionId == sessionId;
	}

	public void RemoveSaveFile(UISaveLoadSave obj)
	{
		obj.gameObject.SetActive(value: false);
		GameUtilities.Destroy(obj.gameObject);
		m_SaveBars.Remove(obj);
		SaveBarGrid.Reposition();
		if (m_SaveBars.Count == 0)
		{
			UISaveLoadManager.Instance.SaveList.RemoveGroup(this);
		}
		else
		{
			UISaveLoadManager.Instance.SaveList.Reposition();
		}
	}

	public void AddSaveFile(SaveGameInfo info)
	{
		m_LoadedSessionId = info.SessionID;
		UISaveLoadSave component = NGUITools.AddChild(RootSave.transform.parent.gameObject, RootSave.gameObject).GetComponent<UISaveLoadSave>();
		component.gameObject.SetActive(!m_Collapsed);
		component.LoadMetadata(info);
		m_SaveBars.Add(component);
		NameLabel.text = info.PlayerName;
		SortSaveFiles();
		if (info.RealTimestamp > MostRecentSave)
		{
			MostRecentSave = info.RealTimestamp;
		}
	}

	public void AddNewSaveFile(Guid sessionId)
	{
		m_LoadedSessionId = sessionId;
		UISaveLoadSave component = NGUITools.AddChild(RootSave.transform.parent.gameObject, RootSave.gameObject).GetComponent<UISaveLoadSave>();
		component.gameObject.SetActive(!m_Collapsed);
		component.SetNew();
		m_SaveBars.Add(component);
		NameLabel.text = CharacterStats.Name(GameState.s_playerCharacter.gameObject);
		SortSaveFiles();
		MostRecentSave = DateTime.Now;
	}

	public void SortSaveFiles()
	{
		m_SaveBars.Sort(CompareSaveFiles);
		RenameSaveBars();
		SaveBarGrid.Reposition();
	}

	public int CompareSaveFiles(UISaveLoadSave x, UISaveLoadSave y)
	{
		if (x.IsNew)
		{
			return -1;
		}
		if (y.IsNew)
		{
			return 1;
		}
		SaveGameInfo metaData = x.MetaData;
		SaveGameInfo metaData2 = y.MetaData;
		bool flag = metaData.IsAutoSave();
		bool flag2 = metaData2.IsAutoSave();
		for (int i = 0; i < 2; i++)
		{
			if (flag && flag2)
			{
				return -DateTime.Compare(metaData.RealTimestamp, metaData2.RealTimestamp);
			}
			if (flag)
			{
				return -1;
			}
			if (flag2)
			{
				return 1;
			}
			flag = metaData.IsQuickSave();
			flag2 = metaData2.IsQuickSave();
		}
		return -DateTime.Compare(metaData.RealTimestamp, metaData2.RealTimestamp);
	}

	public void RenameSaveBars()
	{
		int num = 0;
		foreach (UISaveLoadSave saveBar in m_SaveBars)
		{
			saveBar.gameObject.name = "SaveItem" + num.ToString("0000000");
			num++;
		}
	}
}

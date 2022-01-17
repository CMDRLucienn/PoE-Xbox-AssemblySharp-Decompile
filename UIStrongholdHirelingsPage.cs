using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIStrongholdHirelingsPage : UIStrongholdParchmentSizer
{
	public UIStrongholdHirelingItem RootItem;

	public UIGrid Grid;

	public UILabel NoBarracks;

	private List<UIStrongholdHirelingItem> m_HirelingItems;

	private UIStrongholdHirelingItem m_AvailableGuest;

	protected override float ContentHeight => (float)Grid.ChildCount * Grid.cellHeight;

	private void OnEnable()
	{
		NoBarracks.gameObject.SetActive(value: false);
		Reload();
		Reposition();
	}

	private void Start()
	{
		Init();
		Stronghold instance = Stronghold.Instance;
		instance.OnHirelingStatusChanged = (Stronghold.HirelingStatusChanged)Delegate.Combine(instance.OnHirelingStatusChanged, new Stronghold.HirelingStatusChanged(OnHirelingChanged));
	}

	private void OnHirelingChanged(StrongholdHireling hireling)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Reload();
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
		if ((bool)Stronghold.Instance)
		{
			Stronghold instance = Stronghold.Instance;
			instance.OnHirelingStatusChanged = (Stronghold.HirelingStatusChanged)Delegate.Remove(instance.OnHirelingStatusChanged, new Stronghold.HirelingStatusChanged(OnHirelingChanged));
		}
	}

	private void Init()
	{
		if (m_HirelingItems == null)
		{
			RootItem.gameObject.SetActive(value: false);
			m_HirelingItems = new List<UIStrongholdHirelingItem>();
			Load();
		}
	}

	public void Reload()
	{
		Init();
		if (!UIStrongholdManager.Instance)
		{
			return;
		}
		foreach (UIStrongholdHirelingItem hirelingItem in m_HirelingItems)
		{
			hirelingItem.Reload();
		}
		if (m_AvailableGuest != null)
		{
			if (UIStrongholdManager.Instance.Stronghold.GuestHirelingAvailable != null)
			{
				m_AvailableGuest.gameObject.SetActive(value: true);
				m_AvailableGuest.SetStandard(UIStrongholdManager.Instance.Stronghold.GuestHirelingAvailable);
			}
			else
			{
				m_AvailableGuest.gameObject.SetActive(value: false);
				m_AvailableGuest.SetStandard(null);
			}
		}
		UpdateEmptyMessage();
	}

	private void Reposition()
	{
		Grid.Reposition();
		UpdateParchmentSize();
	}

	public void Load()
	{
		StrongholdHireling[] standardHirelings = GameState.Stronghold.StandardHirelings;
		foreach (StrongholdHireling strongholdHireling in standardHirelings)
		{
			UIStrongholdHirelingItem component = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UIStrongholdHirelingItem>();
			component.gameObject.SetActive(value: true);
			component.SetStandard(strongholdHireling);
			component.name = "02." + strongholdHireling.Name;
			m_HirelingItems.Add(component);
		}
		StrongholdGuestHireling[] guestHirelings = GameState.Stronghold.GuestHirelings;
		foreach (StrongholdHireling strongholdHireling2 in guestHirelings)
		{
			UIStrongholdHirelingItem component2 = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UIStrongholdHirelingItem>();
			component2.gameObject.SetActive(value: true);
			component2.SetGuest(strongholdHireling2);
			component2.name = "01." + strongholdHireling2.Name;
			m_HirelingItems.Add(component2);
		}
		m_AvailableGuest = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UIStrongholdHirelingItem>();
		m_AvailableGuest.gameObject.SetActive(value: false);
		m_AvailableGuest.NameLabel.color = new Color(0f, 0.5f, 0f);
		m_AvailableGuest.gameObject.name = "00.Guest";
		UpdateEmptyMessage();
		Grid.Reposition();
	}

	public void UpdateEmptyMessage()
	{
		if ((!m_AvailableGuest || !m_AvailableGuest.IsVisible) && !m_HirelingItems.Any((UIStrongholdHirelingItem item) => item.IsVisible))
		{
			if (!UIStrongholdManager.Instance.Stronghold.HasUpgrade(StrongholdUpgrade.Type.Barracks))
			{
				NoBarracks.text = GUIUtils.GetText(657);
			}
			else
			{
				NoBarracks.text = GUIUtils.GetText(1101);
			}
			NoBarracks.gameObject.SetActive(value: true);
		}
		else
		{
			NoBarracks.text = "";
			NoBarracks.gameObject.SetActive(value: false);
		}
	}
}

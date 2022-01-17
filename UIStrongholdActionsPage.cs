using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStrongholdActionsPage : UIStrongholdParchmentSizer
{
	private List<UIStrongholdActionVisitor> m_Visitors;

	private List<UIStrongholdActionAdventure> m_Adventures;

	public UIStrongholdActionVisitor VisitorPrefab;

	public UIStrongholdActionAdventure AdventurePrefab;

	public UIStrongholdActionDebt DebtBar;

	public UIStrongholdActionAttack AttackBar;

	public UITable Layout;

	public UILabel CountLabel;

	public GameObject CountParent;

	public GameObject NoEvents;

	protected override float ContentHeight => NGUIMath.CalculateRelativeWidgetBounds(Layout.transform).size.y;

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		Reload();
	}

	public void Init()
	{
		if (m_Visitors == null)
		{
			Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
			stronghold.OnAdventureStatusChanged = (Stronghold.AdventureStatusChanged)Delegate.Combine(stronghold.OnAdventureStatusChanged, new Stronghold.AdventureStatusChanged(OnAdventureChanged));
			Stronghold stronghold2 = UIStrongholdManager.Instance.Stronghold;
			stronghold2.OnVisitorStatusChanged = (Stronghold.VisitorStatusChanged)Delegate.Combine(stronghold2.OnVisitorStatusChanged, new Stronghold.VisitorStatusChanged(OnVisitorChanged));
			Stronghold stronghold3 = UIStrongholdManager.Instance.Stronghold;
			stronghold3.OnEventChanged = (Stronghold.EventChanged)Delegate.Combine(stronghold3.OnEventChanged, new Stronghold.EventChanged(OnEventChanged));
			Stronghold stronghold4 = UIStrongholdManager.Instance.Stronghold;
			stronghold4.OnDebtChanged = (Stronghold.DebtChanged)Delegate.Combine(stronghold4.OnDebtChanged, new Stronghold.DebtChanged(OnDebtChanged));
			m_Visitors = new List<UIStrongholdActionVisitor>();
			m_Adventures = new List<UIStrongholdActionAdventure>();
			VisitorPrefab.gameObject.SetActive(value: false);
			AdventurePrefab.gameObject.SetActive(value: false);
			AttackBar.gameObject.SetActive(value: false);
			DebtBar.gameObject.SetActive(UIStrongholdManager.Instance.Stronghold.Debt > 0);
		}
	}

	private void OnAdventureChanged(StrongholdAdventure adventure)
	{
		Reload();
	}

	private void OnVisitorChanged(StrongholdVisitor visitor)
	{
		Reload();
	}

	private void OnEventChanged(StrongholdEvent sevent)
	{
		Reload();
	}

	private void OnDebtChanged()
	{
		DebtBar.gameObject.SetActive(UIStrongholdManager.Instance.Stronghold.Debt > 0);
	}

	public void Reload()
	{
		Init();
		OnDebtChanged();
		bool flag = false;
		int num = 0;
		for (int i = 0; i < UIStrongholdManager.Instance.Stronghold.GetAdventuresAvailable.Count; i++)
		{
			GetAdventure(num).Set(UIStrongholdManager.Instance.Stronghold.GetAdventuresAvailable[i], i);
			num++;
		}
		foreach (KeyValuePair<int, StrongholdAdventure> item in UIStrongholdManager.Instance.Stronghold.GetAdventuresEngaged)
		{
			StrongholdAdventure value = item.Value;
			if (value != null && value.Adventurer != null)
			{
				GetAdventure(num).Set(item.Value, item.Key);
				num++;
			}
		}
		flag = flag || num > 0;
		for (int j = num; j < m_Adventures.Count; j++)
		{
			m_Adventures[j].gameObject.SetActive(value: false);
		}
		int num2 = 0;
		foreach (StrongholdVisitor getVisitor in UIStrongholdManager.Instance.Stronghold.GetVisitors)
		{
			if (getVisitor.VisitorType != StrongholdVisitor.Type.PrisonerRequest)
			{
				GetVisitor(num2).SetVisitor(getVisitor);
				num2++;
			}
		}
		foreach (StrongholdEvent getEvent in UIStrongholdManager.Instance.Stronghold.GetEvents)
		{
			if (getEvent.EventType == StrongholdEvent.Type.Kidnapped)
			{
				UIStrongholdActionVisitor visitor = GetVisitor(num2);
				if (getEvent.EventCompanion == null)
				{
					visitor.SetKidnapped((StrongholdVisitor)getEvent.EventData);
				}
				else
				{
					visitor.SetKidnappedRescue((StrongholdVisitor)getEvent.EventData, getEvent);
				}
				num2++;
			}
			else if (getEvent.EventType == StrongholdEvent.Type.Escorting)
			{
				GetVisitor(num2).SetRecall(getEvent);
				num2++;
			}
			else
			{
				_ = getEvent.EventCompanion != null;
			}
		}
		foreach (StrongholdPrisonerData prisoner in UIStrongholdManager.Instance.Stronghold.GetPrisoners())
		{
			GetVisitor(num2).SetPrisoner(prisoner);
			num2++;
		}
		flag = flag || num2 > 0;
		for (int k = num2; k < m_Visitors.Count; k++)
		{
			m_Visitors[k].gameObject.SetActive(value: false);
		}
		AttackBar.gameObject.SetActive(UIStrongholdManager.Instance.Stronghold.HasEvent(StrongholdEvent.Type.Attack));
		flag |= AttackBar.gameObject.activeSelf;
		flag |= DebtBar.gameObject.activeSelf;
		NoEvents.SetActive(!flag);
		int num3 = num + num2;
		if (AttackBar.gameObject.activeSelf)
		{
			num3++;
		}
		if (DebtBar.gameObject.activeSelf)
		{
			num3++;
		}
		CountLabel.text = num3.ToString();
		CountParent.gameObject.SetActive(num3 > 0);
		Layout.Reposition();
		UpdateParchmentSize();
		ParchmentNeedsReposition = 1;
	}

	private UIStrongholdActionVisitor GetVisitor(int index)
	{
		while (index >= m_Visitors.Count)
		{
			UIStrongholdActionVisitor component = NGUITools.AddChild(VisitorPrefab.transform.parent.gameObject, VisitorPrefab.gameObject).GetComponent<UIStrongholdActionVisitor>();
			component.gameObject.SetActive(value: false);
			m_Visitors.Add(component);
		}
		m_Visitors[index].gameObject.SetActive(value: true);
		return m_Visitors[index];
	}

	private UIStrongholdActionAdventure GetAdventure(int index)
	{
		while (index >= m_Adventures.Count)
		{
			UIStrongholdActionAdventure component = NGUITools.AddChild(AdventurePrefab.transform.parent.gameObject, AdventurePrefab.gameObject).GetComponent<UIStrongholdActionAdventure>();
			component.gameObject.SetActive(value: false);
			m_Adventures.Add(component);
		}
		m_Adventures[index].gameObject.SetActive(value: true);
		return m_Adventures[index];
	}
}

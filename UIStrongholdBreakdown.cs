public class UIStrongholdBreakdown : UIPopulator
{
	public UILabel TitleLabel;

	private UIAnchor m_Anchor;

	protected override void Awake()
	{
		base.Awake();
		m_Anchor = GetComponent<UIAnchor>();
	}

	private void OnDisable()
	{
		Hide();
	}

	public void Show(Stronghold.StatType stat, UIWidget anchor)
	{
		if (stat == Stronghold.StatType.None)
		{
			Hide();
			return;
		}
		TitleLabel.text = GUIUtils.GetStrongholdStatString(stat);
		m_Anchor.widgetContainer = anchor;
		Populate(0);
		Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StrongholdUpgrade[] upgrades = stronghold.Upgrades;
		foreach (StrongholdUpgrade strongholdUpgrade in upgrades)
		{
			if (stronghold.HasUpgrade(strongholdUpgrade.UpgradeType))
			{
				num3 = ((stat != Stronghold.StatType.Prestige) ? (num3 + strongholdUpgrade.SecurityAdjustment) : (num3 + strongholdUpgrade.PrestigeAdjustment));
			}
		}
		if (num3 != 0)
		{
			ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num3, GUIUtils.GetText(758));
			num2 += num3;
		}
		int num4 = stronghold.HirelingTotalStat(stat);
		if (num4 != 0)
		{
			ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num4, GUIUtils.GetText(759));
			num2 += num4;
		}
		foreach (StrongholdVisitor getVisitor in stronghold.GetVisitors)
		{
			int statAdjustment = getVisitor.GetStatAdjustment(stat);
			if (statAdjustment != 0)
			{
				ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(statAdjustment, getVisitor.Name);
				num2 += statAdjustment;
			}
		}
		foreach (StrongholdEvent getEvent in stronghold.GetEvents)
		{
			if (getEvent.EventType == StrongholdEvent.Type.SupplicantEffectsWearOff)
			{
				StrongholdVisitor strongholdVisitor = (StrongholdVisitor)getEvent.EventData;
				int num5 = ((stat == Stronghold.StatType.Prestige) ? strongholdVisitor.SupplicantPrestigeAdjustment : 0);
				if (num5 != 0)
				{
					ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num5, StrongholdUtils.Format(87, strongholdVisitor.Name));
					num2 += num5;
				}
			}
			else if (getEvent.EventType == StrongholdEvent.Type.Kidnapped)
			{
				StrongholdVisitor strongholdVisitor2 = (StrongholdVisitor)getEvent.EventData;
				int num6 = ((stat == Stronghold.StatType.Prestige) ? strongholdVisitor2.KidnapPrestigeAdjustment : 0);
				if (num6 != 0)
				{
					ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num6, StrongholdUtils.Format(85, strongholdVisitor2.Name));
					num2 += num6;
				}
			}
			else if (getEvent.EventType == StrongholdEvent.Type.VisitorKilled)
			{
				StrongholdVisitor strongholdVisitor3 = (StrongholdVisitor)getEvent.EventData;
				int num7 = ((stat == Stronghold.StatType.Prestige) ? strongholdVisitor3.KilledPrestigeAdjustment : 0);
				if (num7 != 0)
				{
					ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num7, StrongholdUtils.Format(84, strongholdVisitor3.Name));
					num2 += num7;
				}
			}
		}
		int num8 = stronghold.GetStat(stat) - num2;
		if (num8 != 0)
		{
			ActivateClone(num++).GetComponent<UIStrongholdBreakdownLine>().Set(num8, StrongholdUtils.GetText(88));
		}
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}

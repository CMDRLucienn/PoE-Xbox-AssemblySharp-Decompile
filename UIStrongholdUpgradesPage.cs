using System.Collections.Generic;

public class UIStrongholdUpgradesPage : UIStrongholdParchmentSizer
{
	public UIStrongholdUpgradeItem RootItem;

	public UITable Table;

	private List<UIStrongholdUpgradeItem> m_Items;

	private bool m_FirstUpdate = true;

	protected override float ContentHeight => NGUIMath.CalculateRelativeWidgetBounds(Table.transform).size.y;

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnEnable()
	{
		Reload();
	}

	protected override void Update()
	{
		if (m_FirstUpdate)
		{
			m_FirstUpdate = false;
			Table.Reposition();
			UpdateParchmentSize();
		}
		base.Update();
	}

	public void Reload()
	{
		Init();
		if (m_Items != null)
		{
			foreach (UIStrongholdUpgradeItem item in m_Items)
			{
				item.Reload();
			}
		}
		UpdateParchmentSize();
		ParchmentNeedsReposition = 1;
	}

	private void Init()
	{
		if (m_Items != null || !UIStrongholdManager.Instance || !UIStrongholdManager.Instance.Stronghold)
		{
			return;
		}
		m_Items = new List<UIStrongholdUpgradeItem>();
		for (StrongholdUpgrade.Type type = StrongholdUpgrade.Type.Barbican; type < StrongholdUpgrade.Type.Count; type++)
		{
			StrongholdUpgrade upgradeInfo = UIStrongholdManager.Instance.Stronghold.GetUpgradeInfo(type);
			if (upgradeInfo != null)
			{
				UIStrongholdUpgradeItem component = NGUITools.AddChild(RootItem.transform.parent.gameObject, RootItem.gameObject).GetComponent<UIStrongholdUpgradeItem>();
				component.gameObject.name = upgradeInfo.UiOrderNumber.ToString("000") + "." + upgradeInfo.UpgradeType;
				component.gameObject.SetActive(value: true);
				component.Set(upgradeInfo);
				m_Items.Add(component);
			}
		}
		GameUtilities.Destroy(RootItem.gameObject);
		Table.Reposition();
	}
}

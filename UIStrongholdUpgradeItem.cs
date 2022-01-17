using System;
using UnityEngine;

public class UIStrongholdUpgradeItem : UIStrongholdExpandableItem
{
	public UILabel CostLabel;

	public UILabel SecurityLabel;

	public UILabel PrestigeLabel;

	public UILabel BuildProgressLabel;

	public UILabel PrerequisiteLabel;

	public UIMultiSpriteImageButton PurchaseButton;

	public UITexture Image;

	public GameObject Unbuilt;

	public GameObject InProgress;

	public GameObject Completed;

	private StrongholdUpgrade m_Upgrade;

	private void Start()
	{
		UIMultiSpriteImageButton purchaseButton = PurchaseButton;
		purchaseButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(purchaseButton.onClick, new UIEventListener.VoidDelegate(OnPurchase));
		Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
		stronghold.OnUpgradeStatusChanged = (Stronghold.UpgradeStatusChanged)Delegate.Combine(stronghold.OnUpgradeStatusChanged, new Stronghold.UpgradeStatusChanged(UpgradeChanged));
	}

	private void UpgradeChanged(StrongholdUpgrade.Type upgradeType)
	{
		if (m_Upgrade != null)
		{
			Reload();
		}
	}

	private void OnPurchase(GameObject sender)
	{
		Stronghold.WhyCantBuild whyCantBuild = GameState.Stronghold.WhyCantBuildUpgrade(m_Upgrade.UpgradeType);
		switch (whyCantBuild)
		{
		case Stronghold.WhyCantBuild.NONE:
			UIStrongholdManager.Instance.Stronghold.BuyUpgrade(m_Upgrade.UpgradeType);
			break;
		case Stronghold.WhyCantBuild.ALREADY_IN_PROGRESS:
		{
			StrongholdUpgrade upgradeInfo = UIStrongholdManager.Instance.Stronghold.GetUpgradeInfo(UIStrongholdManager.Instance.Stronghold.GetBuildingUpgrade());
			UIStrongholdManager.Instance.ShowMessage(StringUtility.Format(GUIUtils.GetWhyCantBuildString(whyCantBuild), upgradeInfo.Name.GetText(), UIStrongholdManager.Instance.Stronghold.GetUpgradeInfo(m_Upgrade.Prerequisite).Name.GetText()));
			break;
		}
		default:
			UIStrongholdManager.Instance.ShowMessage(StringUtility.Format(GUIUtils.GetWhyCantBuildString(whyCantBuild), m_Upgrade.Name.GetText(), UIStrongholdManager.Instance.Stronghold.GetUpgradeInfo(m_Upgrade.Prerequisite).Name.GetText()));
			break;
		}
	}

	public override void Reload()
	{
		if (m_Upgrade != null)
		{
			Set(m_Upgrade);
		}
	}

	public void Set(StrongholdUpgrade upgrade)
	{
		m_Upgrade = upgrade;
		Image.mainTexture = upgrade.Icon;
		if (upgrade.TimeToBuild > 0)
		{
			string text = new EternityTimeInterval((int)UIStrongholdManager.Instance.Stronghold.DaysToGTU(upgrade.TimeToBuild) * WorldTime.Instance.SecondsPerDay).FormatNonZero(2);
			if (upgrade.Cost > 0)
			{
				CostLabel.text = GUIUtils.Format(466, upgrade.Cost) + " - " + text;
			}
			else
			{
				CostLabel.text = text;
			}
		}
		else
		{
			CostLabel.text = "";
		}
		SetDescriptionText(upgrade.Description.GetText());
		NameLabel.text = upgrade.Name.GetText();
		SecurityLabel.text = TextUtils.NumberBonus(upgrade.SecurityAdjustment);
		PrestigeLabel.text = TextUtils.NumberBonus(upgrade.PrestigeAdjustment);
		int secondsLeft;
		if (UIStrongholdManager.Instance.Stronghold.HasUpgrade(upgrade.UpgradeType))
		{
			Unbuilt.gameObject.SetActive(value: false);
			InProgress.gameObject.SetActive(value: false);
			Completed.gameObject.SetActive(value: true);
		}
		else if (UIStrongholdManager.Instance.Stronghold.IsBuildingUpgrade(upgrade.UpgradeType, out secondsLeft))
		{
			BuildProgressLabel.text = Stronghold.Format(59, new EternityTimeInterval(secondsLeft).FormatNonZero(2));
			Unbuilt.gameObject.SetActive(value: false);
			InProgress.gameObject.SetActive(value: true);
			Completed.gameObject.SetActive(value: false);
		}
		else
		{
			Unbuilt.gameObject.SetActive(value: true);
			InProgress.gameObject.SetActive(value: false);
			Completed.gameObject.SetActive(value: false);
		}
		bool flag = false;
		if (UIStrongholdManager.Instance.Stronghold.CanBuyUpgrade(upgrade.UpgradeType))
		{
			PurchaseButton.gameObject.SetActive(value: true);
			PrerequisiteLabel.gameObject.SetActive(value: false);
		}
		else
		{
			bool flag2 = true;
			if (upgrade.Prerequisite != StrongholdUpgrade.Type.None && !UIStrongholdManager.Instance.Stronghold.HasUpgrade(upgrade.Prerequisite))
			{
				PrerequisiteLabel.text = GUIUtils.Format(664, UIStrongholdManager.Instance.Stronghold.GetUpgradeInfo(upgrade.Prerequisite).Name.GetText());
			}
			else if (UIStrongholdManager.Instance.Stronghold.AvailableCP < upgrade.Cost)
			{
				PrerequisiteLabel.text = GUIUtils.GetText(721);
			}
			else if (UIStrongholdManager.Instance.Stronghold.GetBuildingUpgrade() != StrongholdUpgrade.Type.None)
			{
				flag2 = false;
				flag = true;
			}
			else
			{
				PrerequisiteLabel.text = GUIUtils.GetText(722);
			}
			PurchaseButton.gameObject.SetActive(!flag2);
			PrerequisiteLabel.gameObject.SetActive(flag2);
		}
		Transform transform = PurchaseButton.transform.Find("Background");
		if ((bool)transform)
		{
			UIImageButtonRevised component = transform.GetComponent<UIImageButtonRevised>();
			if ((bool)component)
			{
				Color overrideMousedColor = component.OverrideMousedColor;
				overrideMousedColor.a = (flag ? 0.470588237f : 1f);
				component.SetMousedColor(overrideMousedColor);
				overrideMousedColor = component.OverrideNeutralColor;
				overrideMousedColor.a = (flag ? 0.470588237f : 1f);
				component.SetNeutralColor(overrideMousedColor);
				component.UpdateImage();
			}
		}
		transform = PurchaseButton.transform.Find("Shadow");
		if ((bool)transform)
		{
			UISprite component2 = transform.GetComponent<UISprite>();
			if ((bool)component2)
			{
				component2.alpha = (flag ? 0.286274523f : 1f);
			}
		}
	}
}

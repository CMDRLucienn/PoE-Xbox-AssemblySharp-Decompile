using System;
using UnityEngine;

public class UIInstalledPackageHighlight : MonoBehaviour
{
	public ProductConfiguration.Package RequiredPackage;

	public float UninstalledAlphaAmount = 0.5f;

	public GUIDatabaseString InstalledString;

	public GUIDatabaseString UninstalledString;

	public ProductConfiguration.Package OtherPackageDependency;

	public GUIDatabaseString MissingDependencyString;

	public UIImageButtonRevised Button;

	public UILabel HoverLabel;

	public int SteamAppID;

	public string NotInstalledClickURL;

	private const string NotInstalledUrlMacStore = "https://itunes.apple.com/us/app/pillars-of-eternity/id979217373?mt=12";

	public MainMenuBackgroundType BackgroundType = MainMenuBackgroundType.BaseGame;

	private bool RequiredPackageInstalled()
	{
		return (ProductConfiguration.ActivePackage & RequiredPackage) == RequiredPackage;
	}

	private bool HasNeededDependency()
	{
		return (ProductConfiguration.ActivePackage & OtherPackageDependency) == OtherPackageDependency;
	}

	private void Start()
	{
		HoverLabel.enabled = false;
		if (RequiredPackageInstalled())
		{
			Button.enabled = true;
			if (!HasNeededDependency())
			{
			}
		}
		else
		{
			Button.enabled = true;
			UISprite component = Button.GetComponent<UISprite>();
			if ((bool)component)
			{
				Button.ChangeNormalSprite(component.spriteName += "_g");
			}
		}
		UIEventListener uIEventListener = UIEventListener.Get(Button.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(Button.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
	}

	private void OnHover(GameObject sender, bool over)
	{
		if ((bool)HoverLabel)
		{
			if (over)
			{
				HoverLabel.enabled = true;
				HoverLabel.text = ((!RequiredPackageInstalled()) ? UninstalledString.GetText() : (HasNeededDependency() ? InstalledString.GetText() : MissingDependencyString.GetText()));
			}
			else
			{
				HoverLabel.enabled = false;
			}
		}
	}

	private void OnClick(GameObject source)
	{
		if (!RequiredPackageInstalled())
		{
			Application.OpenURL(NotInstalledClickURL);
			return;
		}
		_ = FrontEndTitleIntroductionManager.Instance.TargetBackground;
		_ = BackgroundType;
	}
}

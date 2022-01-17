using System;
using UnityEngine;

public class UIMainMenuClickHandler : UIIsButton
{
	public enum MainMenuButtonType
	{
		NewGame,
		Continue,
		Options,
		Credits,
		Quit,
		Load
	}

	public MainMenuButtonType ButtonType;

	private void OnEnable()
	{
		RefreshState();
	}

	public void RefreshState()
	{
		UIMultiSpriteImageButton component = base.transform.parent.GetComponent<UIMultiSpriteImageButton>();
		if ((bool)component && (ButtonType == MainMenuButtonType.Continue || ButtonType == MainMenuButtonType.Load))
		{
			bool flag2 = (component.enabled = SaveGameInfo.SaveCachingComplete() && GameResources.SaveGameExists());
			component.Label.color = (component.enabled ? new Color(0.86f, 0.86f, 0.86f) : new Color(0.6f, 0.6f, 0.6f));
		}
	}

	private void OnClick()
	{
		if (!base.enabled || !UIMainMenuManager.Instance.MenuActive)
		{
			return;
		}
		switch (ButtonType)
		{
		case MainMenuButtonType.NewGame:
		{
			UIMainMenuManager.Instance.MenuActive = false;
			if (GameUtilities.HasPX1() && Conditionals.CommandLineArg("e3"))
			{
				StartIntro();
				break;
			}
			UIOptionsManager.Instance.ToggleAlt();
			UIOptionsManager instance3 = UIOptionsManager.Instance;
			instance3.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance3.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleDifficultyOptionsClosed));
			break;
		}
		case MainMenuButtonType.Continue:
			GameResources.LoadLastGame(fadeOut: true);
			UIMainMenuManager.Instance.SetButtonsEnabled(enabled: true);
			break;
		case MainMenuButtonType.Options:
		{
			UIMainMenuManager.Instance.MenuActive = false;
			UIOptionsManager.Instance.SetMenuLayout(UIOptionsManager.OptionsMenuLayout.MainMenu);
			UIOptionsManager.Instance.Toggle();
			UIOptionsManager instance2 = UIOptionsManager.Instance;
			instance2.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance2.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleGameOptionsClosed));
			UIMainMenuManager.Instance.SetButtonsEnabled(enabled: false);
			break;
		}
		case MainMenuButtonType.Credits:
			Credits.RunRequested = true;
			break;
		case MainMenuButtonType.Quit:
			Application.Quit();
			break;
		case MainMenuButtonType.Load:
		{
			UIMainMenuManager.Instance.MenuActive = false;
			UISaveLoadManager.Instance.SaveMode = false;
			UISaveLoadManager.Instance.ToggleAlt();
			UISaveLoadManager instance = UISaveLoadManager.Instance;
			instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Combine(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleLoadClose));
			UIMainMenuManager.Instance.SetButtonsEnabled(enabled: false);
			break;
		}
		}
	}

	private void StartIntro()
	{
		GameState.NewGame = true;
		UnityEngine.Object.FindObjectOfType<FrontEndTitleIntroductionManager>().StartFrontEndIntroduction();
	}

	private void HandleLoadClose(UIHudWindow window)
	{
		UIMainMenuManager uIMainMenuManager = UnityEngine.Object.FindObjectOfType<UIMainMenuManager>();
		if ((bool)uIMainMenuManager)
		{
			uIMainMenuManager.MenuActive = true;
			uIMainMenuManager.SetButtonsEnabled(enabled: true);
		}
		UISaveLoadManager instance = UISaveLoadManager.Instance;
		instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleLoadClose));
	}

	private void HandleGameOptionsClosed(UIHudWindow window)
	{
		UIMainMenuManager uIMainMenuManager = UnityEngine.Object.FindObjectOfType<UIMainMenuManager>();
		if ((bool)uIMainMenuManager)
		{
			uIMainMenuManager.MenuActive = true;
			uIMainMenuManager.SetButtonsEnabled(enabled: true);
		}
		UIOptionsManager instance = UIOptionsManager.Instance;
		instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleGameOptionsClosed));
	}

	private void HandleDifficultyOptionsClosed(UIHudWindow window)
	{
		UIOptionsManager instance = UIOptionsManager.Instance;
		instance.OnWindowHidden = (UIHudWindow.WindowHiddenDelegate)Delegate.Remove(instance.OnWindowHidden, new UIHudWindow.WindowHiddenDelegate(HandleDifficultyOptionsClosed));
		if (UIOptionsManager.Instance.Accepted)
		{
			StartIntro();
			return;
		}
		UIMainMenuManager uIMainMenuManager = UnityEngine.Object.FindObjectOfType<UIMainMenuManager>();
		if ((bool)uIMainMenuManager)
		{
			uIMainMenuManager.MenuActive = true;
		}
	}
}

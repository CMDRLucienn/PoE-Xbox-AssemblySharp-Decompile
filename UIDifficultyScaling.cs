using System;

public static class UIDifficultyScaling
{
	private static MapData m_PendingMap;

	public static event EventHandler PromptCompleted;

	public static void PromptScalersAndChangeLevel(MapType target, EventHandler handler)
	{
		PromptScalersAndChangeLevel(WorldMap.Instance.GetMap(target), handler);
	}

	public static void PromptScalersAndChangeLevel(MapData target, EventHandler handler)
	{
		m_PendingMap = target;
		if (handler != null)
		{
			PromptCompleted += handler;
		}
		TransitionOnEndFade();
	}

	private static void TransitionOnEndFade()
	{
		FadeManager instance = FadeManager.Instance;
		instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(TransitionOnEndFade));
		CharacterStats characterStats = (GameState.s_playerCharacter ? GameState.s_playerCharacter.GetComponent<CharacterStats>() : null);
		if (m_PendingMap.IsValidOnMap("px1") && !GameState.Instance.HasEnteredPX1 && (bool)characterStats && characterStats.Level >= 9)
		{
			PromptToEnableScaler(DifficultyScaling.Scaler.PX1_HIGH_LEVEL);
		}
		else if (m_PendingMap.IsValidOnMap("px1") && !GameState.Instance.HasEnteredPX2 && GameGlobalVariables.HasStartedPX2() && (bool)characterStats && characterStats.Level >= 12)
		{
			PromptToEnableScaler(DifficultyScaling.Scaler.PX2_HIGH_LEVEL);
		}
		else if (m_PendingMap.SceneName == "AR_1402_Court_of_the_Penitents" && !WorldMap.Instance.GetMap(MapType.AR_1404_Sun_In_Shadow_01).HasBeenVisited && (bool)characterStats && characterStats.Level >= 13)
		{
			PromptToEnableScaler(DifficultyScaling.Scaler.ACT4_HIGH_LEVEL);
		}
		else if (m_PendingMap.SceneName == "AR_0803_Elmshore" && !WorldMap.Instance.GetMap(MapType.AR_0803_Elmshore).HasBeenVisited && (bool)characterStats && characterStats.Level >= 10)
		{
			PromptToEnableScaler(DifficultyScaling.Scaler.ELMSHORE_HIGH_LEVEL);
		}
		else
		{
			DoTransition();
		}
	}

	public static void PromptToEnableScaler(DifficultyScaling.Scaler scaler)
	{
		string text = "*ERROR:" + scaler.ToString() + "*";
		switch (scaler)
		{
		case DifficultyScaling.Scaler.PX1_HIGH_LEVEL:
			text = GUIUtils.Format(2014);
			break;
		case DifficultyScaling.Scaler.ACT4_HIGH_LEVEL:
			text = GUIUtils.Format(2219, 4);
			break;
		case DifficultyScaling.Scaler.ELMSHORE_HIGH_LEVEL:
			text = WorldMap.Instance.GetMap(MapType.AR_0803_Elmshore).DisplayName.GetText();
			break;
		case DifficultyScaling.Scaler.PX2_HIGH_LEVEL:
			text = GUIUtils.Format(2422);
			break;
		}
		UIMessageBox uIMessageBox = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.YESNO, "", GUIUtils.Format(2050, text));
		uIMessageBox.UserData = scaler;
		uIMessageBox.OverrideButtonText(GUIUtils.GetText(2052), GUIUtils.GetText(2051));
		uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, new UIMessageBox.OnEndDialog(OnDialogEnd));
	}

	private static void OnDialogEnd(UIMessageBox.Result result, UIMessageBox sender)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			DifficultyScaling.Instance.SetScalerActive((DifficultyScaling.Scaler)sender.UserData, active: true);
		}
		if (result == UIMessageBox.Result.CANCEL)
		{
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.75f);
		}
		else
		{
			DoTransition();
		}
	}

	private static void DoTransition()
	{
		if (UIDifficultyScaling.PromptCompleted != null)
		{
			UIDifficultyScaling.PromptCompleted(null, new TransitionEventArgs(m_PendingMap));
		}
		UIDifficultyScaling.PromptCompleted = null;
		GameState.LoadedGame = false;
		GameState.ChangeLevel(m_PendingMap);
		m_PendingMap = null;
	}
}

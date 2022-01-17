using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIDeathManager : UIHudWindow
{
	public UIMultiSpriteImageButton LoadButton;

	public UIMultiSpriteImageButton QuitButton;

	public UIMultiSpriteImageButton RespawnButton;

	public UIGrid ButtonGrid;

	public UILabel TitleLabel;

	private static UIDeathManager s_Instance;

	public static UIDeathManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		UIMultiSpriteImageButton loadButton = LoadButton;
		loadButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(loadButton.onClick, new UIEventListener.VoidDelegate(OnLoadClicked));
		UIMultiSpriteImageButton quitButton = QuitButton;
		quitButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(quitButton.onClick, new UIEventListener.VoidDelegate(OnQuitClicked));
		UIMultiSpriteImageButton respawnButton = RespawnButton;
		respawnButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(respawnButton.onClick, new UIEventListener.VoidDelegate(OnRespawnClicked));
	}

	protected override void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Show()
	{
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.PartyDead);
		if ((bool)UIDeveloperCommentary.Instance)
		{
			UIDeveloperCommentary.Instance.StopAndHide();
		}
		LoadButton.gameObject.SetActive(!GameState.Mode.TrialOfIron);
		ButtonGrid.Reposition();
		if (!GameState.s_playerCharacter || GameState.s_playerCharacter.GetComponent<Health>().Dead || PartyMemberAI.OnlyPrimaryPartyMembers.Count() == 1)
		{
			TitleLabel.text = GUIUtils.GetText(1526);
		}
		else
		{
			TitleLabel.text = GUIUtils.GetText(361);
		}
		GameState.TrialOfIronDelete();
	}

	private string GetLastSaveGame()
	{
		IEnumerable<SaveGameInfo> enumerable = SaveGameInfo.CachedSaveGameInfo.Where((SaveGameInfo sv) => sv != null && sv.SessionID == GameState.s_playerCharacter.SessionID);
		if (enumerable != null && enumerable.Any())
		{
			return enumerable.OrderByDescending((SaveGameInfo sv) => sv.RealTimestamp).First().FileName;
		}
		return "";
	}

	private void OnLoadClicked(GameObject go)
	{
		UIWindowManager.Instance.SuspendFor(UISaveLoadManager.Instance);
		UISaveLoadManager.Instance.ToggleAlt();
	}

	private void OnQuitClicked(GameObject go)
	{
		GameState.LoadMainMenu(fadeOut: true);
	}

	private void OnRespawnClicked(GameObject go)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				Health component = partyMemberAI.GetComponent<Health>();
				component.ApplyStaminaChangeDirectly(component.MaxStamina - component.CurrentStamina, applyIfDead: true);
				component.ApplyHealthChangeDirectly(component.MaxHealth - component.CurrentHealth, applyIfDead: true);
			}
		}
		GameState.PartyDead = false;
		GameState.GameOver = false;
		HideWindow();
	}
}

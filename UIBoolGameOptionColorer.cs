using UnityEngine;

public class UIBoolGameOptionColorer : MonoBehaviour
{
	public GameOption.BoolOption OptionToFollow;

	public UIWidget TargetWidget;

	public Color ColorWhenTrue;

	public Color ColorWhenFalse;

	private void Start()
	{
		if (TargetWidget == null)
		{
			TargetWidget = GetComponent<UIWidget>();
		}
		if (TargetWidget != null)
		{
			GameState.Mode.OptionsReloaded += OnGameOptionsChanged;
		}
	}

	private void OnDestroy()
	{
		GameState.Mode.OptionsReloaded -= OnGameOptionsChanged;
	}

	private void OnGameOptionsChanged(GameMode optionsUpdated)
	{
		if (TargetWidget != null)
		{
			TargetWidget.color = (optionsUpdated.GetOption(GameOption.BoolOption.SOLID_HUD) ? ColorWhenTrue : ColorWhenFalse);
		}
	}
}

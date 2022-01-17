using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIStrongholdTurnLabel : MonoBehaviour
{
	public enum DisplayMode
	{
		CURRENT,
		AVAILABLE
	}

	public DisplayMode Mode;

	private void Start()
	{
		RefreshText();
	}

	private void Update()
	{
		RefreshText();
	}

	private void RefreshText()
	{
		switch (Mode)
		{
		case DisplayMode.CURRENT:
			GetComponent<UILabel>().text = GameState.Stronghold.CurrentTurn.ToString();
			break;
		case DisplayMode.AVAILABLE:
			GetComponent<UILabel>().text = GameState.Stronghold.AvailableTurns.ToString();
			break;
		}
	}
}

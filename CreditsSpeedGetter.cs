using UnityEngine;

public class CreditsSpeedGetter : MonoBehaviour
{
	private void Start()
	{
		Object.FindObjectOfType<UIScrollController>().OnSpeedChanged += OnSpeedChanged;
	}

	public void OnSpeedChanged(GameObject obj, GameEventArgs args)
	{
		UIScrollController uIScrollController = Object.FindObjectOfType<UIScrollController>();
		GetComponent<UILabel>().text = uIScrollController.GetYSpeedMultiplier() + "x";
	}
}

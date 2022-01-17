using UnityEngine;

public class CreditsModifier : MonoBehaviour
{
	public enum CreditsModifierType
	{
		IncreaseSpeed,
		DecreaseSpeed,
		End,
		Restart
	}

	public CreditsModifierType ModifierType;

	private Credits m_Credits;

	private void Start()
	{
		m_Credits = Object.FindObjectOfType<Credits>();
	}

	private void OnClick()
	{
		if (m_Credits.GetCreditsState() == Credits.CreditsState.Running)
		{
			switch (ModifierType)
			{
			case CreditsModifierType.IncreaseSpeed:
				Object.FindObjectOfType<UIScrollController>().IncreaseSpeed();
				break;
			case CreditsModifierType.DecreaseSpeed:
				Object.FindObjectOfType<UIScrollController>().DecreaseSpeed();
				break;
			case CreditsModifierType.End:
				Object.FindObjectOfType<Credits>().HandleCreditsFinished();
				break;
			case CreditsModifierType.Restart:
				Object.FindObjectOfType<Credits>().StartCredits();
				break;
			}
		}
	}
}

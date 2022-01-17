using UnityEngine;

public class UIStrongholdUnviewedEventsWidget : MonoBehaviour
{
	private int m_LastValue = -1;

	public UISprite Sprite;

	public UILabel Label;

	private void Update()
	{
		if (GameState.Stronghold.UnviewedEventCount != m_LastValue)
		{
			Label.text = Mathf.Min(99, GameState.Stronghold.UnviewedEventCount).ToString();
			m_LastValue = GameState.Stronghold.UnviewedEventCount;
		}
		base.transform.GetChild(0).gameObject.SetActive(GameState.Stronghold.UnviewedEventCount > 0);
	}
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UIChangeCursor : MonoBehaviour
{
	public GameCursor.CursorType CursorType = GameCursor.CursorType.Normal;

	private bool m_Hovered;

	private void OnHover(bool state)
	{
		m_Hovered = state;
	}

	private void Update()
	{
		if (m_Hovered)
		{
			GameCursor.UiCursor = CursorType;
		}
	}
}

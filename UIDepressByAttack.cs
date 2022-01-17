using UnityEngine;

public class UIDepressByAttack : MonoBehaviour
{
	public GameCursor.CursorType CursorType;

	private UIMultiSpriteImageButton m_Button;

	private void Start()
	{
		m_Button = GetComponent<UIMultiSpriteImageButton>();
	}

	private void Update()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			m_Button.ForceDown(GameState.s_playerCharacter.IsInForceAttackMode);
		}
	}
}

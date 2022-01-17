using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UINoClick : MonoBehaviour
{
	public bool BlockScrolling = true;

	public bool BlockClicking = true;

	private bool m_MouseOver;

	private bool m_IHaveScrollingBlocked;

	public static bool MouseOverUI { get; private set; }

	private void OnDisable()
	{
		Exit();
	}

	private void Update()
	{
		if (m_MouseOver && BlockClicking)
		{
			MouseOverUI = true;
			GameInput.HandleAllClicks();
			if (GameCursor.UiCursor == GameCursor.CursorType.None)
			{
				GameCursor.UiCursor = GameCursor.CursorType.Normal;
			}
		}
	}

	private void LateUpdate()
	{
		MouseOverUI = false;
	}

	private void OnPress(bool down)
	{
		if (BlockClicking)
		{
			GameInput.HandleAllClicks();
		}
	}

	private void OnHover(bool over)
	{
		if (over)
		{
			Enter();
		}
		else
		{
			Exit();
		}
	}

	private void Exit()
	{
		m_MouseOver = false;
		if ((bool)CameraControl.Instance && m_IHaveScrollingBlocked)
		{
			m_IHaveScrollingBlocked = false;
			CameraControl.Instance.EnablePlayerScroll(enableScroll: true);
		}
	}

	private void Enter()
	{
		m_MouseOver = true;
		if (BlockScrolling && (bool)CameraControl.Instance && !m_IHaveScrollingBlocked)
		{
			m_IHaveScrollingBlocked = true;
			CameraControl.Instance.EnablePlayerScroll(enableScroll: false);
		}
	}
}

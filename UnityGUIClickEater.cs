using UnityEngine;

public class UnityGUIClickEater : MonoBehaviour
{
	private CameraControl m_CameraControl;

	private GameCursor.CursorType m_MyCursorType;

	private bool m_IDisabledScroll;

	private static bool mEat;

	public static void DoEat()
	{
		mEat = true;
	}

	public static void EatInRect(Rect window)
	{
		if (window.Contains(new Vector3(GameInput.MousePosition.x, (float)Screen.height - GameInput.MousePosition.y, Input.mousePosition.z)))
		{
			DoEat();
		}
	}

	private void Start()
	{
		base.transform.localScale = new Vector3(Screen.width, Screen.height, 1f);
	}

	private void Update()
	{
		if (m_CameraControl == null)
		{
			m_CameraControl = CameraControl.Instance;
		}
		if (mEat)
		{
			m_MyCursorType = GameCursor.CursorType.Normal;
			GameCursor.UiCursor = m_MyCursorType;
			GameInput.HandleAllClicks();
			if (!m_IDisabledScroll)
			{
				m_IDisabledScroll = true;
				m_CameraControl.EnablePlayerScroll(enableScroll: false);
			}
			mEat = false;
		}
		else
		{
			if (m_MyCursorType == GameCursor.CursorType.Normal)
			{
				m_MyCursorType = GameCursor.CursorType.None;
				GameCursor.UiCursor = m_MyCursorType;
			}
			if (m_IDisabledScroll)
			{
				m_IDisabledScroll = false;
				m_CameraControl.EnablePlayerScroll(enableScroll: true);
			}
		}
	}
}

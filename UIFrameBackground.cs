using UnityEngine;

public class UIFrameBackground : MonoBehaviour
{
	public int TargetWidth = 1280;

	public int TargetHeight = 720;

	public Vector2 ScaleDown = new Vector2(8f, 8f);

	private Vector3 m_OriginalScale;

	private void Start()
	{
		m_OriginalScale = base.transform.localScale;
		Apply();
	}

	public void Apply()
	{
		if (Screen.width <= TargetWidth && Screen.height <= TargetHeight)
		{
			base.transform.localScale = m_OriginalScale - (Vector3)ScaleDown;
		}
	}
}

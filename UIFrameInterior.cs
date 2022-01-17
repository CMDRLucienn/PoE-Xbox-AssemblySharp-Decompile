using UnityEngine;

public class UIFrameInterior : MonoBehaviour
{
	public int TargetWidth = 1280;

	public int TargetHeight = 720;

	public Vector2 TargetOffset = Vector2.zero;

	private void Start()
	{
		Apply();
	}

	public void Apply()
	{
		if (Screen.width <= TargetWidth && Screen.height <= TargetHeight)
		{
			UIAnchor component = GetComponent<UIAnchor>();
			if ((bool)component)
			{
				component.pixelOffset = TargetOffset;
			}
		}
	}
}

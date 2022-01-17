using UnityEngine;

public class UIFrameExterior : MonoBehaviour
{
	public int TargetWidth = 1280;

	public int TargetHeight = 720;

	private void Start()
	{
		Apply();
	}

	public void Apply()
	{
		if (Screen.width <= TargetWidth && Screen.height <= TargetHeight)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}
}

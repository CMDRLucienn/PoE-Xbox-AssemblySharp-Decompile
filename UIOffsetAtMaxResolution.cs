using UnityEngine;

public class UIOffsetAtMaxResolution : MonoBehaviour
{
	public Vector2 Offset;

	public int TriggerHeight = 900;

	private void Awake()
	{
		Update();
	}

	private void Update()
	{
		if (Screen.height >= TriggerHeight)
		{
			UIAnchor component = GetComponent<UIAnchor>();
			if ((bool)component)
			{
				component.pixelOffset = Offset;
			}
			else
			{
				base.transform.localPosition += (Vector3)Offset;
			}
		}
	}
}

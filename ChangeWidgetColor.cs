using UnityEngine;

[ExecuteInEditMode]
public class ChangeWidgetColor : MonoBehaviour
{
	public Color color;

	private void Update()
	{
		UIWidget component = GetComponent<UIWidget>();
		if ((bool)component)
		{
			component.color = color;
		}
	}
}

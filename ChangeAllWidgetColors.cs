using UnityEngine;

[ExecuteInEditMode]
public class ChangeAllWidgetColors : MonoBehaviour
{
	private void Start()
	{
		DoRecolor();
	}

	private void DoRecolor()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>();
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (!(uIWidget is UILabel))
			{
				uIWidget.color = new Color(0f, 0f, 0f, 1f);
			}
		}
	}
}

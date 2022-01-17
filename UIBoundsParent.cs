using UnityEngine;

public class UIBoundsParent : MonoBehaviour
{
	public UIWidget BoundedBy;

	private void Awake()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (uIWidget != BoundedBy)
			{
				uIWidget.containedBy = this;
			}
		}
	}
}

using UnityEngine;

public class PanelFocusModifier : MonoBehaviour
{
	public UIPanel PanelToModifier;

	public float HoverAlpha;

	public float DefaultAlpha;

	private void Update()
	{
		if (!PanelToModifier || (double)PanelToModifier.alpha == 0.0)
		{
			return;
		}
		if (UICamera.Raycast(Input.mousePosition, out var hit))
		{
			if (PanelToModifier == NGUITools.FindInParents<UIPanel>(hit.collider.gameObject))
			{
				PanelToModifier.alpha = HoverAlpha;
			}
		}
		else
		{
			PanelToModifier.alpha = DefaultAlpha;
		}
	}
}

using UnityEngine;

public class UISelectionWidgetHighlight : MonoBehaviour
{
	public UIAnchor Anchor;

	public UIStretch Stretch;

	public void SetWidgetTarget(UIWidget widgetToTarget)
	{
		if (Anchor != null)
		{
			Anchor.widgetContainer = widgetToTarget;
		}
		if (Stretch != null)
		{
			Stretch.widgetContainer = widgetToTarget;
		}
	}

	public void Refresh()
	{
		if (Anchor != null)
		{
			Anchor.Update();
		}
		if (Stretch != null)
		{
			Stretch.Update();
		}
	}
}

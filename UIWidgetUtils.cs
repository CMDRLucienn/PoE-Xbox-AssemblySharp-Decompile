using UnityEngine;

public static class UIWidgetUtils
{
	public static bool PivotIsLeft(UIWidget.Pivot piv)
	{
		if (piv != UIWidget.Pivot.Left && piv != 0)
		{
			return piv == UIWidget.Pivot.BottomLeft;
		}
		return true;
	}

	public static bool PivotIsRight(UIWidget.Pivot piv)
	{
		if (piv != UIWidget.Pivot.Right && piv != UIWidget.Pivot.TopRight)
		{
			return piv == UIWidget.Pivot.BottomRight;
		}
		return true;
	}

	public static bool PivotIsTop(UIWidget.Pivot piv)
	{
		if (piv != 0 && piv != UIWidget.Pivot.Top)
		{
			return piv == UIWidget.Pivot.TopRight;
		}
		return true;
	}

	public static bool PivotIsBottom(UIWidget.Pivot piv)
	{
		if (piv != UIWidget.Pivot.BottomLeft && piv != UIWidget.Pivot.Bottom)
		{
			return piv == UIWidget.Pivot.BottomRight;
		}
		return true;
	}

	public static bool AnchorIsLeft(UIAnchor.Side side)
	{
		if (side != UIAnchor.Side.Left && side != UIAnchor.Side.TopLeft)
		{
			return side == UIAnchor.Side.BottomLeft;
		}
		return true;
	}

	public static bool AnchorIsRight(UIAnchor.Side side)
	{
		if (side != UIAnchor.Side.Right && side != UIAnchor.Side.TopRight)
		{
			return side == UIAnchor.Side.BottomRight;
		}
		return true;
	}

	public static bool AnchorIsTop(UIAnchor.Side side)
	{
		if (side != UIAnchor.Side.Top && side != UIAnchor.Side.TopLeft)
		{
			return side == UIAnchor.Side.TopRight;
		}
		return true;
	}

	public static bool AnchorIsBottom(UIAnchor.Side side)
	{
		if (side != UIAnchor.Side.Bottom && side != 0)
		{
			return side == UIAnchor.Side.BottomRight;
		}
		return true;
	}

	public static int PivotDirX(UIWidget.Pivot piv)
	{
		if (PivotIsRight(piv))
		{
			return 1;
		}
		if (PivotIsLeft(piv))
		{
			return -1;
		}
		return 0;
	}

	public static int PivotDirY(UIWidget.Pivot piv)
	{
		if (PivotIsTop(piv))
		{
			return 1;
		}
		if (PivotIsBottom(piv))
		{
			return -1;
		}
		return 0;
	}

	public static UIWidget.Pivot PivotFrom(int x, int y)
	{
		if (x < 0)
		{
			if (y < 0)
			{
				return UIWidget.Pivot.BottomLeft;
			}
			if (y > 0)
			{
				return UIWidget.Pivot.TopLeft;
			}
			return UIWidget.Pivot.Left;
		}
		if (x > 0)
		{
			if (y < 0)
			{
				return UIWidget.Pivot.BottomRight;
			}
			if (y > 0)
			{
				return UIWidget.Pivot.TopRight;
			}
			return UIWidget.Pivot.Right;
		}
		if (y < 0)
		{
			return UIWidget.Pivot.Bottom;
		}
		if (y > 0)
		{
			return UIWidget.Pivot.Top;
		}
		return UIWidget.Pivot.Center;
	}

	public static UIWidget.Pivot PivotFrom(UIWidget.Pivot root, int xoff, int yoff)
	{
		int num = PivotDirX(root);
		int num2 = PivotDirY(root);
		if (xoff != 0 && xoff == -num)
		{
			num = -num;
		}
		if (yoff != 0 && yoff == -num2)
		{
			num2 = -num2;
		}
		return PivotFrom(num, num2);
	}

	public static void UpdateAnchors(GameObject parent, int level)
	{
		UIAnchor[] componentsInChildren = parent.GetComponentsInChildren<UIAnchor>(includeInactive: true);
		for (int num = level - 1; num >= 0; num--)
		{
			UIAnchor[] array = componentsInChildren;
			foreach (UIAnchor uIAnchor in array)
			{
				if (uIAnchor.enabled)
				{
					uIAnchor.Update();
				}
			}
		}
		UIAnchorToMaximum[] componentsInChildren2 = parent.GetComponentsInChildren<UIAnchorToMaximum>(includeInactive: true);
		for (int num2 = level - 1; num2 >= 0; num2--)
		{
			UIAnchorToMaximum[] array2 = componentsInChildren2;
			foreach (UIAnchorToMaximum uIAnchorToMaximum in array2)
			{
				if (uIAnchorToMaximum.enabled)
				{
					uIAnchorToMaximum.Update();
				}
			}
		}
	}

	public static void UpdateStretches(GameObject parent, int level)
	{
		UIStretch[] componentsInChildren = parent.GetComponentsInChildren<UIStretch>(includeInactive: true);
		for (int num = level - 1; num >= 0; num--)
		{
			UIStretch[] array = componentsInChildren;
			foreach (UIStretch uIStretch in array)
			{
				if (uIStretch.enabled)
				{
					uIStretch.Update();
				}
			}
		}
	}

	public static void UpdateDependents(GameObject parent, int level)
	{
		UIPanelOrigin[] componentsInChildren = parent.GetComponentsInChildren<UIPanelOrigin>(includeInactive: true);
		UIStretchToContents[] componentsInChildren2 = parent.GetComponentsInChildren<UIStretchToContents>();
		UIShrinkOpposingWidget[] componentsInChildren3 = parent.GetComponentsInChildren<UIShrinkOpposingWidget>();
		for (int num = level - 1; num >= 0; num--)
		{
			UpdateAnchors(parent, 1);
			UpdateStretches(parent, 1);
			UIPanelOrigin[] array = componentsInChildren;
			foreach (UIPanelOrigin uIPanelOrigin in array)
			{
				if (uIPanelOrigin.enabled)
				{
					uIPanelOrigin.DoUpdate();
				}
			}
			UIStretchToContents[] array2 = componentsInChildren2;
			foreach (UIStretchToContents uIStretchToContents in array2)
			{
				if (uIStretchToContents.enabled)
				{
					uIStretchToContents.DoUpdate();
				}
			}
			UIShrinkOpposingWidget[] array3 = componentsInChildren3;
			foreach (UIShrinkOpposingWidget uIShrinkOpposingWidget in array3)
			{
				if (uIShrinkOpposingWidget.enabled)
				{
					uIShrinkOpposingWidget.DoUpdate();
				}
			}
		}
	}
}

using UnityEngine;

[ExecuteInEditMode]
public class UIStretchToContents : MonoBehaviour
{
	public GameObject Container;

	private Bounds m_Bounds;

	public UIWidget[] Ignore;

	public float LeftRightMargin;

	public float TopBottomMargin;

	public bool enableAnchoring;

	public UIWidget.Pivot BoundsAnchor;

	private Vector3 anchorNewPos;

	private Vector3 boundsMin;

	private Vector3 boundsMax;

	private Vector3 boundsCenter;

	public float MinWidth;

	public float MinHeight;

	public bool DisableX;

	public bool DisableY;

	public Bounds Bounds => m_Bounds;

	public void DoUpdate()
	{
		Update();
	}

	private void Update()
	{
		if (!Container)
		{
			return;
		}
		Transform transform = base.transform;
		Vector3 localScale = transform.localScale;
		transform.localScale = Vector3.zero;
		UIWidget[] ignore;
		if (Ignore != null)
		{
			ignore = Ignore;
			foreach (UIWidget uIWidget in ignore)
			{
				if ((bool)uIWidget)
				{
					uIWidget.alpha = 0f;
				}
			}
		}
		m_Bounds = NGUIMath.CalculateRelativeWidgetBounds(Container.transform);
		Vector3 localScale2 = new Vector3(Mathf.Floor(Mathf.Max(MinWidth, m_Bounds.size.x + LeftRightMargin * 2f)), Mathf.Floor(Mathf.Max(MinHeight, m_Bounds.size.y + TopBottomMargin * 2f)), 1f);
		if (DisableX)
		{
			localScale2.x = localScale.x;
		}
		if (DisableY)
		{
			localScale2.y = localScale.y;
		}
		transform.localScale = localScale2;
		if (enableAnchoring)
		{
			boundsMin = m_Bounds.min;
			boundsMax = m_Bounds.max;
			boundsCenter = m_Bounds.center;
			switch (BoundsAnchor)
			{
			case UIWidget.Pivot.Center:
				anchorNewPos = m_Bounds.center;
				break;
			case UIWidget.Pivot.Bottom:
				anchorNewPos.Set(boundsCenter.x, boundsMin.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.BottomLeft:
				anchorNewPos.Set(boundsMin.x, boundsMin.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.BottomRight:
				anchorNewPos.Set(boundsMax.x, boundsMin.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.Top:
				anchorNewPos.Set(boundsCenter.x, boundsMax.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.TopLeft:
				anchorNewPos.Set(boundsMin.x, boundsMax.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.TopRight:
				anchorNewPos.Set(boundsMax.x, boundsMax.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.Left:
				anchorNewPos.Set(boundsMin.x, boundsCenter.y, boundsCenter.z);
				break;
			case UIWidget.Pivot.Right:
				anchorNewPos.Set(boundsMax.x, boundsCenter.y, boundsCenter.z);
				break;
			}
			anchorNewPos = Container.transform.TransformPoint(anchorNewPos);
			anchorNewPos.z = transform.position.z;
			transform.position = anchorNewPos;
		}
		if (Ignore == null)
		{
			return;
		}
		ignore = Ignore;
		foreach (UIWidget uIWidget2 in ignore)
		{
			if ((bool)uIWidget2)
			{
				uIWidget2.alpha = 1f;
			}
		}
	}
}

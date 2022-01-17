using UnityEngine;

public class UILikeContext : MonoBehaviour
{
	public delegate void Flipped(bool flipped);

	private UIWidget m_Wig;

	public UIWidget.Pivot Prefer;

	public UIAnchor PivotAnchor;

	private UIWidget m_LastWidgetContainer;

	private UIResolutionScaler m_cachedParentScaler;

	private bool m_IsFlipped;

	public bool IsFlipped
	{
		get
		{
			return m_IsFlipped;
		}
		private set
		{
			if (value != m_IsFlipped)
			{
				m_IsFlipped = value;
				if (this.OnFlipped != null)
				{
					this.OnFlipped(m_IsFlipped);
				}
			}
		}
	}

	public event Flipped OnFlipped;

	private void Start()
	{
		m_Wig = GetComponent<UIWidget>();
	}

	private void Recalculate()
	{
		if ((bool)PivotAnchor)
		{
			if (m_LastWidgetContainer != PivotAnchor.widgetContainer)
			{
				m_LastWidgetContainer = PivotAnchor.widgetContainer;
				m_cachedParentScaler = (m_LastWidgetContainer ? m_LastWidgetContainer.GetComponentInParent<UIResolutionScaler>() : null);
			}
		}
		else
		{
			m_LastWidgetContainer = null;
			m_cachedParentScaler = null;
		}
		m_Wig.pivot = Prefer;
		if ((bool)PivotAnchor && PivotAnchor.enabled)
		{
			if (UIWidgetUtils.PivotIsBottom(m_Wig.pivot))
			{
				PivotAnchor.side = UIAnchor.Side.TopLeft;
			}
			else
			{
				PivotAnchor.side = UIAnchor.Side.BottomLeft;
			}
			PivotAnchor.pixelOffset = Vector3.zero;
			PivotAnchor.Update();
		}
		m_Wig.transform.localPosition = Vector3.zero;
		m_Wig.MarkAsChanged();
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(m_Wig.transform);
		Vector3 vector = InGameUILayout.NguiToScreen(new Vector3(bounds.min.x / InGameUILayout.Root.transform.localScale.x, bounds.min.y / InGameUILayout.Root.transform.localScale.y, bounds.min.z / InGameUILayout.Root.transform.localScale.z));
		Vector3 vector2 = InGameUILayout.NguiToScreen(new Vector3(bounds.max.x / InGameUILayout.Root.transform.localScale.x, bounds.max.y / InGameUILayout.Root.transform.localScale.y, bounds.max.z / InGameUILayout.Root.transform.localScale.z));
		int xoff = 0;
		if (UIWidgetUtils.PivotIsLeft(Prefer) && Mathf.Max(vector.x, vector2.x) >= (float)Screen.width)
		{
			xoff = 1;
		}
		else if (UIWidgetUtils.PivotIsRight(Prefer) && Mathf.Min(vector.x, vector2.x) < 0f)
		{
			xoff = -1;
		}
		int yoff = 0;
		if (UIWidgetUtils.PivotIsTop(Prefer) && Mathf.Max(vector.y, vector2.y) >= (float)Screen.height)
		{
			yoff = -1;
		}
		else if (UIWidgetUtils.PivotIsBottom(Prefer) && Mathf.Min(vector.y, vector2.y) < 0f)
		{
			yoff = 1;
		}
		m_Wig.pivot = UIWidgetUtils.PivotFrom(Prefer, xoff, yoff);
		m_Wig.transform.localPosition = Vector3.zero;
		IsFlipped = m_Wig.pivot != Prefer;
		if ((bool)PivotAnchor && PivotAnchor.enabled)
		{
			switch (m_Wig.pivot)
			{
			case UIWidget.Pivot.TopLeft:
				PivotAnchor.side = UIAnchor.Side.BottomLeft;
				break;
			case UIWidget.Pivot.BottomLeft:
				PivotAnchor.side = UIAnchor.Side.TopLeft;
				break;
			case UIWidget.Pivot.BottomRight:
				PivotAnchor.side = UIAnchor.Side.BottomLeft;
				break;
			case UIWidget.Pivot.TopRight:
				PivotAnchor.side = UIAnchor.Side.TopLeft;
				break;
			}
			PivotAnchor.Update();
		}
		bounds = NGUIMath.CalculateAbsoluteWidgetBounds(m_Wig.transform);
		vector = InGameUILayout.NguiToScreen(new Vector3(bounds.min.x / InGameUILayout.Root.transform.localScale.x, bounds.min.y / InGameUILayout.Root.transform.localScale.y, bounds.min.z / InGameUILayout.Root.transform.localScale.z));
		vector2 = InGameUILayout.NguiToScreen(new Vector3(bounds.max.x / InGameUILayout.Root.transform.localScale.x, bounds.max.y / InGameUILayout.Root.transform.localScale.y, bounds.max.z / InGameUILayout.Root.transform.localScale.z));
		Vector2 zero = Vector2.zero;
		if (UIWidgetUtils.PivotIsLeft(m_Wig.pivot) && Mathf.Max(vector.x, vector2.x) >= (float)Screen.width)
		{
			zero.x = (int)Mathf.Max(vector.x, vector2.x) - Screen.width;
		}
		else if (UIWidgetUtils.PivotIsRight(m_Wig.pivot) && Mathf.Min(vector.x, vector2.x) < 0f)
		{
			zero.x = (int)Mathf.Min(vector.x, vector2.x);
		}
		if (UIWidgetUtils.PivotIsTop(m_Wig.pivot) && Mathf.Max(vector.y, vector2.y) >= (float)Screen.height)
		{
			zero.y = (int)Mathf.Max(vector.y, vector2.y) - Screen.height;
		}
		else if (UIWidgetUtils.PivotIsBottom(m_Wig.pivot) && Mathf.Min(vector.y, vector2.y) < 0f)
		{
			zero.y = (int)Mathf.Min(vector.y, vector2.y);
		}
		if ((bool)PivotAnchor && PivotAnchor.enabled)
		{
			if ((bool)m_cachedParentScaler)
			{
				zero.x /= m_cachedParentScaler.transform.localScale.x;
				zero.y /= m_cachedParentScaler.transform.localScale.y;
			}
			PivotAnchor.pixelOffset = zero;
		}
		else
		{
			m_Wig.transform.localPosition += (Vector3)zero;
		}
	}

	private void Update()
	{
		Recalculate();
	}
}

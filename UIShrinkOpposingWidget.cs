using UnityEngine;

[ExecuteInEditMode]
public class UIShrinkOpposingWidget : MonoBehaviour
{
	public bool ResizeX;

	public bool ResizeY;

	public Vector2 BaseSize;

	public UIWidget Widget;

	public UIWidget Widget2;

	private UIPanel m_Panel;

	private UILabel m_Label;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (!m_Panel)
		{
			m_Panel = GetComponent<UIPanel>();
		}
		if (!m_Label)
		{
			m_Label = GetComponent<UILabel>();
		}
	}

	public void DoUpdate()
	{
		Init();
		Update();
	}

	private void Update()
	{
		Vector2 zero = Vector2.zero;
		if ((bool)Widget)
		{
			zero += GetWidgetEffectiveSize(Widget);
		}
		if ((bool)Widget2)
		{
			zero += GetWidgetEffectiveSize(Widget2);
		}
		Vector2 vector = BaseSize - zero;
		if ((bool)m_Panel)
		{
			m_Panel.clipRange = new Vector4(m_Panel.clipRange.x, m_Panel.clipRange.y, ResizeX ? Mathf.Max(1f, vector.x) : m_Panel.clipRange.z, ResizeY ? Mathf.Max(1f, vector.y) : m_Panel.clipRange.w);
		}
		else if ((bool)m_Label)
		{
			if (ResizeX)
			{
				m_Label.lineWidth = (int)vector.x;
			}
		}
		else
		{
			base.transform.localScale = new Vector3(ResizeX ? vector.x : base.transform.localScale.x, ResizeY ? vector.y : base.transform.localScale.y, base.transform.localScale.z);
		}
	}

	private Vector2 GetWidgetEffectiveSize(UIWidget widget)
	{
		Vector2 result = new Vector2(widget.transform.localScale.x * widget.relativeSize.x, widget.transform.localScale.y * widget.relativeSize.y);
		if (!widget.isActiveAndEnabled || widget.alpha == 0f)
		{
			return Vector2.zero;
		}
		if (widget is UITexture && !(widget as UITexture).mainTexture)
		{
			return Vector2.zero;
		}
		if (UIWidgetUtils.PivotDirX(widget.pivot) == 0)
		{
			result.x /= 2f;
		}
		if (UIWidgetUtils.PivotDirY(widget.pivot) == 0)
		{
			result.y /= 2f;
		}
		return result;
	}
}

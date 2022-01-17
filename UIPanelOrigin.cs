using UnityEngine;

[RequireComponent(typeof(UIPanel))]
[ExecuteInEditMode]
public class UIPanelOrigin : MonoBehaviour
{
	public UIWidget.Pivot Pivot;

	public Vector2 PixelOffset;

	private UIPanel m_Panel;

	public bool RunOnlyOnce = true;

	public bool DetectSizeOnly;

	private Vector2 m_LastSize;

	private bool m_WillBeDestroyed;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Panel == null)
		{
			m_Panel = GetComponent<UIPanel>();
			if ((bool)m_Panel)
			{
				m_LastSize = new Vector2(m_Panel.clipRange.z, m_Panel.clipRange.w);
			}
		}
	}

	public void DoUpdate()
	{
		Update();
	}

	private void Update()
	{
		Init();
		if (m_Panel == null)
		{
			return;
		}
		if (DetectSizeOnly && Application.isPlaying)
		{
			Vector2 vector = new Vector2(m_Panel.clipRange.z, m_Panel.clipRange.w);
			Vector2 vector2 = m_LastSize - vector;
			if (vector2 != Vector2.zero)
			{
				m_Panel.clipRange = new Vector4(m_Panel.clipRange.x + vector2.x * 0.5f * (float)UIWidgetUtils.PivotDirX(Pivot) + PixelOffset.x, m_Panel.clipRange.y + vector2.y * 0.5f * (float)UIWidgetUtils.PivotDirY(Pivot) + PixelOffset.y, m_Panel.clipRange.z, m_Panel.clipRange.w);
			}
			m_LastSize = vector;
		}
		else
		{
			m_Panel.clipRange = new Vector4(-0.5f * (float)UIWidgetUtils.PivotDirX(Pivot) * m_Panel.clipRange.z + PixelOffset.x, -0.5f * (float)UIWidgetUtils.PivotDirY(Pivot) * m_Panel.clipRange.w + PixelOffset.y, m_Panel.clipRange.z, m_Panel.clipRange.w);
		}
		if (RunOnlyOnce && Application.isPlaying && !m_WillBeDestroyed)
		{
			m_WillBeDestroyed = true;
			GameUtilities.Destroy(this);
		}
	}
}

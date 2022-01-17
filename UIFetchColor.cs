using System;
using UnityEngine;

[ExecuteInEditMode]
public class UIFetchColor : MonoBehaviour
{
	public UIGlobalColor.TextColor ColorType;

	public bool PreserveAlpha;

	private UIWidget m_Widget;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
		SetColor();
		if ((bool)UIGlobalColor.Instance)
		{
			UIGlobalColor instance = UIGlobalColor.Instance;
			instance.OnColorChanged = (UIGlobalColor.ColorChanged)Delegate.Combine(instance.OnColorChanged, new UIGlobalColor.ColorChanged(OnColorChanged));
		}
	}

	private void OnDestroy()
	{
		if ((bool)UIGlobalColor.Instance)
		{
			UIGlobalColor instance = UIGlobalColor.Instance;
			instance.OnColorChanged = (UIGlobalColor.ColorChanged)Delegate.Remove(instance.OnColorChanged, new UIGlobalColor.ColorChanged(OnColorChanged));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnColorChanged()
	{
		SetColor();
	}

	private void SetColor()
	{
		if ((bool)m_Widget && UIGlobalColor.Instance != null)
		{
			Color color = UIGlobalColor.Instance.Get(ColorType);
			if (PreserveAlpha)
			{
				color.a = m_Widget.color.a;
			}
			m_Widget.color = color;
		}
	}
}

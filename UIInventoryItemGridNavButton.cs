using System;
using UnityEngine;

public class UIInventoryItemGridNavButton : MonoBehaviour
{
	public enum HideType
	{
		HIDE,
		DISABLE,
		SHOW
	}

	public UIInventoryItemGrid Grid;

	private UIIsButton m_Button;

	private UIWidget m_Widget;

	public HideType WhenInvalid = HideType.DISABLE;

	public HideType WhenSinglePage = HideType.DISABLE;

	public bool ScrollWheel = true;

	public int Delta = 1;

	private void Awake()
	{
		m_Button = GetComponent<UIIsButton>();
		m_Widget = GetComponent<UIWidget>();
		if ((bool)Grid)
		{
			UIInventoryItemGrid grid = Grid;
			grid.OnItemScrolled = (UIEventListener.FloatDelegate)Delegate.Combine(grid.OnItemScrolled, new UIEventListener.FloatDelegate(OnGridScrolled));
		}
	}

	private void OnDestroy()
	{
		if ((bool)Grid)
		{
			UIInventoryItemGrid grid = Grid;
			grid.OnItemScrolled = (UIEventListener.FloatDelegate)Delegate.Remove(grid.OnItemScrolled, new UIEventListener.FloatDelegate(OnGridScrolled));
		}
	}

	private void OnGridScrolled(GameObject sender, float delta)
	{
		Grid.CurrentPage -= (int)Mathf.Sign(delta);
	}

	private void Update()
	{
		HideType hideType = HideType.SHOW;
		if (Grid.CurrentPage < -Delta || Grid.CurrentPage >= Grid.PageCount - Delta)
		{
			hideType = WhenInvalid;
		}
		if (Grid.PageCount <= 1 && WhenSinglePage < hideType)
		{
			hideType = WhenSinglePage;
		}
		if ((bool)m_Widget)
		{
			m_Widget.enabled = hideType > HideType.HIDE;
		}
		if ((bool)m_Button)
		{
			m_Button.enabled = hideType > HideType.DISABLE;
		}
	}

	private void OnClick()
	{
		Grid.CurrentPage += Delta;
	}
}

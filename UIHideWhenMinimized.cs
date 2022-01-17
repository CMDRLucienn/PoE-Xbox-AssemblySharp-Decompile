using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIHideWhenMinimized : MonoBehaviour
{
	public UIHUDMinimizeButton Controller;

	public bool Invert;

	private UIWidget m_Widget;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		m_Widget.alpha = ((Controller.MinimizedState ^ Invert) ? 0f : 1f);
	}
}

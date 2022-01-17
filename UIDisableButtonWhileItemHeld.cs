using UnityEngine;

public class UIDisableButtonWhileItemHeld : MonoBehaviour
{
	private UIIsButton m_Button;

	private UIEventListener m_Listener;

	private void Start()
	{
		m_Button = GetComponent<UIIsButton>();
		m_Listener = GetComponent<UIEventListener>();
		UIGlobalInventory.Instance.OnDraggingChanged += OnDraggingChanged;
	}

	private void OnDestroy()
	{
		if ((bool)UIGlobalInventory.Instance)
		{
			UIGlobalInventory.Instance.OnDraggingChanged -= OnDraggingChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnDraggingChanged(bool dragging)
	{
		if ((bool)m_Button)
		{
			m_Button.enabled = !dragging;
		}
		if ((bool)m_Listener)
		{
			m_Listener.enabled = !dragging;
		}
	}
}

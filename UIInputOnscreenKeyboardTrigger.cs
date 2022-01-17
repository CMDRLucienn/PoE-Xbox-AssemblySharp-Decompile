using UnityEngine;

public class UIInputOnscreenKeyboardTrigger : MonoBehaviour
{
	public UIInput LinkedInput;

	private UIWidget m_Widget;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
		m_Widget.enabled = false;
		Update();
	}

	private void Update()
	{
	}

	private void OnClick()
	{
	}
}

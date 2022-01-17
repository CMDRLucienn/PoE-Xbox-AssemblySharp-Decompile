using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIHideWhenFillEmpty : MonoBehaviour
{
	public UISprite Target;

	private UIWidget m_Widget;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		m_Widget.enabled = Target.fillAmount > 0f;
	}
}

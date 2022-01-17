using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class BackerBetaVisibility : MonoBehaviour
{
	private UIWidget m_Widget;

	private void Start()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		m_Widget.alpha = (Conditionals.s_TestCommandLineArgs.Contains("bb") ? 1 : 0);
	}
}

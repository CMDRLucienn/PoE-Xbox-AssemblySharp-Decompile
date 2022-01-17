using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIAnchorWidgetActivation : MonoBehaviour
{
	public GameObject[] AllOf;

	public GameObject[] AnyOf;

	private UIWidget m_Widget;

	private void Awake()
	{
		m_Widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		bool flag = false;
		bool flag2 = AllOf.Length != 0;
		for (int i = 0; i < AllOf.Length; i++)
		{
			if (!AllOf[i].activeSelf)
			{
				flag2 = false;
				break;
			}
		}
		for (int j = 0; j < AnyOf.Length; j++)
		{
			if (AnyOf[j].activeSelf)
			{
				flag = true;
				break;
			}
		}
		m_Widget.enabled = flag || flag2;
	}
}

using UnityEngine;

public class UITweenAtPanelEdge : MonoBehaviour
{
	public UIDraggablePanel[] DragPanels;

	private UITweener[] m_Tweeners;

	public bool WithLeft;

	public bool WithRight;

	public bool WithTop;

	public bool WithBottom;

	public bool WhileNotDrag;

	private void Start()
	{
		m_Tweeners = GetComponents<UITweener>();
	}

	private void Update()
	{
		bool forward = true;
		UIDraggablePanel[] dragPanels = DragPanels;
		foreach (UIDraggablePanel uIDraggablePanel in dragPanels)
		{
			if ((bool)uIDraggablePanel && (bool)uIDraggablePanel.panel && uIDraggablePanel.isActiveAndEnabled)
			{
				Vector2 vector = uIDraggablePanel.panel.Extremes(uIDraggablePanel.bounds.min, uIDraggablePanel.bounds.max);
				forward = (WithLeft && vector.x < 0f) || (WithRight && vector.x > 0f) || (WithTop && vector.y > 0f) || (WithBottom && vector.y < 0f) || (WhileNotDrag && !uIDraggablePanel.IsDragging);
			}
		}
		UITweener[] tweeners = m_Tweeners;
		for (int i = 0; i < tweeners.Length; i++)
		{
			tweeners[i].Play(forward);
		}
	}
}

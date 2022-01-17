using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Panel Contents")]
public class UIDragPanelContents : MonoBehaviour
{
	public UIDraggablePanel draggablePanel;

	[HideInInspector]
	[SerializeField]
	private UIPanel panel;

	[Tooltip("If unset, dragging won't move the panel - only scrolling will.")]
	public bool DragEnabled = true;

	private void Awake()
	{
		if (!(panel != null))
		{
			return;
		}
		if (draggablePanel == null)
		{
			draggablePanel = panel.GetComponent<UIDraggablePanel>();
			if (draggablePanel == null)
			{
				draggablePanel = panel.gameObject.AddComponent<UIDraggablePanel>();
			}
		}
		panel = null;
	}

	private void Start()
	{
		if (draggablePanel == null)
		{
			draggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnPress(bool pressed)
	{
		if (DragEnabled && base.enabled && NGUITools.GetActive(base.gameObject) && draggablePanel != null)
		{
			draggablePanel.Press(pressed);
		}
	}

	private void OnRightPress(bool pressed)
	{
		OnPress(pressed);
	}

	private void OnDrag(Vector2 delta)
	{
		if (DragEnabled && base.enabled && NGUITools.GetActive(base.gameObject) && draggablePanel != null)
		{
			draggablePanel.Drag();
		}
	}

	private void OnScroll(float delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && draggablePanel != null)
		{
			draggablePanel.Scroll(delta);
		}
	}
}

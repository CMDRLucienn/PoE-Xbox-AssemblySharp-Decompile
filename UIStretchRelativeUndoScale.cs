using UnityEngine;

[RequireComponent(typeof(UIStretch))]
[ExecuteInEditMode]
public class UIStretchRelativeUndoScale : MonoBehaviour
{
	public Transform Target;

	private UIStretch m_Stretch;

	public bool X = true;

	public bool Y = true;

	private void Start()
	{
		m_Stretch = GetComponent<UIStretch>();
	}

	private void Update()
	{
		m_Stretch.relativeSize = new Vector2(X ? (1f / Target.transform.localScale.x) : 1f, Y ? (1f / Target.transform.localScale.y) : 1f);
	}
}

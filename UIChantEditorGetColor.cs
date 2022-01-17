using UnityEngine;

[ExecuteInEditMode]
public class UIChantEditorGetColor : MonoBehaviour
{
	public enum ChantColor
	{
		NOTE,
		RECITATION,
		LINGER
	}

	public ChantColor UseColor;

	private UIChantEditor m_Editor;

	private void Start()
	{
		m_Editor = UIChantEditor.Instance;
		UpdateColor();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void UpdateColor()
	{
		UIWidget component = GetComponent<UIWidget>();
		if ((bool)component && (bool)m_Editor)
		{
			component.color = m_Editor.GetColor(UseColor);
		}
	}
}

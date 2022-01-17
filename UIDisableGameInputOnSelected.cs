using UnityEngine;

public class UIDisableGameInputOnSelected : MonoBehaviour
{
	public UIInput InputField;

	private bool m_WasGameInputDisabled;

	private void Awake()
	{
		if (!InputField)
		{
			InputField = GetComponent<UIInput>();
		}
		if ((bool)InputField)
		{
			InputField.OnSelectedChanged += OnInputFieldSelectedChanged;
		}
	}

	private void OnDisable()
	{
		if (!m_WasGameInputDisabled && GameInput.DisableInput)
		{
			GameInput.DisableInput = false;
		}
	}

	private void OnDestroy()
	{
		if (InputField != null)
		{
			InputField.OnSelectedChanged -= OnInputFieldSelectedChanged;
		}
	}

	public void OnInputFieldSelectedChanged(UIInput source, bool willBeSelected)
	{
		if (!(source == null))
		{
			if (willBeSelected)
			{
				m_WasGameInputDisabled = GameInput.DisableInput;
				GameInput.DisableInput = true;
			}
			else if (GameInput.DisableInput)
			{
				m_WasGameInputDisabled = false;
				GameInput.DisableInput = false;
			}
		}
	}
}

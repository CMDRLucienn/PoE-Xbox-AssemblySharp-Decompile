using UnityEngine;

[RequireComponent(typeof(UIPanel))]
[ExecuteInEditMode]
public class UICopyPanelAlpha : MonoBehaviour
{
	public UIPanel From;

	private UIPanel m_Panel;

	private float m_Multiplier = 1f;

	public float Multiplier
	{
		get
		{
			return m_Multiplier;
		}
		set
		{
			m_Multiplier = value;
			m_Panel.alpha = From.alpha * Multiplier;
		}
	}

	private void Awake()
	{
		m_Panel = GetComponent<UIPanel>();
		if ((bool)From)
		{
			From.OnAlphaChanged += OnAlphaChanged;
			m_Panel.alpha = From.alpha * Multiplier;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying && (bool)From)
		{
			m_Panel.alpha = From.alpha * Multiplier;
		}
	}

	private void OnAlphaChanged(float alpha)
	{
		m_Panel.alpha = alpha * Multiplier;
	}
}

using UnityEngine;

public class UISystemMessager : MonoBehaviour
{
	private static UISystemMessager s_Instance;

	private UILabel m_Label;

	private UITweener m_Tween;

	private float m_LifeTime;

	public static UISystemMessager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		m_Label = GetComponent<UILabel>();
		m_Tween = GetComponent<UITweener>();
		m_Label.alpha = 0f;
	}

	private void Update()
	{
		if (m_LifeTime > 0f)
		{
			m_LifeTime -= TimeController.sUnscaledDelta;
			if (m_LifeTime <= 0f)
			{
				m_Tween.Play(forward: true);
			}
		}
	}

	public void PostMessage(string message, Color color)
	{
		m_Label.text = "[" + NGUITools.EncodeColor(color) + "]" + message;
		m_LifeTime = 2f + (float)message.Length * 0.05f;
		m_Tween.Reset();
		m_Label.alpha = 1f;
	}
}

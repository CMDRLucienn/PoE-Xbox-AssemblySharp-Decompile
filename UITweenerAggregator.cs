using System;
using System.Linq;
using UnityEngine;

public class UITweenerAggregator : MonoBehaviour
{
	public delegate void OnFinished(UITweenerAggregator tween);

	private UITweener[] m_Tweeners;

	private float m_MaxLen;

	public OnFinished OnAllFinished;

	[Tooltip("If set, includes all tweens here and on all children. Otherwise, just tweens here.")]
	public bool IncludeChildren = true;

	public UITweener[] Tweeners => m_Tweeners;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Tweeners == null)
		{
			if (IncludeChildren)
			{
				m_Tweeners = GetComponentsInChildren<UITweener>(includeInactive: true);
			}
			else
			{
				m_Tweeners = GetComponents<UITweener>();
			}
			for (int i = 0; i < m_Tweeners.Length; i++)
			{
				UITweener obj = m_Tweeners[i];
				obj.onFinished = (UITweener.OnFinished)Delegate.Combine(obj.onFinished, new UITweener.OnFinished(OnTweenerFinished));
			}
			m_MaxLen = m_Tweeners.Max((UITweener tw) => tw.duration);
		}
	}

	public void Play(bool forward)
	{
		Init();
		UITweener[] tweeners = m_Tweeners;
		foreach (UITweener uITweener in tweeners)
		{
			if (!forward)
			{
				uITweener.delay = m_MaxLen - uITweener.duration;
			}
			else
			{
				uITweener.delay = 0f;
			}
			uITweener.Play(forward);
		}
	}

	public void ResetTo(float pos)
	{
		Init();
		UITweener[] tweeners = m_Tweeners;
		for (int i = 0; i < tweeners.Length; i++)
		{
			tweeners[i].ResetTo(pos);
		}
	}

	private void OnTweenerFinished(UITweener tween)
	{
		for (int i = 0; i < m_Tweeners.Length; i++)
		{
			if (m_Tweeners[i].tweenFactor > 0f && m_Tweeners[i].tweenFactor < 1f)
			{
				return;
			}
		}
		if (OnAllFinished != null)
		{
			OnAllFinished(null);
		}
	}
}

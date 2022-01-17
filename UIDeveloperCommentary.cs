using System;
using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

public class UIDeveloperCommentary : MonoBehaviour
{
	public UILabel DeveloperTitle;

	public UILabel DeveloperName;

	public UIImageButtonRevised StopButton;

	public UIPanel ParchmentPanel;

	public UIPanel ParchmentSubPanel;

	private UITweener[] m_ParchmentTweens;

	private DeveloperCommentary m_Current;

	private Queue<DeveloperCommentary> m_Queue = new Queue<DeveloperCommentary>();

	public static UIDeveloperCommentary Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		GameState.OnLevelUnload += OnLevelUnload;
		ParchmentPanel.alpha = 0f;
		ParchmentSubPanel.alpha = 0f;
		m_ParchmentTweens = ParchmentPanel.GetComponentsInChildren<UITweener>(includeInactive: true);
		if (StopButton != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(StopButton.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnStopClicked));
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if ((bool)m_Current && m_Current.IsFinished)
		{
			StopAndHide();
		}
	}

	public void QueueCommentary(DeveloperCommentary commentary)
	{
		m_Queue.Enqueue(commentary);
		TryPlayNext();
	}

	private void TryPlayNext()
	{
		if (!m_Current && m_Queue.Count > 0)
		{
			m_Current = m_Queue.Dequeue();
			m_Current.PlayAudio();
			Show();
		}
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		m_Queue.Clear();
		StopAndHide();
	}

	private void OnStopClicked(GameObject go)
	{
		StopAndHide();
	}

	public void StopAndHide()
	{
		if ((bool)m_Current)
		{
			m_Current.StopAudio();
			m_Current = null;
		}
		UITweener[] parchmentTweens = m_ParchmentTweens;
		for (int i = 0; i < parchmentTweens.Length; i++)
		{
			parchmentTweens[i].Play(forward: false);
		}
	}

	private void ParchmentTweenFinished(UITweener tween)
	{
		if (tween.direction == Direction.Reverse)
		{
			m_Current = null;
			ParchmentPanel.alpha = 0f;
			ParchmentSubPanel.alpha = 0f;
			TryPlayNext();
		}
	}

	private void Show()
	{
		if (!UIWindowManager.Instance.AnyWindowShowing())
		{
			DeveloperTitle.text = m_Current.DeveloperTitle.GetText();
			DeveloperName.text = m_Current.DeveloperName.GetText();
			UITweener[] parchmentTweens = m_ParchmentTweens;
			for (int i = 0; i < parchmentTweens.Length; i++)
			{
				parchmentTweens[i].Play(forward: true);
			}
			ParchmentPanel.alpha = 1f;
			ParchmentSubPanel.alpha = 1f;
		}
	}
}

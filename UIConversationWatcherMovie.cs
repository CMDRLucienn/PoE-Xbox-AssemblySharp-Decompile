using System;
using UnityEngine;

public class UIConversationWatcherMovie : MonoBehaviour
{
	[ResourcesImageProperty]
	public string Movie;

	public AudioClip Audio;

	public MovieManager MovieMan;

	public UIWidget BlackBackground;

	public UIWidget Fader;

	private float m_Timeout;

	public static UIConversationWatcherMovie Instance { get; private set; }

	public bool Active => base.gameObject.activeSelf;

	private void Awake()
	{
		Instance = this;
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if (m_Timeout > 0f)
		{
			m_Timeout -= Time.unscaledDeltaTime;
			if (m_Timeout <= 0f)
			{
				Cancel();
			}
		}
	}

	public void Show()
	{
		m_Timeout = 0f;
		base.gameObject.SetActive(value: true);
		MovieMan.PlayTexture.alpha = 0f;
		BlackBackground.alpha = 0f;
		UITweener component = Fader.GetComponent<UITweener>();
		component.onFinished = null;
		component.Play(forward: true);
		component.onFinished = (UITweener.OnFinished)Delegate.Combine(component.onFinished, new UITweener.OnFinished(OnShowTweenFinished));
	}

	private void OnShowTweenFinished(UITweener tween)
	{
		tween.onFinished = (UITweener.OnFinished)Delegate.Remove(tween.onFinished, new UITweener.OnFinished(OnShowTweenFinished));
		MovieMan.PlayTexture.alpha = 1f;
		BlackBackground.alpha = 1f;
		MovieMan.PlayMovieAtPath(Movie, skippable: false, Audio);
		MovieMan.Loop(state: true);
		tween.Play(forward: false);
	}

	public void Hide()
	{
		if (!Active)
		{
			Cancel();
			return;
		}
		m_Timeout = 3f;
		UITweener component = Fader.GetComponent<UITweener>();
		component.onFinished = null;
		component.Play(forward: true);
		component.onFinished = (UITweener.OnFinished)Delegate.Combine(component.onFinished, new UITweener.OnFinished(OnHideTweenFinished1));
	}

	public void Cancel()
	{
		UITweener component = Fader.GetComponent<UITweener>();
		component.onFinished = null;
		component.enabled = false;
		component.ResetTo(0f);
		MovieMan.PlayTexture.alpha = 0f;
		BlackBackground.alpha = 0f;
		MovieMan.StopMovie();
		base.gameObject.SetActive(value: false);
	}

	private void OnHideTweenFinished1(UITweener tween)
	{
		tween.onFinished = (UITweener.OnFinished)Delegate.Remove(tween.onFinished, new UITweener.OnFinished(OnHideTweenFinished1));
		MovieMan.PlayTexture.alpha = 0f;
		BlackBackground.alpha = 0f;
		MovieMan.StopMovie();
		tween.Play(forward: false);
		tween.onFinished = (UITweener.OnFinished)Delegate.Combine(tween.onFinished, new UITweener.OnFinished(OnHideTweenFinished2));
	}

	private void OnHideTweenFinished2(UITweener tween)
	{
		tween.onFinished = (UITweener.OnFinished)Delegate.Remove(tween.onFinished, new UITweener.OnFinished(OnHideTweenFinished2));
		base.gameObject.SetActive(value: false);
	}
}

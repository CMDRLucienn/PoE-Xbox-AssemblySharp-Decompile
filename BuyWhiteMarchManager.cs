using System;
using UnityEngine;

public class BuyWhiteMarchManager : MonoBehaviour
{
	public GameObject PurchaseButton;

	public GameObject CloseButton;

	public UILabel ReleaseDateLabel;

	public string NotInstalledClickURL = "http://buy.pillarsofeternity.com/";

	private const string NotInstalledUrlMacStore = "https://itunes.apple.com/us/app/pillars-of-eternity/id979217373?mt=12";

	public DateTime ReleaseDate = new DateTime(2016, 2, 16);

	public UIPanel MasterPanel;

	public UIPanel Stage2Panel;

	public UITweener MasterFade;

	public UITweener Stage2Fade;

	public UITweener ScaleTween;

	private bool m_IsClosing;

	private int m_FramesToFirstShow = 30;

	public UIPanel Panel;

	public static BuyWhiteMarchManager Instance { get; private set; }

	public bool IsVisible => Panel.alpha > 0f;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		ReleaseDateLabel.text = ReleaseDate.ToLongDateString();
		UIEventListener uIEventListener = UIEventListener.Get(PurchaseButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnPurchase));
		UIEventListener uIEventListener2 = UIEventListener.Get(CloseButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClose));
		MasterFade.ResetTo(0f);
		Stage2Fade.ResetTo(0f);
		ScaleTween.ResetTo(0f);
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
		if (m_FramesToFirstShow > 0)
		{
			m_FramesToFirstShow--;
			if (m_FramesToFirstShow <= 0)
			{
				Show();
			}
		}
	}

	private void OnLanguageChanged(Language language)
	{
		ReleaseDateLabel.text = ReleaseDate.ToLongDateString();
	}

	private void OnPurchase(GameObject sender)
	{
		Application.OpenURL(NotInstalledClickURL);
	}

	private void OnClose(GameObject sender)
	{
		PlayerPrefs.SetInt("DontRemindWM2", 1);
		Close();
	}

	private void MasterFadeFinished()
	{
		if (!m_IsClosing)
		{
			ScaleTween.Play(forward: true);
		}
	}

	private void ScaleTweenFinished()
	{
		if (m_IsClosing)
		{
			MasterFade.Play(forward: false);
		}
		else
		{
			Stage2Fade.Play(forward: true);
		}
	}

	private void Stage2FadeFinished()
	{
		if (m_IsClosing)
		{
			ScaleTween.Play(forward: false);
		}
	}

	public void Show()
	{
		if (PlayerPrefs.GetInt("DontRemindWM2", 0) > 0 || GameUtilities.HasPX2())
		{
			Close();
		}
		else
		{
			MasterFade.Play(forward: true);
		}
	}

	public void Close()
	{
		m_IsClosing = true;
		Stage2Fade.Play(forward: false);
	}
}

using System;
using UnityEngine;

public class UIStealthIndicator : MonoBehaviour
{
	public const float CHAR_STEALTH_OPACITY = 0.6f;

	public UISprite Suspicious;

	public UISprite Combat;

	[Tooltip("UITweener to play when when you are being investigated")]
	public UITweener BeingInvestigatedTweener;

	[Tooltip("UITweener to play when you are being detected. Reverse play if you are being investigated.")]
	public UITweener BeingDetectedTweener;

	[Tooltip("UITweeners to tween when entering and exiting stealth mode")]
	public UITweener[] InStealthTweeners;

	private Transform m_targetTransform;

	private Transform m_transform;

	private Transform m_mainCameraTransform;

	private Health m_targetHealth;

	private AlphaControl m_targetAlphaControl;

	private UITweener m_suspiciousTweener;

	private Stealth m_stealthComponent;

	private float m_suspicion;

	public Stealth Target
	{
		get
		{
			return m_stealthComponent;
		}
		set
		{
			if ((bool)m_stealthComponent)
			{
				m_stealthComponent.OnStealthStateChanged -= GameState_OnStealthStateChanged;
			}
			m_stealthComponent = value;
			Mover component;
			if (m_stealthComponent != null)
			{
				component = m_stealthComponent.GetComponent<Mover>();
				if (component != null)
				{
					component.OnMovementUpdated -= Follow;
					component.OnMovementLateUpdated -= Follow;
				}
			}
			m_targetTransform = ((value != null) ? value.transform : null);
			m_targetHealth = ((value != null) ? value.GetComponent<Health>() : null);
			component = ((value != null) ? value.GetComponent<Mover>() : null);
			m_targetAlphaControl = ((value != null) ? value.GetComponent<AlphaControl>() : null);
			if (m_targetAlphaControl == null && value != null)
			{
				m_targetAlphaControl = value.gameObject.AddComponent<AlphaControl>();
			}
			if (component != null)
			{
				component.OnMovementUpdated += Follow;
				component.OnMovementLateUpdated += Follow;
				float num = (component.Radius - (InGameHUD.Instance ? InGameHUD.Instance.SelectionCircleWidth : 0f)) * 2f;
				Suspicious.transform.localScale = new Vector3(num, num, num);
				Combat.transform.localScale = Suspicious.transform.localScale;
			}
			if ((bool)m_stealthComponent)
			{
				m_stealthComponent.OnStealthStateChanged += GameState_OnStealthStateChanged;
			}
			if (value != null)
			{
				Follow(null, EventArgs.Empty);
			}
			GameState_OnStealthStateChanged(m_stealthComponent.IsInStealthMode(), null);
		}
	}

	private Transform MainCameraTransform
	{
		get
		{
			if (m_mainCameraTransform == null && (bool)CameraControl.Instance)
			{
				m_mainCameraTransform = CameraControl.Instance.transform;
			}
			return m_mainCameraTransform;
		}
	}

	private void Awake()
	{
		m_transform = base.transform;
		m_suspiciousTweener = Suspicious.GetComponent<UITweener>();
		GameState.OnLevelLoaded += LevelLoaded;
		GameState.OnLevelUnload += LevelUnload;
		LevelLoaded(null, EventArgs.Empty);
	}

	private void RemoveStealthingDelegates()
	{
		if (m_stealthComponent != null)
		{
			Mover component = m_stealthComponent.GetComponent<Mover>();
			if (component != null)
			{
				component.OnMovementUpdated -= Follow;
				component.OnMovementLateUpdated -= Follow;
			}
			m_stealthComponent.OnStealthStateChanged -= GameState_OnStealthStateChanged;
		}
	}

	private void LevelUnload(object sender, EventArgs e)
	{
		BeingDetectedTweener.Play(forward: false);
		BeingInvestigatedTweener.Play(forward: false);
	}

	private void LevelLoaded(object sender, EventArgs e)
	{
		if (m_stealthComponent != null)
		{
			GameState_OnStealthStateChanged(m_stealthComponent.IsInStealthMode(), null);
			Mover component = m_stealthComponent.GetComponent<Mover>();
			if (component != null)
			{
				component.OnMovementUpdated += Follow;
				component.OnMovementLateUpdated += Follow;
			}
		}
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= LevelLoaded;
		GameState.OnLevelUnload -= LevelUnload;
		RemoveStealthingDelegates();
	}

	private void GameState_OnStealthStateChanged(object sender, EventArgs e)
	{
		bool flag = Convert.ToBoolean(sender);
		UITweener[] inStealthTweeners = InStealthTweeners;
		for (int i = 0; i < inStealthTweeners.Length; i++)
		{
			inStealthTweeners[i].Play(flag);
		}
		if (MainCameraTransform != null && m_transform != null)
		{
			m_transform.rotation = Quaternion.Euler(m_transform.rotation.eulerAngles.x, MainCameraTransform.rotation.eulerAngles.y + 180f, m_transform.rotation.eulerAngles.z);
		}
		SetCharOpacity(flag);
	}

	private void SetCharOpacity(bool inStealth)
	{
		if (inStealth)
		{
			if (m_targetAlphaControl != null)
			{
				m_targetAlphaControl.FadeTo(0.6f, InStealthTweeners[0].duration);
			}
		}
		else if (m_targetAlphaControl != null)
		{
			m_targetAlphaControl.FadeIn(InStealthTweeners[0].duration);
		}
	}

	private void Follow(object sender, EventArgs e)
	{
		if (m_transform != null && m_targetTransform != null)
		{
			m_transform.position = m_targetTransform.position;
		}
	}

	private void Update()
	{
		if (m_targetHealth == null)
		{
			return;
		}
		bool flag = (bool)m_stealthComponent && m_stealthComponent.IsInStealthMode();
		BeingDetectedTweener.Play(flag && Target.IsBeingDetected && Target.HighestSuspicion < 100f);
		BeingInvestigatedTweener.Play(flag && Target.IsBeingDetected && Target.HighestSuspicion >= 100f);
		if (flag && Target != null && !m_targetHealth.Unconscious && !m_targetHealth.Dead && m_targetHealth.gameObject.activeInHierarchy)
		{
			m_suspicion = Target.HighestSuspicion;
			Suspicious.fillAmount = m_suspicion / 100f;
			m_suspiciousTweener.Play(m_suspicion < 100f);
			Combat.fillAmount = (m_suspicion - 100f) / 100f;
			if ((double)Suspicious.fillAmount >= 0.25)
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.SCOUTING_DETECTION_BEGIN);
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
	public enum FadeType
	{
		AreaTransition,
		Cutscene,
		Script,
		None
	}

	public enum FadeState
	{
		None,
		ToBlack,
		FromBlack,
		Full
	}

	private class Fade
	{
		public FadeState state;

		public float v;

		public float time;

		public float totalTime;
	}

	public delegate void OnFadeEnd();

	public OnFadeEnd OnFadeEnded;

	private Fade AreaTransitionFade = new Fade();

	private Fade CutsceneFade = new Fade();

	private Fade ScriptFade = new Fade();

	private bool m_IHaveCameraLocked;

	private FadeType m_fadeType = FadeType.None;

	private List<AudioSource> m_audioFadeList = new List<AudioSource>();

	public const float AREA_TRANSITION_FADE_IN_SPEED = 0.75f;

	public const float AREA_TRANSITION_FADE_OUT_SPEED = 0.35f;

	public UIWidget FadeTarget;

	public static FadeManager Instance { get; private set; }

	public float FadeValue { get; private set; }

	public bool FadeMusic => (AudioFadeMode & AudioFadeMode.Music) != 0;

	public bool FadeAudio => (AudioFadeMode & AudioFadeMode.Fx) != 0;

	public AudioFadeMode AudioFadeMode { get; set; }

	public FadeState CutsceneFadeState => CutsceneFade.state;

	public FadeType CurrentFadeType => m_fadeType;

	public void SetFadeTarget(UIWidget target)
	{
		if ((bool)FadeTarget)
		{
			GameUtilities.Destroy(FadeTarget.gameObject);
		}
		FadeTarget = target;
		FadeTarget.alpha = FadeValue;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FadeManager' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		GameState.OnLevelLoaded -= OnLevelLoad;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		GameState.OnLevelUnload += OnLevelUnload;
		GameState.OnLevelLoaded += OnLevelLoad;
	}

	public IEnumerator UnfadeOnLoadRoutine()
	{
		while (!UIWindowManager.s_windowsInitialized)
		{
			yield return null;
		}
		yield return null;
		yield return null;
		FadeFromBlack(FadeType.AreaTransition, 0.75f);
	}

	public bool IsFadeActive()
	{
		if (ScriptFade.state == FadeState.None && CutsceneFade.state == FadeState.None)
		{
			return AreaTransitionFade.state != FadeState.None;
		}
		return true;
	}

	private void OnLevelLoad(object sender, EventArgs e)
	{
		StartCoroutine(UnfadeOnLoadRoutine());
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
	}

	private void Update()
	{
		if (ScriptFade != null && ScriptFade.state != 0)
		{
			FadeValue = ScriptFade.v;
			m_fadeType = FadeType.Script;
		}
		else if (CutsceneFade != null && CutsceneFade.state != 0)
		{
			FadeValue = CutsceneFade.v;
			m_fadeType = FadeType.Cutscene;
		}
		else if (AreaTransitionFade != null && AreaTransitionFade.state != 0)
		{
			FadeValue = AreaTransitionFade.v;
			m_fadeType = FadeType.AreaTransition;
		}
		else
		{
			FadeValue = 0f;
			m_fadeType = FadeType.None;
			if (m_audioFadeList != null)
			{
				m_audioFadeList.Clear();
			}
		}
		UpdateFade(FadeType.AreaTransition, AreaTransitionFade);
		UpdateFade(FadeType.Cutscene, CutsceneFade);
		UpdateFade(FadeType.Script, ScriptFade);
		if ((bool)FadeTarget)
		{
			FadeTarget.alpha = FadeValue;
		}
	}

	public IEnumerator SignalOnFadeEndAfterFrameDelay()
	{
		yield return 0;
		ScriptEvent.BroadcastEvent(ScriptEvent.ScriptEvents.OnFadeToBlackFinished);
		if (OnFadeEnded != null)
		{
			OnFadeEnded();
		}
	}

	private void UpdateFade(FadeType fadeType, Fade fade)
	{
		if (fade == null)
		{
			if (CameraControl.Instance != null && m_IHaveCameraLocked)
			{
				m_IHaveCameraLocked = false;
				CameraControl.Instance.EnablePlayerControl(enableControl: true);
			}
			Debug.LogError("FadeManager: fade was null! You may end up in a black screen!");
		}
		else if (fade.state == FadeState.None)
		{
			fade.v = 0f;
		}
		else if (fade.state == FadeState.ToBlack)
		{
			fade.time += TimeController.sUnscaledDelta;
			if (fade.time >= fade.totalTime)
			{
				fade.state = FadeState.Full;
				fade.v = 1f;
				StartCoroutine(SignalOnFadeEndAfterFrameDelay());
			}
			else
			{
				fade.v = Mathf.SmoothStep(0f, 1f, fade.time / fade.totalTime);
			}
		}
		else if (fade.state == FadeState.FromBlack)
		{
			fade.time += TimeController.sUnscaledDelta;
			if (fade.time >= fade.totalTime)
			{
				FadeEnd(fadeType);
				fade.state = FadeState.None;
				fade.v = 0f;
				StartCoroutine(SignalOnFadeEndAfterFrameDelay());
			}
			else
			{
				fade.v = Mathf.SmoothStep(1f, 0f, fade.time / fade.totalTime);
			}
		}
		else if (fade.state == FadeState.Full)
		{
			fade.v = 1f;
		}
	}

	public void FadeToBlack(FadeType fadeType, float time, AudioFadeMode audioMode = AudioFadeMode.None)
	{
		FadeTo(fadeType, time, Color.black, audioMode);
	}

	public void FadeTo(FadeType fadeType, float time, Color color, AudioFadeMode audioMode = AudioFadeMode.None)
	{
		AudioFadeMode = audioMode;
		FadeStart(fadeType);
		if ((bool)FadeTarget)
		{
			FadeTarget.color = color;
		}
		switch (fadeType)
		{
		case FadeType.AreaTransition:
			if (CutsceneFade.state == FadeState.None && ScriptFade.state == FadeState.None)
			{
				AreaTransitionFade.state = FadeState.ToBlack;
				AreaTransitionFade.v = 0f;
				AreaTransitionFade.time = 0f;
				AreaTransitionFade.totalTime = time;
			}
			else
			{
				CancelFade(FadeType.AreaTransition);
			}
			break;
		case FadeType.Cutscene:
			CutsceneFade.state = FadeState.ToBlack;
			CutsceneFade.v = 0f;
			CutsceneFade.time = 0f;
			CutsceneFade.totalTime = time;
			break;
		default:
			ScriptFade.state = FadeState.ToBlack;
			ScriptFade.v = 0f;
			ScriptFade.time = 0f;
			ScriptFade.totalTime = time;
			break;
		}
		if (time == 0f && OnFadeEnded != null)
		{
			OnFadeEnded();
		}
		Update();
	}

	public void FadeFromBlack(FadeType fadeType, float time, AudioFadeMode audioMode = AudioFadeMode.None)
	{
		FadeFrom(fadeType, time, Color.black);
	}

	public void FadeFrom(FadeType fadeType, float time, Color color, AudioFadeMode audioMode = AudioFadeMode.None)
	{
		FadeEnd(fadeType);
		AudioFadeMode = audioMode;
		FadeStart(fadeType);
		if ((bool)FadeTarget)
		{
			FadeTarget.color = color;
		}
		switch (fadeType)
		{
		case FadeType.AreaTransition:
			if (CutsceneFade.state == FadeState.None && ScriptFade.state == FadeState.None)
			{
				AreaTransitionFade.state = FadeState.FromBlack;
				AreaTransitionFade.v = 1f;
				AreaTransitionFade.time = 0f;
				AreaTransitionFade.totalTime = time;
			}
			else
			{
				CancelFade(FadeType.AreaTransition);
			}
			break;
		case FadeType.Cutscene:
			CutsceneFade.state = FadeState.FromBlack;
			CutsceneFade.v = 1f;
			CutsceneFade.time = 0f;
			CutsceneFade.totalTime = time;
			break;
		default:
			ScriptFade.state = FadeState.FromBlack;
			ScriptFade.v = 1f;
			ScriptFade.time = 0f;
			ScriptFade.totalTime = time;
			break;
		}
		if (time == 0f && OnFadeEnded != null)
		{
			OnFadeEnded();
		}
		Update();
	}

	private void FadeStart(FadeType fadeType)
	{
		if (fadeType == FadeType.Script && ScriptFade.state == FadeState.None && CameraControl.Instance != null && !m_IHaveCameraLocked)
		{
			m_IHaveCameraLocked = true;
			CameraControl.Instance.EnablePlayerControl(enableControl: false);
		}
	}

	private void FadeEnd(FadeType fadeType)
	{
		if (fadeType == FadeType.Script && (bool)CameraControl.Instance && m_IHaveCameraLocked)
		{
			m_IHaveCameraLocked = false;
			CameraControl.Instance.EnablePlayerControl(enableControl: true);
		}
	}

	public void CancelFade(FadeType fadeType)
	{
		switch (fadeType)
		{
		case FadeType.AreaTransition:
			AreaTransitionFade.state = FadeState.None;
			AreaTransitionFade.v = 0f;
			break;
		case FadeType.Cutscene:
			CutsceneFade.state = FadeState.None;
			CutsceneFade.v = 0f;
			break;
		case FadeType.Script:
			ScriptFade.state = FadeState.None;
			ScriptFade.v = 0f;
			break;
		}
		if (OnFadeEnded != null)
		{
			OnFadeEnded();
		}
		FadeEnd(fadeType);
		Update();
	}
}

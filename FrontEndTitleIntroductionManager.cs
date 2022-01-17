using UnityEngine;
using UnityEngine.SceneManagement;

public class FrontEndTitleIntroductionManager : MonoBehaviour
{
	private enum IntroductionState
	{
		WaitingToStart,
		FadeOutMainMenu,
		DeveloperPresents,
		PanUpCamera,
		TitleReveal,
		FadeOutIntroduction,
		ExpansionTitleReveal,
		Complete,
		FadeOutSandwich
	}

	private enum BackgroundState
	{
		BaseGame,
		FadeToBlack,
		WhiteMarch
	}

	public AudioClip TransitionMusic;

	public Camera BackgroundCamera;

	public UILabel DeveloperPresents;

	public UISprite TitleSprite;

	public UITexture ExpansionTitle;

	public UITweener BlackFade;

	public GameObject BaseGameContent;

	public GameObject WhiteMarchContent;

	public static FrontEndTitleIntroductionManager Instance;

	public static bool ShowDebug;

	private float m_StateVariable;

	private IntroductionState m_IntroductionState;

	private float m_BackgroundStateTimer;

	private BackgroundState m_BackgroundState = BackgroundState.WhiteMarch;

	public MainMenuBackgroundType TargetBackground { get; private set; }

	public void StartFrontEndIntroduction()
	{
		m_StateVariable = 0f;
		m_IntroductionState = IntroductionState.FadeOutMainMenu;
		if (Conditionals.CommandLineArg("e3") && GameUtilities.HasPX1())
		{
			m_IntroductionState = IntroductionState.ExpansionTitleReveal;
		}
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: false);
		}
		AudioSource component = GetComponent<AudioSource>();
		if (!(component == null))
		{
			component.clip = TransitionMusic;
			component.ignoreListenerVolume = true;
			if (component.enabled)
			{
				component.Play();
			}
			TweenVolume component2 = GetComponent<TweenVolume>();
			component2.to = MusicManager.Instance.FinalMusicVolume;
			component2.Reset();
			component2.Play(forward: true);
			BlackFade.Play(forward: true);
			if ((bool)BuyWhiteMarchManager.Instance)
			{
				BuyWhiteMarchManager.Instance.Close();
			}
		}
	}

	public void SwitchBackgroundImmediate(MainMenuBackgroundType background)
	{
		TargetBackground = background;
		BaseGameContent.SetActive(background == MainMenuBackgroundType.BaseGame);
		WhiteMarchContent.SetActive(background == MainMenuBackgroundType.WhiteMarch);
	}

	public void SwitchBackground(MainMenuBackgroundType background)
	{
		TargetBackground = background;
	}

	public static bool HasStarted()
	{
		if ((bool)Instance)
		{
			return Instance.m_IntroductionState > IntroductionState.WaitingToStart;
		}
		return false;
	}

	public bool AllowMenuActivation()
	{
		return TargetBackground == MainMenuBackgroundType.BaseGame == BaseGameContent.activeSelf;
	}

	private void Start()
	{
		DeveloperPresents.alpha = 0f;
		TitleSprite.alpha = 0f;
		ExpansionTitle.alpha = 0f;
		Instance = this;
		BaseGameContent.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)UIDebug.Instance)
		{
			UIDebug.Instance.RemoveText("IntroDebug");
		}
	}

	private void OutputDebug()
	{
		if ((bool)UIDebug.Instance)
		{
			UIDebug.Instance.SetText("IntroDebug", string.Concat("State = ", m_IntroductionState, "\nStateVariable = ", m_StateVariable, "\nDeveloperImage.alpha = ", DeveloperPresents.alpha, "\nTitle.alpha = ", TitleSprite.alpha, "\nFade.alpha = ", FadeManager.Instance.FadeValue), Color.cyan, 0.15f, 0.9f);
			UIDebug.Instance.SetTextPosition("IntroDebug", 0.15f, 0.9f, UIWidget.Pivot.TopLeft);
		}
	}

	public void SkipIntroToEnd()
	{
		if (m_IntroductionState < IntroductionState.FadeOutIntroduction && m_IntroductionState != 0)
		{
			m_IntroductionState = IntroductionState.FadeOutIntroduction;
		}
	}

	private void Update()
	{
		if (ShowDebug)
		{
			OutputDebug();
		}
		if (GameInput.GetKeyDown(KeyCode.Escape))
		{
			SkipIntroToEnd();
		}
		if (m_IntroductionState == IntroductionState.WaitingToStart)
		{
			bool flag = TargetBackground == MainMenuBackgroundType.BaseGame;
			if (flag == BaseGameContent.activeSelf)
			{
				m_BackgroundState = ((!flag) ? BackgroundState.WhiteMarch : BackgroundState.BaseGame);
				m_BackgroundStateTimer = 0f;
				BlackFade.Play(forward: false);
			}
			switch (m_BackgroundState)
			{
			case BackgroundState.BaseGame:
				if (!flag)
				{
					m_BackgroundState = BackgroundState.FadeToBlack;
					m_BackgroundStateTimer = 0f;
					BlackFade.Play(forward: true);
				}
				break;
			case BackgroundState.FadeToBlack:
				m_BackgroundStateTimer += Time.deltaTime;
				if (m_BackgroundStateTimer >= BlackFade.duration)
				{
					m_StateVariable = 0f;
					if (flag)
					{
						m_BackgroundState = BackgroundState.BaseGame;
						BaseGameContent.gameObject.SetActive(value: true);
						WhiteMarchContent.gameObject.SetActive(value: false);
					}
					else
					{
						m_BackgroundState = BackgroundState.WhiteMarch;
						BaseGameContent.gameObject.SetActive(value: false);
						WhiteMarchContent.gameObject.SetActive(value: true);
					}
					BlackFade.Play(forward: false);
				}
				break;
			case BackgroundState.WhiteMarch:
				if (flag)
				{
					m_BackgroundState = BackgroundState.FadeToBlack;
					m_BackgroundStateTimer = 0f;
					BlackFade.Play(forward: true);
				}
				break;
			}
		}
		switch (m_IntroductionState)
		{
		case IntroductionState.FadeOutMainMenu:
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= BlackFade.duration)
			{
				XboxOneNativeWrapper.Instance.SetPresence("character_creation");
				m_StateVariable = 0f;
				m_IntroductionState = IntroductionState.FadeOutSandwich;
				WhiteMarchContent.gameObject.SetActive(value: false);
				BaseGameContent.gameObject.SetActive(value: true);
				BlackFade.Play(forward: false);
			}
			break;
		case IntroductionState.FadeOutSandwich:
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= BlackFade.duration)
			{
				m_StateVariable = 0f;
				m_IntroductionState = IntroductionState.PanUpCamera;
			}
			break;
		case IntroductionState.PanUpCamera:
			if (m_StateVariable == 0f)
			{
				UISwayMotion component = BackgroundCamera.GetComponent<UISwayMotion>();
				if ((bool)component)
				{
					component.enabled = false;
				}
				TweenPosition component2 = BackgroundCamera.GetComponent<TweenPosition>();
				component2.from = component2.transform.localPosition;
				component2.enabled = true;
				component2.Reset();
				component2.Play(forward: true);
				TweenOrthoSize component3 = BackgroundCamera.GetComponent<TweenOrthoSize>();
				component3.enabled = true;
				component3.Reset();
				component3.Play(forward: true);
			}
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= 3.25f)
			{
				m_StateVariable = 0f;
				m_IntroductionState = IntroductionState.DeveloperPresents;
			}
			break;
		case IntroductionState.DeveloperPresents:
		{
			float num = 2.75f;
			DeveloperPresents.gameObject.SetActive(value: true);
			if (m_StateVariable <= num)
			{
				DeveloperPresents.alpha = Mathf.Min(1f, DeveloperPresents.alpha + Time.deltaTime / 1f);
			}
			else
			{
				DeveloperPresents.alpha = Mathf.Max(0f, DeveloperPresents.alpha - Time.deltaTime / 1f);
			}
			if (DeveloperPresents.alpha == 1f)
			{
				m_StateVariable += Time.deltaTime;
			}
			if (m_StateVariable >= num && DeveloperPresents.alpha == 0f)
			{
				m_StateVariable = 0f;
				m_IntroductionState = IntroductionState.TitleReveal;
			}
			break;
		}
		case IntroductionState.TitleReveal:
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= 2.25f)
			{
				TitleSprite.gameObject.SetActive(value: true);
				TitleSprite.alpha = Mathf.Min(1f, TitleSprite.alpha + Time.deltaTime / 2f);
				if (TitleSprite.alpha == 1f)
				{
					m_StateVariable += Time.deltaTime;
				}
				if (m_StateVariable >= 11f)
				{
					m_StateVariable = 0f;
					m_IntroductionState = IntroductionState.FadeOutIntroduction;
				}
			}
			break;
		case IntroductionState.ExpansionTitleReveal:
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= 2.25f)
			{
				ExpansionTitle.gameObject.SetActive(value: true);
				ExpansionTitle.alpha = Mathf.Min(1f, ExpansionTitle.alpha + Time.deltaTime / 2f);
				if (ExpansionTitle.alpha == 1f)
				{
					m_StateVariable += Time.deltaTime;
				}
				if (m_StateVariable >= 11f)
				{
					m_StateVariable = 0f;
					m_IntroductionState = IntroductionState.FadeOutIntroduction;
				}
			}
			break;
		case IntroductionState.FadeOutIntroduction:
			m_StateVariable += Time.deltaTime;
			if (m_StateVariable >= 1.5f)
			{
				if (FadeManager.Instance.FadeValue == 0f)
				{
					GetComponent<TweenVolume>().Play(forward: false);
					FadeManager.Instance.FadeToBlack(FadeManager.FadeType.AreaTransition, 2f, AudioFadeMode.MusicAndFx);
				}
				if (FadeManager.Instance.FadeValue == 1f)
				{
					m_StateVariable = 0f;
					m_IntroductionState = IntroductionState.Complete;
				}
			}
			break;
		case IntroductionState.Complete:
		{
			m_StateVariable += Time.deltaTime;
			if (!(m_StateVariable >= 0.5f))
			{
				break;
			}
			UILoadingScreen uILoadingScreen = Object.FindObjectOfType<UILoadingScreen>();
			if ((bool)GameCursor.Instance)
			{
				GameCursor.Instance.SetShowCursor(this, state: true);
			}
			uILoadingScreen.Show();
			string text = "AR_0701_Encampment";
			if (Conditionals.CommandLineArg("bb"))
			{
				text = "AR_0001_Dyrford_Village";
			}
			else if (GameUtilities.HasPX1() && Conditionals.CommandLineArg("e3"))
			{
				text = "PX1_0101_Ogre_Camp_01";
				if (GameResources.SaveFileExists("e3demo.savegame"))
				{
					GameResources.LoadGame("e3demo.savegame");
					break;
				}
			}
			Debug.Log("\n");
			Debug.Log("------- BEGIN LEVEL LOAD INITIATED --------        Pending level = " + text + ". (NEW GAME)");
			SceneManager.LoadScene(text);
			break;
		}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Credits : MonoBehaviour
{
	[Serializable]
	public class StringToLabelPair
	{
		public string XMLNameTag;

		public UILabel LabelPrefab;
	}

	[Serializable]
	public class StringToSpritePair
	{
		public string XMLAtlasTag;

		public UISprite SpritePrefab;
	}

	public enum CreditsState
	{
		NotPlaying,
		Running,
		Finished
	}

	public static bool RunRequested;

	public static bool RunRequestedImmediate;

	public UIMainMenuManager MainMenu;

	public StringToLabelPair[] StringToLabelPairs;

	public StringToSpritePair[] StringToSpritePairs;

	public GameObject StartLocation;

	public GameObject LeftJustificationLocation;

	public GameObject RightJustificationLocation;

	public GameObject ScrollContainer;

	public GameObject StaticContainer;

	public AudioClip[] CreditsTracks;

	private int m_CreditsTrackIndex;

	private AudioSource m_AudioSource;

	private List<GameObject> m_CreditObjects = new List<GameObject>();

	private Dictionary<string, UILabel> m_TextLabelTypes = new Dictionary<string, UILabel>();

	private Dictionary<string, UISprite> m_SpriteTypes = new Dictionary<string, UISprite>();

	private UIPanel[] m_CreditsPanels;

	private Transform m_ScrollTransform;

	private Vector3 m_StopScrollPos;

	private float m_FirstObjY;

	private float m_LastObjY;

	private float m_StartEndingTimestamp;

	private float m_NextYValue;

	private CreditsState m_State;

	public CreditsState GetCreditsState()
	{
		return m_State;
	}

	private UILabel CreateNewTextLabel(Dictionary<string, string> attributes)
	{
		string key = attributes["name"];
		UILabel value = null;
		UILabel uILabel = null;
		float num = 1f;
		if (m_TextLabelTypes.TryGetValue(key, out value))
		{
			uILabel = UnityEngine.Object.Instantiate(value);
			num = value.transform.localScale.y;
			float x = 0f;
			if (attributes.ContainsKey("justify"))
			{
				string text = attributes["justify"];
				if (!(text == "left"))
				{
					if (text == "right")
					{
						x = RightJustificationLocation.transform.localPosition.x;
						uILabel.pivot = UIWidget.Pivot.Right;
					}
				}
				else
				{
					x = LeftJustificationLocation.transform.localPosition.x;
					uILabel.pivot = UIWidget.Pivot.Left;
				}
			}
			uILabel.transform.parent = ScrollContainer.gameObject.transform;
			uILabel.transform.localPosition = new Vector3(x, m_NextYValue, -5f);
			uILabel.transform.localScale = new Vector3(num, num, 1f);
		}
		return uILabel;
	}

	private UISprite CreateNewSprite(Dictionary<string, string> attributes)
	{
		if (!attributes.ContainsKey("atlas"))
		{
			return null;
		}
		string key = attributes["atlas"];
		UISprite value = null;
		UISprite uISprite = null;
		if (m_SpriteTypes.TryGetValue(key, out value))
		{
			uISprite = UnityEngine.Object.Instantiate(value);
			uISprite.spriteName = attributes["name"];
			if (uISprite.isValid)
			{
				if (attributes.ContainsKey("hold") && attributes.ContainsKey("y"))
				{
					uISprite.transform.parent = StaticContainer.gameObject.transform;
					uISprite.transform.localPosition = new Vector3(0f, FloatUtils.ParseInvariant(attributes["y"]), -5f);
					uISprite.MakePixelPerfect();
				}
				else
				{
					uISprite.transform.parent = ScrollContainer.gameObject.transform;
					uISprite.transform.localPosition = new Vector3(0f, m_NextYValue, -5f);
					uISprite.MakePixelPerfect();
				}
				float val = uISprite.transform.localScale.x;
				float val2 = uISprite.transform.localScale.y;
				float val3 = 1f;
				if (attributes.ContainsKey("width"))
				{
					FloatUtils.TryParseInvariant(attributes["width"], out val);
				}
				if (attributes.ContainsKey("height"))
				{
					FloatUtils.TryParseInvariant(attributes["height"], out val2);
				}
				if (attributes.ContainsKey("scale"))
				{
					FloatUtils.TryParseInvariant(attributes["scale"], out val3);
				}
				uISprite.transform.localScale = new Vector3(val * val3, val2 * val3, 1f);
			}
			else
			{
				GameUtilities.Destroy(uISprite.gameObject);
				uISprite = null;
			}
		}
		return uISprite;
	}

	private void LoadCredits()
	{
		XmlDocument xmlDocument = new XmlDocument();
		string xml = Resources.Load("Data/UI/credits").ToString();
		xmlDocument.LoadXml(xml);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			dictionary.Add("justify", "center");
			dictionary.Add("br_height", "6");
			dictionary.Add("name", "text_normal");
			if (childNode.Attributes != null)
			{
				foreach (XmlAttribute attribute in childNode.Attributes)
				{
					if (dictionary.ContainsKey(attribute.Name))
					{
						dictionary[attribute.Name] = attribute.Value;
					}
					else
					{
						dictionary.Add(attribute.Name, attribute.Value);
					}
				}
			}
			bool flag = true;
			switch (childNode.Name)
			{
			case "sprite":
			{
				UISprite uISprite = CreateNewSprite(dictionary);
				if (uISprite != null)
				{
					m_CreditObjects.Add(uISprite.gameObject);
					m_NextYValue -= uISprite.transform.localScale.y;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "localized_text":
			{
				UILabel uILabel = CreateNewTextLabel(dictionary);
				if (!uILabel)
				{
					break;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					uILabel.text = childNode2.InnerText;
				}
				m_CreditObjects.Add(uILabel.gameObject);
				m_NextYValue -= uILabel.transform.localScale.y;
				break;
			}
			case "text":
			{
				UILabel uILabel = CreateNewTextLabel(dictionary);
				if (!(uILabel != null))
				{
					break;
				}
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					uILabel.text = childNode3.InnerText;
				}
				m_CreditObjects.Add(uILabel.gameObject);
				m_NextYValue -= uILabel.transform.localScale.y;
				break;
			}
			}
			if (flag)
			{
				m_NextYValue -= FloatUtils.ParseInvariant(dictionary["br_height"]);
			}
			dictionary.Clear();
		}
	}

	private void PlayCreditsTrack(int index)
	{
		m_AudioSource.time = 0f;
		m_AudioSource.clip = CreditsTracks[index];
		m_AudioSource.Play();
	}

	private void PlayNextCreditsTrack()
	{
		m_CreditsTrackIndex++;
		if (m_CreditsTrackIndex >= CreditsTracks.Length)
		{
			m_CreditsTrackIndex = 0;
		}
		PlayCreditsTrack(m_CreditsTrackIndex);
	}

	public void StartCredits()
	{
		FrontEndTitleIntroductionManager.Instance.SwitchBackground(MainMenuBackgroundType.BaseGame);
		base.gameObject.SetActive(value: true);
		MainMenu.MenuActive = false;
		ScrollContainer.transform.localPosition = new Vector3(ScrollContainer.transform.localPosition.x, 0f, ScrollContainer.transform.localPosition.z);
		UIScrollController component = ScrollContainer.GetComponent<UIScrollController>();
		if (component != null)
		{
			component.Reset();
			component.SetPaused(paused: false);
		}
		m_CreditsTrackIndex = 0;
		if (CreditsTracks.Length != 0)
		{
			if ((bool)m_AudioSource)
			{
				m_AudioSource.volume = MusicManager.Instance.FinalMusicVolume;
				m_AudioSource.ignoreListenerVolume = true;
			}
			PlayCreditsTrack(m_CreditsTrackIndex);
		}
		m_State = CreditsState.Running;
		RunRequested = false;
		RunRequestedImmediate = false;
	}

	public void HandleCreditsFinished()
	{
		UIScrollController component = ScrollContainer.GetComponent<UIScrollController>();
		if (component != null)
		{
			component.SetPaused(paused: true);
		}
		FrontEndTitleIntroductionManager.Instance.SwitchBackground(GameState.Option.PreferredMainMenuBackground);
		MainMenu.MenuActive = true;
		m_State = CreditsState.Finished;
	}

	private void Start()
	{
		m_CreditsPanels = GetComponentsInChildren<UIPanel>();
		MainMenu = UnityEngine.Object.FindObjectOfType<UIMainMenuManager>();
		StringToLabelPair[] stringToLabelPairs = StringToLabelPairs;
		foreach (StringToLabelPair stringToLabelPair in stringToLabelPairs)
		{
			m_TextLabelTypes.Add(stringToLabelPair.XMLNameTag, stringToLabelPair.LabelPrefab);
		}
		StringToSpritePair[] stringToSpritePairs = StringToSpritePairs;
		foreach (StringToSpritePair stringToSpritePair in stringToSpritePairs)
		{
			m_SpriteTypes.Add(stringToSpritePair.XMLAtlasTag, stringToSpritePair.SpritePrefab);
		}
		m_NextYValue = StartLocation.gameObject.transform.localPosition.y;
		m_AudioSource = GetComponent<AudioSource>();
		LoadCredits();
		m_ScrollTransform = ScrollContainer.transform;
		GameObject gameObject = m_CreditObjects[0];
		GameObject gameObject2 = m_CreditObjects[m_CreditObjects.Count - 1];
		m_FirstObjY = m_ScrollTransform.InverseTransformPoint(gameObject.transform.position).y;
		m_LastObjY = m_ScrollTransform.InverseTransformPoint(gameObject2.transform.position).y - gameObject2.transform.localScale.y / 2f;
		m_StartEndingTimestamp = 0f;
		m_StopScrollPos = m_ScrollTransform.localPosition;
		m_StopScrollPos.y = 0f - m_LastObjY;
		if (RunRequested || RunRequestedImmediate)
		{
			if (RunRequestedImmediate)
			{
				FrontEndTitleIntroductionManager.Instance.SwitchBackgroundImmediate(MainMenuBackgroundType.BaseGame);
			}
			StartCredits();
		}
	}

	private void Update()
	{
		if (RunRequested || RunRequestedImmediate)
		{
			if (RunRequestedImmediate)
			{
				FrontEndTitleIntroductionManager.Instance.SwitchBackgroundImmediate(MainMenuBackgroundType.BaseGame);
			}
			StartCredits();
		}
		if (m_State == CreditsState.Running)
		{
			Time.timeScale = 1f;
			if (GameInput.GetKeyDown(KeyCode.Escape) || GameInput.GetKeyUp(KeyCode.Escape))
			{
				HandleCreditsFinished();
			}
			if (CreditsTracks.Length != 0 && (bool)m_AudioSource && !m_AudioSource.isPlaying)
			{
				PlayNextCreditsTrack();
			}
			UIPanel[] creditsPanels = m_CreditsPanels;
			foreach (UIPanel uIPanel in creditsPanels)
			{
				uIPanel.alpha = Mathf.Min(1f, uIPanel.alpha + 2f * Time.deltaTime);
			}
			if (m_ScrollTransform.localPosition.y + m_FirstObjY < m_FirstObjY)
			{
				m_ScrollTransform.localPosition = Vector3.zero;
			}
			else if (m_ScrollTransform.localPosition.y + m_LastObjY >= 0f)
			{
				UIScrollController component = ScrollContainer.GetComponent<UIScrollController>();
				if (component != null)
				{
					component.SetSpeed(0f);
					component.SetPaused(paused: false);
				}
				m_ScrollTransform.localPosition = m_StopScrollPos;
				if (m_StartEndingTimestamp < 0f)
				{
					m_StartEndingTimestamp = Time.time;
				}
				if (Time.time - m_StartEndingTimestamp > 4f)
				{
					HandleCreditsFinished();
				}
			}
			else
			{
				m_StartEndingTimestamp = -1f;
			}
		}
		else
		{
			if (m_State != CreditsState.Finished)
			{
				return;
			}
			if ((bool)m_AudioSource && m_AudioSource.isPlaying)
			{
				m_AudioSource.volume -= 1f * Time.deltaTime;
				if (m_AudioSource.volume <= 0f)
				{
					m_AudioSource.volume = 0f;
					m_AudioSource.Stop();
				}
			}
			UIPanel[] creditsPanels = m_CreditsPanels;
			foreach (UIPanel uIPanel2 in creditsPanels)
			{
				uIPanel2.alpha = Mathf.Max(0f, uIPanel2.alpha - 2f * Time.deltaTime);
			}
		}
	}
}

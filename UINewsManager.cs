using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using OEICommon;
using UnityEngine;
using UnityEngine.Networking;

public class UINewsManager : MonoBehaviour
{
	public TweenAlpha AnimButton;

	public TweenScale AnimBoxScaler;

	public TweenAlpha AnimTextNotification;

	public TweenAlpha AnimBannerNotification;

	public UILabel LabelNotification;

	public Collider ColliderBoxArea;

	public TweenAlpha AnimBigPanel;

	public UIDraggablePanel PanelBigNews;

	public UITable LayoutBigNews;

	public UIImageButtonRevised ButtonBigNewsClose;

	public UINewsArticle NewsArticlePrefab;

	private List<UINewsArticle> m_NewsViews = new List<UINewsArticle>();

	private List<NewsModel> m_NewsModels = new List<NewsModel>();

	private List<int> m_readArticleIDs = new List<int>();

	private char[] readArticleSeperator = new char[1] { ',' };

	private DateTime m_LastInternetRefreshTime;

	private XDocument m_OurXMLDoc;

	private string m_LoadedLanguage;

	private const string ButtonSpriteActive = "sel_circle_selected";

	private const string ButtonSpriteInactive = "sel_circle";

	private const string LocalNewsCacheFileName = "NewsArchive.xml";

	private const string LocalBannerImageCacheFileName = "NewsBanner.png";

	private const int REFRESH_DURATION = 24;

	private const string XML_Root = "Root";

	private const string XML_BannerParent = "Banner";

	private const string XML_NewsParent = "News";

	private const string XML_NewsNode = "Article";

	private const string XML_ArticlesRead = "ReadArticleIDs";

	private const string XML_LastInternetRefreshTime = "LastRefresh";

	private const string XML_Language = "Language";

	private const string NewsWebURL_Format = "http://d1079ywfijtdjs.cloudfront.net/eternity/news/{0}.xml";

	private static bool s_imageIsLoading;

	public static UINewsManager Instance { get; private set; }

	public string BannerLinkURL { get; private set; }

	public Texture2D NewsBannerImage { get; private set; }

	public static event Action<Texture2D> OnBannerTextureLoaded;

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)ColliderBoxArea)
		{
			UIEventListener uIEventListener = UIEventListener.Get(ColliderBoxArea);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnButtonClicked));
		}
		if ((bool)ButtonBigNewsClose)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(ButtonBigNewsClose);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnBigCloseClicked));
		}
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		NewsArticlePrefab.gameObject.SetActive(value: false);
		UIEventListener uIEventListener = UIEventListener.Get(ColliderBoxArea);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnButtonClicked));
		UIEventListener uIEventListener2 = UIEventListener.Get(ButtonBigNewsClose);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnBigCloseClicked));
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
	}

	public void ShowNews()
	{
		StopAllCoroutines();
		HideNewsImmediate();
		StartCoroutine(Coroutine_Startup());
	}

	public void HideNews()
	{
		StopAllCoroutines();
		AnimButton.Play(forward: false);
		AnimBigPanel.Play(forward: false);
		StartCoroutine(Coroutine_HideNotification(clearNotification: false));
	}

	public void HideNewsImmediate()
	{
		AnimButton.ResetTo(0f);
		AnimBigPanel.ResetTo(0f);
		AnimTextNotification.ResetTo(0f);
		AnimBannerNotification.ResetTo(0f);
		AnimBoxScaler.ResetTo(0f);
		AnimButton.alpha = 0f;
		AnimTextNotification.alpha = 0f;
		AnimBannerNotification.alpha = 0f;
		AnimBigPanel.alpha = 0f;
	}

	private void OnButtonClicked(GameObject go)
	{
		StopAllCoroutines();
		StartCoroutine(Coroutine_HideNotification(clearNotification: true));
		bool flag = AnimBigPanel.alpha <= 0f;
		if (flag)
		{
			LayoutBigNews.Reposition();
			LayoutBigNews.repositionNow = true;
		}
		AnimBigPanel.Play(flag);
		MarkAllNewsArticlesAsReadAndSave();
	}

	private void OnBigCloseClicked(GameObject go)
	{
		AnimBigPanel.Play(forward: false);
	}

	private void OnLanguageChanged(Language newLang)
	{
		ShowNews();
	}

	private IEnumerator Coroutine_Startup()
	{
		yield return null;
		if (GameState.Option.GetOption(GameOption.BoolOption.SHOW_NEWS))
		{
			if (m_OurXMLDoc == null)
			{
				yield return StartCoroutine(Coroutine_ReadCachedNewsFile());
			}
			if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork && ((DateTime.Now - m_LastInternetRefreshTime).TotalHours > 24.0 || m_LoadedLanguage != OEICommon.Localization.GetLanguageCode(StringTableManager.CurrentLanguage.EnumLanguage)))
			{
				yield return StartCoroutine(Coroutine_ReadInternetNewsFileAndCache());
			}
			yield return StartCoroutine(Coroutine_DisplayNewsArticles());
		}
	}

	private IEnumerator Coroutine_DisplayNewsArticles()
	{
		yield return new WaitForSeconds(2f);
		if (m_OurXMLDoc == null)
		{
			yield break;
		}
		while ((bool)BuyWhiteMarchManager.Instance && BuyWhiteMarchManager.Instance.IsVisible)
		{
			yield return null;
		}
		AnimButton.Play(forward: true);
		while (AnimButton.enabled)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		while (s_imageIsLoading)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(LabelNotification.text) || NewsBannerImage != null)
		{
			AnimBoxScaler.Play(forward: true);
			while (AnimBoxScaler.enabled)
			{
				yield return null;
			}
			AnimTextNotification.Play(forward: true);
			AnimBannerNotification.Play(forward: true);
		}
	}

	private IEnumerator Coroutine_HideNotification(bool clearNotification)
	{
		AnimTextNotification.Play(forward: false);
		AnimBannerNotification.Play(forward: false);
		while (AnimTextNotification.enabled || AnimBannerNotification.enabled)
		{
			yield return null;
		}
		AnimBoxScaler.Play(forward: false);
		if (clearNotification)
		{
			LabelNotification.text = string.Empty;
		}
	}

	private IEnumerator Coroutine_ReadInternetNewsFileAndCache()
	{
		string polledLanguage = OEICommon.Localization.GetLanguageCode(StringTableManager.CurrentLanguage.EnumLanguage);
		string webURL2 = $"http://d1079ywfijtdjs.cloudfront.net/eternity/news/{polledLanguage}.xml";
		XDocument xDocument;
		using (UnityWebRequest localizedRequest = UnityWebRequest.Get(webURL2))
		{
			yield return localizedRequest.SendWebRequest();
			if (localizedRequest.isNetworkError || localizedRequest.isHttpError)
			{
				Debug.Log("Unable to load news articles at url: " + webURL2 + " falling back on default language: " + StringTableManager.DefaultLanguage.Name);
				polledLanguage = StringTableManager.DefaultLanguage.Name;
				webURL2 = $"http://d1079ywfijtdjs.cloudfront.net/eternity/news/{polledLanguage}.xml";
				using UnityWebRequest defaultRequest = UnityWebRequest.Get(webURL2);
				yield return defaultRequest.SendWebRequest();
				if (!defaultRequest.isNetworkError && !defaultRequest.isHttpError)
				{
					Debug.LogWarning("Unable to load default news articles at url: " + webURL2);
					yield break;
				}
				using MemoryStream stream = new MemoryStream(localizedRequest.downloadHandler.data);
				xDocument = XDocument.Load(stream);
			}
			else
			{
				using MemoryStream stream2 = new MemoryStream(localizedRequest.downloadHandler.data);
				xDocument = XDocument.Load(stream2);
			}
		}
		if (xDocument == null)
		{
			yield break;
		}
		XElement xElement = xDocument.Element("Root");
		if (xElement != null)
		{
			XElement xElement2 = xElement.Element("LastRefresh");
			if (xElement2 != null)
			{
				xElement2.Value = DateTime.Now.ToString();
			}
			else
			{
				xElement.Add(new XElement("LastRefresh", DateTime.Now.ToString()));
			}
			xElement2 = xElement.Element("ReadArticleIDs");
			if (xElement2 != null)
			{
				xElement2.Value = SerializeReadArticles(m_readArticleIDs);
			}
			else
			{
				xElement.Add(new XElement("ReadArticleIDs", SerializeReadArticles(m_readArticleIDs)));
			}
			xElement2 = xElement.Element("Language");
			if (xElement2 != null)
			{
				xElement2.Value = polledLanguage;
			}
			else
			{
				xElement.Add(new XElement("Language", polledLanguage));
			}
			xDocument.Save(Path.Combine(Application.persistentDataPath, "NewsArchive.xml"));
			ReadNewsXML(xDocument, isCached: false);
		}
	}

	private IEnumerator Coroutine_ReadCachedNewsFile()
	{
		string text = Path.Combine(Application.persistentDataPath, "NewsArchive.xml");
		if (!File.Exists(text))
		{
			yield break;
		}
		string fileURI = TextUtils.LiteEscapeUrl("file:///" + text);
		using (UnityWebRequest wwwRequest = UnityWebRequest.Get(fileURI))
		{
			yield return wwwRequest.SendWebRequest();
			if (wwwRequest.isNetworkError || wwwRequest.isHttpError)
			{
				Debug.Log("Unable to load file at: " + fileURI);
				yield break;
			}
			try
			{
				using MemoryStream stream = new MemoryStream(wwwRequest.downloadHandler.data);
				XDocument xDoc = XDocument.Load(stream);
				ReadNewsXML(xDoc, isCached: true);
			}
			catch (Exception ex)
			{
				Debug.Log("Unable to read XML Document: " + ex.Message);
			}
		}
		yield return null;
	}

	private void ReadNewsXML(XDocument xDoc, bool isCached)
	{
		try
		{
			if (xDoc == null)
			{
				throw new Exception("The file: " + Path.Combine(Application.persistentDataPath, "NewsArchive.xml") + " does not have a xDoc");
			}
			XElement obj = xDoc.Root ?? throw new Exception("The file: " + Path.Combine(Application.persistentDataPath, "NewsArchive.xml") + " does not have a ROOT xml element");
			XElement xElement = obj.Element("LastRefresh");
			if (xElement != null)
			{
				DateTime.TryParse(xElement.Value, out m_LastInternetRefreshTime);
			}
			xElement = obj.Element("ReadArticleIDs");
			if (xElement != null)
			{
				m_readArticleIDs = ParseReadArticles(xElement.Value);
			}
			xElement = obj.Element("Language");
			if (xElement != null)
			{
				m_LoadedLanguage = xElement.Value;
			}
			xElement = obj.Element("Banner");
			if (xElement != null)
			{
				BannerModel bannerData = new BannerModel(xElement);
				StartCoroutine(Coroutine_LoadNewsBanner(bannerData, !isCached));
			}
			xElement = obj.Element("News");
			if (xElement == null)
			{
				throw new Exception("The file: " + Path.Combine(Application.persistentDataPath, "NewsArchive.xml") + " does not have xml element: News");
			}
			m_NewsModels.Clear();
			foreach (XElement item in xElement.Elements("Article"))
			{
				NewsModel newsModel = new NewsModel(item);
				if (newsModel.ShouldDisplay)
				{
					m_NewsModels.Add(newsModel);
				}
			}
			if (m_NewsModels.Count == 0)
			{
				throw new Exception("The file: " + Path.Combine(Application.persistentDataPath, "NewsArchive.xml") + " does not have xml element: Article");
			}
			m_NewsModels.Sort();
			bool flag = false;
			NewsModel newsModel2 = null;
			UINewsArticle uINewsArticle = null;
			while (m_NewsViews.Count < m_NewsModels.Count)
			{
				GameObject gameObject = NGUITools.AddChild(LayoutBigNews.gameObject, NewsArticlePrefab.gameObject);
				if (!(gameObject == null))
				{
					gameObject.SetActive(value: false);
					UINewsArticle component = gameObject.GetComponent<UINewsArticle>();
					m_NewsViews.Add(component);
				}
			}
			for (int i = 0; i < m_NewsModels.Count; i++)
			{
				uINewsArticle = m_NewsViews[i];
				uINewsArticle.gameObject.name = $"{i:00}_NewsArticle";
				uINewsArticle.gameObject.SetActive(value: true);
				newsModel2 = m_NewsModels[i];
				if (newsModel2 == null)
				{
					uINewsArticle.gameObject.SetActive(value: false);
					continue;
				}
				if (!flag && !m_readArticleIDs.Contains(newsModel2.ID))
				{
					flag = true;
					LabelNotification.text = $"[{NGUITools.EncodeColor(Color.white)}]{newsModel2.Date.ToShortDateString()} - [-]{newsModel2.Ticker}";
				}
				uINewsArticle.Load(newsModel2);
			}
			for (int j = m_NewsModels.Count; j < m_NewsViews.Count; j++)
			{
				m_NewsViews[j].gameObject.SetActive(value: false);
			}
			PanelBigNews.ResetPosition();
			m_OurXMLDoc = xDoc;
		}
		catch (Exception exception)
		{
			Debug.LogError("Unable to load the news due to exception.");
			Debug.LogException(exception);
		}
	}

	private IEnumerator Coroutine_LoadNewsBanner(BannerModel bannerData, bool skipCache)
	{
		if (bannerData != null && !string.IsNullOrEmpty(bannerData.ImageUrl))
		{
			BannerLinkURL = bannerData.LinkUrl;
			s_imageIsLoading = true;
			if (!skipCache)
			{
				string text = Path.Combine(Application.persistentDataPath, "NewsBanner.png");
				string uri = TextUtils.LiteEscapeUrl("file:///" + text);
				yield return StartCoroutine(Coroutine_LoadNewsBannerHelper(uri, doCache: false));
			}
			if (NewsBannerImage == null)
			{
				Debug.Log("News Banner: failed to load cached banner, going to web.");
				string uri2 = TextUtils.LiteEscapeUrl(bannerData.ImageUrl);
				yield return StartCoroutine(Coroutine_LoadNewsBannerHelper(uri2, doCache: true));
			}
			s_imageIsLoading = false;
		}
	}

	private IEnumerator Coroutine_LoadNewsBannerHelper(string uri, bool doCache)
	{
		NewsBannerImage = null;
		using (UnityWebRequest wwwRequest = UnityWebRequestTexture.GetTexture(TextUtils.LiteEscapeUrl(uri)))
		{
			yield return wwwRequest.SendWebRequest();
			if (!wwwRequest.isNetworkError && !wwwRequest.isHttpError)
			{
				NewsBannerImage = DownloadHandlerTexture.GetContent(wwwRequest);
				if (doCache)
				{
					File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "NewsBanner.png"), NewsBannerImage.EncodeToPNG());
				}
			}
			else
			{
				Debug.LogError("Error loading News Banner: " + wwwRequest.error);
			}
		}
		if (UINewsManager.OnBannerTextureLoaded != null && NewsBannerImage != null)
		{
			UINewsManager.OnBannerTextureLoaded(NewsBannerImage);
		}
	}

	private void MarkAllNewsArticlesAsReadAndSave()
	{
		bool flag = false;
		for (int i = 0; i < m_NewsModels.Count; i++)
		{
			if (!m_readArticleIDs.Contains(m_NewsModels[i].ID))
			{
				m_readArticleIDs.Add(m_NewsModels[i].ID);
				flag = true;
			}
		}
		if (!(m_OurXMLDoc != null && flag))
		{
			return;
		}
		XElement root = m_OurXMLDoc.Root;
		if (root != null)
		{
			XElement xElement = root.Element("ReadArticleIDs");
			if (xElement != null)
			{
				xElement.Value = SerializeReadArticles(m_readArticleIDs);
				m_OurXMLDoc.Save(Path.Combine(Application.persistentDataPath, "NewsArchive.xml"));
			}
		}
	}

	private string SerializeReadArticles(List<int> readArticlesCollection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < readArticlesCollection.Count; i++)
		{
			stringBuilder.Append(readArticlesCollection[i].ToString());
			stringBuilder.Append(readArticleSeperator[0]);
		}
		return stringBuilder.ToString();
	}

	private List<int> ParseReadArticles(string xmlValue)
	{
		List<int> list = new List<int>();
		string[] array = xmlValue.Split(readArticleSeperator, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoadingScreen : MonoBehaviour
{
	public GameObject Content;

	public UITexture LoadingImage;

	public UILabel TipLabel;

	public UICapitularLabel CapLabel;

	public LoadingTipListList MasterTipList;

	private MapData m_NextMap;

	private Texture2D m_LoadedTexture;

	private UIAnchor m_CapLabelAnchor;

	private UIAnchor m_LowLabelAnchor;

	private void OnEnable()
	{
		Reposition();
	}

	public void Reposition()
	{
		TipLabel.MarkAsChanged();
		UIWidgetUtils.UpdateDependents(base.gameObject, 2);
	}

	private void Start()
	{
		Hide();
		m_CapLabelAnchor = CapLabel.CapitalLabel.GetComponent<UIAnchor>();
		m_LowLabelAnchor = CapLabel.LowerLabel.GetComponent<UIAnchor>();
		GameState.OnLevelLoaded += OnLevelLoaded;
		GameState.OnLevelUnload += OnLevelUnload;
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		GameState.OnLevelUnload -= OnLevelUnload;
		if ((bool)FadeManager.Instance && (bool)FadeManager.Instance.FadeTarget)
		{
			FadeManager.Instance.FadeTarget.enabled = true;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		string text = sender as string;
		if (text != null)
		{
			m_NextMap = WorldMap.Maps.GetMap(text);
		}
		else
		{
			m_NextMap = null;
			Debug.LogWarning("UILoadingScreen OnLevelUnload wasn't passed a string.");
		}
		XboxOneNativeWrapper.Instance.SetPresence(text);
		Show();
	}

	private void OnLevelLoaded(object sender, EventArgs e)
	{
		Hide();
	}

	protected void Hide()
	{
		if ((bool)Content)
		{
			UIPanel component = Content.GetComponent<UIPanel>();
			if ((bool)component)
			{
				component.alpha = 0f;
			}
			else
			{
				Content.SetActive(value: false);
			}
		}
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: true);
		}
		if ((bool)m_LoadedTexture)
		{
			Resources.UnloadAsset(m_LoadedTexture);
		}
		if ((bool)FadeManager.Instance && (bool)FadeManager.Instance.FadeTarget)
		{
			FadeManager.Instance.FadeTarget.enabled = true;
		}
	}

	public void Show()
	{
		CapLabel.text = GUIUtils.GetText(1524).ToUpper();
		if (StringTableManager.CurrentLanguage != null && StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Cyrillic)
		{
			m_CapLabelAnchor.pixelOffset.y = -50f;
			m_LowLabelAnchor.pixelOffset.y = -50f;
		}
		else
		{
			m_CapLabelAnchor.pixelOffset.y = 9.2f;
			m_LowLabelAnchor.pixelOffset.y = 9.2f;
		}
		m_LoadedTexture = LoadMapImage(m_NextMap);
		int num = 0;
		LoadingTipList[] lists = MasterTipList.Lists;
		foreach (LoadingTipList loadingTipList in lists)
		{
			if (ListApproved(loadingTipList))
			{
				num += loadingTipList.Length;
			}
		}
		int num2 = OEIRandom.Index(num);
		num = 0;
		lists = MasterTipList.Lists;
		foreach (LoadingTipList loadingTipList2 in lists)
		{
			if (!ListApproved(loadingTipList2))
			{
				continue;
			}
			if (num + loadingTipList2.Length > num2)
			{
				TipLabel.text = loadingTipList2[num2 - num].Text;
				Console.AddMessage(TipLabel.text, Console.ConsoleState.Dialogue);
				UIStretch[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIStretch>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].Update();
				}
				UIAnchor[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<UIAnchor>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].Update();
				}
				break;
			}
			num += loadingTipList2.Length;
		}
		if ((bool)Content)
		{
			UIPanel component = Content.GetComponent<UIPanel>();
			if ((bool)component)
			{
				component.alpha = 1f;
			}
			else
			{
				Content.SetActive(value: true);
			}
		}
		if ((bool)GameCursor.Instance)
		{
			GameCursor.Instance.SetShowCursor(this, state: false);
		}
		if ((bool)LoadingImage)
		{
			LoadingImage.mainTexture = m_LoadedTexture;
		}
		Reposition();
		if ((bool)FadeManager.Instance && (bool)FadeManager.Instance.FadeTarget && SceneManager.GetActiveScene().name.ToLower().Equals("mainmenu"))
		{
			FadeManager.Instance.FadeTarget.enabled = false;
		}
	}

	private bool ListApproved(LoadingTipList list)
	{
		if (m_NextMap == null)
		{
			Debug.LogWarning("NextMap is null in LoadingScreen.");
			if (!list.RestrictByArea && !list.RestrictByAct)
			{
				return !list.RestrictByMapName;
			}
			return false;
		}
		if (list != null && (!list.RestrictByArea || m_NextMap.LoadScreenType == list.Area))
		{
			_ = list.RestrictByAct;
			if (list.RestrictByMapName)
			{
				if (m_NextMap.SceneName != null)
				{
					return m_NextMap.SceneName.Equals(list.MapName);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private Texture2D LoadMapImage(MapData map)
	{
		if (Conditionals.CommandLineArg("e3") && GameUtilities.HasPX1())
		{
			return LoadMapImage(map?.LoadScreenType ?? MapData.LoadingScreenType.PX1_Ogre_Cave);
		}
		if (Conditionals.CommandLineArg("bb"))
		{
			return LoadMapImage(map?.LoadScreenType ?? MapData.LoadingScreenType.Dyrford);
		}
		return LoadMapImage(map?.LoadScreenType ?? MapData.LoadingScreenType.Encampment);
	}

	private Texture2D LoadMapImage(MapData.LoadingScreenType loadingScreenType)
	{
		return Resources.Load("Art/LoadingScreens/LSC_" + loadingScreenType.ToString() + "_01") as Texture2D;
	}
}

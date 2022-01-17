using System;
using System.Collections.Generic;
using UnityEngine;

public class UITriggerManager : MonoBehaviour
{
	public UITriggerIcon RootIcon;

	private List<UITriggerIcon> m_IconPool = new List<UITriggerIcon>();

	private List<UITriggerIcon> m_Active = new List<UITriggerIcon>();

	private bool m_Loading;

	public static UITriggerManager Instance { get; private set; }

	public UITriggerIcon SafetyIcon { get; set; }

	private void Awake()
	{
		Instance = this;
		RootIcon.gameObject.SetActive(value: false);
		GameState.OnLevelUnload += OnLevelUnload;
		GameState.OnLevelLoaded += OnLevelLoad;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameState.OnLevelUnload -= OnLevelUnload;
		GameState.OnLevelLoaded -= OnLevelLoad;
		m_IconPool.Clear();
		m_Active.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelLoad(object sender, EventArgs e)
	{
		m_Loading = false;
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		m_Loading = true;
		for (int num = m_Active.Count - 1; num >= 0; num--)
		{
			Hide(m_Active[num]);
		}
	}

	public UITriggerIcon Show(Vector3 world, Texture texture, UIEventListener.VoidDelegate clickCallback, UIEventListener.BoolDelegate hoverCallback, bool avoidHud)
	{
		if (m_Loading)
		{
			return null;
		}
		UITriggerIcon freeIcon = GetFreeIcon();
		freeIcon.Icon.mainTexture = texture;
		freeIcon.Icon.MakePixelPerfect();
		freeIcon.gameObject.SetActive(value: true);
		freeIcon.Set(world, avoidHud);
		if (clickCallback != null)
		{
			UIEventListener.Get(freeIcon.Icon.gameObject).onClick = clickCallback;
		}
		if (hoverCallback != null)
		{
			UIEventListener.Get(freeIcon.Icon.gameObject).onHover = hoverCallback;
		}
		freeIcon.ClickCallback = clickCallback;
		freeIcon.HoverCallback = hoverCallback;
		freeIcon.Show();
		m_IconPool.Remove(freeIcon);
		m_Active.Add(freeIcon);
		return freeIcon;
	}

	public void Hide(UITriggerIcon icon)
	{
		if ((bool)icon.Icon)
		{
			if (icon.ClickCallback != null)
			{
				UIEventListener.Get(icon.Icon).onClick = null;
			}
			if (icon.HoverCallback != null)
			{
				icon.HoverCallback(icon.Icon.gameObject, state: false);
				UIEventListener.Get(icon.Icon).onHover = null;
			}
		}
		icon.ClickCallback = null;
		icon.HoverCallback = null;
		if (icon == SafetyIcon)
		{
			SafetyIcon = null;
		}
		icon.Hide();
	}

	public void Pool(UITriggerIcon icon)
	{
		if (icon == SafetyIcon)
		{
			SafetyIcon = null;
		}
		m_Active.Remove(icon);
		m_IconPool.Add(icon);
	}

	private UITriggerIcon GetFreeIcon()
	{
		if (m_IconPool.Count <= 0)
		{
			UITriggerIcon component = NGUITools.AddChild(base.gameObject, RootIcon.gameObject).GetComponent<UITriggerIcon>();
			component.transform.localScale = RootIcon.transform.localScale;
			m_IconPool.Add(component);
		}
		return m_IconPool[0];
	}
}

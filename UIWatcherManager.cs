using System.Collections.Generic;
using UnityEngine;

public class UIWatcherManager : MonoBehaviour
{
	private static UIWatcherManager s_Instance;

	public UIWatcherIcon WatcherIconPrefab;

	private List<UIWatcherIcon> m_WatcherIcons = new List<UIWatcherIcon>();

	public static UIWatcherManager Instance => s_Instance;

	private void Start()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		m_WatcherIcons.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void AddWatcherIcon(GameObject target)
	{
		UIWatcherIcon uIWatcherIcon = null;
		foreach (UIWatcherIcon watcherIcon in m_WatcherIcons)
		{
			if (watcherIcon.Target == target)
			{
				watcherIcon.gameObject.SetActive(value: true);
				return;
			}
			if (!watcherIcon.gameObject.activeSelf)
			{
				uIWatcherIcon = watcherIcon;
				break;
			}
		}
		if (uIWatcherIcon == null)
		{
			uIWatcherIcon = NGUITools.AddChild(base.gameObject, WatcherIconPrefab.gameObject).GetComponent<UIWatcherIcon>();
			m_WatcherIcons.Add(uIWatcherIcon);
		}
		uIWatcherIcon.gameObject.SetActive(value: true);
		uIWatcherIcon.Target = target;
	}

	public void ShowIcon(GameObject target)
	{
		foreach (UIWatcherIcon watcherIcon in m_WatcherIcons)
		{
			if (watcherIcon.Target == target)
			{
				watcherIcon.gameObject.SetActive(value: true);
				break;
			}
		}
	}

	public void HideIcon(GameObject target)
	{
		foreach (UIWatcherIcon watcherIcon in m_WatcherIcons)
		{
			if (watcherIcon.Target == target)
			{
				watcherIcon.gameObject.SetActive(value: false);
				break;
			}
		}
	}

	public void RemovePointer(GameObject target)
	{
		foreach (UIWatcherIcon watcherIcon in m_WatcherIcons)
		{
			if (watcherIcon.Target == target)
			{
				watcherIcon.gameObject.SetActive(value: false);
			}
		}
	}
}

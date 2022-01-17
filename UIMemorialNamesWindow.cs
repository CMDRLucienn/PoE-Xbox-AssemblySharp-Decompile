using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMemorialNamesWindow : MonoBehaviour
{
	public UIGrid NameGrid;

	public GameObject NameButtonPrefab;

	public GameObject EpitaphWindow;

	public UILabel EpitapthTitle;

	public UICapitularLabel EpitaphDescription;

	private Dictionary<GameObject, MemorialContainer.Memorial> m_memorials = new Dictionary<GameObject, MemorialContainer.Memorial>();

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Clear()
	{
		foreach (KeyValuePair<GameObject, MemorialContainer.Memorial> memorial in m_memorials)
		{
			GameUtilities.Destroy(memorial.Key);
		}
		m_memorials.Clear();
	}

	public void AddMemorialName(string name, string description)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(NameButtonPrefab);
		gameObject.transform.parent = NameButtonPrefab.transform.parent;
		gameObject.transform.localScale = NameButtonPrefab.transform.localScale;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.gameObject.SetActive(value: true);
		gameObject.GetComponentsInChildren<UILabel>(includeInactive: true)[0].text = name;
		m_memorials.Add(gameObject, new MemorialContainer.Memorial(name, description));
		UIEventListener uIEventListener = UIEventListener.Get(gameObject.GetComponentsInChildren<Collider>(includeInactive: true)[0]);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
	}

	private void DeselectAllEnties()
	{
		foreach (KeyValuePair<GameObject, MemorialContainer.Memorial> memorial in m_memorials)
		{
			UIImageButtonRevised uIImageButtonRevised = memorial.Key.GetComponentsInChildren<UIImageButtonRevised>(includeInactive: true)[0];
			if ((bool)uIImageButtonRevised)
			{
				uIImageButtonRevised.ForceDown(val: false);
			}
		}
	}

	public void SelectEntry(int index)
	{
		int num = 0;
		foreach (KeyValuePair<GameObject, MemorialContainer.Memorial> memorial in m_memorials)
		{
			if (num == index)
			{
				DeselectAllEnties();
				UIImageButtonRevised uIImageButtonRevised = memorial.Key.GetComponentsInChildren<UIImageButtonRevised>(includeInactive: true)[0];
				if ((bool)uIImageButtonRevised)
				{
					uIImageButtonRevised.ForceDown(val: true);
				}
				if ((bool)EpitapthTitle)
				{
					EpitapthTitle.text = memorial.Value.Name;
				}
				if (EpitaphDescription != null)
				{
					EpitaphDescription.text = memorial.Value.Description;
				}
				break;
			}
			num++;
		}
	}

	private void OnClick(GameObject go)
	{
		foreach (KeyValuePair<GameObject, MemorialContainer.Memorial> memorial in m_memorials)
		{
			if (memorial.Key == go.transform.parent.gameObject)
			{
				EpitaphWindow.GetComponentsInChildren<UICapitularLabel>(includeInactive: true)[0].text = memorial.Value.Description;
				DeselectAllEnties();
				UIImageButtonRevised uIImageButtonRevised = memorial.Key.GetComponentsInChildren<UIImageButtonRevised>(includeInactive: true)[0];
				if ((bool)uIImageButtonRevised)
				{
					uIImageButtonRevised.ForceDown(val: true);
				}
				if ((bool)EpitapthTitle)
				{
					EpitapthTitle.text = memorial.Value.Name;
				}
				break;
			}
		}
	}
}

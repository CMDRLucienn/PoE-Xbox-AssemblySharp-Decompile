using System.Collections.Generic;
using UnityEngine;

public class UIOffscreenObjectManager : MonoBehaviour
{
	private static UIOffscreenObjectManager s_Instance;

	public UIOffscreenObjectPointer RootPointer;

	private List<UIOffscreenObjectPointer> m_Pointers = new List<UIOffscreenObjectPointer>();

	private static List<GameObject> s_Queued = new List<GameObject>();

	public static UIOffscreenObjectManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
		if (s_Queued != null)
		{
			foreach (GameObject item in s_Queued)
			{
				AddPointerHelper(item);
			}
		}
		s_Queued = null;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		m_Pointers.Clear();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		RootPointer.gameObject.SetActive(value: false);
	}

	public static void AddPointer(GameObject target)
	{
		if ((bool)Instance)
		{
			Instance.AddPointerHelper(target);
		}
		else if (s_Queued != null)
		{
			s_Queued.Add(target);
		}
	}

	private void AddPointerHelper(GameObject target)
	{
		UIOffscreenObjectPointer uIOffscreenObjectPointer = null;
		foreach (UIOffscreenObjectPointer pointer in m_Pointers)
		{
			if (pointer.Target == target)
			{
				pointer.gameObject.SetActive(value: true);
				return;
			}
			if (!pointer.gameObject.activeSelf)
			{
				uIOffscreenObjectPointer = pointer;
				break;
			}
		}
		if (uIOffscreenObjectPointer == null)
		{
			uIOffscreenObjectPointer = NGUITools.AddChild(base.gameObject, RootPointer.gameObject).GetComponent<UIOffscreenObjectPointer>();
			m_Pointers.Add(uIOffscreenObjectPointer);
		}
		uIOffscreenObjectPointer.gameObject.SetActive(value: true);
		uIOffscreenObjectPointer.Target = target;
	}

	public static void RemovePointer(GameObject target)
	{
		if ((bool)Instance)
		{
			Instance.RemovePointerHelper(target);
		}
		else if (s_Queued != null)
		{
			s_Queued.Remove(target);
		}
	}

	private void RemovePointerHelper(GameObject target)
	{
		foreach (UIOffscreenObjectPointer pointer in m_Pointers)
		{
			if (!(pointer == null) && pointer.Target == target)
			{
				pointer.gameObject.SetActive(value: false);
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

public class UIOptionsTooltip : MonoBehaviour
{
	public UILabel Label;

	private UIDraggablePanel m_DraggableParent;

	private static List<UIOptionsTooltip> s_Instances;

	private void OnDisable()
	{
		Show("");
	}

	private void Awake()
	{
		m_DraggableParent = GetComponentInParent<UIDraggablePanel>();
		if (s_Instances == null)
		{
			s_Instances = new List<UIOptionsTooltip>();
		}
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
		if ((bool)Label)
		{
			Label.text = "";
		}
		s_Instances.Add(this);
	}

	private void OnDestroy()
	{
		if (s_Instances != null)
		{
			s_Instances.Remove(this);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static void Show(string message)
	{
		if (s_Instances == null)
		{
			return;
		}
		foreach (UIOptionsTooltip s_Instance in s_Instances)
		{
			if (!(message == s_Instance.Label.text))
			{
				if (s_Instance.m_DraggableParent != null)
				{
					s_Instance.m_DraggableParent.ResetPosition();
				}
				s_Instance.Label.text = message;
				if (s_Instance.m_DraggableParent != null)
				{
					s_Instance.m_DraggableParent.ResetPosition();
				}
			}
		}
	}

	public static void Hide()
	{
		if (s_Instances == null)
		{
			return;
		}
		foreach (UIOptionsTooltip s_Instance in s_Instances)
		{
			if (s_Instance.m_DraggableParent != null)
			{
				s_Instance.m_DraggableParent.ResetPosition();
			}
			s_Instance.Label.text = "";
			if (s_Instance.m_DraggableParent != null)
			{
				s_Instance.m_DraggableParent.ResetPosition();
			}
		}
	}
}

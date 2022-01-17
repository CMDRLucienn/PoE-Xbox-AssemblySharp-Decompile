using System;
using UnityEngine;

public class UIEternityClock : MonoBehaviour
{
	public UILabel Label;

	private void Start()
	{
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
	}

	private void Update()
	{
		Label.text = WorldTime.Instance.CurrentTime.Format(GUIUtils.GetText(264)) + Environment.NewLine + WorldTime.Instance.CurrentTime.GetDate();
	}
}

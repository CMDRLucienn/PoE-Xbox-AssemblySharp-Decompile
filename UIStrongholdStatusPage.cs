using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStrongholdStatusPage : UIStrongholdParchmentSizer
{
	public UITextList TextList;

	public GameObject NoMessages;

	private List<UIStrongholdStatusItem> m_Items;

	public UILabel CountLabel;

	public GameObject CountParent;

	protected override float ContentHeight => TextList.textLabel.relativeSize.y * TextList.textLabel.transform.localScale.y;

	private void OnEnable()
	{
		Reposition();
	}

	private void Start()
	{
		Init();
		GameState.OnLevelLoaded += Reload;
		Reload(null, null);
	}

	private void OnDestroy()
	{
		if ((bool)UIStrongholdManager.Instance && (bool)UIStrongholdManager.Instance.Stronghold)
		{
			Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
			stronghold.OnLogMessage = (Stronghold.LogMessageDelegate)Delegate.Remove(stronghold.OnLogMessage, new Stronghold.LogMessageDelegate(OnNotification));
		}
		GameState.OnLevelLoaded -= Reload;
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Update()
	{
		if (UIStrongholdManager.Instance.Stronghold.UnviewedEventCountInternal > 0)
		{
			Stronghold.Instance.ViewInternal();
			RefreshCount();
		}
		base.Update();
	}

	private void Reposition()
	{
		if ((bool)TextList.textLabel && base.gameObject.activeSelf)
		{
			UpdateParchmentSize();
		}
	}

	private void Init()
	{
		if (m_Items == null)
		{
			m_Items = new List<UIStrongholdStatusItem>();
			TextList.textLabel.text = "";
			Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
			stronghold.OnLogMessage = (Stronghold.LogMessageDelegate)Delegate.Combine(stronghold.OnLogMessage, new Stronghold.LogMessageDelegate(OnNotification));
		}
	}

	private void OnNotification(Stronghold.NotificationType type, string timestamp, string message)
	{
		AddItem(timestamp + ": " + message);
	}

	public void AddItem(string message)
	{
		Init();
		NoMessages.gameObject.SetActive(value: false);
		TextList.Add(message + "\n");
		Reposition();
		RefreshCount();
	}

	private void Reload(object sender, EventArgs e)
	{
		Init();
		TextList.Clear();
		NoMessages.gameObject.SetActive(value: true);
		foreach (string item in UIStrongholdManager.Instance.Stronghold.Log)
		{
			AddItem(item);
		}
		RefreshCount();
	}

	private void RefreshCount()
	{
		CountLabel.text = Stronghold.Instance.UnviewedEventCountInternal.ToString();
		CountParent.SetActive(Stronghold.Instance.UnviewedEventCountInternal > 0);
	}
}

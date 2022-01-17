using System;
using System.Collections;
using UnityEngine;

public class RoyalEditionButton : MonoBehaviour
{
	public static bool isRoyalEditionProgressComplete;

	public ProductConfiguration.Package RequiredPackage;

	public string NotInstalledClickURL;

	public UIImageButtonRevised openPromptBttn;

	public GameObject cancelBtn;

	public GameObject continueBtn;

	private bool isPanelButtonsActive;

	public UILabel promptBttnHoverLabel;

	public GUIStringLabel text;

	private XboxOneNativeWrapper gamePass = XboxOneNativeWrapper.Instance;

	private GUIDatabaseString InstalledString = new GUIDatabaseString(2475);

	private GUIDatabaseString UninstalledString = new GUIDatabaseString(2474);

	private UISprite buttonSprite;

	public GameObject promptPanel;

	private void Start()
	{
		buttonSprite = base.transform.GetChild(0).GetComponent<UISprite>();
		promptBttnHoverLabel.enabled = false;
		openPromptBttn.enabled = true;
		SetPromptPanel(isVisable: false);
		UIEventListener uIEventListener = UIEventListener.Get(openPromptBttn.gameObject);
		uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
		UIEventListener uIEventListener2 = UIEventListener.Get(openPromptBttn.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
		UIEventListener uIEventListener3 = UIEventListener.Get(cancelBtn);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClose));
		UIEventListener uIEventListener4 = UIEventListener.Get(continueBtn);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(InitialiseCopy));
	}

	private void OnDestroy()
	{
	}

	private void SetPromptPanel(bool isVisable)
	{
		UIPanel[] componentsInChildren = promptPanel.GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = isVisable;
		}
		text.DatabaseString.StringID = 2471;
		isPanelButtonsActive = isVisable;
	}

	private void SetCopyPanel(bool isVisable)
	{
		UIPanel[] componentsInChildren = promptPanel.GetComponentsInChildren<UIPanel>();
		componentsInChildren[0].enabled = true;
		componentsInChildren[1].enabled = false;
		isPanelButtonsActive = false;
		text.DatabaseString.StringID = 2470;
		text.RefreshText();
	}

	private void OnClose(GameObject sender)
	{
		if (isPanelButtonsActive)
		{
			SetPromptPanel(isVisable: false);
		}
	}

	private void InitialiseCopy(GameObject sender)
	{
		if (isPanelButtonsActive)
		{
			CopyComplete(result: false);
		}
	}

	private IEnumerator EndCopy(bool result)
	{
		try
		{
			if (result)
			{
				text.DatabaseString.StringID = 2472;
			}
			else
			{
				text.DatabaseString.StringID = 2473;
			}
			text.RefreshText();
			yield return new WaitForSeconds(3f);
			SetPromptPanel(isVisable: false);
		}
		finally
		{
			Cursor.lockState = CursorLockMode.None;
			GameCursor.LockCursor = false;
		}
	}

	private void CopyComplete(bool result)
	{
		StartCoroutine(EndCopy(result));
	}

	private void OnHover(GameObject sender, bool over)
	{
		GamePassManager.Instance.ReEnumerate();
		if ((bool)promptBttnHoverLabel)
		{
			if (over)
			{
				promptBttnHoverLabel.enabled = true;
			}
			else
			{
				promptBttnHoverLabel.enabled = false;
			}
		}
	}

	private void OnClick(GameObject source)
	{
	}
}

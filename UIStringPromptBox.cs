using System;
using UnityEngine;

public class UIStringPromptBox : UIHudWindow
{
	public delegate void OnEndDialog(UIMessageBox.Result result, UIStringPromptBox sender);

	public UIGrid ButtonGrid;

	public UIMultiSpriteImageButton AcceptButton;

	public UIMultiSpriteImageButton CancelButton;

	private UIMessageBox.Result m_Result = UIMessageBox.Result.NEGATIVE;

	public OnEndDialog OnDialogEnd;

	public UILabel Title;

	public UILabel Text;

	public UIInput Input;

	[HideInInspector]
	public GUIDatabaseString TitleString = new GUIDatabaseString(672);

	public string ResultString { get; private set; }

	private void Start()
	{
		UIMultiSpriteImageButton acceptButton = AcceptButton;
		acceptButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(acceptButton.onClick, new UIEventListener.VoidDelegate(OnAccept));
		UIMultiSpriteImageButton cancelButton = CancelButton;
		cancelButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(cancelButton.onClick, new UIEventListener.VoidDelegate(OnCancel));
		UIInput input = Input;
		input.onSubmit = (UIInput.OnSubmit)Delegate.Combine(input.onSubmit, new UIInput.OnSubmit(OnSubmit));
	}

	private void OnSubmit(string str)
	{
		ResultString = str;
		m_Result = UIMessageBox.Result.AFFIRMATIVE;
		HideWindow();
	}

	private void OnAccept(GameObject sender)
	{
		ResultString = Input.text;
		m_Result = UIMessageBox.Result.AFFIRMATIVE;
		HideWindow();
	}

	private void OnCancel(GameObject sender)
	{
		m_Result = UIMessageBox.Result.NEGATIVE;
		HideWindow();
	}

	public override void HandleInput()
	{
		if (GameInput.GetControlDown(MappedControl.MB_CANCEL))
		{
			m_Result = UIMessageBox.Result.NEGATIVE;
			HideWindow();
		}
	}

	protected override void OnDestroy()
	{
		UIWindowManager.Instance.WindowDestroyed(this);
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected override void Show()
	{
		Title.text = TitleString.GetText();
		Input.IdentifierString = TitleString;
		Input.selected = true;
	}

	protected override bool Hide(bool forced)
	{
		if (OnDialogEnd != null)
		{
			OnDialogEnd(m_Result, this);
		}
		GameUtilities.Destroy(base.gameObject);
		return base.Hide(forced);
	}

	public void SetCurrentText(string text)
	{
		ResultString = text;
		Input.text = text;
	}
}

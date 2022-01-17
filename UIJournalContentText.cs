using System;
using System.Text;
using UnityEngine;

public class UIJournalContentText : MonoBehaviour
{
	public UILabel Label;

	private NotesPage m_Entry;

	private string m_OverrideString;

	public Collider ContentCollider;

	public Collider NameCollider;

	public UIInput NameInput;

	private float m_Blinker;

	private bool m_BlinkState;

	private string m_RawText;

	public bool DisableKeys;

	private bool m_HasKeysDisabled;

	public bool Editable => m_Entry != null;

	public bool selected
	{
		get
		{
			if ((bool)ContentCollider)
			{
				return UICamera.selectedObject == ContentCollider.gameObject;
			}
			return UICamera.selectedObject == base.gameObject;
		}
		set
		{
			GameObject gameObject = ((ContentCollider == null) ? base.gameObject : ContentCollider.gameObject);
			m_BlinkState = true;
			m_Blinker = 0.5f;
			if (!value && UICamera.selectedObject == gameObject)
			{
				UICamera.selectedObject = null;
			}
			else if (value && UICamera.selectedObject != gameObject)
			{
				TryShowKeyboard();
				UICamera.selectedObject = gameObject;
			}
			UpdateBlink();
		}
	}

	public event Action<UIJournalContentText> ContentUpdated;

	private void Start()
	{
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
		UIJournalManager instance = UIJournalManager.Instance;
		instance.OnHandleInput = (UIHudWindow.HandleInputDelegate)Delegate.Combine(instance.OnHandleInput, new UIHudWindow.HandleInputDelegate(HandleInput));
		if ((bool)ContentCollider)
		{
			UIEventListener uIEventListener = UIEventListener.Get(ContentCollider);
			uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnChildSelect));
			UIEventListener uIEventListener2 = UIEventListener.Get(ContentCollider);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		}
		if ((bool)NameCollider)
		{
			UIEventListener uIEventListener3 = UIEventListener.Get(NameCollider);
			uIEventListener3.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onSelect, new UIEventListener.BoolDelegate(OnNameSelect));
		}
	}

	private void OnNameSelect(GameObject sender, bool state)
	{
		if (Editable)
		{
			NameInput.selected = state;
		}
	}

	private void OnDestroy()
	{
		this.ContentUpdated = null;
	}

	private void OnChildClick(GameObject sender)
	{
		TryShowKeyboard();
	}

	private void OnChildSelect(GameObject sender, bool state)
	{
		if (state)
		{
			selected = true;
		}
	}

	private void OnTitleSubmit(string istring)
	{
		if (m_Entry != null)
		{
			m_Entry.UserTitle = istring;
			UIJournalManager.Instance.RefreshItems();
		}
	}

	private void OnRecievedText(string text)
	{
		m_Entry.SetFullText(text);
		UpdateLabel();
	}

	private void OnEnable()
	{
		selected = true;
	}

	private void OnDisable()
	{
		selected = false;
		if (DisableKeys && m_HasKeysDisabled)
		{
			GameInput.EndBlockAllKeys();
			m_HasKeysDisabled = false;
		}
	}

	private void Update()
	{
		if (Editable)
		{
			m_Blinker -= TimeController.sUnscaledDelta;
			if (m_Blinker <= 0f)
			{
				m_Blinker += 0.5f;
				m_BlinkState = !m_BlinkState;
				UpdateBlink();
			}
		}
		if (DisableKeys)
		{
			if ((selected || NameInput.selected) && !m_HasKeysDisabled)
			{
				GameInput.BeginBlockAllKeys();
				m_HasKeysDisabled = true;
			}
			else if (!selected && !NameInput.selected && m_HasKeysDisabled)
			{
				GameInput.EndBlockAllKeys();
				m_HasKeysDisabled = false;
			}
		}
	}

	private void TryShowKeyboard()
	{
		_ = m_Entry;
	}

	public void SetNote(NotesPage note)
	{
		m_Entry = note;
		m_OverrideString = null;
		selected = true;
		UpdateLabel();
	}

	public void SetText(string text)
	{
		m_Entry = null;
		m_OverrideString = text;
		UpdateLabel();
	}

	public void SetClear()
	{
		m_Entry = null;
		m_OverrideString = null;
		UpdateLabel();
	}

	public void AppendText(string text)
	{
		if (m_OverrideString != null)
		{
			m_OverrideString += text;
		}
		UpdateLabel();
	}

	private void HandleInput()
	{
		if (!Editable || !selected)
		{
			return;
		}
		string inputString = Input.inputString;
		if (string.IsNullOrEmpty(inputString))
		{
			return;
		}
		m_BlinkState = true;
		m_Blinker = 0.5f;
		for (int i = 0; i < inputString.Length; i++)
		{
			NotesPage.NoteEntry noteEntry = m_Entry.Notes[m_Entry.Notes.Count - 1];
			if (inputString[i] == '\b' && m_Entry.Notes.Count > 0)
			{
				if (noteEntry.Text != null && noteEntry.Text.Length > 0)
				{
					noteEntry.Backspace();
				}
				else if (m_Entry.Notes.Count > 1)
				{
					m_Entry.RemoveLast();
				}
			}
			else if (inputString[i] == '\r' || inputString[i] == '\n')
			{
				if (!string.IsNullOrEmpty(m_Entry.Notes[m_Entry.Notes.Count - 1].Text))
				{
					m_Entry.AddNew();
				}
			}
			else
			{
				noteEntry.Text += inputString[i];
			}
		}
		UpdateLabel();
		if (this.ContentUpdated != null)
		{
			this.ContentUpdated(this);
		}
	}

	private void UpdateLabel()
	{
		if (m_Entry != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < m_Entry.Notes.Count; i++)
			{
				EternityDateTime timestamp = m_Entry.Notes[i].Timestamp;
				stringBuilder.AppendLine(timestamp.Format(GUIUtils.GetText(264)) + " (" + timestamp.GetDate() + ")");
				stringBuilder.Append(m_Entry.Notes[i].Text);
				if (i < m_Entry.Notes.Count - 1)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
			}
			m_RawText = stringBuilder.ToString();
		}
		else
		{
			m_RawText = m_OverrideString;
		}
		UpdateBlink();
	}

	private void UpdateBlink()
	{
		if (Editable && m_BlinkState && selected)
		{
			Label.text = m_RawText + "|";
		}
		else
		{
			Label.text = m_RawText;
		}
	}
}

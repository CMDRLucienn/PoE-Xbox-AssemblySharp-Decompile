using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICommandLine : MonoBehaviour
{
	public UILabel CommandLabel;

	public UIInput InputField;

	private bool m_InputSelected;

	private bool m_wasSelected;

	private List<string> m_History = new List<string>();

	private int m_HistoryLoc;

	private const int MaxHistory = 32;

	private string m_TextBeforeTab = "";

	private List<string> m_TabPossibles;

	private int m_CurrentTabIndex;

	public bool Active => CommandLabel.transform.parent.gameObject.activeSelf;

	private void Start()
	{
		CommandLabel.transform.parent.gameObject.SetActive(value: false);
		LoadHistory();
		ResetText();
		InputField.OnCaratMoved += OnCaratMoved;
		InputField.OnInputRecieved += OnInputChanged;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (GameInput.GetControlDown(MappedControl.TOGGLE_CONSOLE, handle: true))
		{
			Activate(!Active);
		}
		if (Active && Input.GetKeyDown(KeyCode.Escape))
		{
			Activate(active: false);
		}
		InputField.selected = m_InputSelected;
		if (InputField.selected && m_History.Count > 0)
		{
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				m_HistoryLoc--;
				if (m_HistoryLoc < 0)
				{
					m_HistoryLoc = 0;
				}
				InputField.text = m_History[m_HistoryLoc];
				GameInput.GetKeyUp(KeyCode.UpArrow);
			}
			if (Input.GetKeyUp(KeyCode.DownArrow) && m_HistoryLoc < m_History.Count)
			{
				m_HistoryLoc++;
				if (m_HistoryLoc >= m_History.Count)
				{
					m_HistoryLoc = m_History.Count - 1;
				}
				InputField.text = m_History[m_HistoryLoc];
			}
			if (Input.GetKeyUp(KeyCode.Tab))
			{
				if (m_TabPossibles == null)
				{
					m_TextBeforeTab = InputField.text;
					m_TabPossibles = CommandLineRun.GetPossibleCompletions(m_TextBeforeTab)?.ToList();
					m_CurrentTabIndex = 0;
				}
				else
				{
					m_CurrentTabIndex += ((!GameInput.GetShiftkey()) ? 1 : (-1));
					if (m_CurrentTabIndex < 0)
					{
						m_CurrentTabIndex += m_TabPossibles.Count;
					}
					if (m_CurrentTabIndex >= m_TabPossibles.Count)
					{
						m_CurrentTabIndex -= m_TabPossibles.Count;
					}
				}
				if (m_TabPossibles != null && m_TabPossibles.Count > 0)
				{
					InputField.text = m_TextBeforeTab.Substring(0, m_TextBeforeTab.LastIndexOf(' ') + 1) + m_TabPossibles[m_CurrentTabIndex] + " ";
				}
			}
		}
		if (CommandLabel.transform.parent.gameObject.activeSelf && GameInput.GetKeyUp(KeyCode.Return, setHandled: true))
		{
			if (!InputField.selected)
			{
				m_InputSelected = true;
				m_HistoryLoc = m_History.Count;
			}
			else
			{
				ResetText();
			}
		}
		if (m_wasSelected && !InputField.selected)
		{
			GameInput.EndBlockAllKeys();
		}
		if (!m_wasSelected && InputField.selected)
		{
			GameInput.BeginBlockAllKeys();
		}
		m_wasSelected = InputField.selected;
	}

	private void OnSubmit()
	{
		string text = NGUITools.StripSymbols(InputField.text);
		m_InputSelected = false;
		if (!string.IsNullOrEmpty(text))
		{
			AddToHistory(text);
			SaveHistory();
			CommandLineRun.RunCommand(text);
		}
		Activate(active: false);
	}

	private void OnCaratMoved(UIInput sender, int position)
	{
		m_TabPossibles = null;
	}

	private void OnInputChanged(UIInput sender, string text)
	{
		m_TabPossibles = null;
	}

	private void ResetText()
	{
		CommandLabel.text = GUIUtils.GetText(893);
		InputField.defaultText = CommandLabel.text;
		m_InputSelected = false;
	}

	public void Activate(bool active)
	{
		CommandLabel.transform.parent.gameObject.SetActive(active);
		ResetText();
		if ((bool)CameraControl.Instance)
		{
			CameraControl.Instance.EnablePlayerControl(!active);
		}
	}

	private void AddToHistory(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			m_History.Add(text);
			if (m_History.Count > 32)
			{
				m_History.RemoveAt(0);
			}
		}
	}

	private void LoadHistory()
	{
	}

	private void SaveHistory()
	{
	}
}

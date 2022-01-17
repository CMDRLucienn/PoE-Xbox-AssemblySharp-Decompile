using System;
using System.Collections.Generic;
using UnityEngine;

public class UIConsoleMode : MonoBehaviour
{
	public UITable Table;

	private List<UIConsoleEntry> m_Entries = new List<UIConsoleEntry>();

	private int m_EndPointer;

	private const float tweenSpeedPerPixel = 5f;

	private float m_tweenRootY = 20f;

	private int m_repoTable;

	public Console.ConsoleState Mode { get; set; }

	private void OnEnable()
	{
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, m_tweenRootY, base.transform.localPosition.z);
		Reposition();
		m_repoTable = 2;
	}

	private void Start()
	{
		if (Mode == Console.ConsoleState.Dialogue)
		{
			Table.padding.y += 8f;
		}
	}

	private void Awake()
	{
		GameMode option = GameState.Option;
		option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Combine(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		StringTableManager.OnLanguageChanged += OnLanguageChangedAll;
	}

	private void OnDestroy()
	{
		GameMode option = GameState.Option;
		option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Remove(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		StringTableManager.OnLanguageChanged -= OnLanguageChangedAll;
	}

	private void Update()
	{
		if (m_repoTable > 0)
		{
			m_repoTable--;
			Reposition();
		}
		if (base.transform.localPosition.y < m_tweenRootY)
		{
			float num = 5f * Mathf.Max(1f, m_tweenRootY - base.transform.localPosition.y);
			base.transform.localPosition += new Vector3(0f, num * TimeController.sUnscaledDelta, 0f);
			if (base.transform.localPosition.y > m_tweenRootY)
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, m_tweenRootY, base.transform.localPosition.z);
			}
		}
	}

	public void Clear()
	{
		foreach (UIConsoleEntry entry in m_Entries)
		{
			if (entry != null)
			{
				GameUtilities.Destroy(entry.gameObject);
			}
		}
		m_Entries.Clear();
		m_EndPointer = 0;
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, m_tweenRootY, base.transform.localPosition.z);
	}

	public void Reposition()
	{
		for (int i = 0; i < m_Entries.Count; i++)
		{
			m_Entries[i].Reposition();
		}
		Table.Reposition();
	}

	public void AddEntry(Console.ConsoleMessage message)
	{
		if (!message.ForMode(Mode))
		{
			return;
		}
		UIConsoleEntry uIConsoleEntry;
		if (m_Entries.Count < UIConsole.Instance.MaxEntries)
		{
			uIConsoleEntry = UnityEngine.Object.Instantiate(UIConsole.Instance.EntryPrefab.gameObject).GetComponent<UIConsoleEntry>();
			uIConsoleEntry.transform.parent = base.transform;
			uIConsoleEntry.transform.localPosition = Vector3.zero;
			uIConsoleEntry.transform.localScale = UIConsole.Instance.EntryPrefab.transform.localScale;
			uIConsoleEntry.transform.localRotation = Quaternion.identity;
			uIConsoleEntry.gameObject.SetActive(value: true);
			m_Entries.Add(uIConsoleEntry);
		}
		else
		{
			uIConsoleEntry = m_Entries[m_EndPointer];
			m_EndPointer++;
			if (m_EndPointer >= m_Entries.Count)
			{
				m_EndPointer = 0;
			}
		}
		uIConsoleEntry.Set(message);
		Reposition();
		m_repoTable = 4;
		float num = uIConsoleEntry.Label.relativeSize.y * uIConsoleEntry.Label.transform.localScale.y + Table.padding.y;
		if (base.gameObject.activeSelf)
		{
			base.transform.localPosition = base.transform.localPosition + new Vector3(0f, 0f - num, 0f);
		}
	}

	private void OnFontScaleChangedAll(float scale)
	{
		m_repoTable = 1;
	}

	private void OnLanguageChangedAll(Language lang)
	{
		m_repoTable = 1;
	}
}

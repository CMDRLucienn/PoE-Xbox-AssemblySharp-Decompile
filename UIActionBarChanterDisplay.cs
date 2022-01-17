using System;
using System.Collections.Generic;
using UnityEngine;

public class UIActionBarChanterDisplay : MonoBehaviour
{
	private ChanterTrait m_LoadedChanting;

	private Chant m_CurrentChant;

	private CharacterStats m_chantingCharacter;

	public UIPanel ClippingPanel;

	public UIActionBarChanterPhrase RootPhrase;

	public UITexture OffPhrase;

	private List<UIActionBarChanterPhrase> m_Phrases = new List<UIActionBarChanterPhrase>();

	private int lastUpdateCurrentPhraseIndex = -1;

	private float m_ClipRangeHalfWidth;

	private Vector3 m_CurPhrasePosition;

	private List<int> m_pastChanterPhraseIndices = new List<int>();

	private UIActionBarChanterPhrase FirstPhrase
	{
		get
		{
			if (m_Phrases.Count > 1)
			{
				return m_Phrases[1];
			}
			return null;
		}
	}

	private void Awake()
	{
		GameState.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnDestroy()
	{
		GameState.OnLevelLoaded -= OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLevelLoaded(object sender, EventArgs args)
	{
		Reset();
	}

	private void Start()
	{
		RootPhrase.gameObject.SetActive(value: false);
		OffPhrase.alpha = 0.5f;
		m_ClipRangeHalfWidth = ClippingPanel.clipRange.x + ClippingPanel.clipRange.z / 2f;
		UIEventListener uIEventListener = UIEventListener.Get(OffPhrase.gameObject);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		if (over && (bool)FirstPhrase)
		{
			UIAbilityTooltip.GlobalShow(sender.GetComponent<UIWidget>(), UIAbilityBar.GetSelectedForBars(), FirstPhrase.Phrase);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	public void Load(CharacterStats character)
	{
		foreach (GenericAbility activeAbility in character.ActiveAbilities)
		{
			if (activeAbility is ChanterTrait)
			{
				m_LoadedChanting = (ChanterTrait)activeAbility;
				m_chantingCharacter = character;
				break;
			}
		}
	}

	public void Reset()
	{
		OffPhrase.alpha = 0f;
		foreach (UIActionBarChanterPhrase phrase in m_Phrases)
		{
			phrase.gameObject.SetActive(value: false);
		}
		m_pastChanterPhraseIndices.Clear();
		lastUpdateCurrentPhraseIndex = -1;
	}

	private void Update()
	{
		if (m_LoadedChanting == null)
		{
			return;
		}
		Chant chant = m_LoadedChanting.Chant;
		if (!m_LoadedChanting || !m_LoadedChanting.Chant || !m_LoadedChanting.Chant.IsActive())
		{
			chant = null;
		}
		if (m_CurrentChant != chant)
		{
			m_CurrentChant = chant;
			if (m_CurrentChant != null)
			{
				m_CurrentChant.SetPhraseOwner(m_chantingCharacter);
			}
			Reset();
		}
		if (!m_CurrentChant)
		{
			return;
		}
		if ((bool)m_LoadedChanting && (bool)m_LoadedChanting.Chant)
		{
			int num = chant.CurrentPhrase;
			if (lastUpdateCurrentPhraseIndex < 0)
			{
				lastUpdateCurrentPhraseIndex = num;
			}
			else if (lastUpdateCurrentPhraseIndex != num)
			{
				m_pastChanterPhraseIndices.Add(lastUpdateCurrentPhraseIndex);
				lastUpdateCurrentPhraseIndex = num;
			}
			float num2 = 0f - m_ClipRangeHalfWidth - (chant.PhraseInstances[num].Recitation - (m_CurrentChant.TimeToNextPhrase - m_CurrentChant.Timer)) * (float)UIActionBarChanter.Instance.PixelsPerSecond;
			for (int num3 = m_pastChanterPhraseIndices.Count - 1; num3 >= 0; num3--)
			{
				num = m_pastChanterPhraseIndices[num3];
				num2 -= chant.PhraseInstances[num].Recitation * (float)UIActionBarChanter.Instance.PixelsPerSecond;
				if (num3 == 0 && num2 + chant.PhraseInstances[num].Duration * (float)UIActionBarChanter.Instance.PixelsPerSecond < 0f - m_ClipRangeHalfWidth)
				{
					m_pastChanterPhraseIndices.RemoveAt(0);
				}
			}
			int i = 0;
			while (num2 < m_ClipRangeHalfWidth)
			{
				UIActionBarChanterPhrase phrase = GetPhrase(i);
				phrase.SetPhrase(chant.PhraseInstances[num], num % 2);
				m_CurPhrasePosition = phrase.transform.localPosition;
				m_CurPhrasePosition.x = num2;
				phrase.transform.localPosition = m_CurPhrasePosition;
				num2 += chant.PhraseInstances[num].Recitation * (float)UIActionBarChanter.Instance.PixelsPerSecond;
				i++;
				num = (num + 1) % m_CurrentChant.PhraseInstances.Length;
			}
			for (; i < m_Phrases.Count; i++)
			{
				m_Phrases[i].gameObject.SetActive(value: false);
			}
		}
		if ((bool)FirstPhrase)
		{
			OffPhrase.mainTexture = FirstPhrase.Icon.mainTexture;
			OffPhrase.alpha = 0.5f;
		}
		else
		{
			OffPhrase.alpha = 0f;
		}
	}

	private UIActionBarChanterPhrase GetPhrase(int index)
	{
		while (m_Phrases.Count <= index)
		{
			UIActionBarChanterPhrase component = NGUITools.AddChild(RootPhrase.transform.parent.gameObject, RootPhrase.gameObject).GetComponent<UIActionBarChanterPhrase>();
			component.transform.localPosition = RootPhrase.transform.localPosition;
			component.gameObject.SetActive(value: false);
			m_Phrases.Add(component);
		}
		m_Phrases[index].gameObject.SetActive(value: true);
		return m_Phrases[index];
	}
}

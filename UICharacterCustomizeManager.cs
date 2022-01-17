using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UICharacterCustomizeManager : UIHudWindow
{
	public static List<string> PortraitOptions = new List<string>();

	private static int m_malePortraitIndex = 0;

	private static Dictionary<CharacterStats.Race, int> m_maleRacePortraitOffset = new Dictionary<CharacterStats.Race, int>();

	private static int m_femalePortraitIndex = 0;

	private static Dictionary<CharacterStats.Race, int> m_femaleRacePortraitOffsets = new Dictionary<CharacterStats.Race, int>();

	public UITexture TexturePortrait;

	public UIImageButtonRevised ButtonPortraitUp;

	public UIImageButtonRevised ButtonPortraitDown;

	public UILabel LabelPortraitCount;

	public UIImageButtonRevised ButtonVoiceUp;

	public UIImageButtonRevised ButtonVoiceDown;

	public UILabel LabelVoiceCount;

	public UILabel LabelSelectedVoiceName;

	public UIMultiSpriteImageButton ButtonSave;

	public UIMultiSpriteImageButton ButtonCancel;

	public const string SMALL_PORTRAIT_SUFFIX = "_sm";

	public const string LARGE_PORTRAIT_SUFFIX = "_lg";

	private CharacterStats m_ModifyingCharacter;

	private Portrait m_ModifyingPortrait;

	private SoundSetComponent m_ModifyingSoundset;

	private int m_PendingPortraitIndex;

	private SoundSet.SoundAction[] SelectedVoiceSoundHooks = new SoundSet.SoundAction[4]
	{
		SoundSet.SoundAction.Selected,
		SoundSet.SoundAction.Leading,
		SoundSet.SoundAction.Attack,
		SoundSet.SoundAction.Scouting
	};

	private PlayerVoiceSetList m_VoiceSetList;

	private SoundSet[] m_VoiceSoundSets;

	private int m_PendingVoiceSetIndex;

	private Coroutine m_portraitTextureLoadCoroutine;

	public static UICharacterCustomizeManager Instance { get; private set; }

	public static bool NeedsPortraitCaching
	{
		get
		{
			if (PortraitOptions != null)
			{
				return PortraitOptions.Count == 0;
			}
			return true;
		}
	}

	private void Awake()
	{
		Instance = this;
		UIEventListener uIEventListener = UIEventListener.Get(ButtonPortraitUp);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnPortraitUp));
		UIEventListener uIEventListener2 = UIEventListener.Get(ButtonPortraitDown);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnPortraitDown));
		UIEventListener uIEventListener3 = UIEventListener.Get(ButtonVoiceDown);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnVoiceDown));
		UIEventListener uIEventListener4 = UIEventListener.Get(ButtonVoiceUp);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnVoiceUp));
		UIMultiSpriteImageButton buttonSave = ButtonSave;
		buttonSave.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(buttonSave.onClick, new UIEventListener.VoidDelegate(OnSaveChanges));
		UIMultiSpriteImageButton buttonCancel = ButtonCancel;
		buttonCancel.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(buttonCancel.onClick, new UIEventListener.VoidDelegate(OnCancelButton));
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void LoadCharacter(CharacterStats characterToMod)
	{
		if (!(characterToMod == null))
		{
			m_ModifyingCharacter = characterToMod;
			m_ModifyingPortrait = m_ModifyingCharacter.GetComponent<Portrait>();
			m_ModifyingSoundset = m_ModifyingCharacter.GetComponent<SoundSetComponent>();
			LoadPlayerSoundSetOptions();
			m_VoiceSoundSets = m_VoiceSetList.GetPrioritySortedVoiceSets(characterToMod);
		}
	}

	protected override void Show()
	{
		if (PortraitOptions == null || PortraitOptions.Count == 0)
		{
			LoadPortraitCache();
		}
		if (PortraitOptions == null || PortraitOptions.Count == 0)
		{
			HideWindow();
			return;
		}
		LoadStartingPortrait();
		LoadStartingSoundSet();
	}

	private void OnCancelButton(GameObject go)
	{
		HideWindow();
	}

	private void OnSaveChanges(GameObject go)
	{
		if (m_ModifyingPortrait.TextureSmallPath != PortraitOptions[m_PendingPortraitIndex])
		{
			m_ModifyingPortrait.SetTextures(PortraitOptions[m_PendingPortraitIndex], PortraitOptions[m_PendingPortraitIndex].Replace("_sm", "_lg"));
		}
		if (m_ModifyingSoundset == null || m_ModifyingSoundset.SoundSet == null || m_ModifyingSoundset.SoundSet.DisplayName != m_VoiceSoundSets[m_PendingVoiceSetIndex].DisplayName)
		{
			PartyMemberAI component = m_ModifyingCharacter.gameObject.GetComponent<PartyMemberAI>();
			if (component != null)
			{
				SoundSet soundSet2 = (component.SoundSet = UnityEngine.Object.Instantiate(m_VoiceSoundSets[m_PendingVoiceSetIndex]));
			}
		}
		HideWindow();
	}

	private void LoadPlayerSoundSetOptions()
	{
		if (m_VoiceSetList == null)
		{
			m_VoiceSetList = GameResources.LoadPrefab<PlayerVoiceSetList>(PlayerVoiceSetList.DefaultPlayerSoundSetList, instantiate: false);
			_ = m_VoiceSetList == null;
		}
	}

	private void LoadStartingSoundSet()
	{
		m_PendingVoiceSetIndex = 0;
		for (int i = 0; i < m_VoiceSoundSets.Length; i++)
		{
			if (m_ModifyingSoundset == null || m_ModifyingSoundset.SoundSet == null || m_VoiceSoundSets[i].DisplayName.Equals(m_ModifyingSoundset.SoundSet.DisplayName))
			{
				m_PendingVoiceSetIndex = i;
				break;
			}
		}
		SetVoiceIndex(m_PendingVoiceSetIndex, playSound: false);
	}

	private void OnVoiceUp(GameObject go)
	{
		SetVoiceIndex(m_PendingVoiceSetIndex + 1, playSound: true);
	}

	private void OnVoiceDown(GameObject go)
	{
		SetVoiceIndex(m_PendingVoiceSetIndex - 1, playSound: true);
	}

	private void SetVoiceIndex(int newIndex, bool playSound)
	{
		m_PendingVoiceSetIndex = newIndex;
		if (m_PendingVoiceSetIndex >= m_VoiceSoundSets.Length)
		{
			m_PendingVoiceSetIndex %= m_VoiceSoundSets.Length;
		}
		while (m_PendingVoiceSetIndex < 0 && m_VoiceSoundSets.Length != 0)
		{
			m_PendingVoiceSetIndex += m_VoiceSoundSets.Length;
		}
		LabelSelectedVoiceName.text = m_VoiceSoundSets[m_PendingVoiceSetIndex].DisplayName.GetText();
		LabelVoiceCount.text = $"{m_PendingVoiceSetIndex + 1}/{m_VoiceSoundSets.Length}";
		if (playSound)
		{
			int num = OEIRandom.Index(SelectedVoiceSoundHooks.Length);
			m_VoiceSoundSets[m_PendingVoiceSetIndex].InterruptAudio();
			m_VoiceSoundSets[m_PendingVoiceSetIndex].PlaySound(UICharacterCreationManager.Instance.TargetCharacter, SelectedVoiceSoundHooks[num]);
		}
	}

	private void OnPortraitUp(GameObject go)
	{
		SetPortraitIndex(m_PendingPortraitIndex + 1);
	}

	private void OnPortraitDown(GameObject go)
	{
		SetPortraitIndex(m_PendingPortraitIndex - 1);
	}

	private void LoadStartingPortrait()
	{
		if (NeedsPortraitCaching)
		{
			LoadPortraitCache();
			if (NeedsPortraitCaching)
			{
				return;
			}
		}
		string textureSmallPath = m_ModifyingPortrait.TextureSmallPath;
		m_PendingPortraitIndex = -1;
		for (int i = 0; i < PortraitOptions.Count; i++)
		{
			if (string.Compare(textureSmallPath, PortraitOptions[i], ignoreCase: true) == 0)
			{
				m_PendingPortraitIndex = i;
				break;
			}
		}
		if (m_PendingPortraitIndex < 0)
		{
			m_PendingPortraitIndex = GetPortraitIndexFor(m_ModifyingCharacter.Gender, m_ModifyingCharacter.CharacterRace);
		}
		SetPortraitIndex(m_PendingPortraitIndex);
	}

	private void SetPortraitIndex(int newIndex)
	{
		m_PendingPortraitIndex = newIndex;
		if (m_PendingPortraitIndex >= PortraitOptions.Count)
		{
			m_PendingPortraitIndex %= PortraitOptions.Count;
		}
		while (m_PendingPortraitIndex < 0 && PortraitOptions.Count > 0)
		{
			m_PendingPortraitIndex += PortraitOptions.Count;
		}
		string text = PortraitOptions[m_PendingPortraitIndex];
		text = text.Replace("_sm", "_lg");
		LabelPortraitCount.text = $"{m_PendingPortraitIndex + 1}/{PortraitOptions.Count}";
		if (m_portraitTextureLoadCoroutine != null)
		{
			StopCoroutine(m_portraitTextureLoadCoroutine);
		}
		m_portraitTextureLoadCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(text, PortraitTextureLoaded));
	}

	private void PortraitTextureLoaded(Texture2D loadedTexture)
	{
		if (TexturePortrait.mainTexture != null)
		{
			GameUtilities.Destroy(TexturePortrait.mainTexture);
		}
		TexturePortrait.mainTexture = loadedTexture;
	}

	public static int GetPortraitIndexFor(Gender gender, CharacterStats.Race race)
	{
		int result = -1;
		switch (gender)
		{
		case Gender.Male:
			result = m_malePortraitIndex;
			if (m_maleRacePortraitOffset.ContainsKey(race))
			{
				result = m_maleRacePortraitOffset[race];
			}
			break;
		case Gender.Female:
			result = m_femalePortraitIndex;
			if (m_femaleRacePortraitOffsets.ContainsKey(race))
			{
				result = m_femaleRacePortraitOffsets[race];
			}
			break;
		}
		return result;
	}

	public static void LoadPortraitCache()
	{
		PortraitOptions = new List<string>();
		string text = Application.dataPath + "/data/art/gui/portraits/player/male/";
		string text2 = Application.dataPath + "/data/art/gui/portraits/player/female/";
		try
		{
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
		}
		catch (Exception)
		{
		}
		m_malePortraitIndex = 0;
		AddPortraitsWithRacialMarkers(text, m_maleRacePortraitOffset);
		m_femalePortraitIndex = PortraitOptions.Count;
		AddPortraitsWithRacialMarkers(text2, m_femaleRacePortraitOffsets);
		m_femalePortraitIndex = Mathf.Min(m_femalePortraitIndex, PortraitOptions.Count - 1);
	}

	public static void ClearPortraitCache()
	{
		if (PortraitOptions != null)
		{
			PortraitOptions.Clear();
			PortraitOptions = null;
		}
		m_malePortraitIndex = 0;
		m_femalePortraitIndex = 0;
		m_maleRacePortraitOffset.Clear();
		m_femaleRacePortraitOffsets.Clear();
	}

	private static void AddPortraitsWithRacialMarkers(string genderPath, Dictionary<CharacterStats.Race, int> raceIndexMarkers)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		raceIndexMarkers.Clear();
		try
		{
			list.AddRange(Directory.GetFiles(genderPath, "*sm.png", SearchOption.AllDirectories));
			list.AddRange(Directory.GetFiles(genderPath, "*sm.jpg", SearchOption.AllDirectories));
		}
		catch
		{
			Debug.LogWarning("Unable to find any portraits in the specified folder path: " + genderPath);
		}
		CharacterStats.Race race = CharacterStats.Race.Undefined;
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i].Remove(0, Application.dataPath.Length + 1);
			string[] array = text.Split('_');
			if (array == null || array.Length < 2)
			{
				list2.Add(text);
				continue;
			}
			CharacterStats.Race race2 = ConvertPortaitStringToRace(array[1]);
			if (race2 == CharacterStats.Race.Undefined)
			{
				list2.Add(text);
				continue;
			}
			if (race2 != race && !raceIndexMarkers.ContainsKey(race2))
			{
				raceIndexMarkers.Add(race2, PortraitOptions.Count);
				race = race2;
			}
			PortraitOptions.Add(text);
		}
		raceIndexMarkers.Add(CharacterStats.Race.Undefined, PortraitOptions.Count);
		PortraitOptions.AddRange(list2);
	}

	private static CharacterStats.Race ConvertPortaitStringToRace(string raceString)
	{
		for (int i = 0; i < 15; i++)
		{
			CharacterStats.Race race = (CharacterStats.Race)i;
			if (string.Compare(raceString, race.ToString(), ignoreCase: true) == 0)
			{
				return (CharacterStats.Race)i;
			}
		}
		return CharacterStats.Race.Undefined;
	}
}

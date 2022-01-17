using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalAudioPlayer : MonoBehaviour
{
	public delegate void ClipLoaded(AudioClip clip, object tag);

	private class AudioDelayTracker
	{
		public string ClipName = string.Empty;

		public float Time;
	}

	public enum UIInventoryAction
	{
		PickUpItem,
		DropItem,
		EquipItem,
		UseItem
	}

	public UIAudioList DefaultAudioList;

	[Tooltip("The number of seconds to wait to play a sound that has just played(same clip name).")]
	public float DelayBetweenPlays = 0.2f;

	[Tooltip("A list of all the clip names which will not have a delay between plays. All instances will be played even if they start at the same time.")]
	public List<string> DelayExemptClipNames = new List<string>();

	private const string WEAPON_HIT_SOUND_NAME = "Impact";

	private const string WEAPON_MISS_SOUND_NAME = "Miss";

	private const string WEAPON_SWITCH_SOUNDSET_NAME = "Switch";

	private const string WEAPON_MINDAMAGE_SOUND_NAME = "MinDamage";

	private Dictionary<UIAudioList.UIAudioType, ClipBankSet> m_audioMap = new Dictionary<UIAudioList.UIAudioType, ClipBankSet>();

	private Dictionary<WeaponSpecializationData.WeaponType, ClipBankSet> m_weaponSwitchMap = new Dictionary<WeaponSpecializationData.WeaponType, ClipBankSet>();

	private Dictionary<WeaponSpecializationData.WeaponType, AudioBankList> m_weaponSoundEffectMap = new Dictionary<WeaponSpecializationData.WeaponType, AudioBankList>();

	private Dictionary<Item.UIDragDropSoundType, ClipBankSet> m_itemPickupMap = new Dictionary<Item.UIDragDropSoundType, ClipBankSet>();

	private Dictionary<Item.UIDragDropSoundType, ClipBankSet> m_itemDropMap = new Dictionary<Item.UIDragDropSoundType, ClipBankSet>();

	private Dictionary<Item.UIDragDropSoundType, ClipBankSet> m_useMap = new Dictionary<Item.UIDragDropSoundType, ClipBankSet>();

	private Dictionary<Item.UIEquipSoundType, ClipBankSet> m_equipMap = new Dictionary<Item.UIEquipSoundType, ClipBankSet>();

	private Dictionary<Stronghold.UIActionSoundType, ClipBankSet> m_strongholdMap = new Dictionary<Stronghold.UIActionSoundType, ClipBankSet>();

	private Dictionary<UIHudWindow.WindowType, ClipBankSet> m_windowAudioOpenMap = new Dictionary<UIHudWindow.WindowType, ClipBankSet>();

	private Dictionary<UIHudWindow.WindowType, ClipBankSet> m_windowAudioCloseMap = new Dictionary<UIHudWindow.WindowType, ClipBankSet>();

	private AudioSource m_audio;

	private bool m_allowPlayingOfTakeSound = true;

	private static List<string> StreamingAudioClipNames = new List<string>();

	private static List<AudioDelayTracker> s_activeDelayTrackers = new List<AudioDelayTracker>(100);

	private static List<AudioDelayTracker> s_freeDelayTrackers = new List<AudioDelayTracker>(100);

	public static GlobalAudioPlayer Instance { get; set; }

	public AudioSource AudioSource => m_audio;

	public bool AllowPlayingOfTakeSound
	{
		get
		{
			return m_allowPlayingOfTakeSound;
		}
		set
		{
			m_allowPlayingOfTakeSound = value;
		}
	}

	private void OnEnable()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'GlobalAudioPlayer' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		if (s_freeDelayTrackers.Count <= 0)
		{
			for (int i = 0; i < 100; i++)
			{
				s_freeDelayTrackers.Add(new AudioDelayTracker());
			}
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		UIWeaponHitAudioEntry[] weaponHitList = DefaultAudioList.WeaponHitList;
		foreach (UIWeaponHitAudioEntry uIWeaponHitAudioEntry in weaponHitList)
		{
			if (uIWeaponHitAudioEntry != null && uIWeaponHitAudioEntry.Clips != null)
			{
				uIWeaponHitAudioEntry.Clips.UnregisterUse();
			}
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		if (DefaultAudioList != null)
		{
			if (DefaultAudioList.AudioList != null)
			{
				UIAudioEntry[] audioList = DefaultAudioList.AudioList;
				foreach (UIAudioEntry uIAudioEntry in audioList)
				{
					if (uIAudioEntry == null)
					{
						continue;
					}
					if (!m_audioMap.ContainsKey(uIAudioEntry.UIType))
					{
						if (uIAudioEntry.Clips != null)
						{
							m_audioMap.Add(uIAudioEntry.UIType, uIAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for AudioType '" + uIAudioEntry.UIType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.WeaponChangeList != null)
			{
				UIWeaponSetAudioEntry[] weaponChangeList = DefaultAudioList.WeaponChangeList;
				foreach (UIWeaponSetAudioEntry uIWeaponSetAudioEntry in weaponChangeList)
				{
					if (uIWeaponSetAudioEntry == null)
					{
						continue;
					}
					if (!m_weaponSwitchMap.ContainsKey(uIWeaponSetAudioEntry.WeaponType))
					{
						if (uIWeaponSetAudioEntry.Clips != null)
						{
							m_weaponSwitchMap.Add(uIWeaponSetAudioEntry.WeaponType, uIWeaponSetAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate WeaponChange entry for WeaponType '" + uIWeaponSetAudioEntry.WeaponType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.WeaponHitList != null)
			{
				UIWeaponHitAudioEntry[] weaponHitList = DefaultAudioList.WeaponHitList;
				foreach (UIWeaponHitAudioEntry uIWeaponHitAudioEntry in weaponHitList)
				{
					if (uIWeaponHitAudioEntry == null)
					{
						continue;
					}
					if (!m_weaponSoundEffectMap.ContainsKey(uIWeaponHitAudioEntry.WeaponType))
					{
						if (uIWeaponHitAudioEntry.Clips != null)
						{
							m_weaponSoundEffectMap.Add(uIWeaponHitAudioEntry.WeaponType, uIWeaponHitAudioEntry.Clips);
							uIWeaponHitAudioEntry.Clips.RegisterUse();
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate WeaponHit entry for WeaponType '" + uIWeaponHitAudioEntry.WeaponType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.ItemPickupList != null)
			{
				UIItemPickupAudioEntry[] itemPickupList = DefaultAudioList.ItemPickupList;
				foreach (UIItemPickupAudioEntry uIItemPickupAudioEntry in itemPickupList)
				{
					if (uIItemPickupAudioEntry == null)
					{
						continue;
					}
					if (!m_itemPickupMap.ContainsKey(uIItemPickupAudioEntry.ItemType))
					{
						if (uIItemPickupAudioEntry.Clips != null)
						{
							m_itemPickupMap.Add(uIItemPickupAudioEntry.ItemType, uIItemPickupAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for ItemType '" + uIItemPickupAudioEntry.ItemType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.ItemDropList != null)
			{
				UIItemDropAudioEntry[] itemDropList = DefaultAudioList.ItemDropList;
				foreach (UIItemDropAudioEntry uIItemDropAudioEntry in itemDropList)
				{
					if (uIItemDropAudioEntry == null)
					{
						continue;
					}
					if (!m_itemDropMap.ContainsKey(uIItemDropAudioEntry.ItemType))
					{
						if (uIItemDropAudioEntry.Clips != null)
						{
							m_itemDropMap.Add(uIItemDropAudioEntry.ItemType, uIItemDropAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for ItemType '" + uIItemDropAudioEntry.ItemType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.EquipList != null)
			{
				UIEquipAudioEntry[] equipList = DefaultAudioList.EquipList;
				foreach (UIEquipAudioEntry uIEquipAudioEntry in equipList)
				{
					if (uIEquipAudioEntry == null)
					{
						continue;
					}
					if (!m_equipMap.ContainsKey(uIEquipAudioEntry.ItemType))
					{
						if (uIEquipAudioEntry.Clips != null)
						{
							m_equipMap.Add(uIEquipAudioEntry.ItemType, uIEquipAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for ItemType '" + uIEquipAudioEntry.ItemType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.StrongholdList != null)
			{
				UIStrongholdAudioEntry[] strongholdList = DefaultAudioList.StrongholdList;
				foreach (UIStrongholdAudioEntry uIStrongholdAudioEntry in strongholdList)
				{
					if (uIStrongholdAudioEntry == null)
					{
						continue;
					}
					if (!m_strongholdMap.ContainsKey(uIStrongholdAudioEntry.ActionType))
					{
						if (uIStrongholdAudioEntry.Clips != null)
						{
							m_strongholdMap.Add(uIStrongholdAudioEntry.ActionType, uIStrongholdAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for stronghold action '" + uIStrongholdAudioEntry.ActionType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.UseList != null)
			{
				UIUseAudioEntry[] useList = DefaultAudioList.UseList;
				foreach (UIUseAudioEntry uIUseAudioEntry in useList)
				{
					if (uIUseAudioEntry == null)
					{
						continue;
					}
					if (!m_useMap.ContainsKey(uIUseAudioEntry.ItemType))
					{
						if (uIUseAudioEntry.Clips != null)
						{
							m_useMap.Add(uIUseAudioEntry.ItemType, uIUseAudioEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for ItemType '" + uIUseAudioEntry.ItemType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.WindowOpenList != null)
			{
				UIHudWindowEntry[] windowOpenList = DefaultAudioList.WindowOpenList;
				foreach (UIHudWindowEntry uIHudWindowEntry in windowOpenList)
				{
					if (uIHudWindowEntry == null)
					{
						continue;
					}
					if (!m_windowAudioOpenMap.ContainsKey(uIHudWindowEntry.WindowType))
					{
						if (uIHudWindowEntry.Clips != null)
						{
							m_windowAudioOpenMap.Add(uIHudWindowEntry.WindowType, uIHudWindowEntry.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for WindowType '" + uIHudWindowEntry.WindowType.ToString() + "'.");
					}
				}
			}
			if (DefaultAudioList.WindowCloseList != null)
			{
				UIHudWindowEntry[] windowOpenList = DefaultAudioList.WindowCloseList;
				foreach (UIHudWindowEntry uIHudWindowEntry2 in windowOpenList)
				{
					if (uIHudWindowEntry2 == null)
					{
						continue;
					}
					if (!m_windowAudioCloseMap.ContainsKey(uIHudWindowEntry2.WindowType))
					{
						if (uIHudWindowEntry2.Clips != null)
						{
							m_windowAudioCloseMap.Add(uIHudWindowEntry2.WindowType, uIHudWindowEntry2.Clips);
						}
					}
					else
					{
						Debug.LogError("DefaultAudioList in GlobalAudioPlayer has a duplicate entry for WindowType '" + uIHudWindowEntry2.WindowType.ToString() + "'.");
					}
				}
			}
		}
		m_audio = base.gameObject.AddComponent<AudioSource>();
		m_audio.ignoreListenerPause = true;
		m_audio.spatialBlend = 0f;
	}

	private void Update()
	{
		for (int num = s_activeDelayTrackers.Count - 1; num >= 0; num--)
		{
			s_activeDelayTrackers[num].Time += Time.unscaledDeltaTime;
			if (s_activeDelayTrackers[num].Time >= DelayBetweenPlays)
			{
				s_activeDelayTrackers[num].ClipName = string.Empty;
				s_activeDelayTrackers[num].Time = 0f;
				s_freeDelayTrackers.Add(s_activeDelayTrackers[num]);
				s_activeDelayTrackers.RemoveAt(num);
			}
		}
	}

	public static void SPlay(UIAudioList.UIAudioType playType)
	{
		if ((bool)Instance)
		{
			Instance.Play(playType);
		}
	}

	public void Play(UIAudioList.UIAudioType playType)
	{
		Play(playType, preventOverlap: true);
	}

	public void Play(UIAudioList.UIAudioType playType, bool preventOverlap)
	{
		if (!(m_audio == null) && m_audioMap.ContainsKey(playType) && m_audioMap[playType].clips != null)
		{
			float volume = 1f;
			float pitch = 1f;
			AudioClip clip = m_audioMap[playType].GetClip(forbidImmediateRepeat: false, out volume, out pitch);
			if (clip != null && clip.loadType != AudioClipLoadType.Streaming && clip.loadState != AudioDataLoadState.Loaded)
			{
				Debug.LogError("Attempted to play unloaded audio clip \"" + ((clip.name != string.Empty) ? clip.name : "<Name not available>") + "\" from audiosource \"GlobalAudioSource\". The sound will not play!");
			}
			else
			{
				PlayOneShot(m_audio, clip, volume, preventOverlap);
			}
		}
	}

	public void PlayOneShot(AudioSource audioSource, AudioClip clip, float volume)
	{
		if (clip != null && clip.loadType != AudioClipLoadType.Streaming && clip.loadState != AudioDataLoadState.Loaded)
		{
			Debug.LogError("Attempted to play unloaded audio clip \"" + ((clip.name != string.Empty) ? clip.name : "<Name not available>") + "\" from audiosource \"" + ((audioSource.name != string.Empty) ? audioSource.name : "<Name not available>") + "\". The sound will not play!");
		}
		else
		{
			PlayOneShot(audioSource, clip, volume, preventOverlap: true);
		}
	}

	public void PlayOneShot(AudioSource audioSource, AudioClip clip, float volume, bool preventOverlap)
	{
		if (clip == null)
		{
			return;
		}
		bool flag = !preventOverlap || DelayExemptClipNames.Contains(clip.name);
		if (flag || !IsDelayed(clip))
		{
			audioSource.PlayOneShot(clip, volume);
			if (!flag)
			{
				AddDelayTracker(clip);
			}
		}
	}

	public static void Play(AudioSource audioSource, bool bIs3DSound)
	{
		audioSource.spatialBlend = (bIs3DSound ? 1f : 0f);
		Play(audioSource);
	}

	public static void Play(AudioSource audioSource)
	{
		if (audioSource == null || Instance == null)
		{
			return;
		}
		bool flag = audioSource.clip == null || Instance.DelayExemptClipNames.Contains(audioSource.clip.name) || string.Equals(audioSource.name, "NPC_NARRATOR", StringComparison.InvariantCulture);
		if (!flag && IsDelayed(audioSource.clip))
		{
			return;
		}
		if (audioSource.clip != null)
		{
			if (audioSource.clip.loadType != AudioClipLoadType.Streaming && audioSource.clip.loadState != AudioDataLoadState.Loaded)
			{
				Debug.LogError("Attempted to play unloaded audio clip \"" + ((audioSource.clip.name != string.Empty) ? audioSource.clip.name : "<Name not available>") + "\" from audiosource \"" + ((audioSource.name != string.Empty) ? audioSource.name : "<Name not available>") + "\". The sound will not play!");
				return;
			}
			if (audioSource.clip.name.StartsWith("mus_", StringComparison.OrdinalIgnoreCase) && audioSource.gameObject.GetComponent<VolumeAsCategory>() == null)
			{
				VolumeAsCategory volumeAsCategory = audioSource.gameObject.AddComponent<VolumeAsCategory>();
				volumeAsCategory.Category = MusicManager.SoundCategory.MUSIC;
				volumeAsCategory.ExternalVolume = 1f;
			}
		}
		if (audioSource.volume != 0f)
		{
			audioSource.Play();
		}
		if (!flag)
		{
			AddDelayTracker(audioSource.clip);
		}
	}

	private static bool IsDelayed(AudioClip clip)
	{
		foreach (AudioDelayTracker s_activeDelayTracker in s_activeDelayTrackers)
		{
			if (s_activeDelayTracker.ClipName == clip.name)
			{
				return true;
			}
		}
		return false;
	}

	private static void AddDelayTracker(AudioClip clip)
	{
		if (s_freeDelayTrackers.Count <= 0)
		{
			for (int i = 0; i < 20; i++)
			{
				s_freeDelayTrackers.Add(new AudioDelayTracker());
			}
		}
		AudioDelayTracker audioDelayTracker = s_freeDelayTrackers[0];
		s_freeDelayTrackers.RemoveAt(0);
		audioDelayTracker.ClipName = clip.name;
		s_activeDelayTrackers.Add(audioDelayTracker);
	}

	public static void SPlay(WeaponSet newSet)
	{
		if ((bool)Instance)
		{
			Instance.Play(newSet);
		}
	}

	public void Play(WeaponSet newSet)
	{
		if (newSet == null)
		{
			return;
		}
		WeaponSpecializationData.WeaponType weaponType = WeaponSpecializationData.WeaponType.Arbalest;
		bool flag = false;
		if (((bool)newSet.PrimaryWeapon && (bool)newSet.PrimaryWeapon.GetComponent<Shield>()) || ((bool)newSet.SecondaryWeapon && (bool)newSet.SecondaryWeapon.GetComponent<Shield>()))
		{
			Play(UIAudioList.UIAudioType.ChangeWeaponSetShield);
			return;
		}
		if ((bool)newSet.PrimaryWeapon)
		{
			Weapon weapon = newSet.PrimaryWeapon as Weapon;
			if ((bool)weapon)
			{
				weaponType = weapon.WeaponType;
				flag = true;
			}
		}
		if (!flag && (bool)newSet.SecondaryWeapon)
		{
			Weapon weapon2 = newSet.SecondaryWeapon as Weapon;
			if ((bool)weapon2)
			{
				weaponType = weapon2.WeaponType;
				flag = true;
			}
		}
		bool flag2 = false;
		if (flag)
		{
			float volume = 1f;
			float pitch = 1f;
			ClipBankSet weaponSwitchSound = GetWeaponSwitchSound(weaponType);
			AudioClip audioClip = null;
			if (weaponSwitchSound != null)
			{
				audioClip = weaponSwitchSound.GetClip(forbidImmediateRepeat: false, out volume, out pitch);
			}
			else if (m_weaponSwitchMap.ContainsKey(weaponType) && m_weaponSwitchMap[weaponType].clips != null)
			{
				audioClip = m_weaponSwitchMap[weaponType].GetClip(forbidImmediateRepeat: false, out volume, out pitch);
			}
			if ((bool)audioClip)
			{
				PlayOneShot(m_audio, audioClip, volume);
				flag2 = true;
			}
		}
		if (!flag2)
		{
			Play(UIAudioList.UIAudioType.ChangeWeaponSet);
		}
	}

	public ClipBankSet GetWeaponSwitchSound(WeaponSpecializationData.WeaponType weaponType)
	{
		return GetWeaponSound(weaponType, "Switch");
	}

	public ClipBankSet GetWeaponHitSoundSet(WeaponSpecializationData.WeaponType weaponType)
	{
		return GetWeaponSound(weaponType, "Impact");
	}

	public ClipBankSet GetWeaponMissSoundSet(WeaponSpecializationData.WeaponType weaponType)
	{
		return GetWeaponSound(weaponType, "Miss");
	}

	public ClipBankSet GetWeaponMinDamageSoundSet(WeaponSpecializationData.WeaponType weaponType)
	{
		return GetWeaponSound(weaponType, "MinDamage");
	}

	private ClipBankSet GetWeaponSound(WeaponSpecializationData.WeaponType weaponType, string soundTypeName)
	{
		ClipBankSet result = null;
		if (m_weaponSoundEffectMap != null && m_weaponSoundEffectMap.ContainsKey(weaponType))
		{
			ClipBankSet[] bank = m_weaponSoundEffectMap[weaponType].bank;
			for (int i = 0; i < bank.Length; i++)
			{
				if (bank[i].name.Equals(soundTypeName, StringComparison.CurrentCultureIgnoreCase))
				{
					return bank[i];
				}
			}
		}
		return result;
	}

	public static void SPlay(Item i, UIInventoryAction actionType)
	{
		if ((bool)Instance)
		{
			Instance.Play(i, actionType);
		}
	}

	public void Play(Item i, UIInventoryAction actionType)
	{
		if (i == null || m_audio == null)
		{
			return;
		}
		switch (actionType)
		{
		case UIInventoryAction.PickUpItem:
		{
			Item.UIDragDropSoundType inventorySoundType3 = i.InventorySoundType;
			if (m_itemPickupMap.ContainsKey(inventorySoundType3) && m_itemPickupMap[inventorySoundType3].clips != null)
			{
				float volume4 = 1f;
				float pitch4 = 1f;
				AudioClip clip4 = m_itemPickupMap[inventorySoundType3].GetClip(forbidImmediateRepeat: false, out volume4, out pitch4);
				if ((bool)clip4)
				{
					PlayOneShot(m_audio, clip4, volume4);
				}
				else
				{
					Play(UIAudioList.UIAudioType.PickInventoryItem);
				}
			}
			else
			{
				Play(UIAudioList.UIAudioType.PickInventoryItem);
			}
			break;
		}
		case UIInventoryAction.DropItem:
		{
			Item.UIDragDropSoundType inventorySoundType2 = i.InventorySoundType;
			if (m_itemDropMap.ContainsKey(inventorySoundType2) && m_itemDropMap[inventorySoundType2].clips != null)
			{
				float volume2 = 1f;
				float pitch2 = 1f;
				AudioClip clip2 = m_itemDropMap[inventorySoundType2].GetClip(forbidImmediateRepeat: false, out volume2, out pitch2);
				if ((bool)clip2)
				{
					PlayOneShot(m_audio, clip2, volume2);
				}
				else
				{
					Play(UIAudioList.UIAudioType.DropInventoryItem);
				}
			}
			else
			{
				Play(UIAudioList.UIAudioType.DropInventoryItem);
			}
			break;
		}
		case UIInventoryAction.EquipItem:
		{
			Item.UIEquipSoundType inventoryEquipSound = i.InventoryEquipSound;
			if (m_equipMap.ContainsKey(inventoryEquipSound) && m_equipMap[inventoryEquipSound].clips != null)
			{
				float volume3 = 1f;
				float pitch3 = 1f;
				AudioClip clip3 = m_equipMap[inventoryEquipSound].GetClip(forbidImmediateRepeat: false, out volume3, out pitch3);
				if ((bool)clip3)
				{
					PlayOneShot(m_audio, clip3, volume3);
				}
				else
				{
					Play(UIAudioList.UIAudioType.DropInventoryItem);
				}
			}
			else
			{
				Play(UIAudioList.UIAudioType.DropInventoryItem);
			}
			break;
		}
		case UIInventoryAction.UseItem:
		{
			Item.UIDragDropSoundType inventorySoundType = i.InventorySoundType;
			if (m_useMap.ContainsKey(inventorySoundType) && m_useMap[inventorySoundType].clips != null)
			{
				float volume = 1f;
				float pitch = 1f;
				AudioClip clip = m_useMap[inventorySoundType].GetClip(forbidImmediateRepeat: false, out volume, out pitch);
				if ((bool)clip)
				{
					PlayOneShot(m_audio, clip, volume);
				}
				else
				{
					Play(UIAudioList.UIAudioType.PickInventoryItem);
				}
			}
			else
			{
				Play(UIAudioList.UIAudioType.PickInventoryItem);
			}
			break;
		}
		}
	}

	public static void SPlay(Stronghold.UIActionSoundType type)
	{
		if ((bool)Instance)
		{
			Instance.Play(type);
		}
	}

	public void Play(Stronghold.UIActionSoundType type)
	{
		if (m_audio == null)
		{
			return;
		}
		Debug.Log("Get clip for type " + type);
		if (m_strongholdMap.ContainsKey(type) && m_strongholdMap[type].clips != null)
		{
			float volume = 1f;
			float pitch = 1f;
			AudioClip clip = m_strongholdMap[type].GetClip(forbidImmediateRepeat: false, out volume, out pitch);
			if ((bool)clip)
			{
				PlayOneShot(m_audio, clip, volume);
			}
		}
	}

	public static void SPlay(UIHudWindow.WindowType type, UIHudWindow.WindowActionType action)
	{
		if ((bool)Instance)
		{
			Instance.Play(type, action);
		}
	}

	public void Play(UIHudWindow.WindowType type, UIHudWindow.WindowActionType action)
	{
		if (m_audio == null)
		{
			return;
		}
		switch (action)
		{
		case UIHudWindow.WindowActionType.Open:
			if (m_windowAudioOpenMap.ContainsKey(type) && m_windowAudioOpenMap[type].clips != null)
			{
				float volume2 = 1f;
				float pitch2 = 1f;
				AudioClip clip2 = m_windowAudioOpenMap[type].GetClip(forbidImmediateRepeat: false, out volume2, out pitch2);
				PlayOneShot(m_audio, clip2, volume2);
			}
			break;
		case UIHudWindow.WindowActionType.Close:
			if (m_windowAudioCloseMap.ContainsKey(type) && m_windowAudioCloseMap[type].clips != null)
			{
				float volume = 1f;
				float pitch = 1f;
				AudioClip clip = m_windowAudioCloseMap[type].GetClip(forbidImmediateRepeat: false, out volume, out pitch);
				PlayOneShot(m_audio, clip, volume);
			}
			break;
		}
	}

	public static AudioSource StreamClipAtPoint(string clipFilename, Vector3 position, float volume, bool bIs3DSound)
	{
		return StreamClipAtPoint(clipFilename, position, volume, ignoreListenerVolume: false, bIs3DSound, MusicManager.SoundCategory.EFFECTS);
	}

	public static AudioSource StreamClipAtPoint(string clipFilename, Vector3 position, float volume, bool ignoreListenerVolume, bool bIs3DSound, MusicManager.SoundCategory category)
	{
		if (AudioBank.DefaultSource == null || string.IsNullOrEmpty(clipFilename))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(AudioBank.DefaultSource);
		gameObject.name = "Audio_" + clipFilename;
		gameObject.transform.parent = AudioBank.AudioBankHierarchyParent;
		gameObject.transform.position = position;
		AudioSource component = gameObject.GetComponent<AudioSource>();
		component.ignoreListenerVolume = ignoreListenerVolume;
		if (category != MusicManager.SoundCategory.EFFECTS && !component.GetComponent<VolumeAsCategory>())
		{
			VolumeAsCategory volumeAsCategory = component.gameObject.AddComponent<VolumeAsCategory>();
			volumeAsCategory.Category = MusicManager.SoundCategory.VOICE;
			volumeAsCategory.Source = component;
			volumeAsCategory.ExternalVolume = volume;
			volumeAsCategory.Init();
		}
		Instance.StartCoroutine(Coroutine_StreamClip(clipFilename, gameObject, component, component.ignoreListenerVolume, bIs3DSound, null, null));
		return component;
	}

	public static void StreamClipAtSource(AudioSource source, string clipFilename, bool bIs3DSound)
	{
		StreamClipAtSource(source, clipFilename, bIs3DSound, null, null);
	}

	public static void StreamClipAtSource(AudioSource source, string clipFilename, bool bIs3DSound, ClipLoaded onClipLoaded, object tag)
	{
		if (!(source == null))
		{
			source.Stop();
			if (!string.IsNullOrEmpty(clipFilename))
			{
				Instance.StartCoroutine(Coroutine_StreamClip(clipFilename, null, source, source.ignoreListenerVolume, bIs3DSound, onClipLoaded, tag));
			}
		}
	}

	public static bool DoesStreamClipExist(string clipFilename)
	{
		string text = Path.Combine(Application.dataPath, clipFilename.ToLower());
		text = text.Replace('\\', '/');
		text = Path.ChangeExtension(text, ".ogg");
		if (File.Exists(text))
		{
			return true;
		}
		text = Path.ChangeExtension(text, ".wav");
		return File.Exists(text);
	}

	private static IEnumerator Coroutine_StreamClip(string clipFilename, GameObject tempObject, AudioSource source, bool ignoreListenerVolume, bool bIs3DSound, ClipLoaded onClipLoaded, object tag)
	{
		string path2 = clipFilename.ToLowerInvariant().Replace("data/", null);
		path2 = Path.ChangeExtension(path2, null);
		for (int i = 0; i < StreamingAudioClipNames.Count; i++)
		{
			if (path2 == StreamingAudioClipNames[i])
			{
				yield break;
			}
		}
		ResourceRequest rr = Resources.LoadAsync<AudioClip>(path2);
		yield return rr;
		AudioClip clip = rr.asset as AudioClip;
		if (rr.asset == null)
		{
			Debug.LogError("Failed to stream audio clip from path: \"" + path2 + "\"");
			yield break;
		}
		while (clip.loadState != AudioDataLoadState.Loaded)
		{
			yield return null;
		}
		onClipLoaded?.Invoke(clip, tag);
		if (source != null)
		{
			source.clip = clip;
			source.ignoreListenerVolume = ignoreListenerVolume;
			VolumeAsCategory component = source.gameObject.GetComponent<VolumeAsCategory>();
			if (component != null)
			{
				component.UpdateVolume();
			}
			StreamingAudioClipNames.Add(path2);
			Play(source, bIs3DSound);
		}
		while (((bool)source && source.isPlaying) || ((bool)TimeController.Instance && TimeController.Instance.Paused))
		{
			yield return null;
		}
		if ((bool)source)
		{
			source.clip = null;
			StreamingAudioClipNames.Remove(path2);
		}
		if ((bool)clip)
		{
			Resources.UnloadAsset(clip);
		}
		if ((bool)tempObject)
		{
			GameUtilities.Destroy(tempObject);
		}
	}
}

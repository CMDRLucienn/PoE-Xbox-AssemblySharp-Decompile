using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AreaMusic : ScriptableObject
{
	[Serializable]
	public class CombatTrackSettings
	{
		[Serializable]
		public class CombatIntro
		{
			public AudioClip IntroAsset;

			public float[] IntroTransitionTimes;

			[HideInInspector]
			public int lastUsedTime = -1;
		}

		public AudioClip MainAsset;

		public float[] MainTransitionTimes;

		public List<CombatIntro> IntroAssets = new List<CombatIntro>();

		[HideInInspector]
		public int lastUsedCombatIntro = -1;

		[HideInInspector]
		public int lastUsedSeekTime = -1;
	}

	public string NormalTrackFile;

	private AudioClip m_NormalTrack;

	[Tooltip("Will play by default if 'Combat Tracks' have not yet been setup.")]
	public string CombatTrackFile;

	private AudioClip m_CombatTrack;

	[Tooltip("Additional tracks that can be played via script calls.")]
	public string[] ScriptedMusic;

	[HideInInspector]
	public List<CombatTrackSettings> CombatMusicSets;

	[HideInInspector]
	public int LastUsedCombatTrack = -1;

	public AudioClip NormalTrack => m_NormalTrack;

	public AudioClip CombatTrack => m_CombatTrack;

	public AudioClip[] ScriptedMusicClips { get; private set; }

	public float NormalSeekTime { get; set; }

	public void LoadMusic()
	{
		UnloadMusic();
		if (NormalTrackFile != null)
		{
			Debug.Log("--------------------\nNormalTrack: " + NormalTrackFile + "\nTo Lower: " + NormalTrackFile.ToLowerInvariant() + "\nPath: " + Path.Combine("Audio/mus", NormalTrackFile.ToLowerInvariant()).Replace('\\', '/'));
			string text = Path.Combine("Audio/mus", NormalTrackFile.ToLowerInvariant()).Replace('\\', '/');
			AudioClip audioClip = Resources.Load<AudioClip>(text);
			if (audioClip != null)
			{
				m_NormalTrack = audioClip;
				m_NormalTrack.LoadAudioData();
				if (m_NormalTrack.loadType != AudioClipLoadType.Streaming)
				{
					Debug.LogWarning(string.Concat("Failed to stream NormalAudioTrack \"", (m_NormalTrack.name != "") ? m_NormalTrack.name : "<Name not available>", "\" as streaming audio. Loadtype \"", m_NormalTrack.loadType, "\" will be used instead."));
				}
			}
			else
			{
				Debug.LogError("Failed to load NormalAudioTrack from path: \"" + text + "\"");
			}
		}
		if (CombatTrackFile != null)
		{
			string text2 = Path.Combine("Audio/mus", CombatTrackFile.ToLowerInvariant()).Replace('\\', '/');
			AudioClip audioClip2 = Resources.Load<AudioClip>(text2);
			if (audioClip2 != null)
			{
				m_CombatTrack = audioClip2;
				m_CombatTrack.LoadAudioData();
				if (m_CombatTrack.loadType != AudioClipLoadType.Streaming)
				{
					Debug.LogWarning(string.Concat("Failed to load CombatAudioTrack \"", (m_CombatTrack.name != "") ? m_CombatTrack.name : "<Name not available>", "\" as streaming audio. Loadtype \"", m_CombatTrack.loadType, "\" will be used instead."));
				}
			}
			else
			{
				Debug.LogError("Failed to load CombatAudioTrack from path: \"" + text2 + "\"");
			}
		}
		string[] array = new string[ScriptedMusic.Length + MusicManager.Instance.GlobalScriptedMusic.Length];
		ScriptedMusic.CopyTo(array, 0);
		for (int i = 0; i < MusicManager.Instance.GlobalScriptedMusic.Length; i++)
		{
			array[ScriptedMusic.Length + i] = MusicManager.Instance.GlobalScriptedMusic[i];
		}
		ScriptedMusic = array;
		if (ScriptedMusic == null || ScriptedMusic.Length == 0)
		{
			return;
		}
		ScriptedMusicClips = new AudioClip[ScriptedMusic.Length];
		for (int j = 0; j < ScriptedMusic.Length; j++)
		{
			string text3 = ScriptedMusic[j];
			string text4 = Path.Combine("Audio/mus", text3.ToLowerInvariant()).Replace('\\', '/');
			AudioClip audioClip3 = Resources.Load<AudioClip>(text4);
			if (audioClip3 != null)
			{
				ScriptedMusicClips[j] = audioClip3;
				ScriptedMusicClips[j].LoadAudioData();
				if (ScriptedMusicClips[j].loadType != AudioClipLoadType.Streaming)
				{
					Debug.LogWarning(string.Concat("Failed to stream ScriptedAudioTrack \"", (ScriptedMusicClips[j].name != "") ? ScriptedMusicClips[j].name : "<Name not available>", "\" as streaming audio. Loadtype \"", ScriptedMusicClips[j].loadType, "\" will be used instead."));
				}
			}
			else
			{
				Debug.LogError("Failed to load ScriptedAudioTrack from path: \"" + text4 + "\" from \"" + base.name + "\"");
			}
		}
	}

	public void UnloadMusic()
	{
		if (CombatTrack != null)
		{
			Resources.UnloadAsset(m_CombatTrack);
			m_CombatTrack = null;
			LastUsedCombatTrack = -1;
		}
		if (NormalTrack != null)
		{
			Resources.UnloadAsset(m_NormalTrack);
			m_NormalTrack = null;
		}
		if (ScriptedMusicClips != null)
		{
			for (int i = 0; i < ScriptedMusicClips.Length; i++)
			{
				Resources.UnloadAsset(ScriptedMusicClips[i]);
			}
			ScriptedMusicClips = null;
		}
	}
}

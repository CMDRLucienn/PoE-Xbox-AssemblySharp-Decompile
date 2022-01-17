using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ScriptedInteraction : Usable
{
	public ConversationObject ConversationFile = new ConversationObject();

	public Texture2D[] Images;

	public AudioClip[] AudioClips;

	[AssetByName(typeof(ScriptedInteractionAssets))]
	public string Assets;

	public static ScriptedInteraction ActiveInteraction;

	public float UseRadius = 5f;

	public float ArrivalDistance;

	[Tooltip("If true, fade to black when the interaction ends and then fade to the game. If false, the game will remain at a black screen waiting for a cutscene script.")]
	public bool FadeFromBlackOnExit = true;

	private int m_currentPortrait;

	private FlowChartPlayer ActiveConversation;

	private ScriptedInteractionAssets m_LoadedAssets => GameResources.LoadPrefab<ScriptedInteractionAssets>(Path.GetFileNameWithoutExtension(Assets), instantiate: false);

	public Texture CurrentPortrait
	{
		get
		{
			if (!string.IsNullOrEmpty(Assets))
			{
				if (m_LoadedAssets.Images.Length > m_currentPortrait)
				{
					return m_LoadedAssets.Images[m_currentPortrait];
				}
				UIDebug.Instance.LogOnScreenWarning("ScriptedInteraction tried to use image '" + m_currentPortrait + "' from assetbundle but there are only " + m_LoadedAssets.Images.Length + " images available.", UIDebug.Department.Design, 10f);
				return null;
			}
			if (Images.Length > m_currentPortrait)
			{
				return Images[m_currentPortrait];
			}
			UIDebug.Instance.LogOnScreenWarning("ScriptedInteraction tried to use image '" + m_currentPortrait + "' but there are only " + Images.Length + " images available.", UIDebug.Department.Design, 10f);
			return null;
		}
	}

	public override float UsableRadius => UseRadius;

	public override float ArrivalRadius => ArrivalDistance;

	public override bool IsUsable
	{
		get
		{
			if (base.IsVisible && !GameState.InCombat && GameState.CutsceneAllowed)
			{
				return !PartyMemberAI.IsPartyMemberUnconscious();
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		base.gameObject.layer = LayerUtility.FindLayerValue("Dynamics");
		ActiveConversation = null;
	}

	protected override void OnDestroy()
	{
		if (ActiveInteraction == this)
		{
			ActiveInteraction = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (ActiveConversation != null && !ConversationManager.Instance.IsConversationActive(ActiveConversation))
		{
			ActiveConversation = null;
			ActiveInteraction = null;
		}
	}

	public void StartConversation()
	{
		SetState(0);
		ActiveInteraction = this;
		ActiveConversation = ConversationManager.Instance.StartScriptedInteraction(ConversationFile.Filename, base.gameObject);
		if (ActiveConversation != null)
		{
			ActiveConversation.FadeFromBlackOnExit = FadeFromBlackOnExit;
		}
	}

	public void SetState(int index)
	{
		m_currentPortrait = index;
		AudioClip audioClip = null;
		if (!string.IsNullOrEmpty(Assets))
		{
			if (m_LoadedAssets.AudioClips.Length > m_currentPortrait)
			{
				audioClip = m_LoadedAssets.AudioClips[m_currentPortrait];
			}
		}
		else if (AudioClips.Length > m_currentPortrait)
		{
			audioClip = AudioClips[m_currentPortrait];
		}
		if (!audioClip)
		{
			return;
		}
		if (GetComponent<AudioSource>() != null)
		{
			if (GetComponent<AudioSource>().isPlaying)
			{
				GetComponent<AudioSource>().Stop();
			}
			GetComponent<AudioSource>().clip = audioClip;
			GlobalAudioPlayer.Play(GetComponent<AudioSource>());
		}
		else
		{
			Debug.LogError(base.name + " is trying to play audio but doesn't have an AudioSource component!", base.gameObject);
		}
	}

	public void PlayScriptAudioClip(int index)
	{
		if (!m_LoadedAssets || m_LoadedAssets.ScriptAudioClips == null)
		{
			return;
		}
		if (index < 0 || index >= m_LoadedAssets.ScriptAudioClips.Length)
		{
			UIDebug.Instance.LogOnScreenWarning("Interaction '" + base.name + "' tried to play AudioClip '" + index + "' but that was out of bounds.", UIDebug.Department.Design, 10f);
		}
		else if (GetComponent<AudioSource>() != null)
		{
			if (GetComponent<AudioSource>().isPlaying)
			{
				GetComponent<AudioSource>().Stop();
			}
			GetComponent<AudioSource>().clip = m_LoadedAssets.ScriptAudioClips[index];
			GlobalAudioPlayer.Play(GetComponent<AudioSource>());
		}
		else
		{
			Debug.LogError(base.name + " is trying to play audio but doesn't have an AudioSource component!", base.gameObject);
		}
	}

	public AudioClip GetScriptMusicClip(int index)
	{
		if ((bool)m_LoadedAssets && m_LoadedAssets.ScriptMusic != null)
		{
			if (index < 0 || index >= m_LoadedAssets.ScriptMusic.Length)
			{
				UIDebug.Instance.LogOnScreenWarning("Interaction '" + base.name + "' tried to play music track '" + index + "' but that was out of bounds.", UIDebug.Department.Design, 10f);
				return null;
			}
			return m_LoadedAssets.ScriptMusic[index];
		}
		return null;
	}

	private void OnDrawGizmos()
	{
		if (!(GetComponent<Collider>() == null))
		{
			DrawUtility.DrawCollider(base.transform, GetComponent<Collider>(), Color.blue);
		}
	}

	public override bool Use(GameObject user)
	{
		if (ConversationFile.Filename != null)
		{
			GameState.LastPersonToUseScriptedInteraction = user;
			FireUseAudio();
			StartConversation();
		}
		return true;
	}
}

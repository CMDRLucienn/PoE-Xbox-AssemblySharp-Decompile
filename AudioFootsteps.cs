using System;
using System.Text;
using UnityEngine;

public class AudioFootsteps : MonoBehaviour
{
	public FootstepSound[] Footsteps = new FootstepSound[1];

	private FootstepSound m_default;

	private AudioSource m_source;

	private AudioBank m_bank;

	private Equipment m_equipment;

	private VolumeAsCategory m_volumeCat;

	private static StringBuilder s_stringBuilder = new StringBuilder();

	private static GroundMaterial last = GroundMaterial.Default;

	private void Start()
	{
		m_source = GetComponent<AudioSource>();
		if (m_source == null)
		{
			Debug.LogError(base.gameObject.name + " doesn't have an audio source! It can't play footstep sounds.", base.gameObject);
			return;
		}
		m_bank = GetComponent<AudioBank>();
		m_equipment = GetComponent<Equipment>();
		m_volumeCat = GetComponent<VolumeAsCategory>() ?? base.gameObject.AddComponent<VolumeAsCategory>();
		for (int i = 0; i < Footsteps.Length; i++)
		{
			if (Footsteps[i].Equals(GroundMaterial.Default))
			{
				m_default = Footsteps[i];
				break;
			}
		}
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.OnEventFootstep += anim_OnEventFootstep;
			component.OnEventJostle += anim_OnEventJostle;
		}
	}

	private void anim_OnEventJostle(object sender, EventArgs e)
	{
		string text = "jostle";
		if ((bool)m_equipment && m_equipment.CurrentItems != null && m_equipment.CurrentItems.Chest != null)
		{
			Armor component = m_equipment.CurrentItems.Chest.GetComponent<Armor>();
			if (component != null && component.Material != 0)
			{
				s_stringBuilder.Remove(0, s_stringBuilder.Length);
				s_stringBuilder.Append(text);
				s_stringBuilder.Append("_");
				s_stringBuilder.Append(Armor.ArmorMatrialStrings[(int)component.Material]);
				text = s_stringBuilder.ToString();
			}
		}
		if ((bool)m_bank)
		{
			m_bank.PlayFrom(text);
		}
	}

	private void anim_OnEventFootstep(object sender, EventArgs e)
	{
		GroundMaterial groundMaterial = FindCurrentMaterial();
		bool flag = false;
		for (int i = 0; i < Footsteps.Length; i++)
		{
			if (Footsteps[i].Equals(groundMaterial))
			{
				PlayFootstep(Footsteps[i]);
				flag = true;
				break;
			}
		}
		if (!flag && m_default != null)
		{
			PlayFootstep(m_default);
		}
	}

	private GroundMaterial FindCurrentMaterial()
	{
		if (GameState.Instance.CurrentMap == null)
		{
			return GroundMaterial.Default;
		}
		GroundMaterial materialAtPoint = FootstepMapLoader.Instance.GetMaterialAtPoint(base.transform.localPosition);
		if (materialAtPoint != last)
		{
			last = materialAtPoint;
		}
		return materialAtPoint;
	}

	public void PlayFootstep(FootstepSound sound)
	{
		float volume;
		float pitch;
		AudioClip clip = sound.Clips.GetClip(forbidImmediateRepeat: false, out volume, out pitch);
		if (m_source.clip == null)
		{
			if ((bool)m_volumeCat)
			{
				m_volumeCat.SetCategory(MusicManager.SoundCategory.EFFECTS, volume);
			}
			else
			{
				m_source.volume = volume;
			}
		}
		GlobalAudioPlayer.Instance.PlayOneShot(m_source, clip, volume);
	}

	private void OnDestroy()
	{
		AnimationController component = GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.OnEventFootstep -= anim_OnEventFootstep;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}
}

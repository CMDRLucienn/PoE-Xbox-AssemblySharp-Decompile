using System.Collections.Generic;
using UnityEngine;

public class Chatter : MonoBehaviour
{
	public SoundSet SoundSet;

	public float Radius = 2f;

	public float Cooldown = 3f;

	public int Charges;

	public bool OnlyChatAtPlayers = true;

	private bool m_initialized;

	private int m_uses;

	private float m_checkTimer;

	private float m_cooldownTimer;

	private List<GameObject> m_targets = new List<GameObject>();

	private const float CheckTime = 1f;

	private void Start()
	{
		if (SoundSet == null)
		{
			Debug.LogError("Chatter has no SoundSet!");
		}
		else
		{
			m_initialized = true;
		}
	}

	private void Update()
	{
		if (!m_initialized || (Charges > 0 && m_uses >= Charges))
		{
			return;
		}
		if (m_cooldownTimer >= 0f)
		{
			m_cooldownTimer -= Time.deltaTime;
		}
		if (m_checkTimer >= 0f)
		{
			m_checkTimer -= Time.deltaTime;
			if (m_checkTimer >= 0f)
			{
				return;
			}
			m_checkTimer = 1f;
		}
		for (int num = m_targets.Count - 1; num >= 0; num--)
		{
			if (m_targets[num] == null)
			{
				m_targets.RemoveAt(num);
			}
			else if ((m_targets[num].transform.position - base.gameObject.transform.position).sqrMagnitude > Radius * Radius)
			{
				m_targets.RemoveAt(num);
				PlayChatter(SoundSet.SoundAction.ChatterOnExit);
			}
		}
		GameObject[] array = GameUtilities.CreaturesInRange(base.gameObject.transform.position, Radius, playerEnemiesOnly: false, includeUnconscious: false);
		if (array == null)
		{
			return;
		}
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject == null) && !(gameObject == base.gameObject) && !m_targets.Contains(gameObject) && (!OnlyChatAtPlayers || !(gameObject.GetComponent<PartyMemberAI>() == null)))
			{
				m_targets.Add(gameObject);
				PlayChatter(SoundSet.SoundAction.ChatterOnEnter);
			}
		}
	}

	private void PlayChatter(SoundSet.SoundAction action)
	{
		if (!(m_cooldownTimer >= 0f))
		{
			SoundSet.PlayBark(base.gameObject, action);
			m_cooldownTimer = Cooldown;
			if (Charges > 0)
			{
				m_uses++;
			}
		}
	}
}

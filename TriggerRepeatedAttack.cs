using System.Collections.Generic;
using UnityEngine;

internal class TriggerRepeatedAttack : Trigger
{
	public AttackBase AttackPrefab;

	public float Interval = 1f;

	private float m_Timer;

	private AttackBase m_Attack;

	private List<GameObject> m_CharactersInTrigger = new List<GameObject>();

	protected override void Start()
	{
		m_Attack = GameResources.Instantiate<AttackBase>(AttackPrefab);
		m_Attack.Owner = base.gameObject;
		m_Attack.transform.parent = base.transform;
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!TimeController.Instance || TimeController.Instance.Paused)
		{
			return;
		}
		m_Timer -= Time.deltaTime;
		if (!(m_Timer <= 0f))
		{
			return;
		}
		m_Timer = Interval;
		for (int i = 0; i < m_CharactersInTrigger.Count; i++)
		{
			Health health = (m_CharactersInTrigger[i] ? m_CharactersInTrigger[i].GetComponent<Health>() : null);
			if (!health || !(health.CurrentStamina <= 1f) || !(health.CurrentHealth <= 1f) || !m_Attack.DamageData.NonLethal)
			{
				LaunchAttack(m_CharactersInTrigger[i]);
			}
		}
	}

	private void LaunchAttack(GameObject target)
	{
		if ((bool)target)
		{
			m_Attack.SkipAnimation = true;
			m_Attack.LaunchingDirectlyToImpact = false;
			m_Attack.Launch(target);
		}
	}

	protected override void HandleTriggerEnter(GameObject obj)
	{
		if (!m_CharactersInTrigger.Contains(obj))
		{
			m_CharactersInTrigger.Add(obj);
		}
		base.HandleTriggerEnter(obj);
	}

	protected override void HandleTriggerExit(GameObject obj)
	{
		if (m_CharactersInTrigger.Contains(obj))
		{
			m_CharactersInTrigger.Remove(obj);
		}
		base.HandleTriggerExit(obj);
	}
}

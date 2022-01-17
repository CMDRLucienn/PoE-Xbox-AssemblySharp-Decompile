using System;
using UnityEngine;

public class CombatEventArgs : EventArgs
{
	public DamageInfo Damage;

	public GameObject Attacker;

	public GameObject Victim;

	public Vector3 Destination;

	public object CustomData;

	public bool Handled;

	public CombatEventArgs(GameObject attacker, GameObject victim)
	{
		Attacker = attacker;
		Victim = victim;
		if (victim != null)
		{
			Destination = victim.transform.position;
		}
		Handled = false;
	}

	public CombatEventArgs(GameObject attacker, Vector3 destination)
	{
		Attacker = attacker;
		Destination = destination;
		Handled = false;
	}

	public CombatEventArgs(DamageInfo info, GameObject attacker, GameObject victim)
	{
		Damage = info;
		Attacker = attacker;
		Victim = victim;
		Destination = (victim ? victim.transform.position : Vector3.zero);
		Handled = false;
	}

	public CombatEventArgs(DamageInfo info, GameObject attacker, Vector3 destination)
	{
		Damage = info;
		Attacker = attacker;
		Victim = null;
		Destination = destination;
		Handled = false;
	}

	public CombatEventArgs(GameObject attacker, GameObject victim, StatusEffect effect)
	{
		Attacker = attacker;
		Victim = victim;
		Destination = victim.transform.position;
		CustomData = effect;
		Handled = false;
	}
}

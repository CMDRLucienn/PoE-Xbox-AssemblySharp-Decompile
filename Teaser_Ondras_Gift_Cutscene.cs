using System.Collections;
using UnityEngine;

public class Teaser_Ondras_Gift_Cutscene : BasePuppetScript
{
	public PartyWaypoint EndWaypoint;

	public Transform CameraEnd;

	public GameObject Fighter1;

	public GameObject Fighter2;

	public GameObject Fighter3;

	public GameObject Rogue1;

	public GameObject Rogue2;

	public GameObject Rogue3;

	public GameObject Rogue4;

	public GameObject Rogue5;

	public bool Bumps(Mover other, bool staleMate)
	{
		return false;
	}

	public bool Clips(Mover other)
	{
		return true;
	}

	public override IEnumerator RunScript()
	{
		CameraControl c = CameraControl.Instance;
		GameState.ForceCombatMode = true;
		PuppetModeController f1pmc = Fighter1.GetComponent<PuppetModeController>();
		PuppetModeController f2pmc = Fighter2.GetComponent<PuppetModeController>();
		PuppetModeController f3pmc = Fighter3.GetComponent<PuppetModeController>();
		Equipment f1eq = Fighter1.GetComponent<Equipment>();
		Equipment f2eq = Fighter2.GetComponent<Equipment>();
		Equipment f3eq = Fighter3.GetComponent<Equipment>();
		PuppetModeController r1pmc = Rogue1.GetComponent<PuppetModeController>();
		PuppetModeController r2pmc = Rogue2.GetComponent<PuppetModeController>();
		PuppetModeController r3pmc = Rogue3.GetComponent<PuppetModeController>();
		PuppetModeController r4pmc = Rogue4.GetComponent<PuppetModeController>();
		PuppetModeController r5pmc = Rogue5.GetComponent<PuppetModeController>();
		Equipment r1eq = Rogue1.GetComponent<Equipment>();
		Equipment r2eq = Rogue2.GetComponent<Equipment>();
		Equipment r3eq = Rogue3.GetComponent<Equipment>();
		Equipment r4eq = Rogue4.GetComponent<Equipment>();
		Equipment r5eq = Rogue5.GetComponent<Equipment>();
		yield return new WaitForSeconds(1.1f);
		c.FocusOnPoint(CameraEnd.position, 3f);
		StartCoroutine(f1pmc.LaunchAttack(Rogue2, f1eq.PrimaryAttack, allowDamage: true));
		StartCoroutine(f2pmc.LaunchAttack(Rogue5, f2eq.PrimaryAttack, allowDamage: true));
		StartCoroutine(f3pmc.LaunchAttack(Rogue5, f3eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(r1pmc.LaunchAttack(Fighter1, r1eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(0.2f);
		StartCoroutine(r3pmc.LaunchAttack(Fighter3, r3eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(0.2f);
		StartCoroutine(r4pmc.LaunchAttack(Fighter2, r4eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(2.5f);
		StartCoroutine(r5pmc.LaunchAttack(Fighter3, r5eq.SecondaryAttack, allowDamage: true));
		StartCoroutine(r2pmc.LaunchAttack(Fighter2, r2eq.PrimaryAttack, allowDamage: true));
		StartCoroutine(f1pmc.LaunchAttack(Rogue2, f1eq.PrimaryAttack, allowDamage: true));
		StartCoroutine(f2pmc.LaunchAttack(Rogue5, f2eq.PrimaryAttack, allowDamage: true));
		StartCoroutine(f3pmc.LaunchAttack(Rogue5, f3eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(1f);
		StartCoroutine(r5pmc.LaunchAttack(Fighter3, r5eq.SecondaryAttack, allowDamage: true));
		StartCoroutine(r2pmc.LaunchAttack(Fighter2, r2eq.PrimaryAttack, allowDamage: true));
		yield return StartCoroutine(WaitForCamera());
		yield return new WaitForSeconds(1.5f);
		EndScene();
	}
}

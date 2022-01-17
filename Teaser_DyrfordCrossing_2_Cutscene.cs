using System.Collections;
using UnityEngine;

public class Teaser_DyrfordCrossing_2_Cutscene : BasePuppetScript
{
	public GameObject Mage1;

	public GameObject Mage2;

	public GameObject Mage3;

	public GameObject Mage4;

	public GameObject Mage5;

	public GameObject Druid1;

	public GameObject ShieldEffect;

	public GameObject LightningEffect;

	public GameObject LightningImpact;

	public GenericAbility Missile;

	public GenericAbility Shield;

	public GenericAbility Fireball;

	public GenericAbility Storm;

	private SyncCameraOrthoSettings cameraSettings;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override IEnumerator RunScript()
	{
		cameraSettings = SyncCameraOrthoSettings.Instance;
		GameState.ForceCombatMode = true;
		PuppetModeController pmc1 = Mage1.GetComponent<PuppetModeController>();
		PuppetModeController pmc2 = Mage2.GetComponent<PuppetModeController>();
		PuppetModeController pmc3 = Mage3.GetComponent<PuppetModeController>();
		PuppetModeController pmc4 = Mage4.GetComponent<PuppetModeController>();
		PuppetModeController pmc5 = Mage5.GetComponent<PuppetModeController>();
		Equipment r1eq = Mage1.GetComponent<Equipment>();
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(ZoomIn(6f));
		StartCoroutine(pmc2.LaunchAbility(Mage5, Missile, allowDamage: true));
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(pmc5.LaunchAbility(Mage2, Missile, allowDamage: true));
		StartCoroutine(CastFakeSpellSelf(Mage4, ShieldEffect));
		StartCoroutine(pmc1.LaunchAttack(Mage5, r1eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(2f);
		StartCoroutine(pmc4.LaunchAbility(Mage2, Missile, allowDamage: true));
		StartCoroutine(CastFakeSpellSelf(Druid1, null));
		StartCoroutine(pmc1.LaunchAttack(Mage5, r1eq.PrimaryAttack, allowDamage: true));
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(pmc3.LaunchAbility(Mage4, Missile, allowDamage: true));
		yield return new WaitForSeconds(0.1f);
		yield return new WaitForSeconds(2f);
		StartCoroutine(pmc1.LaunchAttack(Mage5, r1eq.PrimaryAttack, allowDamage: true));
		Object.Instantiate(LightningEffect, Mage1.transform.position, Mage1.transform.rotation);
		Object.Instantiate(LightningImpact, Mage1.transform.position, Mage1.transform.rotation);
		StartCoroutine(pmc5.LaunchAbility(Mage2, Fireball, allowDamage: true));
		yield return new WaitForSeconds(0.8f);
		Object.Instantiate(LightningEffect, Mage2.transform.position, Mage2.transform.rotation);
		Object.Instantiate(LightningImpact, Mage2.transform.position, Mage2.transform.rotation);
		yield return new WaitForSeconds(0.8f);
		Object.Instantiate(LightningEffect, Mage3.transform.position, Mage3.transform.rotation);
		Object.Instantiate(LightningImpact, Mage3.transform.position, Mage3.transform.rotation);
		yield return new WaitForSeconds(4.5f);
		EndScene();
	}

	private IEnumerator CastFakeSpellSelf(GameObject wizard, GameObject spell)
	{
		AnimationController anim = wizard.GetComponent<AnimationController>();
		if (anim != null)
		{
			AnimationController.Action action = new AnimationController.Action();
			action.m_actionType = AnimationController.ActionType.Attack;
			action.m_variation = 9;
			action.m_speed = 1f;
			action.m_offhand = false;
			anim.DesiredAction = action;
		}
		yield return new WaitForSeconds(1.6f);
		if (spell != null)
		{
			Object.Instantiate(spell, wizard.transform.position, wizard.transform.rotation);
		}
		if (anim != null)
		{
			anim.DesiredAction.Reset();
			anim.ClearActions();
		}
	}

	private IEnumerator ZoomIn(float time)
	{
		float t = 0f;
		float rate = 1f / time;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			cameraSettings.SetZoomLevel(Mathf.SmoothStep(1.3f, 1f, t), force: false);
			yield return null;
		}
	}
}

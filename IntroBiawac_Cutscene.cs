using System.Collections;
using UnityEngine;

public class IntroBiawac_Cutscene : BasePuppetScript
{
	public float CutsceneDuration = 15f;

	public GameObject FXStorm;

	public string StormAnimationName = "env_storm01";

	public GameObject FXFire;

	public string FireAnimationName = "";

	public GameObject[] Whirlwinds;

	public float[] WhirlwindDelays;

	public GameObject OldFire;

	private void MyActivateObject(GameObject obj, bool bActivate)
	{
		Persistence component = obj.GetComponent<Persistence>();
		if ((bool)component)
		{
			Scripts.ActivateObject(component.GUID, bActivate);
		}
		else
		{
			obj.SetActive(bActivate);
		}
	}

	public override IEnumerator RunScript()
	{
		if ((bool)OldFire)
		{
			MyActivateObject(OldFire, bActivate: false);
		}
		if ((bool)FXStorm)
		{
			MyActivateObject(FXStorm, bActivate: true);
		}
		if ((bool)FXFire)
		{
			MyActivateObject(FXFire, bActivate: true);
		}
		Animator animator = null;
		Animator animator2 = null;
		float fTotalWhirlwindDelays = 0f;
		if ((bool)FXStorm)
		{
			animator = FXStorm.GetComponentInChildren<Animator>();
		}
		if ((bool)animator)
		{
			animator.Play(StormAnimationName);
		}
		if ((bool)FXFire)
		{
			animator2 = FXFire.GetComponentInChildren<Animator>();
		}
		if ((bool)animator2)
		{
			animator2.Play(FireAnimationName);
		}
		for (int i = 0; i < Whirlwinds.Length; i++)
		{
			if (WhirlwindDelays.Length > i)
			{
				yield return new WaitForSeconds(WhirlwindDelays[i]);
				fTotalWhirlwindDelays += WhirlwindDelays[i];
			}
			MyActivateObject(Whirlwinds[i], bActivate: true);
		}
		if (CutsceneDuration > fTotalWhirlwindDelays)
		{
			yield return new WaitForSeconds(CutsceneDuration - fTotalWhirlwindDelays);
		}
		EndScene();
	}
}

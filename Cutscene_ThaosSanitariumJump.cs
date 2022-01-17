using System.Collections;
using UnityEngine;

public class Cutscene_ThaosSanitariumJump : BasePuppetScript
{
	public GameObject[] SoulJumpSequence;

	public GameObject LastCameraFocusWP;

	public Door[] OpenDoorSequence;

	public GameObject[] DoorFXSequence;

	public Waypoint MovingToWaypoint;

	public GameObject PosessedFX;

	public float TimeBetweenDoors = 1f;

	public float TimeBetweenJumps = 1f;

	[Range(0f, 1f)]
	public float StartGlowEffectNormalizedTime = 0.9f;

	public GameObject[] Inmates;

	public GameObject[] InmateWaypoints;

	private float[] InmateDelays = new float[4] { 0.35f, 0.1f, 0.4f, 0f };

	private float[] SoulSpeed = new float[5] { 0.7f, 1f, 1f, 1f, 1f };

	public float InitialHoldTime = 1f;

	public float EndHoldTime = 1f;

	public GameObject SoulObjectPrefab;

	public GameObject DoorOpensFromObject;

	public GameObject PosessionEffect;

	public GameObject RuneCastFX;

	public string CastAnimationState = string.Empty;

	public string PosessedAnimationState = string.Empty;

	private IEnumerator PlayAnimationState(GameObject src, string animationState, bool loop, float fSpeed)
	{
		Animator myAnimator = src.GetComponent<Animator>();
		AnimationController myController = src.GetComponent<AnimationController>();
		float fDefaultCutsceneSpeed = myController.CutsceneSpeed;
		myAnimator.speed = fSpeed;
		myController.CutsceneSpeed = fSpeed;
		myAnimator.SetBool("Loop", loop);
		myAnimator.Play(animationState);
		yield return null;
		int hash = myAnimator.GetCurrentAnimatorStateInfo(0).tagHash;
		while (myAnimator.GetCurrentAnimatorStateInfo(0).tagHash == hash && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		myController.CutsceneSpeed = fDefaultCutsceneSpeed;
	}

	private void MyActivateObject(GameObject obj, bool bActivate)
	{
		obj.SetActive(bActivate);
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
		CameraControl cam = CameraControl.Instance;
		GameObject Uscgrim = SoulJumpSequence[0];
		AnimationController uscgrimController = Uscgrim.GetComponent<AnimationController>();
		yield return new WaitForSeconds(0.5f);
		for (int j = 0; j < SoulJumpSequence.Length - 2; j++)
		{
			GameObject gameObject = SoulJumpSequence[j];
			if (j == 0)
			{
				StartCoroutine(PlayAnimationState(Uscgrim, "Soul_Suck_Start", loop: true, 0.5f));
				continue;
			}
			PuppetModeController component = gameObject.GetComponent<PuppetModeController>();
			if ((bool)component)
			{
				StartCoroutine(component.PathToPoint(MovingToWaypoint.transform.position, 0f, walk: true));
			}
		}
		yield return new WaitForSeconds(0.5f);
		GameObject soulObject = GameResources.Instantiate<GameObject>(SoulObjectPrefab, SoulJumpSequence[0].transform.position, SoulJumpSequence[0].transform.rotation);
		Scripts.SoulMemoryCameraEnable(enabled: true);
		cam.FocusOnObject(soulObject, 0.5f);
		yield return new WaitForSeconds(InitialHoldTime);
		uscgrimController.CutsceneSpeed = 1f;
		for (int i = 1; i < SoulJumpSequence.Length; i++)
		{
			float time = 0f;
			while (time < 1f)
			{
				soulObject.transform.position = Vector3.Slerp(SoulJumpSequence[i - 1].transform.position, SoulJumpSequence[i].transform.position, time);
				if (i < SoulJumpSequence.Length - 2)
				{
					cam.FocusOnObject(soulObject, 0.1f);
				}
				else if (i < SoulJumpSequence.Length - 1)
				{
					cam.FocusOnObject(LastCameraFocusWP, 0.1f);
				}
				time += Time.deltaTime * SoulSpeed[i - 1];
				yield return null;
			}
			SoulJumpSequence[i].GetComponent<PuppetModeController>().StopMovement();
			GameUtilities.LaunchEffect(PosessionEffect, 1f, SoulJumpSequence[i].transform, null);
			AnimationBoneMapper mapper = SoulJumpSequence[i].GetComponent<AnimationBoneMapper>();
			if ((bool)mapper)
			{
				GameUtilities.LaunchLoopingEffect(PosessedFX, 1f, mapper[SoulJumpSequence[i], AttackBase.EffectAttachType.Head], null);
			}
			else
			{
				GameUtilities.LaunchLoopingEffect(PosessedFX, 1f, SoulJumpSequence[i].transform, null);
			}
			if (i == SoulJumpSequence.Length - 2)
			{
				Animator myAnimator = SoulJumpSequence[i].GetComponent<Animator>();
				AnimationController myAnimController = SoulJumpSequence[i].GetComponent<AnimationController>();
				myAnimator.Play(PosessedAnimationState, 0);
				yield return new WaitForSeconds(TimeBetweenJumps);
				CutsceneComponent.RemovePuppetControllerFromActor(Uscgrim);
				uscgrimController.CutsceneSpeed = 0.6f;
				Health component2 = Uscgrim.GetComponent<Health>();
				if ((bool)component2)
				{
					component2.enabled = true;
					component2.ShouldDecay = true;
					component2.Targetable = true;
					component2.ApplyDamageDirectly(component2.MaxStamina * 10f);
				}
				yield return new WaitForSeconds(0.5f);
				myAnimController.CutsceneSpeed = 0.5f;
				myAnimator.speed = 0.5f;
				myAnimator.Play(CastAnimationState, 0);
				if ((bool)mapper)
				{
					GameUtilities.LaunchEffect(RuneCastFX, 1f, mapper[SoulJumpSequence[i], AttackBase.EffectAttachType.RightHand], null);
				}
				yield return new WaitForSeconds(0.5f);
				int hash = myAnimator.GetCurrentAnimatorStateInfo(0).tagHash;
				while (myAnimator.GetCurrentAnimatorStateInfo(0).tagHash == hash && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < StartGlowEffectNormalizedTime)
				{
					yield return null;
				}
				myAnimController.CutsceneSpeed = 1f;
				yield return new WaitForSeconds(EndHoldTime);
				int fxIndex = 0;
				for (int c2 = 0; c2 < OpenDoorSequence.Length; c2 += 2)
				{
					MyActivateObject(DoorFXSequence[fxIndex], bActivate: true);
					fxIndex++;
					yield return new WaitForSeconds(TimeBetweenDoors);
					Door obj = OpenDoorSequence[c2];
					Animator component3 = obj.GetComponent<Animator>();
					component3.enabled = true;
					component3.speed = 2.5f;
					obj.Open(DoorOpensFromObject, ignoreLock: true, i > 0);
					Door obj2 = OpenDoorSequence[c2 + 1];
					Animator component4 = obj2.GetComponent<Animator>();
					component4.enabled = true;
					component4.speed = 2.5f;
					obj2.Open(DoorOpensFromObject, ignoreLock: true, i > 0);
				}
				for (int c2 = 0; c2 < Inmates.Length; c2++)
				{
					if ((bool)Inmates[c2] && !Inmates[c2].GetComponent<Health>().Dead)
					{
						PuppetModeController component5 = Inmates[c2].GetComponent<PuppetModeController>();
						if ((bool)component5 && Inmates.Length == InmateWaypoints.Length && InmateDelays.Length == Inmates.Length)
						{
							StartCoroutine(component5.PathToPoint(InmateWaypoints[c2].transform.position, 0.5f, walk: false));
							yield return new WaitForSeconds(InmateDelays[c2]);
						}
					}
				}
				Scripts.SoulMemoryCameraEnable(enabled: false);
				cam.FocusOnObject(GameState.s_playerCharacter.gameObject, 1.5f);
				Object.Destroy(soulObject, 1.5f);
			}
			else
			{
				SoulJumpSequence[i].GetComponent<Animator>().Play(PosessedAnimationState, 0);
				yield return new WaitForSeconds(TimeBetweenJumps);
			}
		}
		Door[] openDoorSequence = OpenDoorSequence;
		foreach (Door door in openDoorSequence)
		{
			if (!(door == null))
			{
				door.GetComponent<Animator>().speed = 1f;
			}
		}
		GameUtilities.Destroy(SoulJumpSequence[SoulJumpSequence.Length - 1], 1f);
		EndScene();
	}
}

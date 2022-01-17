using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class First_Fires_Palace_Duc_Assassination_Cutscene : BasePuppetScript
{
	public string Conversation = "Assets/Data/Conversations/02_Defiance_Bay_First_Fires/02_cv_duc_assassination.conversation";

	public GameObject Duc;

	public Door HearingDoor;

	public GameObject RamirWalkUpLocation;

	public GameObject ThaosWalkOnLocation;

	public GameObject ThaosWalkOffLocation;

	public GameObject HearingFocusLocation;

	public GameObject AudienceFocusLocation;

	public GameObject[] PetSpawnWPs;

	public GameObject[] PetBalconyWPs;

	public GameObject[] CrowdStandWPs;

	[Range(0.1f, 5f)]
	public float SoulSpeed = 1f;

	public GameObject SoulObjectPrefab;

	public GameObject PosessionEffect;

	public GameObject PosessionArriveEffect;

	public GameObject SoulJumpEffect;

	public GameObject DucDeathEffect;

	public string ThaosCastAnimationState = string.Empty;

	public string RamirCastAnimationState = string.Empty;

	public GameObject[] Dozens;

	public GameObject[] Animancers;

	public GameObject[] Guards;

	public GameObject[] SittingCharacters;

	public string AmbientSitAnimation = "Sit_Back";

	public bool UseSoulCamera = true;

	public float TimerSitDelay = 0.3f;

	public float TimerOpeningDelay = 2f;

	public float TimerDucBarkToRamirWalkUp = 3f;

	public float TimerRamirWalkUpToCameraPan = 1f;

	public float TimerCameraPanToThaosStartMove = 1.5f;

	public float TimerThaosStartMoveToDoorOpen = 1.5f;

	public float TimerThaosArriveToThaosCollapse = 0.1f;

	public float TimerThaosCollapseToSoulFadeOn = 0.25f;

	public float TimerSoulFadeOnToCameraPan = 1f;

	public float TimerDucBarkToPossessedRamirBark = 6f;

	public float TimerPossessedRamirBarkToCastAnimStart = 4.4f;

	public float TimerCastAnimStartToEadricBark = 0.1f;

	public float TimerEadricBarkToSpellCast = 4f;

	public float TimerSpellCastToSoulFadeOn = 0.5f;

	public float TimerSoulLeavesToEadricBark = 1f;

	public float TimerEadricBarkToDozensApproach = 1f;

	public float TimerDozensApproachToRamirBark = 0.5f;

	public float TimerDozensAttackToCameraPan = 1f;

	public float TimerGuardsAttackToThaosLeave = 2f;

	public float TimerThaosLeaveToEndScene = 4.5f;

	private Dictionary<GameObject, GameObject> PossesionEffectDictionary;

	public static GameObject MyFindPet(GameObject owner)
	{
		if (owner == null)
		{
			return null;
		}
		AIController component = owner.GetComponent<AIController>();
		if ((bool)component)
		{
			foreach (GameObject summonedCreature in component.SummonedCreatureList)
			{
				if (summonedCreature != null)
				{
					AIController component2 = summonedCreature.GetComponent<AIController>();
					if (component2 != null && component2.SummonType == AIController.AISummonType.AnimalCompanion)
					{
						return summonedCreature;
					}
				}
			}
		}
		return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

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

	private IEnumerator StandUp(GameObject SittingCharacter, GameObject WP)
	{
		if ((bool)SittingCharacter && (bool)WP)
		{
			TurnOffLoopingAnimation(SittingCharacter);
			AnimationController ac = SittingCharacter.GetComponent<AnimationController>();
			if ((bool)ac)
			{
				ac.ForceCombatIdle = false;
			}
			Vector3 vStart = SittingCharacter.transform.position;
			Vector3 vDest = WP.transform.position;
			float fLerpTime = 0.35f;
			float fLerpPct = 0f;
			while (fLerpPct < 1f)
			{
				SittingCharacter.transform.position = Vector3.Slerp(vStart, vDest, fLerpPct);
				fLerpPct += Time.deltaTime / fLerpTime;
				yield return null;
			}
			if ((bool)ac)
			{
				ac.ForceCombatIdle = true;
			}
		}
	}

	private IEnumerator StandUpCrowd()
	{
		for (int i = 0; i < SittingCharacters.Length; i++)
		{
			if ((bool)SittingCharacters[i] && i < CrowdStandWPs.Length && (bool)CrowdStandWPs[i])
			{
				StartCoroutine(StandUp(SittingCharacters[i], CrowdStandWPs[i]));
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

	private void WarpPetsToWaypointList(GameObject[] WaypointList)
	{
		for (int i = 0; i < RealParty.Length; i++)
		{
			if ((bool)RealParty[i])
			{
				GameObject gameObject = MyFindPet(RealParty[i]);
				if ((bool)gameObject && i < WaypointList.Length && WaypointList[i] != null)
				{
					gameObject.transform.position = WaypointList[i].transform.position;
					gameObject.transform.rotation = WaypointList[i].transform.rotation;
				}
			}
		}
	}

	private IEnumerator MoveSoul(GameObject soulObj, GameObject srcObj, GameObject destObj, float time, bool addSoulPosessionEffect)
	{
		Vector3 srcLoc = srcObj.transform.position;
		Vector3 destLoc = destObj.transform.position;
		GameUtilities.LaunchEffect(SoulJumpEffect, 1f, srcObj.transform, null);
		GameObject value = null;
		if (PossesionEffectDictionary.TryGetValue(srcObj, out value))
		{
			GameUtilities.ShutDownLoopingEffect(value);
			PossesionEffectDictionary.Remove(srcObj);
		}
		AnimationController component = srcObj.GetComponent<AnimationController>();
		if ((bool)component)
		{
			component.ForceCombatIdle = false;
		}
		float internalTime = 0f;
		while (internalTime < 1f)
		{
			soulObj.transform.position = Vector3.Slerp(srcLoc, destLoc, internalTime);
			internalTime += Time.deltaTime / time;
			yield return null;
		}
		soulObj.transform.position = Vector3.Slerp(srcLoc, destLoc, internalTime);
		GameUtilities.LaunchEffect(PosessionArriveEffect, 1f, destObj.transform, null);
		if (addSoulPosessionEffect)
		{
			AnimationBoneMapper componentInChildren = destObj.GetComponentInChildren<AnimationBoneMapper>();
			GameObject gameObject = ((!componentInChildren) ? GameUtilities.LaunchLoopingEffect(PosessionEffect, 1f, destObj.transform, null) : GameUtilities.LaunchLoopingEffect(PosessionEffect, 1f, componentInChildren[destObj, AttackBase.EffectAttachType.Head], null));
			if ((bool)gameObject)
			{
				PossesionEffectDictionary.Add(destObj, gameObject);
			}
			AnimationController component2 = destObj.GetComponent<AnimationController>();
			if ((bool)component2)
			{
				if (component2.Stance == 0)
				{
					component2.Stance = 1;
				}
				component2.ForceCombatIdle = true;
			}
		}
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(TweenEffect(soulObj, 1f, on: false));
	}

	private IEnumerator TweenEffect(GameObject effect, float time, bool on)
	{
		if (UseSoulCamera)
		{
			Scripts.SoulMemoryCameraEnable(on);
		}
		ParticleSystem[] componentsInChildren = effect.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
			emission.enabled = on;
		}
		PE_DeferredPointLight[] pointLights = effect.GetComponentsInChildren<PE_DeferredPointLight>();
		float internalTime = 0f;
		PE_DeferredPointLight[] array;
		while (internalTime < 1f)
		{
			array = pointLights;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].lightAlpha = Mathf.Lerp(on ? 0f : 1f, on ? 1f : 0f, internalTime);
			}
			if (time > 0f)
			{
				internalTime += Time.deltaTime / time;
				yield return null;
			}
			else
			{
				internalTime = 1f;
			}
		}
		array = pointLights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].lightAlpha = Mathf.Lerp(on ? 0f : 1f, on ? 1f : 0f, internalTime);
		}
	}

	private IEnumerator PlayAnimationState(GameObject src, string animationState, bool loop, float fSpeed)
	{
		Animator myAnimator = src.GetComponent<Animator>();
		myAnimator.speed = fSpeed;
		myAnimator.SetBool("Loop", loop);
		myAnimator.Play(animationState);
		yield return null;
		int hash = myAnimator.GetCurrentAnimatorStateInfo(0).tagHash;
		while (myAnimator.GetCurrentAnimatorStateInfo(0).tagHash == hash && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
	}

	private IEnumerator PlayAnimationStateWithOffset(GameObject src, string animationState, bool loop, float fSpeed, float fMinOffset, float fMaxOffset)
	{
		Animator myAnimator = src.GetComponent<Animator>();
		myAnimator.speed = fSpeed;
		myAnimator.SetBool("Loop", loop);
		myAnimator.Play(animationState, 0, OEIRandom.RangeInclusive(fMinOffset, fMaxOffset));
		yield return null;
		int hash = myAnimator.GetCurrentAnimatorStateInfo(0).tagHash;
		while (myAnimator.GetCurrentAnimatorStateInfo(0).tagHash == hash && myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
	}

	private IEnumerator FaceObject(GameObject src, GameObject objectToFace, float interpolationTime)
	{
		Quaternion currentRotation = src.transform.localRotation;
		Quaternion lookRotation = Quaternion.LookRotation(objectToFace.transform.position - src.transform.position);
		float internalTime = 0f;
		while (internalTime < 1f)
		{
			src.transform.localRotation = Quaternion.Slerp(currentRotation, lookRotation, internalTime);
			internalTime += Time.deltaTime / interpolationTime;
			yield return null;
		}
		src.transform.localRotation = Quaternion.Slerp(currentRotation, lookRotation, internalTime);
	}

	private void TurnOffLoopingAnimation(GameObject src)
	{
		src.GetComponent<Animator>().SetBool("Loop", value: false);
	}

	private void OnRamirDamaged(GameObject myObject, GameEventArgs args)
	{
		Health component = GetActor("NPC_Ramir").GetComponent<Health>();
		if ((bool)component)
		{
			component.ApplyDamageDirectly(component.CurrentStamina * 10f);
			component.OnDamaged -= OnRamirDamaged;
		}
	}

	public override IEnumerator RunScript()
	{
		GameObject Thaos = GetActor("CRE_Thaos");
		GameObject Ramir = GetActor("NPC_Ramir");
		GameObject Eadric = GetActor("NPC_Eadric");
		PossesionEffectDictionary = new Dictionary<GameObject, GameObject>();
		Team team = Team.Create();
		team.ScriptTag = "AssassinationCutscene_Dozens";
		team.Register();
		Team SharedDozensTeam = Team.GetTeamByTag(team.ScriptTag);
		Team team2 = Team.Create();
		team2.ScriptTag = "AssassinationCutscene_Animancers";
		team2.Register();
		Team SharedAnimancersTeam = Team.GetTeamByTag(team2.ScriptTag);
		Team team3 = Team.Create();
		team3.ScriptTag = "AssassinationCutscene_Guards";
		team3.Register();
		Team SharedGuardsTeam = Team.GetTeamByTag(team3.ScriptTag);
		GameObject[] dozens = Dozens;
		foreach (GameObject gameObject in dozens)
		{
			if ((bool)gameObject)
			{
				Faction component = gameObject.GetComponent<Faction>();
				if ((bool)component)
				{
					component.CurrentTeamInstance = SharedDozensTeam;
				}
			}
		}
		dozens = Animancers;
		foreach (GameObject gameObject2 in dozens)
		{
			if ((bool)gameObject2)
			{
				Faction component2 = gameObject2.GetComponent<Faction>();
				if ((bool)component2)
				{
					component2.CurrentTeamInstance = SharedAnimancersTeam;
				}
			}
		}
		dozens = Guards;
		foreach (GameObject gameObject3 in dozens)
		{
			if ((bool)gameObject3)
			{
				Faction component3 = gameObject3.GetComponent<Faction>();
				if ((bool)component3)
				{
					component3.CurrentTeamInstance = SharedGuardsTeam;
				}
			}
		}
		Health component4 = Ramir.GetComponent<Health>();
		if ((bool)component4)
		{
			component4.CurrentHealth = 1f;
			component4.CurrentStamina = 1f;
			component4.OnDamaged += OnRamirDamaged;
		}
		CutsceneComponent.UnPauseObject(Duc);
		dozens = SittingCharacters;
		foreach (GameObject gameObject4 in dozens)
		{
			if ((bool)gameObject4)
			{
				StartCoroutine(PlayAnimationStateWithOffset(gameObject4, AmbientSitAnimation, loop: true, 1f, 0f, 0.5f));
			}
		}
		GameObject soulObject = GameResources.Instantiate<GameObject>(SoulObjectPrefab, new Vector3(9999f, 9999f, 9999f), Thaos.transform.rotation);
		MyActivateObject(soulObject, bActivate: true);
		StartCoroutine(TweenEffect(soulObject, 0f, on: false));
		yield return new WaitForSeconds(TimerSitDelay);
		dozens = SittingCharacters;
		foreach (GameObject gameObject5 in dozens)
		{
			if ((bool)gameObject5)
			{
				StartCoroutine(PlayAnimationStateWithOffset(gameObject5, AmbientSitAnimation, loop: true, 1f, 0f, 0.5f));
			}
		}
		yield return new WaitForSeconds(TimerOpeningDelay);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerDucBarkToRamirWalkUp);
		PuppetModeController component5 = Ramir.GetComponent<PuppetModeController>();
		StartCoroutine(component5.PathToPoint(RamirWalkUpLocation.transform.position, 0f, walk: true));
		yield return new WaitForSeconds(TimerRamirWalkUpToCameraPan);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		HearingDoor.GetComponent<Animator>().enabled = true;
		HearingDoor.Open(Thaos, ignoreLock: true, reverseDirection: true);
		yield return new WaitForSeconds(TimerCameraPanToThaosStartMove);
		PuppetModeController thaosPmc = Thaos.GetComponent<PuppetModeController>();
		StartCoroutine(thaosPmc.PathToPoint(ThaosWalkOnLocation.transform.position, 0f, walk: true));
		CameraControl.Instance.FocusOnObject(ThaosWalkOnLocation, 3.5f);
		yield return new WaitForSeconds(TimerThaosStartMoveToDoorOpen);
		yield return StartCoroutine(WaitForMover(Thaos.GetComponent<Mover>()));
		yield return new WaitForSeconds(TimerThaosArriveToThaosCollapse);
		StartCoroutine(PlayAnimationState(Thaos, ThaosCastAnimationState, loop: true, 0.5f));
		yield return new WaitForSeconds(TimerThaosCollapseToSoulFadeOn);
		soulObject.transform.position = Thaos.transform.position;
		TrailRenderer[] componentsInChildren = soulObject.GetComponentsInChildren<TrailRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Clear();
		}
		StartCoroutine(TweenEffect(soulObject, 1f, on: true));
		yield return new WaitForSeconds(TimerSoulFadeOnToCameraPan);
		CameraControl.Instance.FocusOnPoint(HearingFocusLocation.transform.position, SoulSpeed + 0.25f);
		StartCoroutine(MoveSoul(soulObject, Thaos, Ramir, SoulSpeed, !UseSoulCamera));
		StartCoroutine(FaceObject(Ramir, Duc, 0.5f));
		yield return new WaitForSeconds(SoulSpeed / 4f);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerDucBarkToPossessedRamirBark);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerPossessedRamirBarkToCastAnimStart);
		Animator RamirAnimator = Ramir.GetComponent<Animator>();
		RamirAnimator.speed = 0.5f;
		RamirAnimator.SetBool("Loop", value: true);
		RamirAnimator.Play(RamirCastAnimationState);
		yield return new WaitForSeconds(TimerCastAnimStartToEadricBark);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerEadricBarkToSpellCast);
		TurnOffLoopingAnimation(Ramir);
		RamirAnimator.speed = 1f;
		yield return null;
		GameObject obj = GameResources.Instantiate<GameObject>(DucDeathEffect, Duc.transform.position, Duc.transform.rotation);
		MyActivateObject(obj, bActivate: true);
		yield return null;
		Health component6 = Duc.GetComponent<Health>();
		if ((bool)component6)
		{
			component6.ApplyDamageDirectly(component6.CurrentStamina * 10f);
		}
		dozens = ReferencedObjects;
		foreach (GameObject gameObject6 in dozens)
		{
			if (!gameObject6 || gameObject6 == Ramir)
			{
				continue;
			}
			AnimationController component7 = gameObject6.GetComponent<AnimationController>();
			if ((bool)component7)
			{
				if (component7.Stance == 0)
				{
					component7.Stance = 1;
				}
				component7.ForceCombatIdle = true;
			}
		}
		StartCoroutine(StandUpCrowd());
		yield return new WaitForSeconds(TimerSpellCastToSoulFadeOn);
		StartCoroutine(TweenEffect(soulObject, 1f, on: true));
		yield return new WaitForSeconds(1f);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		Persistence component8 = Eadric.GetComponent<Persistence>();
		if ((bool)component8)
		{
			Scripts.SelectWeaponSet(component8.GUID, 1);
		}
		StartCoroutine(MoveSoul(soulObject, Ramir, Thaos, SoulSpeed, addSoulPosessionEffect: false));
		AnimationController component9 = Ramir.GetComponent<AnimationController>();
		if ((bool)component9)
		{
			component9.ForceCombatIdle = false;
		}
		yield return new WaitForSeconds(TimerSoulLeavesToEadricBark);
		yield return new WaitForSeconds(TimerEadricBarkToDozensApproach);
		Scripts.PlayScriptedMusic("mus_scr\\mus_scr_02_thaos_03", blockCombatMusic: true, MusicManager.FadeType.LinearCrossFade, 0.01f, 0.01f, 0f, loop: false);
		StartCoroutine(FaceObject(Ramir, Guards[4], 0.5f));
		SharedDozensTeam.SetRelationship(SharedAnimancersTeam, Faction.Relationship.Hostile, mutual: false);
		float distance = 1.5f;
		int animancerIndex = 0;
		GameObject[] dozens2 = Dozens;
		foreach (GameObject gameObject7 in dozens2)
		{
			if ((bool)gameObject7)
			{
				PuppetModeController component10 = gameObject7.GetComponent<PuppetModeController>();
				if (gameObject7 == Eadric)
				{
					StartCoroutine(component10.PathToPoint(Ramir.transform.position, distance, walk: false));
				}
				else
				{
					StartCoroutine(component10.PathToPoint(Animancers[animancerIndex].transform.position, distance, walk: false));
				}
				animancerIndex++;
				if (animancerIndex >= Animancers.Length)
				{
					animancerIndex = Animancers.Length - 1;
				}
				yield return new WaitForSeconds(0.15f);
			}
		}
		yield return new WaitForSeconds(TimerDozensApproachToRamirBark);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(PlayAnimationState(Ramir, "Warm_Hands", loop: true, 1.5f));
		yield return new WaitForSeconds(1f);
		dozens = Dozens;
		foreach (GameObject gameObject8 in dozens)
		{
			if ((bool)gameObject8)
			{
				CutsceneComponent.RemovePuppetControllerFromActor(gameObject8);
			}
		}
		dozens = Animancers;
		foreach (GameObject gameObject9 in dozens)
		{
			if ((bool)gameObject9)
			{
				CutsceneComponent.RemovePuppetControllerFromActor(gameObject9);
			}
		}
		TurnOffLoopingAnimation(Thaos);
		yield return new WaitForSeconds(TimerDozensAttackToCameraPan);
		CameraControl.Instance.FocusOnPoint(AudienceFocusLocation.transform.position, 3.5f);
		ConversationManager.Instance.StartConversation(Conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		SharedGuardsTeam.SetRelationship(SharedDozensTeam, Faction.Relationship.Hostile, mutual: false);
		int count = 0;
		dozens2 = Guards;
		foreach (GameObject gameObject10 in dozens2)
		{
			if ((bool)gameObject10)
			{
				PuppetModeController component11 = gameObject10.GetComponent<PuppetModeController>();
				if (count == 0)
				{
					StartCoroutine(component11.PathToPoint(Ramir.transform.position, distance, walk: false));
				}
				else
				{
					StartCoroutine(component11.PathToPoint(Animancers[1].transform.position, distance, walk: false));
				}
				yield return new WaitForSeconds(0.015f);
			}
		}
		yield return new WaitForSeconds(1f);
		dozens = Guards;
		foreach (GameObject gameObject11 in dozens)
		{
			if ((bool)gameObject11)
			{
				CutsceneComponent.RemovePuppetControllerFromActor(gameObject11);
			}
		}
		yield return new WaitForSeconds(TimerGuardsAttackToThaosLeave);
		StartCoroutine(thaosPmc.PathToPoint(ThaosWalkOffLocation.transform.position, 0f, walk: true));
		yield return new WaitForSeconds(TimerThaosLeaveToEndScene);
		dozens = ReferencedObjects;
		foreach (GameObject gameObject12 in dozens)
		{
			if ((bool)gameObject12)
			{
				AnimationController component12 = gameObject12.GetComponent<AnimationController>();
				if ((bool)component12)
				{
					component12.ForceCombatIdle = false;
				}
			}
		}
		EndScene();
		WarpPetsToWaypointList(PetSpawnWPs);
		GameUtilities.Destroy(soulObject);
		yield return null;
	}
}

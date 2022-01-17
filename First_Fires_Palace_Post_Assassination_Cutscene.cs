using System.Collections;
using UnityEngine;

public class First_Fires_Palace_Post_Assassination_Cutscene : BasePuppetScript
{
	public GameObject[] FogRevealerList;

	public GameObject ThaosWalkOnLocation;

	public GameObject ThaosExitLocation;

	public GameObject PlayerWalkToLocation;

	public GameObject[] Guards;

	public GameObject[] PetSpawnWPs;

	public string PlayerCollapseAnimationState;

	public float TimerThaosFaceToThaosBark = 0.6f;

	public float TimerThaosBarToCameraPan = 2f;

	public float TimerCameraPan = 2.5f;

	public float TimerCameraPanToGuardsRunIn = 2.5f;

	public float TimerGuardsRunInToThaosBark = 2.25f;

	public float TimerThaosBarkToThaosRun = 1.5f;

	public float TimerThaosRunToGuardsChase = 0.8f;

	public float TimerCameraPanToPlayer = 2f;

	public float TimerPlayerCollapseToEndScene = 3.5f;

	public float TimerFadeOutTime = 2f;

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

	private void MyActivateObject(GameObject oObj, bool bActivate)
	{
		if ((bool)oObj)
		{
			Persistence component = oObj.GetComponent<Persistence>();
			if ((bool)component)
			{
				Scripts.ActivateObject(component.GUID, bActivate);
			}
		}
	}

	private void WarpPetsOutside()
	{
		int num = 0;
		GameObject[] realParty = RealParty;
		foreach (GameObject gameObject in realParty)
		{
			if ((bool)gameObject)
			{
				GameObject gameObject2 = MyFindPet(gameObject);
				if ((bool)gameObject2 && num < PetSpawnWPs.Length && PetSpawnWPs[num] != null)
				{
					gameObject2.transform.position = PetSpawnWPs[num].transform.position;
					gameObject2.transform.rotation = PetSpawnWPs[num].transform.rotation;
					num++;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
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

	private void TurnOffLoopingAnimation(GameObject src)
	{
		src.GetComponent<Animator>().SetBool("Loop", value: false);
	}

	public override IEnumerator RunScript()
	{
		GameObject Thaos = GetActor("CRE_Thaos");
		GameObject Player = GameState.s_playerCharacter.gameObject;
		string conversation = "Assets/Data/Conversations/02_Defiance_Bay_First_Fires/02_cv_duc_assassination2.conversation";
		GameObject[] fogRevealerList = FogRevealerList;
		foreach (GameObject oObj in fogRevealerList)
		{
			MyActivateObject(oObj, bActivate: true);
		}
		fogRevealerList = ReferencedObjects;
		for (int i = 0; i < fogRevealerList.Length; i++)
		{
			AnimationController component = fogRevealerList[i].GetComponent<AnimationController>();
			if ((bool)component)
			{
				if (component.Stance == 0)
				{
					component.Stance = 1;
				}
				component.ForceCombatIdle = true;
			}
		}
		Vector3 point = (PlayerWalkToLocation.transform.position + ThaosWalkOnLocation.transform.position) / 2f;
		CameraControl.Instance.FocusOnPoint(point, 0f);
		PuppetModeController thaosPmc = Thaos.GetComponent<PuppetModeController>();
		StartCoroutine(thaosPmc.PathToPoint(ThaosWalkOnLocation.transform.position, 0f, walk: true));
		fogRevealerList = ReferencedObjects;
		for (int i = 0; i < fogRevealerList.Length; i++)
		{
			AnimationController component2 = fogRevealerList[i].GetComponent<AnimationController>();
			if ((bool)component2)
			{
				if (component2.Stance == 0)
				{
					component2.Stance = 1;
				}
				component2.ForceCombatIdle = true;
			}
		}
		yield return StartCoroutine(WaitForMover(Thaos.GetComponent<Mover>()));
		StartCoroutine(FaceObject(Thaos, Player, 0.25f));
		yield return new WaitForSeconds(TimerThaosFaceToThaosBark);
		ConversationManager.Instance.StartConversation(conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerThaosBarToCameraPan);
		CameraControl.Instance.FocusOnPoint(ThaosWalkOnLocation.transform.position, TimerCameraPan);
		yield return new WaitForSeconds(TimerCameraPanToGuardsRunIn);
		GameObject[] guards = Guards;
		foreach (GameObject gameObject in guards)
		{
			GameObject[] actors = GetActors(gameObject.name);
			int count = 0;
			Vector3 direction = new Vector3(0f, 0f, 1f);
			Vector3 startPos = Thaos.transform.position - direction * 1f;
			GameObject[] array = actors;
			foreach (GameObject obj in array)
			{
				obj.SetActive(value: true);
				PuppetModeController component3 = obj.GetComponent<PuppetModeController>();
				StartCoroutine(component3.PathToPoint(startPos + direction * count * 1.5f, 3f, walk: false));
				int i = count + 1;
				count = i;
				yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
			}
		}
		yield return new WaitForSeconds(TimerGuardsRunInToThaosBark);
		ConversationManager.Instance.StartConversation(conversation, 0, Thaos.gameObject, FlowChartPlayer.DisplayMode.Standard);
		yield return new WaitForSeconds(TimerThaosBarkToThaosRun);
		StartCoroutine(thaosPmc.PathToPoint(ThaosExitLocation.transform.position, 0f, walk: false));
		yield return new WaitForSeconds(TimerThaosRunToGuardsChase);
		guards = Guards;
		foreach (GameObject gameObject2 in guards)
		{
			GameObject[] actors2 = GetActors(gameObject2.name);
			GameObject[] array = actors2;
			for (int count = 0; count < array.Length; count++)
			{
				PuppetModeController component4 = array[count].GetComponent<PuppetModeController>();
				StartCoroutine(component4.PathToPoint(ThaosExitLocation.transform.position, 0.5f, walk: false));
				yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
			}
		}
		CameraControl.Instance.FocusOnPoint(Player.transform.position, TimerCameraPanToPlayer);
		yield return new WaitForSeconds(TimerCameraPanToPlayer);
		StartCoroutine(PlayAnimationState(Player, PlayerCollapseAnimationState, loop: true, 0.3f));
		yield return new WaitForSeconds(TimerPlayerCollapseToEndScene);
		FadeManager.Instance.FadeTo(FadeManager.FadeType.Script, TimerFadeOutTime, Color.black);
		yield return new WaitForSeconds(TimerFadeOutTime);
		TurnOffLoopingAnimation(Player);
		StartCoroutine(PlayAnimationState(Player, "Idle", loop: false, 1f));
		EndScene();
		fogRevealerList = FogRevealerList;
		foreach (GameObject oObj2 in fogRevealerList)
		{
			MyActivateObject(oObj2, bActivate: false);
		}
		WarpPetsOutside();
		yield return null;
	}
}

using System.Collections;
using UnityEngine;

public class ActivateTheMachine_Cutscene : BasePuppetScript
{
	public GameObject CameraFocusObject;

	public GameObject SecondCameraFocusObject;

	public GameObject ThaosStartMachineFX;

	public Animation TheMachine;

	public GameObject MachineFX;

	public GameObject[] EnvironmentFX;

	public GameObject Thaos;

	public Waypoint[] ThaosWaypoints;

	public GameObject[] Chanters;

	public GameObject SoulSuckFX;

	public GameObject SoulDestination;

	public string ChantAnimationName;

	public string ThaosCastAnim;

	public float PanTime = 5f;

	public float AmbientFXStartTime = 1f;

	public float SoulSuckTime = 2f;

	public float NewSoulSuckInterval = 1.5f;

	public float SoulTravelTime = 1f;

	public float FadeToWhiteTime = 0.5f;

	public float AllWhiteTime = 2f;

	public float FadeFromWhiteTime = 2f;

	public float DelayBeforeFadeTime;

	public float CastAnimationSpeed = 0.4f;

	private const int MAX_PARTY_MEMBERS = 6;

	private bool m_stopProjectiles;

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
		CameraControl cam = CameraControl.Instance;
		cam.FocusOnObject(CameraFocusObject, PanTime);
		PuppetModeController thaosController = Thaos.GetComponent<PuppetModeController>();
		Animator thaosAnimator = Thaos.GetComponent<Animator>();
		AnimationController thaosAnimController = Thaos.GetComponent<AnimationController>();
		StartCoroutine(thaosController.PathToPoint(ThaosWaypoints[0].transform.position, 0.05f, walk: true));
		Mover i = Thaos.GetComponent<Mover>();
		while (!i.ReachedGoal && !thaosController.IsInInactiveState())
		{
			yield return null;
		}
		TheMachine.Play("Machine_Start");
		TheMachine.PlayQueued("Machine_On", QueueMode.CompleteOthers);
		thaosAnimController.CutsceneSpeed = CastAnimationSpeed;
		thaosAnimator.speed = CastAnimationSpeed;
		thaosAnimator.Play(ThaosCastAnim);
		GameUtilities.LaunchEffect(ThaosStartMachineFX, 1f, Thaos.transform.position, null);
		yield return new WaitForSeconds(1.5f / CastAnimationSpeed);
		thaosAnimController.CutsceneSpeed = 1f;
		thaosAnimator.speed = 1f;
		StartCoroutine(thaosController.PathToPoint(ThaosWaypoints[1].transform.position, 0f, walk: true));
		yield return new WaitForSeconds(SoulSuckTime);
		int nextFX = 0;
		cam.FocusOnObject(SecondCameraFocusObject, PanTime);
		GameObject[] chanters = Chanters;
		foreach (GameObject chanter in chanters)
		{
			GameObject soul = GameResources.Instantiate<GameObject>(SoulSuckFX, chanter.transform.position, chanter.transform.rotation);
			StartCoroutine(HandleProjectile(soul, chanter.transform.position));
			yield return new WaitForSeconds(AmbientFXStartTime);
			Animator component = chanter.GetComponent<Animator>();
			component.SetBool("Loop", value: true);
			component.Play(ChantAnimationName);
			if (nextFX < EnvironmentFX.Length)
			{
				MyActivateObject(EnvironmentFX[nextFX], bActivate: true);
				nextFX++;
			}
			yield return new WaitForSeconds(NewSoulSuckInterval);
		}
		MyActivateObject(MachineFX, bActivate: true);
		chanters = EnvironmentFX;
		foreach (GameObject obj in chanters)
		{
			MyActivateObject(obj, bActivate: true);
			yield return new WaitForSeconds(0.75f);
		}
		cam.FocusOnObject(CameraFocusObject, PanTime * 0.5f);
		yield return new WaitForSeconds(PanTime * 0.5f);
		yield return new WaitForSeconds(DelayBeforeFadeTime);
		m_stopProjectiles = true;
		cam.ScreenShake(cam.ScreenShakeCatastrophicDuration * 2f, cam.ScreenShakeCatastrophicStrength);
		yield return new WaitForSeconds(cam.ScreenShakeCatastrophicDuration - cam.ScreenShakeCatastrophicDuration / 2f);
		FadeManager.Instance.FadeTo(FadeManager.FadeType.Script, FadeToWhiteTime, Color.white);
		yield return new WaitForSeconds(FadeToWhiteTime);
		ClearParty();
		int num = 0;
		int[] DeadPartyMemberIndices = new int[6];
		for (int k = 0; k < DeadPartyMemberIndices.Length; k++)
		{
			DeadPartyMemberIndices[k] = -1;
		}
		for (int l = 0; l < RealParty.Length; l++)
		{
			if ((bool)RealParty[l] && RealParty[l] != RealPlayer)
			{
				Loot component2 = RealParty[l].GetComponent<Loot>();
				if ((bool)component2)
				{
					GameUtilities.DestroyImmediate(component2);
				}
				RealParty[l].GetComponent<Health>().ApplyDamageDirectly(5000f);
				if (num < DeadPartyMemberIndices.Length)
				{
					DeadPartyMemberIndices[num] = l;
					num++;
				}
			}
			if (l < ActorParty.Length && (bool)ActorParty[l] && ActorParty[l] != ActorPlayer)
			{
				Loot component2 = ActorParty[l].GetComponent<Loot>();
				if ((bool)component2)
				{
					GameUtilities.DestroyImmediate(component2);
				}
				ActorParty[l].GetComponent<Health>().ApplyDamageDirectly(5000f);
			}
		}
		yield return new WaitForSeconds(1f);
		for (int m = 0; m < DeadPartyMemberIndices.Length; m++)
		{
			if (DeadPartyMemberIndices[m] != -1)
			{
				Loot loot = RealParty[DeadPartyMemberIndices[m]].AddComponent<Loot>();
				if ((bool)loot)
				{
					loot.DropEquipment = true;
					loot.DropInventory = true;
					loot.UseBodyAsLootBag = true;
					loot.DropAllItems();
				}
			}
		}
		yield return new WaitForSeconds(AllWhiteTime);
		EndScene();
	}

	public IEnumerator HandleProjectile(GameObject soul, Vector3 launchPos)
	{
		Vector3 position = SoulDestination.transform.position;
		float num = Vector3.Distance(launchPos, position);
		Vector3[] flightPath = new Vector3[4]
		{
			launchPos,
			launchPos + GameState.s_playerCharacter.transform.forward * num * 0.15f,
			launchPos + GameState.s_playerCharacter.transform.up * num * 0.9f,
			position
		};
		GameObject baseFX = Object.Instantiate(soul);
		MyActivateObject(baseFX, bActivate: false);
		while (!m_stopProjectiles)
		{
			GameObject soulProjectile = Object.Instantiate(baseFX, launchPos, Quaternion.identity);
			float travelTime = 0f;
			MyActivateObject(soulProjectile, bActivate: true);
			while (travelTime < SoulTravelTime)
			{
				soulProjectile.transform.position = GetInterpolatedSplinePoint(travelTime / SoulTravelTime, flightPath);
				travelTime += Time.deltaTime;
				yield return null;
			}
			GameUtilities.ShutDownLoopingEffect(soulProjectile);
			GameUtilities.Destroy(soulProjectile, 1f);
		}
		if (m_stopProjectiles && soul != null)
		{
			GameUtilities.Destroy(soul);
		}
	}

	private void ClearParty()
	{
		for (int i = 1; i < RealParty.Length; i++)
		{
			if ((bool)RealParty[i])
			{
				PartyMemberAI component = RealParty[i].GetComponent<PartyMemberAI>();
				if ((bool)component)
				{
					PartyMemberAI.RemoveFromActiveParty(component, purgePersistencePacket: true);
				}
			}
		}
	}
}

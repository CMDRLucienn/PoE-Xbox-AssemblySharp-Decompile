using System.Collections;
using UnityEngine;

public class SummonStatue_Cutscene : BasePuppetScript
{
	private enum AnimalCompanionAnimationType
	{
		Death,
		Standup
	}

	public GameObject SaveTimer;

	public GameObject CombatStartTimer;

	public GameObject Thaos;

	public GameObject FXCastSpell;

	public GameObject ImpactFX;

	public GameObject ImpactWP;

	public GameObject CentralCamFocusWP;

	public GameObject CastFocusWP;

	public GameObject CutsceneSFX;

	public string CastAnimation;

	public GameObject CameraFocus;

	public float PanTime = 2.5f;

	public GameObject FXStatueBreak;

	public float StatueFXDelay = 3f;

	public GameObject[] Statues = new GameObject[2];

	public float DelayBeforeSpawn = 0.5f;

	public float DelayBetweenEffectAndEmerge = 1f;

	public float DelayBeforeEmerge = 0.5f;

	public float DelayPostEmerge = 1f;

	public float Emerge1AnimationSpeed = 0.5f;

	public float Emerge2AnimationSpeed = 0.5f;

	public float ThaosCastSpeed = 0.6f;

	public float ThaosCastDelay = 1.6f;

	private string[] AnimalCompanionControllerTable = new string[5] { "antelope", "bear", "boar", "dog", "stelgaer" };

	private string[] AnimalCompanionDeathAnimTable = new string[5] { "Dead", "dead", "dead", "dead", "Dead" };

	private string[] AnimalCompanionStandupAnimTable = new string[5] { "standup", "Standup", "standup", "standup", "Standup" };

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

	private string GetAnimalCompanionAnimationName(string szController, AnimalCompanionAnimationType animType)
	{
		for (int i = 0; i < AnimalCompanionControllerTable.Length; i++)
		{
			if (szController.ToLower().Contains(AnimalCompanionControllerTable[i]))
			{
				switch (animType)
				{
				case AnimalCompanionAnimationType.Death:
					return AnimalCompanionDeathAnimTable[i];
				case AnimalCompanionAnimationType.Standup:
					return AnimalCompanionStandupAnimTable[i];
				}
			}
		}
		return "";
	}

	private void PopulatePartyList()
	{
		int num = 1;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				GameObject gameObject = partyMemberAI.gameObject;
				if ((bool)partyMemberAI.GetComponent<Player>())
				{
					RealPlayer = partyMemberAI.gameObject;
					ActorPlayer = gameObject;
					RealParty[0] = RealPlayer;
					ActorParty[0] = ActorPlayer;
				}
				else if (partyMemberAI.Slot < 6)
				{
					RealParty[num] = partyMemberAI.gameObject;
					ActorParty[num] = gameObject;
					num++;
				}
			}
		}
	}

	private IEnumerator SpawnStatue(GameObject statue, int nStatueIndex)
	{
		GameUtilities.LaunchEffect(FXStatueBreak, 1f, statue.transform.position, null);
		yield return new WaitForSeconds(DelayBeforeSpawn);
		GetComponent<TileFlipper>().Flip(nStatueIndex + 1);
		statue.SetActive(value: true);
		yield return new WaitForSeconds(DelayBeforeEmerge);
		yield return new WaitForSeconds(DelayBetweenEffectAndEmerge);
		AnimationController statueAnimController = statue.GetComponent<AnimationController>();
		statueAnimController.CutsceneSpeed = ((nStatueIndex == 0) ? Emerge1AnimationSpeed : Emerge2AnimationSpeed);
		Animator statueAnim = statue.GetComponent<Animator>();
		statueAnim.enabled = true;
		statueAnim.SetInteger("Emerge", nStatueIndex + 1);
		while (!statueAnimController.Idle)
		{
			yield return null;
		}
		yield return new WaitForSeconds(DelayPostEmerge);
		statueAnimController.CutsceneSpeed = -1f;
		statueAnim.SetInteger("Emerge", 0);
	}

	public IEnumerator PlayAnimationOnParty(string szAnimation, float fDelayMin, float fDelayMax)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((bool)ActorParty[i])
			{
				ActorParty[i].GetComponent<Animator>().Play(szAnimation);
			}
			yield return new WaitForSeconds(OEIRandom.RangeInclusive(fDelayMin, fDelayMax));
		}
	}

	private IEnumerator PlayAnimationOnPets(AnimalCompanionAnimationType animType, float fDelayMin, float fDelayMax)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((bool)ActorParty[i])
			{
				GameObject gameObject = MyFindPet(ActorParty[i]);
				if ((bool)gameObject)
				{
					Animator component = gameObject.GetComponent<Animator>();
					if ((bool)component)
					{
						string szController = component.runtimeAnimatorController.name;
						string animalCompanionAnimationName = GetAnimalCompanionAnimationName(szController, animType);
						component.SetBool("Loop", value: true);
						component.Play(animalCompanionAnimationName, 0, 0f);
					}
				}
			}
			yield return new WaitForSeconds(OEIRandom.RangeInclusive(fDelayMin, fDelayMax));
		}
	}

	public IEnumerator PlayRandomAnimationOnParty(string[] AnimationList, float fDelayMin, float fDelayMax, GameObject TargetVFX)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((bool)ActorParty[i])
			{
				Animator component = ActorParty[i].GetComponent<Animator>();
				int num = OEIRandom.Index(AnimationList.Length);
				string stateName = AnimationList[num];
				component.Play(stateName);
				if (ImpactFX != null)
				{
					GameUtilities.LaunchEffect(TargetVFX, 1f, ActorParty[i].transform, null);
				}
			}
			yield return new WaitForSeconds(OEIRandom.RangeInclusive(fDelayMin, fDelayMax));
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

	private IEnumerator PetsFaceThaos(float interpolationTime)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((bool)ActorParty[i])
			{
				GameObject gameObject = MyFindPet(ActorParty[i]);
				if ((bool)gameObject)
				{
					StartCoroutine(FaceObject(gameObject, Thaos, interpolationTime));
				}
			}
		}
		yield return new WaitForSeconds(0f);
	}

	public override IEnumerator RunScript()
	{
		if ((bool)CutsceneSFX)
		{
			AudioBank component = CutsceneSFX.GetComponent<AudioBank>();
			if ((bool)component)
			{
				component.PlayFrom("heralds");
			}
		}
		CameraControl cam = CameraControl.Instance;
		Animator thaosAnim = Thaos.GetComponent<Animator>();
		AnimationController thaosAnimController = Thaos.GetComponent<AnimationController>();
		SoundSetComponent thaosSSComp = Thaos.GetComponent<SoundSetComponent>();
		PopulatePartyList();
		StartCoroutine(PetsFaceThaos(0.5f));
		thaosAnimController.CutsceneSpeed = ThaosCastSpeed;
		thaosAnimController.ForceCombatIdle = true;
		cam.FocusOnObject(CastFocusWP, 1f);
		thaosAnim.Play(CastAnimation);
		AnimationBoneMapper componentInChildren = Thaos.GetComponentInChildren<AnimationBoneMapper>();
		GameObject possessionEffect = ((!componentInChildren) ? GameUtilities.LaunchLoopingEffect(FXCastSpell, 1f, Thaos.transform, null) : GameUtilities.LaunchLoopingEffect(FXCastSpell, 1f, componentInChildren[Thaos, AttackBase.EffectAttachType.Head], null));
		thaosSSComp.PlaySound(SoundSet.SoundAction.PriestAbility, 5);
		yield return new WaitForSeconds(ThaosCastDelay / 2f);
		GameUtilities.LaunchEffect(ImpactFX, 1f, ImpactWP.transform, null);
		yield return new WaitForSeconds(ThaosCastDelay / 2f);
		GameUtilities.ShutDownLoopingEffect(possessionEffect);
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(PlayRandomAnimationOnParty(new string[2] { "uni_Death02", "Knockdown" }, 0.01f, 0.05f, null));
		StartCoroutine(PlayAnimationOnPets(AnimalCompanionAnimationType.Death, 0.01f, 0.05f));
		yield return new WaitForSeconds(2f);
		Thaos.transform.LookAt(Thaos.transform);
		thaosAnim.Play(CastAnimation);
		GameUtilities.LaunchEffect(FXCastSpell, 1f, Thaos.transform, null);
		yield return new WaitForSeconds(StatueFXDelay / 2f);
		cam.FocusOnObject(CentralCamFocusWP, 2f);
		yield return new WaitForSeconds(StatueFXDelay / 2f);
		thaosSSComp.PlaySound(SoundSet.SoundAction.IAttack, 1);
		StartCoroutine(SpawnStatue(Statues[0], 0));
		yield return new WaitForSeconds(1.25f);
		StartCoroutine(SpawnStatue(Statues[1], 1));
		yield return new WaitForSeconds(6.5f);
		cam.FocusOnObject(CastFocusWP, 2f);
		yield return new WaitForSeconds(2f);
		StartCoroutine(PlayAnimationOnParty("Standup", 0.01f, 0.4f));
		StartCoroutine(PlayAnimationOnPets(AnimalCompanionAnimationType.Standup, 0.01f, 0.4f));
		cam.FocusOnObject(CastFocusWP, 1f);
		yield return new WaitForSeconds(2f);
		if (ActorParty[0] != null)
		{
			cam.FocusOnObject(ActorParty[0], 0.3f);
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			cam.FocusOnPlayer();
		}
		thaosAnimController.CutsceneSpeed = -1f;
		thaosAnimController.ForceCombatIdle = false;
		EndScene();
		if ((bool)SaveTimer)
		{
			Timer component2 = SaveTimer.GetComponent<Timer>();
			if ((bool)component2)
			{
				component2.StartTimer();
			}
		}
		yield return new WaitForSeconds(0.1f);
		if ((bool)CombatStartTimer)
		{
			Timer component3 = CombatStartTimer.GetComponent<Timer>();
			if ((bool)component3)
			{
				component3.StartTimer();
			}
		}
	}
}

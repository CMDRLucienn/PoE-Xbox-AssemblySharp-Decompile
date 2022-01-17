using System.Collections;
using UnityEngine;

public class PostRuinVision_Cutscene : BasePuppetScript
{
	private enum AnimalCompanionAnimationType
	{
		Death,
		Standup
	}

	public float KODuration = 4f;

	public float BlackScreenTime = 2f;

	public float FadeInTime = 2f;

	public float InitialDelay = 4f;

	public float PauseOnPlayerDuration = 1f;

	public float PanTime = 3f;

	public GameObject CameraStartWP;

	public Animation TheMachine;

	public GameObject MachineAmbientFX;

	public GameObject FogRemover;

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
		if ((bool)TheMachine)
		{
			TheMachine.Play("Machine_Off");
		}
		if ((bool)MachineAmbientFX)
		{
			MyActivateObject(MachineAmbientFX, bActivate: true);
		}
		Animator petAnimator = null;
		GameObject gameObject = null;
		if ((bool)RealPlayer)
		{
			gameObject = MyFindPet(RealPlayer);
		}
		Animator myAnimator = GameState.s_playerCharacter.GetComponent<Animator>();
		myAnimator.SetBool("Loop", value: true);
		myAnimator.Play("uni_Death01", 0, 1f);
		AnimationController myAnimationController = GameState.s_playerCharacter.GetComponent<AnimationController>();
		if ((bool)myAnimationController)
		{
			myAnimationController.CutsceneSpeed = 0.5f;
		}
		AnimationController petAnimationController = null;
		string szAnimationControllerName = "";
		if ((bool)gameObject)
		{
			petAnimator = gameObject.GetComponent<Animator>();
			petAnimationController = gameObject.GetComponent<AnimationController>();
			if ((bool)petAnimator)
			{
				szAnimationControllerName = petAnimator.runtimeAnimatorController.name;
				string animalCompanionAnimationName = GetAnimalCompanionAnimationName(szAnimationControllerName, AnimalCompanionAnimationType.Death);
				petAnimator.SetBool("Loop", value: true);
				petAnimator.Play(animalCompanionAnimationName, 0, 1f);
			}
			if ((bool)petAnimationController)
			{
				petAnimationController.CutsceneSpeed = 0.3f;
			}
		}
		if ((bool)CameraStartWP)
		{
			CameraControl.Instance.FocusOnObject(CameraStartWP, 0f);
		}
		yield return new WaitForSeconds(BlackScreenTime);
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, FadeInTime);
		yield return new WaitForSeconds(InitialDelay);
		CameraControl.Instance.FocusOnObject(GameState.s_playerCharacter.gameObject, PanTime);
		yield return new WaitForSeconds(KODuration);
		if ((bool)petAnimator)
		{
			string animalCompanionAnimationName = GetAnimalCompanionAnimationName(szAnimationControllerName, AnimalCompanionAnimationType.Standup);
			petAnimator.SetBool("Loop", value: false);
			petAnimator.Play(animalCompanionAnimationName);
			yield return new WaitForSeconds(0.75f);
		}
		myAnimator.SetBool("Loop", value: false);
		myAnimator.Play("Standup");
		yield return new WaitForSeconds(PauseOnPlayerDuration);
		if ((bool)myAnimationController)
		{
			myAnimationController.CutsceneSpeed = 1f;
		}
		if ((bool)petAnimationController)
		{
			petAnimationController.CutsceneSpeed = 1f;
		}
		MyActivateObject(FogRemover, bActivate: false);
		EndScene();
	}
}

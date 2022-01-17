using System;
using System.Collections;
using AI;
using UnityEngine;

public class ThaosTeleport_Cutscene : BasePuppetScript
{
	public GameObject Thaos;

	public GameObject[] Heralds;

	public GameObject SoulObjectPrefab;

	private float MinJumpDuration = 2f;

	private float MinReturnJumpDuration = 0.8f;

	private float JumpVelocity = 5f;

	private float ReturnJumpVelocity = 8f;

	private float HeraldDeathDuration = 1f;

	public bool bSoulJumpReturn;

	public override IEnumerator RunScript()
	{
		CameraControl cam = CameraControl.Instance;
		GameInput.EndBlockAllKeys();
		float fJumpDuration = MinJumpDuration;
		int nHeraldIndex = 0;
		for (int i = 0; i < Heralds.Length; i++)
		{
			if (Heralds[i] != null)
			{
				Health component = Heralds[i].GetComponent<Health>();
				if ((bool)component && component.Dead)
				{
					nHeraldIndex++;
					continue;
				}
				Heralds[i].GetComponent<AIController>().SafePopAllStates();
				PauseAI(Heralds[i], bPause: true);
			}
			else
			{
				nHeraldIndex++;
			}
		}
		if (bSoulJumpReturn)
		{
			nHeraldIndex--;
		}
		if (nHeraldIndex < Heralds.Length && Heralds[nHeraldIndex] != null)
		{
			GameObject soulObject = null;
			float fJumpDistance = Vector3.Magnitude(Thaos.transform.position - Heralds[nHeraldIndex].transform.position);
			Vector3 vJumpOrigin = Vector3.zero;
			_ = Quaternion.identity;
			if (bSoulJumpReturn)
			{
				SoundSetComponent component2 = Thaos.GetComponent<SoundSetComponent>();
				if (component2 != null)
				{
					component2.PlaySound(SoundSet.SoundAction.IAttack, 1);
				}
				vJumpOrigin = Heralds[nHeraldIndex].transform.position;
				Quaternion qJumpOrientation = Heralds[nHeraldIndex].transform.rotation;
				cam.FocusOnObject(Heralds[nHeraldIndex], 0.2f);
				yield return new WaitForSeconds(HeraldDeathDuration);
				soulObject = GameResources.Instantiate<GameObject>(SoulObjectPrefab, vJumpOrigin, qJumpOrientation);
			}
			else
			{
				cam.FocusOnObject(Thaos, 0.2f);
			}
			Scripts.SoulMemoryCameraEnable(enabled: true);
			yield return new WaitForSeconds(0.2f);
			if (fJumpDistance > 0f)
			{
				fJumpDuration = fJumpDistance / (bSoulJumpReturn ? ReturnJumpVelocity : JumpVelocity);
			}
			if (!bSoulJumpReturn && fJumpDuration < MinJumpDuration)
			{
				fJumpDuration = MinJumpDuration;
			}
			else if (bSoulJumpReturn && fJumpDuration < MinReturnJumpDuration)
			{
				fJumpDuration = MinReturnJumpDuration;
			}
			if (bSoulJumpReturn)
			{
				if ((bool)soulObject)
				{
					float time = 0f;
					while (time < 1f)
					{
						soulObject.transform.position = Vector3.Slerp(vJumpOrigin, Thaos.transform.position, time);
						cam.FocusOnObject(soulObject, 0.1f);
						time += Time.deltaTime / fJumpDuration;
						yield return null;
					}
					UnityEngine.Object.Destroy(soulObject, 0.3f);
				}
			}
			else
			{
				cam.FocusOnObject(Heralds[nHeraldIndex], fJumpDuration);
			}
			yield return new WaitForSeconds(fJumpDuration);
		}
		if (bSoulJumpReturn)
		{
			ThaosController component3 = Thaos.GetComponent<ThaosController>();
			if ((bool)component3)
			{
				component3.RegainControl();
			}
		}
		for (int j = 0; j < Heralds.Length; j++)
		{
			PauseAI(Heralds[j], bPause: false);
		}
		Scripts.SoulMemoryCameraEnable(enabled: false);
		bSoulJumpReturn = false;
		EndScene();
	}

	private void PauseAI(GameObject obj, bool bPause)
	{
		if (!obj)
		{
			return;
		}
		Type[] array = new Type[2]
		{
			typeof(AIController),
			typeof(Mover)
		};
		for (int i = 0; i < array.Length; i++)
		{
			Behaviour behaviour = obj.GetComponent(array[i]) as Behaviour;
			if ((bool)behaviour && !(behaviour is PartyMemberAI))
			{
				behaviour.enabled = !bPause;
			}
		}
	}
}

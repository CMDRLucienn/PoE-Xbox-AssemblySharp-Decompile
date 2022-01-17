using System.Collections;
using UnityEngine;

public class VisionsDisappear_Cutscene : BasePuppetScript
{
	public GameObject[] FocusArray;

	public GameObject[] ActorArray;

	public int[] ActorCountArray;

	public GameObject[] EmergeFXArray;

	public GameObject DissolveFX;

	public GameObject ThaosSoulGlowFX;

	public float fInitialDelay;

	public float fInitialPanTime;

	public float fPanTime;

	public float fHoldTimeNoPan;

	public float fFocusTime;

	public float fDisableDelay;

	public float fFinalHoldTime;

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
		Vector3 camStart = cam.transform.position;
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueDisable();
		}
		int nActorsDone = 0;
		GameObject[] emergeFXArray = EmergeFXArray;
		foreach (GameObject obj in emergeFXArray)
		{
			MyActivateObject(obj, bActivate: false);
		}
		int nCameraWP = 0;
		DissolveFX.transform.position = ThaosSoulGlowFX.transform.position;
		DissolveFX.transform.rotation = ThaosSoulGlowFX.transform.rotation;
		MyActivateObject(DissolveFX, bActivate: true);
		yield return new WaitForSeconds(fInitialDelay / 2f);
		MyActivateObject(ThaosSoulGlowFX, bActivate: false);
		yield return new WaitForSeconds(fInitialDelay / 2f);
		for (int i = 0; i < ActorCountArray.Length; i++)
		{
			if (i == 0 || i % 2 == 1)
			{
				cam.FocusOnObject(FocusArray[nCameraWP], (i == 0) ? fInitialPanTime : fPanTime);
				nCameraWP++;
				yield return new WaitForSeconds(fPanTime);
			}
			int nFirstActorInVignette = nActorsDone;
			for (int k = nFirstActorInVignette; k < ActorCountArray[i] + nFirstActorInVignette; k++)
			{
				MyActivateObject(EmergeFXArray[k], bActivate: true);
			}
			yield return new WaitForSeconds(fDisableDelay);
			for (int l = nFirstActorInVignette; l < ActorCountArray[i] + nFirstActorInVignette; l++)
			{
				MyActivateObject(ActorArray[l], bActivate: false);
				nActorsDone++;
			}
			if (i == 0 || i % 2 == 1)
			{
				yield return new WaitForSeconds(fHoldTimeNoPan);
			}
			else
			{
				yield return new WaitForSeconds(fFocusTime);
			}
		}
		MyActivateObject(DissolveFX, bActivate: false);
		yield return new WaitForSeconds(fFinalHoldTime);
		cam.FocusOnPoint(camStart, fPanTime * 1.5f);
		yield return new WaitForSeconds(fPanTime * 1.5f);
		emergeFXArray = EmergeFXArray;
		foreach (GameObject obj2 in emergeFXArray)
		{
			MyActivateObject(obj2, bActivate: false);
		}
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueRevealAll();
		}
		EndScene();
	}
}

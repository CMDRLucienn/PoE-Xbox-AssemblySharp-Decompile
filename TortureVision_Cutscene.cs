using System.Collections;
using UnityEngine;

public class TortureVision_Cutscene : BasePuppetScript
{
	public float fVisionLength = 4f;

	public float SoulCamDelay = 0.5f;

	public float fSpawnDelay = 0.2f;

	public float fDelayBetweenReveals = 0.2f;

	public GameObject[] DeviceList;

	public GameObject FXEmerge;

	public override IEnumerator RunScript()
	{
		Scripts.SoulMemoryCameraEnable(enabled: true);
		EndScene();
		yield return new WaitForSeconds(SoulCamDelay);
		for (int i = 0; i < DeviceList.Length; i++)
		{
			if ((bool)DeviceList[i])
			{
				StartCoroutine(SpawnDevice(DeviceList[i], bActivate: true));
				yield return new WaitForSeconds(fDelayBetweenReveals);
			}
		}
		yield return new WaitForSeconds(fVisionLength);
		for (int j = 0; j < DeviceList.Length; j++)
		{
			if ((bool)DeviceList[j])
			{
				StartCoroutine(SpawnDevice(DeviceList[j], bActivate: false));
			}
		}
		Scripts.SoulMemoryCameraEnable(enabled: false);
	}

	public IEnumerator SpawnDevice(GameObject oDevice, bool bActivate)
	{
		if ((bool)oDevice)
		{
			GameUtilities.LaunchEffect(FXEmerge, 1f, oDevice.transform.position, null);
			if (bActivate)
			{
				yield return new WaitForSeconds(fSpawnDelay);
			}
			oDevice.SetActive(bActivate);
			AudioSource component = oDevice.GetComponent<AudioSource>();
			if ((bool)component)
			{
				GlobalAudioPlayer.Instance.PlayOneShot(component, component.clip, 1f);
			}
		}
	}
}

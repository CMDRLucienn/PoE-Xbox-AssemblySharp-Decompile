using System.Collections;
using UnityEngine;

public class SiSEncounterLighting_Cutscene : BasePuppetScript
{
	public GameObject PlayerWP;

	public GameObject SoulPrefab;

	public float SoulTravelTime = 1f;

	public float TimeBetweenSouls = 0.35f;

	public float LightRampUpTime = 0.5f;

	public float LightIntensity = 5f;

	public GameObject[] SoulBurstFX;

	public GameObject LocalFogRevealer;

	public GameObject GodraysFX;

	public GameObject LocalFogFX;

	public GameObject LittleSoulLight;

	public GameObject AshBodies;

	public GameObject Encounter;

	public GameObject DrakeSpawnFX;

	public GameObject[] SoulWaypoints = new GameObject[0];

	public GameObject[] FogFX = new GameObject[0];

	private float[] SoulArcArray = new float[5] { -90f, -45f, 0f, 45f, 90f };

	public GameObject FXPlayerLight;

	private int m_nextWaypoint;

	private int nArcArrayIndex;

	private int nProjectilesCompleted;

	private int nProjectilesReachedTarget;

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
		if (SoulTravelTime == 0f)
		{
			SoulTravelTime = 1f;
		}
		CameraControl instance = CameraControl.Instance;
		WaitForFade(FadeManager.FadeState.ToBlack);
		GameState.s_playerCharacter.transform.position = PlayerWP.transform.position;
		GameState.s_playerCharacter.transform.rotation = PlayerWP.transform.rotation;
		instance.FocusOnPlayer();
		WaitForFade(FadeManager.FadeState.FromBlack);
		yield return new WaitForSeconds(0.3f);
		GameUtilities.LaunchEffect(FXPlayerLight, 1f, GameState.s_playerCharacter.transform.position, null);
		CameraControl.Instance.ScreenShake(3f, 0.3f);
		yield return new WaitForSeconds(2.5f);
		GameObject[] fogFX = FogFX;
		for (int j = 0; j < fogFX.Length; j++)
		{
			GameUtilities.ShutDownLoopingEffect(fogFX[j]);
		}
		GameObject[] souls = new GameObject[SoulWaypoints.Length];
		MyActivateObject(GodraysFX, bActivate: true);
		for (int i = 0; i < SoulWaypoints.Length; i++)
		{
			souls[i] = GameResources.Instantiate<GameObject>(SoulPrefab);
			StartCoroutine(HandleProjectile(souls[i], GameState.s_playerCharacter.transform.position));
			yield return new WaitForSeconds(TimeBetweenSouls);
		}
		EndScene();
		MyActivateObject(LocalFogRevealer, bActivate: true);
		yield return new WaitForSeconds(1.5f);
		MyActivateObject(Encounter, bActivate: true);
		MyActivateObject(AshBodies, bActivate: false);
		yield return new WaitForSeconds(1.5f);
		while (nProjectilesCompleted < SoulWaypoints.Length)
		{
			yield return null;
		}
		MyActivateObject(LocalFogFX, bActivate: false);
		MyActivateObject(LittleSoulLight, bActivate: false);
		for (int k = 0; k < SoulBurstFX.Length; k++)
		{
			MyActivateObject(SoulBurstFX[k], bActivate: false);
		}
	}

	private Vector3 MyGetInterpolatedPointOnSpline(float t, Vector3[] p)
	{
		float num = t * t;
		float num2 = num * t;
		if (p.Length != 4)
		{
			return Vector3.zero;
		}
		return p[0] * Mathf.Pow(1f - t, 3f) + p[1] * 3f * Mathf.Pow(1f - t, 2f) * t + p[2] * (3f * (1f - t) * num) + p[3] * num2;
	}

	public IEnumerator HandleProjectile(GameObject soul, Vector3 launchPos)
	{
		if (m_nextWaypoint >= SoulWaypoints.Length)
		{
			m_nextWaypoint = 0;
		}
		int myWaypoint = m_nextWaypoint;
		m_nextWaypoint++;
		float travelTime = 0f;
		_ = Vector3.zero;
		Vector3 position = SoulWaypoints[myWaypoint].transform.position;
		float num = Vector3.Distance(launchPos, position);
		Vector3[] flightPath = new Vector3[4]
		{
			launchPos,
			launchPos + GameState.s_playerCharacter.transform.forward * num * 0.15f,
			default(Vector3),
			default(Vector3)
		};
		flightPath[1].y += 10f;
		Quaternion quaternion = Quaternion.AngleAxis(SoulArcArray[nArcArrayIndex], Vector3.up);
		nArcArrayIndex++;
		if (nArcArrayIndex >= SoulArcArray.Length)
		{
			nArcArrayIndex = 0;
		}
		flightPath[2] = launchPos + quaternion * (GameState.s_playerCharacter.transform.forward * num * 0.6f);
		flightPath[3] = position;
		float adjustedFlightTime = SoulTravelTime;
		while (travelTime < SoulTravelTime)
		{
			soul.transform.position = MyGetInterpolatedPointOnSpline(travelTime / adjustedFlightTime, flightPath);
			travelTime += Time.deltaTime;
			yield return null;
		}
		MyActivateObject(SoulBurstFX[myWaypoint], bActivate: true);
		StartCoroutine(RampUpLightIntensity(SoulWaypoints[myWaypoint]));
		GameUtilities.ShutDownLoopingEffect(soul);
		GameUtilities.Destroy(soul, 2f);
		nProjectilesReachedTarget++;
		if (nProjectilesReachedTarget == SoulWaypoints.Length && DrakeSpawnFX != null)
		{
			MyActivateObject(DrakeSpawnFX, bActivate: true);
		}
		yield return new WaitForSeconds(2.8f);
		nProjectilesCompleted++;
	}

	public IEnumerator RampUpLightIntensity(GameObject obj)
	{
		if (obj.activeInHierarchy)
		{
			yield break;
		}
		MyActivateObject(obj, bActivate: true);
		PE_DeferredPointLight[] lights = obj.GetComponentsInChildren<PE_DeferredPointLight>();
		if (lights.Length == 0)
		{
			yield break;
		}
		float intensity = 0f;
		while (intensity < 1f)
		{
			intensity += Time.deltaTime * (1f / LightRampUpTime);
			for (int i = 0; i < lights.Length; i++)
			{
				lights[i].lightIntensity = Mathf.Sin(intensity) * LightIntensity;
			}
			yield return null;
		}
	}
}

using System.Collections;
using UnityEngine;

public class LightingOfSunInShadow_Cutscene : BasePuppetScript
{
	public GameObject PlayerPathToPoint;

	public string SoulSuckAnimation = string.Empty;

	public GameObject SoulPrefab;

	public float SoulTravelTime = 4f;

	public float TimeBetweenSouls = 0.35f;

	public float LightRampUpTime = 0.5f;

	public float LightIntensity = 5f;

	public int SoulCount = 1;

	public GameObject SoulBurstFX;

	public GameObject LocalFogRevealer;

	public GameObject AshBodies;

	public GameObject AshBodiesWithFX;

	public GameObject GodraysFX;

	public GameObject LocalFogFX;

	public GameObject StairFogFX;

	public GameObject LittleSoulLight;

	public GameObject[] SoulWaypoints = new GameObject[0];

	public GameObject[] FogFX = new GameObject[0];

	public GameObject[] RevealerList = new GameObject[0];

	public GameObject[] StairBurstFXList = new GameObject[0];

	private float[] SoulArcArray = new float[5] { -90f, -45f, 0f, 45f, 90f };

	private bool[] IsLantern = new bool[28]
	{
		true, true, true, false, true, true, false, false, true, true,
		true, true, true, true, false, false, false, false, false, false,
		true, true, true, true, false, false, false, false
	};

	private bool[] IsStairIndex = new bool[28]
	{
		false, false, false, false, false, false, false, false, false, false,
		false, true, true, true, false, false, false, false, false, false,
		false, false, true, false, false, false, false, false
	};

	private bool[] ShouldSkip = new bool[28]
	{
		false, false, false, false, false, false, false, false, false, false,
		false, false, true, false, true, true, true, false, false, false,
		false, false, false, true, true, true, true, false
	};

	private float[] PanTimeArray = new float[4] { 6f, 2f, 3f, 3f };

	private float[] SoulDelayArray = new float[27]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 0.1f, 0.5f, 0.2f, 0.3f,
		2.5f, 1f, 1f, 1f, 1f, 1f, 0.3f, 0.3f, 0.3f, 0.3f,
		0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f
	};

	private int[] LampOrderIndices = new int[28]
	{
		17, 16, 4, 14, 5, 15, 26, 22, 13, 12,
		11, 2, 10, 2, 23, 21, 9, 8, 25, 19,
		18, 20, 24, 3, 6, 7, 27, 1
	};

	private int[] StairProjectileIndices = new int[4] { 21, 23, 9, 10 };

	private const float MID_INTENSITY = 2.5f;

	private const float HIGH_INTENSITY = 4.25f;

	private float StairDelay = 2.7f;

	private float StairBonusProjectileTime = 1.7f;

	public float PanTime = 3f;

	public float FirstLanternPanTime = 9f;

	public Waypoint[] ReturnCameraMidPoints = new Waypoint[2];

	public float SoulPopOutStartTime = 1f;

	public float PauseTimeBeforeLightup = 1f;

	public GameObject FXPlayerLight;

	private int m_nextWaypoint;

	private int m_nextLanternIndex;

	private int nStairFXUsed;

	private int nArcArrayIndex;

	private bool m_camDone;

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
		CameraControl cam = CameraControl.Instance;
		WaitForFade(FadeManager.FadeState.ToBlack);
		yield return new WaitForSeconds(0.25f);
		GameState.s_playerCharacter.transform.position = PlayerPathToPoint.transform.position;
		GameState.s_playerCharacter.transform.rotation = PlayerPathToPoint.transform.rotation;
		cam.FocusOnPlayer();
		yield return new WaitForSeconds(0.05f);
		WaitForFade(FadeManager.FadeState.FromBlack);
		Vector3 chest = GameState.s_playerCharacter.transform.position;
		cam.FocusOnObject(PlayerPathToPoint, 1.6f);
		yield return new WaitForSeconds(1.6f);
		GameUtilities.LaunchEffect(FXPlayerLight, 1f, GameState.s_playerCharacter.transform.position, null);
		Animator myAnimator = GameState.s_playerCharacter.GetComponent<Animator>();
		myAnimator.SetBool("Loop", value: true);
		myAnimator.Play(SoulSuckAnimation, 0);
		yield return new WaitForSeconds(2f);
		GameObject[] fogFX = FogFX;
		for (int k = 0; k < fogFX.Length; k++)
		{
			GameUtilities.ShutDownLoopingEffect(fogFX[k]);
		}
		StartCoroutine(HandleCameraPan());
		yield return new WaitForSeconds(SoulPopOutStartTime);
		GameObject[] souls = new GameObject[SoulCount];
		for (int j = 0; j < 5; j++)
		{
			souls[j] = GameResources.Instantiate<GameObject>(SoulPrefab);
			StartCoroutine(HandleProjectile(souls[j], chest, bAllowIllumination: true, bUseLanternIndices: false));
			yield return new WaitForSeconds(TimeBetweenSouls);
			m_nextWaypoint = 0;
		}
		yield return new WaitForSeconds(1f);
		MyActivateObject(SoulBurstFX, bActivate: true);
		MyActivateObject(LocalFogRevealer, bActivate: true);
		MyActivateObject(AshBodies, bActivate: false);
		MyActivateObject(AshBodiesWithFX, bActivate: true);
		yield return new WaitForSeconds(1f);
		MyActivateObject(GodraysFX, bActivate: true);
		yield return new WaitForSeconds(1.5f);
		MyActivateObject(LocalFogFX, bActivate: false);
		MyActivateObject(LittleSoulLight, bActivate: false);
		yield return new WaitForSeconds(1f);
		StartCoroutine(SendScatteredSouls(chest));
		int nSouls = LampOrderIndices.Length;
		GameObject[] lanternSouls = new GameObject[nSouls];
		for (int j = 0; j < nSouls; j++)
		{
			if (!ShouldSkip[j])
			{
				lanternSouls[j] = GameResources.Instantiate<GameObject>(SoulPrefab);
				StartCoroutine(HandleProjectile(lanternSouls[j], chest, bAllowIllumination: true, bUseLanternIndices: true));
				if (SoulDelayArray.Length > j)
				{
					yield return new WaitForSeconds(SoulDelayArray[j]);
				}
				else
				{
					yield return new WaitForSeconds(TimeBetweenSouls);
				}
			}
		}
		while (!m_camDone)
		{
			yield return null;
		}
		int animHash = myAnimator.GetCurrentAnimatorStateInfo(0).tagHash;
		myAnimator.SetBool("Loop", value: false);
		while (myAnimator.GetCurrentAnimatorStateInfo(0).tagHash == animHash)
		{
			yield return null;
		}
		WaitForFade(FadeManager.FadeState.ToBlack);
		yield return new WaitForSeconds(0.25f);
		cam.FocusOnPlayer();
		yield return new WaitForSeconds(0.05f);
		WaitForFade(FadeManager.FadeState.FromBlack);
		for (int l = 0; l < RevealerList.Length; l++)
		{
			MyActivateObject(RevealerList[l], bActivate: true);
		}
		EndScene();
	}

	public IEnumerator SendScatteredSouls(Vector3 vSourcePosition)
	{
		GameObject[] souls = new GameObject[SoulCount];
		for (int i = 0; i < SoulCount; i++)
		{
			souls[i] = GameResources.Instantiate<GameObject>(SoulPrefab);
			StartCoroutine(HandleProjectile(souls[i], vSourcePosition, bAllowIllumination: false, bUseLanternIndices: false));
			yield return new WaitForSeconds(TimeBetweenSouls);
		}
	}

	public IEnumerator SendIlluminationSouls(Vector3 vSourcePosition)
	{
		int nSouls = LampOrderIndices.Length;
		GameObject[] souls = new GameObject[nSouls];
		for (int i = 0; i < nSouls; i++)
		{
			souls[i] = GameResources.Instantiate<GameObject>(SoulPrefab);
			StartCoroutine(HandleProjectile(souls[i], vSourcePosition, bAllowIllumination: true, bUseLanternIndices: true));
			if (SoulDelayArray.Length > i)
			{
				yield return new WaitForSeconds(SoulDelayArray[i]);
			}
			else
			{
				yield return new WaitForSeconds(TimeBetweenSouls);
			}
		}
	}

	public IEnumerator HandleOneOffProjectile(GameObject soul, Vector3 launchPos, int nDestinationLanternIndex)
	{
		float travelTime = 0f;
		_ = Vector3.zero;
		Vector3 position = SoulWaypoints[nDestinationLanternIndex].transform.position;
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
			soul.transform.position = GetInterpolatedSplinePoint(travelTime / adjustedFlightTime, flightPath);
			travelTime += Time.deltaTime;
			yield return null;
		}
		GameUtilities.ShutDownLoopingEffect(soul);
		GameUtilities.Destroy(soul, 2f);
	}

	public IEnumerator HandleProjectile(GameObject soul, Vector3 launchPos, bool bAllowIllumination, bool bUseLanternIndices)
	{
		int myWaypoint;
		if (!bUseLanternIndices)
		{
			while (IsLantern.Length > m_nextWaypoint && m_nextWaypoint != 0 && IsLantern[m_nextWaypoint])
			{
				m_nextWaypoint++;
			}
			if (m_nextWaypoint >= SoulWaypoints.Length)
			{
				m_nextWaypoint = 0;
			}
			myWaypoint = m_nextWaypoint;
			m_nextWaypoint++;
		}
		else
		{
			int i;
			for (i = m_nextLanternIndex; ShouldSkip[i]; i++)
			{
			}
			myWaypoint = LampOrderIndices[i];
			m_nextLanternIndex = i + 1;
			if (m_nextLanternIndex >= LampOrderIndices.Length)
			{
				m_nextLanternIndex = 0;
			}
		}
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
			soul.transform.position = GetInterpolatedSplinePoint(travelTime / adjustedFlightTime, flightPath);
			travelTime += Time.deltaTime;
			yield return null;
		}
		if (bAllowIllumination)
		{
			StartCoroutine(RampUpLightIntensity(SoulWaypoints[myWaypoint], myWaypoint));
		}
		if (IsStairIndex[myWaypoint])
		{
			nStairFXUsed++;
			if (nStairFXUsed == StairBurstFXList.Length)
			{
				for (int j = 0; j < StairBurstFXList.Length; j++)
				{
					MyActivateObject(StairBurstFXList[j], bActivate: true);
				}
			}
		}
		GameUtilities.ShutDownLoopingEffect(soul);
		GameUtilities.Destroy(soul, 2f);
	}

	public IEnumerator HandleCameraPan()
	{
		CameraControl cam = CameraControl.Instance;
		cam.FocusOnObject(SoulWaypoints[0], FirstLanternPanTime / 2f);
		yield return new WaitForSeconds(FirstLanternPanTime);
		for (int i = 0; i < ReturnCameraMidPoints.Length; i++)
		{
			if (PanTimeArray.Length > i)
			{
				cam.FocusOnObject(ReturnCameraMidPoints[i].gameObject, PanTimeArray[i]);
				yield return new WaitForSeconds(PanTimeArray[i]);
			}
			else
			{
				cam.FocusOnObject(ReturnCameraMidPoints[i].gameObject, PanTime);
				yield return new WaitForSeconds(PanTime);
			}
			if (i == 1)
			{
				yield return new WaitForSeconds(StairDelay - StairBonusProjectileTime);
				GameObject[] souls = new GameObject[StairProjectileIndices.Length];
				for (int j = 0; j < StairProjectileIndices.Length; j++)
				{
					souls[j] = GameResources.Instantiate<GameObject>(SoulPrefab);
					StartCoroutine(HandleOneOffProjectile(souls[j], GameState.s_playerCharacter.transform.position, StairProjectileIndices[j]));
					yield return new WaitForSeconds(StairBonusProjectileTime / (float)StairProjectileIndices.Length);
				}
				MyActivateObject(StairFogFX, bActivate: false);
				for (int k = 0; k < StairBurstFXList.Length; k++)
				{
					MyActivateObject(StairBurstFXList[k], bActivate: false);
				}
			}
		}
		m_camDone = true;
	}

	public IEnumerator RampUpLightIntensity(GameObject obj, int nLightIndex)
	{
		if (obj.activeInHierarchy)
		{
			yield break;
		}
		PE_DeferredPointLight light = obj.GetComponent<PE_DeferredPointLight>();
		if ((bool)light)
		{
			light.lightIntensity = 0f;
			MyActivateObject(obj, bActivate: true);
			float intensity = 0f;
			while (intensity < 1f)
			{
				intensity += Time.deltaTime * (1f / LightRampUpTime);
				light.lightIntensity = Mathf.Sin(intensity) * LightIntensity;
				yield return null;
			}
		}
	}
}

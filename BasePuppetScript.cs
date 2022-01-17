using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Cutscene))]
public class BasePuppetScript : MonoBehaviour
{
	public GameObject[] ReferencedObjects;

	[HideInInspector]
	public GameObject RealPlayer;

	[HideInInspector]
	public GameObject ActorPlayer;

	[HideInInspector]
	public GameObject[] RealParty = new GameObject[6];

	[HideInInspector]
	public GameObject[] ActorParty = new GameObject[6];

	public float FailSafeTimer = 10f;

	protected Cutscene CutsceneComponent;

	private Transform FollowTransform;

	public static bool KillCoroutine;

	private float m_failsafeTimer = 10f;

	public virtual void Start()
	{
	}

	public void Update()
	{
		m_failsafeTimer -= Time.deltaTime;
	}

	public void Run()
	{
		CutsceneComponent = GetComponent<Cutscene>();
		StartCoroutine(SetupCutscene());
	}

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private IEnumerator SetupCutscene()
	{
		if (CutsceneComponent.AutoFadeOnStart)
		{
			if (FadeManager.Instance.FadeValue < 1f)
			{
				FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Cutscene, 2f);
				ResetFailsafe();
				yield return StartCoroutine(WaitForFade(FadeManager.FadeState.Full));
			}
			FadeManager.Instance.CancelFade(FadeManager.FadeType.Script);
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Cutscene, 2f);
		}
		bool flag = false;
		bool flag2 = false;
		if (CutsceneComponent.UseCameraStartLookAtLocation && CutsceneComponent.CameraStartLookAtLocation != null)
		{
			flag = true;
		}
		if (CutsceneComponent.UseCameraEndLookAtLocation && CutsceneComponent.CameraEndLookAtLocation != null)
		{
			flag2 = true;
		}
		if (flag && flag2)
		{
			CameraControl.Instance.FocusOnPoint(CutsceneComponent.CameraStartLookAtLocation.position, CutsceneComponent.CameraEndLookAtLocation.position, CutsceneComponent.CameraMoveTime);
		}
		else if (flag)
		{
			CameraControl.Instance.FocusOnPoint(CutsceneComponent.CameraStartLookAtLocation.position, 0f);
		}
		else if (flag2)
		{
			CameraControl.Instance.FocusOnPoint(CutsceneComponent.CameraEndLookAtLocation.position, CutsceneComponent.CameraMoveTime);
		}
		if (CutsceneComponent.CameraFollowObject != null)
		{
			FollowTransform = CutsceneComponent.CameraFollowObject;
			ResetFailsafe();
			StartCoroutine(CameraFollowObject());
		}
		foreach (CutsceneWaypoint spawnWaypoint in CutsceneComponent.SpawnWaypointList)
		{
			if (spawnWaypoint.owner != null)
			{
				spawnWaypoint.owner.transform.position = spawnWaypoint.Location.position;
				spawnWaypoint.owner.transform.rotation = spawnWaypoint.Location.rotation;
				if (spawnWaypoint.TeleportVFX != null)
				{
					Object.Instantiate(spawnWaypoint.TeleportVFX, spawnWaypoint.Location.position, spawnWaypoint.Location.rotation);
				}
			}
		}
		StartCoroutine(MoveActorsToStartLocation());
		StartCoroutine(RunScript());
	}

	private IEnumerator MoveActorsToStartLocation()
	{
		foreach (CutsceneWaypoint moveWaypoint in CutsceneComponent.MoveWaypointList)
		{
			if (!(moveWaypoint.owner != null))
			{
				continue;
			}
			if (moveWaypoint.MoveType == CutsceneWaypoint.CutsceneMoveType.Teleport)
			{
				moveWaypoint.owner.transform.position = moveWaypoint.Location.position;
				moveWaypoint.owner.transform.rotation = moveWaypoint.Location.rotation;
				if (moveWaypoint.TeleportVFX != null)
				{
					Object.Instantiate(moveWaypoint.TeleportVFX, moveWaypoint.Location.position, moveWaypoint.Location.rotation);
				}
			}
			else
			{
				PuppetModeController component = moveWaypoint.owner.GetComponent<PuppetModeController>();
				StartCoroutine(component.PathToPoint(moveWaypoint.Location.position, 0f, moveWaypoint.MoveType == CutsceneWaypoint.CutsceneMoveType.Walk));
			}
		}
		yield break;
	}

	public virtual IEnumerator RunScript()
	{
		if (CutsceneComponent.CameraFollowObject == null)
		{
			ResetFailsafe();
			yield return StartCoroutine(WaitForCamera());
		}
		ResetFailsafe();
		yield return StartCoroutine(WaitForMovers());
		if (CutsceneComponent.AutoFadeOnEnd)
		{
			FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Cutscene, 2f);
			ResetFailsafe();
			yield return StartCoroutine(WaitForFade(FadeManager.FadeState.Full));
			FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Cutscene, 2f);
		}
		EndScene();
	}

	public void EndScene()
	{
		FollowTransform = null;
		CutsceneComponent.EndCutscene(callEndScripts: true);
	}

	public GameObject GetActor(string name)
	{
		for (int i = 0; i < ReferencedObjects.Length; i++)
		{
			if (ReferencedObjects[i].name.ToLower() == name.ToLower())
			{
				return ReferencedObjects[i];
			}
		}
		return null;
	}

	public GameObject[] GetActors(string name)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < ReferencedObjects.Length; i++)
		{
			if (ReferencedObjects[i].name.ToLower() == name.ToLower())
			{
				list.Add(ReferencedObjects[i]);
			}
		}
		return list.ToArray();
	}

	public T GetActorComponent<T>(string name) where T : MonoBehaviour
	{
		GameObject actor = GetActor(name);
		if (actor == null)
		{
			return null;
		}
		return actor.GetComponent<T>();
	}

	private bool CheckKill()
	{
		bool killCoroutine = KillCoroutine;
		KillCoroutine = false;
		return killCoroutine;
	}

	private void ResetFailsafe()
	{
		m_failsafeTimer = FailSafeTimer;
	}

	private bool CheckFailsafe()
	{
		if (m_failsafeTimer < 0f)
		{
			return true;
		}
		return false;
	}

	public IEnumerator CameraFollowObject()
	{
		while (FollowTransform != null && !CheckKill() && !CheckFailsafe())
		{
			CameraControl.Instance.FocusOnPoint(FollowTransform.position, 0f);
			yield return null;
		}
	}

	public IEnumerator WaitForFade(FadeManager.FadeState state)
	{
		while (FadeManager.Instance.CutsceneFadeState != state && !CheckKill() && !CheckFailsafe())
		{
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator WaitForCamera()
	{
		while (CameraControl.Instance.InterpolatingToTarget && !CheckKill())
		{
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator WaitForMovers()
	{
		foreach (CutsceneWaypoint moveWaypoint in CutsceneComponent.MoveWaypointList)
		{
			if (!(moveWaypoint.owner != null))
			{
				continue;
			}
			Mover i = moveWaypoint.owner.GetComponent<Mover>();
			while (!i.ReachedGoal)
			{
				if (CheckKill() || CheckFailsafe())
				{
					yield break;
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	public IEnumerator WaitForMover(Mover m)
	{
		while (!m.ReachedGoal && !CheckKill() && !CheckFailsafe())
		{
			yield return new WaitForSeconds(0.1f);
		}
	}

	public Vector3 GetInterpolatedSplinePoint(float t, Vector3[] flightPath)
	{
		return GetPointOnSpline(t, flightPath[0], flightPath[1], flightPath[2], flightPath[3]);
	}

	protected Vector3 GetPointOnSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = t * t;
		float num2 = num * t;
		return p0 * Mathf.Pow(1f - t, 3f) + p1 * 3f * Mathf.Pow(1f - t, 2f) * t + p2 * (3f * (1f - t) * num) + p3 * num2;
	}
}

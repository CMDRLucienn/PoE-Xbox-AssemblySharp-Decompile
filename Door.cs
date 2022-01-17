using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[AddComponentMenu("Toolbox/Door")]
public class Door : OCL
{
	protected Animator m_anim;

	protected List<NavMeshObstacle> m_obstacles = new List<NavMeshObstacle>();

	private bool m_mouseInsideDoor;

	public bool UseSnapPoints = true;

	public bool BlocksFogOfWar = true;

	private static List<Door> s_doorList = new List<Door>();

	private static RaycastHit s_PhysicsHit;

	private static LayerMask s_PhysicsRaycastLayerMask = 0;

	private static int s_WalkableLayerMask = 0;

	private static bool s_DoorUpdateDone = false;

	public static List<Door> DoorList => s_doorList;

	protected override void Start()
	{
		base.Start();
		m_anim = GetComponent<Animator>();
		int currentState = (int)m_currentState;
		if ((bool)m_anim)
		{
			m_anim.SetInteger("CurrentState", currentState);
		}
		SnapToNearbyPoint(1f);
		m_obstacles.Add(GetComponent<NavMeshObstacle>());
		m_obstacles.AddRange(GetComponentsInChildren<NavMeshObstacle>());
		if ((int)s_PhysicsRaycastLayerMask == 0)
		{
			s_PhysicsRaycastLayerMask = LayerMask.GetMask("Walkable", "Doors", "Dynamics No Shadow No Occlusion");
			s_WalkableLayerMask = LayerMask.GetMask("Walkable");
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogError("Door has no colliders (" + base.name + ")!", base.gameObject);
		}
		else if (componentsInChildren.Length > 1 && base.gameObject.GetComponent<Collider>() != null)
		{
			base.gameObject.GetComponent<Collider>().enabled = false;
		}
		Animator[] componentsInChildren2 = GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			GameUtilities.FastForwardAnimator(componentsInChildren2[i], 5);
		}
	}

	private void OnEnable()
	{
		if (!s_doorList.Contains(this))
		{
			s_doorList.Add(this);
		}
		SceneManager.sceneLoaded += OnLoadSceneCallback;
	}

	private void OnDisable()
	{
		if ((base.gameObject == null || !base.gameObject.activeSelf) && s_doorList.Contains(this))
		{
			s_doorList.Remove(this);
		}
		SceneManager.sceneLoaded -= OnLoadSceneCallback;
	}

	private void OnLoadSceneCallback(Scene scene, LoadSceneMode sceneMode)
	{
		int currentState = (int)m_currentState;
		if ((bool)m_anim)
		{
			m_anim.SetInteger("CurrentState", currentState);
		}
	}

	protected override void OnDestroy()
	{
		if (s_doorList.Contains(this))
		{
			s_doorList.Remove(this);
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Restored()
	{
		if (m_currentState == State.Open || m_currentState == State.SealedOpen)
		{
			bool num = m_currentState == State.SealedOpen;
			m_currentState = State.Closed;
			if ((bool)m_anim)
			{
				m_anim.Play("OpenA");
			}
			Open(null, ignoreLock: true);
			if (num)
			{
				m_currentState = State.SealedOpen;
			}
			else
			{
				m_currentState = State.Open;
			}
		}
		else
		{
			Close(null);
			foreach (NavMeshObstacle obstacle in m_obstacles)
			{
				if (obstacle != null)
				{
					obstacle.carving = true;
				}
			}
		}
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameUtilities.FastForwardAnimator(componentsInChildren[i], 5);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (s_DoorUpdateDone)
		{
			return;
		}
		Door door = null;
		if (GameCursor.ObjectUnderCursor == null || s_doorList.Contains(GameCursor.ObjectUnderCursor.GetComponent<Door>()))
		{
			bool flag = false;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(GameInput.MousePosition), out s_PhysicsHit, float.PositiveInfinity, s_PhysicsRaycastLayerMask) && (s_PhysicsHit.collider.gameObject.layer & s_WalkableLayerMask) <= 0)
			{
				for (int i = 0; i < s_doorList.Count; i++)
				{
					door = s_doorList[i];
					if (door.CurrentState != State.SealedOpen && NGUITools.IsChild(door.transform, s_PhysicsHit.transform))
					{
						flag = true;
						door.m_mouseInsideDoor = true;
						door.OnMouseOverManual();
						if (GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
						{
							door.OnMouseDownManual();
						}
					}
					else
					{
						if (door.m_mouseInsideDoor)
						{
							door.OnMouseExit();
						}
						door.m_mouseInsideDoor = false;
					}
				}
			}
			if (!flag)
			{
				for (int j = 0; j < s_doorList.Count; j++)
				{
					door = s_doorList[j];
					if (door.m_mouseInsideDoor)
					{
						door.OnMouseExit();
					}
					door.m_mouseInsideDoor = false;
				}
			}
		}
		s_DoorUpdateDone = true;
	}

	private void LateUpdate()
	{
		s_DoorUpdateDone = false;
	}

	public override bool Open()
	{
		return Open(null, ignoreLock: false);
	}

	public override bool Open(GameObject user, bool ignoreLock)
	{
		return Open(user, ignoreLock, reverseDirection: false);
	}

	public bool Open(GameObject user, bool ignoreLock, bool reverseDirection)
	{
		if (!base.Open(user, ignoreLock))
		{
			return false;
		}
		base.gameObject.tag = "Untagged";
		LayerUtility.SetAllLayers(base.gameObject, LayerMask.NameToLayer("Dynamics No Shadow No Occlusion"));
		foreach (NavMeshObstacle obstacle in m_obstacles)
		{
			if (obstacle != null)
			{
				obstacle.carving = false;
			}
		}
		if ((bool)m_anim)
		{
			Vector3 vector = base.transform.position + base.transform.forward;
			if (user != null)
			{
				vector = user.transform.position;
			}
			float num = Vector3.Dot((vector - base.transform.position).normalized, base.transform.forward);
			if (reverseDirection)
			{
				num = 0f - num;
			}
			m_anim.SetFloat("Direction", num);
			m_anim.SetInteger("CurrentState", (int)m_currentState);
		}
		return true;
	}

	public override bool Close(GameObject user)
	{
		base.Close(user);
		if (m_currentState != 0 && m_currentState != State.Locked && m_currentState != State.Sealed)
		{
			return false;
		}
		base.gameObject.tag = "BlockPathing";
		LayerUtility.SetAllLayers(base.gameObject, LayerMask.NameToLayer("Doors"));
		foreach (NavMeshObstacle obstacle in m_obstacles)
		{
			if (obstacle != null)
			{
				obstacle.carving = true;
			}
		}
		PathFindingManager.RequestRecalculatePaths();
		if ((bool)m_anim)
		{
			m_anim.SetInteger("CurrentState", (int)m_currentState);
			m_anim.SetFloat("Direction", 0f);
		}
		return true;
	}

	private void OnMouseDownManual()
	{
		GameInput.HandleAllClicks();
		GameState.s_playerCharacter.ObjectClicked(this);
	}

	private void OnMouseOverManual()
	{
		if (FogOfWar.PointVisibleInFog(base.transform.position))
		{
			GameCursor.GenericUnderCursor = base.gameObject;
		}
	}

	private void OnMouseExit()
	{
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
	}

	public void SnapToNearbyPoint(float snapDist)
	{
		if (!UseSnapPoints)
		{
			return;
		}
		object[] array = GameObject.FindGameObjectsWithTag("DoorSnapPoint");
		array = array;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = (GameObject)array[i];
			float num = Vector3.Distance(gameObject.transform.position, base.transform.position);
			if (num > 0.001f && num < snapDist)
			{
				base.transform.position = gameObject.transform.position;
			}
		}
	}

	public bool IsAnyMoverIntersectingNavMeshObstacle()
	{
		foreach (Mover mover in Mover.Movers)
		{
			if (!mover.gameObject.activeInHierarchy || GameUtilities.V3SqrDistance2D(mover.transform.position, base.transform.position) > 25f)
			{
				continue;
			}
			foreach (NavMeshObstacle obstacle in m_obstacles)
			{
				if (!(obstacle == null))
				{
					float num = obstacle.radius + mover.Radius;
					if (GameUtilities.V3SqrDistance2D(mover.transform.position, obstacle.transform.position) < num * num)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, UseRadius);
		SnapToNearbyPoint(1.5f);
	}
}

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[AddComponentMenu("Toolbox/Start Point")]
public class StartPoint : MonoBehaviour
{
	public enum PointLocation
	{
		ReferenceByName,
		North1,
		North2,
		South1,
		South2,
		East1,
		East2,
		West1,
		West2,
		Interior01,
		Interior02,
		Interior03,
		Interior04,
		Interior05,
		Interior06,
		Interior07,
		Interior08
	}

	public static StartPoint s_ChosenStartPoint;

	public PointLocation Location;

	public static int GetDistance(PointLocation a, PointLocation b)
	{
		if (a == b)
		{
			return 0;
		}
		if (a < PointLocation.North1 || a > PointLocation.West2 || b < PointLocation.North1 || b > PointLocation.West2)
		{
			return 10;
		}
		int pointRadialPosition = GetPointRadialPosition(a);
		int pointRadialPosition2 = GetPointRadialPosition(b);
		return Mathf.Min(Mathf.Abs(pointRadialPosition - pointRadialPosition2), Mathf.Min(Mathf.Abs(pointRadialPosition - 4 - pointRadialPosition2), Mathf.Abs(pointRadialPosition2 - 4 - pointRadialPosition)));
	}

	private static int GetPointRadialPosition(PointLocation a)
	{
		switch (a)
		{
		case PointLocation.North1:
		case PointLocation.North2:
			return 0;
		case PointLocation.East1:
		case PointLocation.East2:
			return 1;
		case PointLocation.South1:
		case PointLocation.South2:
			return 2;
		case PointLocation.West1:
		case PointLocation.West2:
			return 3;
		default:
			return 0;
		}
	}

	private void Awake()
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter == null || (Location == PointLocation.ReferenceByName && s_playerCharacter.StartPointName != base.gameObject.name) || s_playerCharacter.StartPointLink != Location)
		{
			return;
		}
		if (s_ChosenStartPoint != null)
		{
			if (UIDebug.Instance != null)
			{
				UIDebug.Instance.LogOnScreenWarning("More than one StartPoint in level meets spawn criteria: #1 = " + s_ChosenStartPoint.name + ", #2 = " + base.name + ". Using " + s_ChosenStartPoint.name + ".", UIDebug.Department.Design, 10f);
			}
			else
			{
				Debug.LogError("More than one StartPoint in level meets spawn criteria: #1 = " + s_ChosenStartPoint.name + ", #2 = " + base.name + ". Using " + s_ChosenStartPoint.name + ".");
			}
		}
		else
		{
			s_ChosenStartPoint = this;
		}
	}

	private void Start()
	{
		if (s_ChosenStartPoint == this)
		{
			SpawnPartyHere();
		}
		else
		{
			if (!(s_ChosenStartPoint == null))
			{
				return;
			}
			Player s_playerCharacter = GameState.s_playerCharacter;
			if (s_playerCharacter == null)
			{
				return;
			}
			if (LocationIsExterior(s_playerCharacter.StartPointLink))
			{
				StartPoint[] array = Object.FindObjectsOfType(typeof(StartPoint)) as StartPoint[];
				int num = -1;
				int num2 = 4;
				PointLocation startPointLink = s_playerCharacter.StartPointLink;
				for (int i = 0; i < array.Length; i++)
				{
					if (LocationIsExterior(array[i].Location))
					{
						PointLocation location = array[i].Location;
						int distance = GetDistance(startPointLink, location);
						if (distance < num2)
						{
							num2 = distance;
							num = i;
						}
					}
				}
				if (num >= 0)
				{
					if (UIDebug.Instance != null)
					{
						UIDebug.Instance.LogOnScreenWarning(string.Concat("Party wanted to spawn at '", startPointLink, " but settled for '", array[num].Location, "'."), UIDebug.Department.Design, 10f);
					}
					else
					{
						Debug.Log(string.Concat("Party wanted to spawn at '", startPointLink, " but settled for '", array[num].Location, "'."));
					}
					s_ChosenStartPoint = array[num];
				}
				else
				{
					if (UIDebug.Instance != null)
					{
						UIDebug.Instance.LogOnScreenWarning("Party tried to travel to '" + SceneManager.GetActiveScene().name + "' but it has no exterior StartPoints. Spawning at " + base.name, UIDebug.Department.Design, 10f);
					}
					else
					{
						Debug.LogError("Party tried to travel to '" + SceneManager.GetActiveScene().name + "' but is has no external StartPoints. Spawning at " + base.name);
					}
					s_ChosenStartPoint = this;
				}
				s_ChosenStartPoint.SpawnPartyHere();
			}
			else if (s_playerCharacter.StartPointLink == PointLocation.ReferenceByName && s_playerCharacter.StartPointName == base.name)
			{
				s_ChosenStartPoint = this;
				s_ChosenStartPoint.SpawnPartyHere();
			}
		}
	}

	public static bool LocationIsExterior(PointLocation location)
	{
		if (location != PointLocation.North1 && location != PointLocation.East1 && location != PointLocation.South1)
		{
			return location == PointLocation.West1;
		}
		return true;
	}

	public void SpawnPartyHere()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI == null)
			{
				continue;
			}
			Mover mover = partyMemberAI.Mover;
			mover.enabled = false;
			PartyWaypoint component = GetComponent<PartyWaypoint>();
			Vector3 vector;
			if (component != null)
			{
				vector = component.GetMarkerPosition(partyMemberAI, reverse: true);
			}
			else
			{
				SceneTransition sceneTransition = FindClosestSceneTransition(base.transform.position);
				vector = ((!(sceneTransition != null)) ? partyMemberAI.CalculateFormationPosition(base.transform.position, ignoreSelection: true, out var _) : sceneTransition.GetMarkerPosition(partyMemberAI, reverse: true));
			}
			if (NavMesh.SamplePosition(vector, out var hit, 100f, -1))
			{
				vector = hit.position;
			}
			partyMemberAI.transform.position = vector;
			partyMemberAI.gameObject.transform.position = vector;
			partyMemberAI.gameObject.transform.rotation = base.transform.rotation;
			mover.enabled = true;
			partyMemberAI.InitAI();
			partyMemberAI.gameObject.transform.rotation = base.transform.rotation;
			for (int j = 0; j < partyMemberAI.SummonedCreatureList.Count; j++)
			{
				if (!(partyMemberAI.SummonedCreatureList[j] != null))
				{
					continue;
				}
				AIController aIController = GameUtilities.FindActiveAIController(partyMemberAI.SummonedCreatureList[j]);
				if ((bool)aIController && aIController.SummonType == AIController.AISummonType.Pet)
				{
					Mover component2 = aIController.GetComponent<Mover>();
					if (component2 != null)
					{
						aIController.transform.position = GameUtilities.NearestUnoccupiedLocation(partyMemberAI.transform.position, component2.Radius, 10f, component2);
					}
					else
					{
						aIController.transform.position = GameUtilities.NearestUnoccupiedLocation(partyMemberAI.transform.position, 0.5f, 10f, null);
					}
					if (aIController.StateManager != null)
					{
						aIController.StateManager.AbortStateStack();
					}
					aIController.InitAI();
				}
			}
		}
		CameraControl instance = CameraControl.Instance;
		if ((bool)instance)
		{
			instance.FocusOnPoint(base.transform.position);
		}
		GameCursor.DesiredCursor = GameCursor.CursorType.Normal;
	}

	public static SceneTransition FindClosestSceneTransition(Vector3 position)
	{
		SceneTransition result = null;
		float num = float.MaxValue;
		for (int i = 0; i < SceneTransition.s_activeSceneTransitionsList.Count; i++)
		{
			float num2 = GameUtilities.V3SqrDistance2D(position, SceneTransition.s_activeSceneTransitionsList[i].transform.position);
			if (num2 < num)
			{
				result = SceneTransition.s_activeSceneTransitionsList[i];
				num = num2;
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward);
	}
}

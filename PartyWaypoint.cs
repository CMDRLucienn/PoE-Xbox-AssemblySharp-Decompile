using UnityEngine;

public class PartyWaypoint : MonoBehaviour
{
	public GameObject[] Waypoints = new GameObject[6];

	public void MakePartyWaypoints()
	{
		int[] array = new int[6] { -1, 1, -1, 1, -1, 1 };
		int[] array2 = new int[6] { -1, -1, 0, 0, 1, 1 };
		for (int i = 0; i < 6; i++)
		{
			GameObject gameObject = new GameObject("pw" + (i + 1));
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.transform.localPosition = new Vector3(array[i], 0f, array2[i]);
			if (Physics.Raycast(gameObject.transform.position + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				gameObject.transform.position = hitInfo.point;
			}
			Waypoints[i] = gameObject;
		}
	}

	public void ForcePartyWaypointsToWalkmesh()
	{
		for (int i = 0; i < 6; i++)
		{
			if (!(Waypoints[i] == null) && Physics.Raycast(Waypoints[i].transform.position + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				Waypoints[i].transform.position = hitInfo.point;
			}
		}
	}

	public Vector3 GetMarkerPosition(PartyMemberAI partyMemberAI, bool reverse)
	{
		int slot = partyMemberAI.Slot;
		bool secondary = false;
		if (partyMemberAI.Secondary)
		{
			secondary = true;
			slot = partyMemberAI.Summoner.GetComponent<PartyMemberAI>().Slot;
		}
		return GetMarkerPositionBySlot(reverse ? (5 - slot) : slot, secondary);
	}

	private Vector3 GetMarkerPositionBySlot(int slot, bool secondary)
	{
		if (Waypoints[slot] == null)
		{
			return base.transform.position;
		}
		if (secondary)
		{
			Vector3 vector = Waypoints[slot].transform.position + Waypoints[slot].transform.right;
			if (Physics.Raycast(vector + Vector3.up * 2f, Vector3.down, out var hitInfo))
			{
				vector = hitInfo.point;
			}
			return vector;
		}
		return Waypoints[slot].transform.position;
	}

	public void TeleportPartyToLocation()
	{
		int num = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (partyMemberAI != null)
			{
				if (num < Waypoints.Length && (bool)Waypoints[num])
				{
					partyMemberAI.transform.position = Waypoints[num].transform.position;
					partyMemberAI.transform.rotation = Waypoints[num].transform.rotation;
				}
				else
				{
					Mover component = partyMemberAI.GetComponent<Mover>();
					partyMemberAI.transform.position = GameUtilities.NearestUnoccupiedLocation(base.transform.position, component ? component.Radius : 0.5f, 2f, null);
					partyMemberAI.transform.rotation = base.transform.rotation;
				}
				Stealth stealthComponent = Stealth.GetStealthComponent(partyMemberAI.gameObject);
				if ((bool)stealthComponent)
				{
					stealthComponent.ClearAllSuspicion();
				}
				if (partyMemberAI.StateManager != null)
				{
					partyMemberAI.StateManager.AbortStateStack();
					partyMemberAI.InitAI();
				}
			}
			num++;
		}
		CameraControl.Instance.FocusOnPoint(base.transform.position);
	}

	public void OnDrawGizmos()
	{
		for (int i = 0; i < 6; i++)
		{
			if (!(Waypoints[i] == null))
			{
				Color color = new Color(0f, 0f, 0.2f * (float)i, 0f);
				Gizmos.color = Color.blue - color;
				Gizmos.DrawSphere(Waypoints[i].transform.position, 0.1f);
				GUIHelper.GizmoDrawCircle(Waypoints[i].transform.position, 0.5f);
			}
		}
	}
}

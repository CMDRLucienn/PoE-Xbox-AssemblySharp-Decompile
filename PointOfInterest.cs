using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
	public enum PointDirection
	{
		NORTH,
		NORTH_EAST,
		EAST,
		SOUTH_EAST,
		SOUTH,
		SOUTH_WEST,
		WEST,
		NORTH_WEST,
		CENTER
	}

	public DatabaseString DBText = new DatabaseString(DatabaseString.StringTableType.Gui);

	public UIPointOfInterestVisualData Visuals;

	public PointDirection ArrowDirection = PointDirection.SOUTH_WEST;

	public bool RecordVisited = true;

	public bool InitiallyVisited;

	public bool GivesDiscoveryXp = true;

	[Persistent]
	private bool m_hasBeenVisited;

	public bool HasBeenVisited
	{
		get
		{
			if (!InitiallyVisited)
			{
				return m_hasBeenVisited;
			}
			return true;
		}
		set
		{
			m_hasBeenVisited = value;
		}
	}

	private string GetDisplayString()
	{
		if (Visuals != null && Visuals.Type == UIPointOfInterestVisualData.PointOfInterestType.UserNote)
		{
			return "";
		}
		return DBText.GetText();
	}

	private void Awake()
	{
		if (GivesDiscoveryXp && !GetComponent<Persistence>())
		{
			UIDebug.Instance.LogOnScreenWarning("POI \"" + GetDisplayString() + "\" grants XP but has no Persistence component. Please add one.", UIDebug.Department.Design, 10f);
		}
	}

	private void Update()
	{
		if (!GameState.IsLoading && GivesDiscoveryXp && !HasBeenVisited && (bool)FogOfWar.Instance && FogOfWar.Instance.PointVisible(base.transform.position))
		{
			int mapMarkerDiscoveryXp = BonusXpManager.Instance.MapMarkerDiscoveryXp;
			Console.AddMessage("[" + NGUITools.EncodeColor(Color.yellow) + "]" + GUIUtils.FormatWithLinks(1637, DBText.GetText(), mapMarkerDiscoveryXp * PartyHelper.NumPartyMembers));
			PartyHelper.AssignXPToParty(mapMarkerDiscoveryXp, printMessage: false);
			HasBeenVisited = true;
		}
	}
}

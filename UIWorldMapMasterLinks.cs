using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIWorldMapMasterLinks : MonoBehaviour
{
	private class ProcessedLink
	{
		public float TravelTime;

		public UIWorldMapIcon Preceding;

		public ProcessedLink()
		{
			TravelTime = -1f;
			Preceding = null;
		}
	}

	[Tooltip("List of links that bridge different map tags.")]
	public List<WorldMapLink> MapLinks = new List<WorldMapLink>();

	private IEnumerable<WorldMapLink> m_allLinks;

	private Dictionary<string, Dictionary<string, ProcessedLink>> TravelTimes;

	public float TravelTimeMultiplier = 1f;

	private List<UIWorldMapIcon> closed;

	private IEnumerable<WorldMapLink> AllMapLinks
	{
		get
		{
			if (m_allLinks == null)
			{
				UIWorldMapLinks[] componentsInChildren = UIWorldMapManager.Instance.GetComponentsInChildren<UIWorldMapLinks>(includeInactive: true);
				m_allLinks = MapLinks;
				UIWorldMapLinks[] array = componentsInChildren;
				foreach (UIWorldMapLinks uIWorldMapLinks in array)
				{
					m_allLinks = m_allLinks.Concat(uIWorldMapLinks.MapLinks);
				}
			}
			return m_allLinks;
		}
	}

	public StartPoint.PointLocation GetStartPoint(MapData from, MapData to)
	{
		foreach (WorldMapLink allMapLink in AllMapLinks)
		{
			if (allMapLink.Place1.GetData() == to && allMapLink.Place2.GetData() == from)
			{
				return allMapLink.OverrideStart1;
			}
			if (allMapLink.Place2.GetData() == to && allMapLink.Place1.GetData() == from)
			{
				return allMapLink.OverrideStart2;
			}
		}
		return StartPoint.PointLocation.ReferenceByName;
	}

	public void RebuildTravelTimes()
	{
		UIWorldMapIcon[] componentsInChildren = UIWorldMapManager.Instance.GetComponentsInChildren<UIWorldMapIcon>(includeInactive: true);
		int num = componentsInChildren.Length;
		TravelTimes = new Dictionary<string, Dictionary<string, ProcessedLink>>();
		closed = new List<UIWorldMapIcon>();
		for (int i = 0; i < num; i++)
		{
			if (componentsInChildren[i].GetData() != null && string.IsNullOrEmpty(componentsInChildren[i].VirtualForTag))
			{
				closed.Clear();
				ExpandGraph(componentsInChildren[i].GetData().SceneName, componentsInChildren[i], null, 0f);
			}
		}
		closed = null;
	}

	private void ExpandGraph(string root, UIWorldMapIcon to, UIWorldMapIcon from, float currentTime)
	{
		closed.Add(to);
		ProcessedLink matrixCell = GetMatrixCell(root, to);
		matrixCell.TravelTime = currentTime;
		matrixCell.Preceding = from;
		foreach (WorldMapLink allMapLink in AllMapLinks)
		{
			float num = currentTime + allMapLink.TravelTimeHours * TravelTimeMultiplier;
			if ((bool)allMapLink.Place2 && allMapLink.Place2 == to && (!closed.Contains(allMapLink.Place1) || num < GetMatrixCell(root, allMapLink.Place1).TravelTime))
			{
				ExpandGraph(root, allMapLink.Place1, to, num);
			}
			else if ((bool)allMapLink.Place1 && allMapLink.Place1 == to && (!closed.Contains(allMapLink.Place2) || num < GetMatrixCell(root, allMapLink.Place2).TravelTime))
			{
				ExpandGraph(root, allMapLink.Place2, to, num);
			}
		}
	}

	private ProcessedLink GetMatrixCell(string root, UIWorldMapIcon target)
	{
		return GetMatrixCell(root, target.GetData().SceneName);
	}

	private ProcessedLink GetMatrixCell(MapData root, MapData target)
	{
		return GetMatrixCell(root.SceneName, target.SceneName);
	}

	private ProcessedLink GetMatrixCell(string root, string target)
	{
		if (!TravelTimes.ContainsKey(root))
		{
			TravelTimes[root] = new Dictionary<string, ProcessedLink>();
		}
		if (!TravelTimes[root].ContainsKey(target))
		{
			TravelTimes[root][target] = new ProcessedLink();
		}
		return TravelTimes[root][target];
	}

	public EternityTimeInterval TravelTimeTo(MapData other)
	{
		return TravelTimeBetween(UIWorldMapManager.Instance.GetCurrentMap(), other);
	}

	public EternityTimeInterval TravelTimeBetween(MapData a, MapData b)
	{
		float travelTime = GetMatrixCell(a, b).TravelTime;
		return new EternityTimeInterval((int)((float)(WorldTime.Instance.SecondsPerMinute * WorldTime.Instance.MinutesPerHour) * travelTime));
	}

	public UIWorldMapIcon LastStopBetween(MapData a, MapData b)
	{
		return GetMatrixCell(a, b).Preceding;
	}

	public bool GetConnected(MapData a, MapData b)
	{
		return (int)GetMatrixCell(a, b).TravelTime >= 0;
	}
}

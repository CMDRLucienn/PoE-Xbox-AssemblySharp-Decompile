using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIWorldMapLinks : MonoBehaviour
{
	[Tooltip("List of internal links for this map tag.")]
	public List<WorldMapLink> MapLinks = new List<WorldMapLink>();

	public UIWorldMapIcons Icons;

	public string MapTag => Icons.MapTag;
}

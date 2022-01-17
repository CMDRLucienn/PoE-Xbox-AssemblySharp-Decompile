using System;
using UnityEngine;

[Serializable]
public class StrongholdEscortData
{
	[Tooltip("Duration in days of this mission.")]
	public int Duration;

	[FormatStringKey(new object[] { "The name of the escorting companion.", "The name of the visitor." })]
	public StrongholdDatabaseString EscortDescription = new StrongholdDatabaseString();

	[FormatStringKey(new object[] { "The name of the escorting companion.", "The name of the visitor." })]
	public StrongholdDatabaseString CompleteMessage = new StrongholdDatabaseString(31);
}

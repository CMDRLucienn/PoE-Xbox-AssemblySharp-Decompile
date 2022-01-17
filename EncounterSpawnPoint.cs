using UnityEngine;

public class EncounterSpawnPoint : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(base.transform.position, 0.25f);
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward);
	}
}

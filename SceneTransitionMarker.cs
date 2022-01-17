using UnityEngine;

public class SceneTransitionMarker : MonoBehaviour
{
	public int slot;

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Color color = new Color(0.2f * (float)slot, 0.2f * (float)slot, 0f, 0f);
		Gizmos.color -= color;
		Gizmos.DrawSphere(base.transform.position, 0.1f);
		GUIHelper.GizmoDrawCircle(base.transform.position, 0.5f);
	}
}

using UnityEngine;

public class UIAnchorToMouse : MonoBehaviour
{
	public void Update()
	{
		Vector3 vector = InGameUILayout.NGUICamera.ScreenToWorldPoint(GameInput.MousePosition);
		Vector3 vector2 = new Vector3(vector.x, vector.y, base.transform.position.z);
		if (base.transform.position != vector2)
		{
			base.transform.position = vector2;
		}
	}
}

using UnityEngine;

public static class DrawUtility
{
	public static Color Max(this Color color, float val)
	{
		return new Color(Mathf.Max(color.r, val), Mathf.Max(color.g, val), Mathf.Max(color.b, val), color.a);
	}

	public static void DrawCollider(Transform transform, Collider collider, Color color)
	{
		Gizmos.color = color;
		Vector3[] array = new Vector3[8];
		if (collider is BoxCollider)
		{
			BoxCollider boxCollider = collider as BoxCollider;
			array[0] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(0.5f, 0.5f, 0.5f));
			array[1] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(-0.5f, 0.5f, 0.5f));
			array[2] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(-0.5f, -0.5f, 0.5f));
			array[3] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(0.5f, -0.5f, 0.5f));
			array[4] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(0.5f, 0.5f, -0.5f));
			array[5] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(-0.5f, 0.5f, -0.5f));
			array[6] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(-0.5f, -0.5f, -0.5f));
			array[7] = transform.position + transform.rotation * Vector3.Scale(boxCollider.size, new Vector3(0.5f, -0.5f, -0.5f));
		}
		else
		{
			array[0] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(0.5f, 0.5f, 0.5f));
			array[1] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(-0.5f, 0.5f, 0.5f));
			array[2] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(-0.5f, -0.5f, 0.5f));
			array[3] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(0.5f, -0.5f, 0.5f));
			array[4] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(0.5f, 0.5f, -0.5f));
			array[5] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(-0.5f, 0.5f, -0.5f));
			array[6] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(-0.5f, -0.5f, -0.5f));
			array[7] = transform.position + transform.rotation * Vector3.Scale(collider.transform.localScale, new Vector3(0.5f, -0.5f, -0.5f));
		}
		Gizmos.DrawLine(array[0], array[1]);
		Gizmos.DrawLine(array[0], array[3]);
		Gizmos.DrawLine(array[0], array[4]);
		Gizmos.DrawLine(array[1], array[2]);
		Gizmos.DrawLine(array[1], array[5]);
		Gizmos.DrawLine(array[2], array[3]);
		Gizmos.DrawLine(array[2], array[6]);
		Gizmos.DrawLine(array[3], array[7]);
		Gizmos.DrawLine(array[4], array[5]);
		Gizmos.DrawLine(array[4], array[7]);
		Gizmos.DrawLine(array[6], array[7]);
		Gizmos.DrawLine(array[6], array[5]);
	}

	public static Texture2D CreateSolidTexture(Color color)
	{
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.SetPixels(new Color[4] { color, color, color, color });
		texture2D.Apply();
		return texture2D;
	}
}

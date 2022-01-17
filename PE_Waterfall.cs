using System;
using UnityEngine;

[ExecuteInEditMode]
public class PE_Waterfall : MonoBehaviour
{
	public int renderOrder;

	private Vector4 m_uvOffset = Vector4.zero;

	private void Update()
	{
		if ((bool)GetComponent<Renderer>())
		{
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			if ((bool)sharedMaterial)
			{
				float deltaTime = Time.deltaTime;
				Vector4 vector = sharedMaterial.GetVector("_FallDirection");
				float z = vector.z;
				Vector4 vector2 = sharedMaterial.GetVector("_ColorTextureScrolling");
				float z2 = vector2.z;
				m_uvOffset.x += deltaTime * z * vector.x;
				m_uvOffset.y += deltaTime * z * vector.y;
				m_uvOffset.z += deltaTime * z2 * vector2.x;
				m_uvOffset.w += deltaTime * z2 * vector2.y;
				m_uvOffset.x = Mathf.Repeat(m_uvOffset.x, 1f);
				m_uvOffset.y = Mathf.Repeat(m_uvOffset.y, 1f);
				m_uvOffset.z = Mathf.Repeat(m_uvOffset.z, 1f);
				m_uvOffset.w = Mathf.Repeat(m_uvOffset.w, 1f);
				sharedMaterial.SetVector("_UVOffset", m_uvOffset);
				sharedMaterial.renderQueue = 2990 + renderOrder;
			}
		}
	}

	private Vector2 GetVectorFromAngle(float degrees)
	{
		Vector2 result = default(Vector2);
		result.x = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		result.y = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		return result;
	}
}

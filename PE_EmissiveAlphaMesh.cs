using UnityEngine;

public class PE_EmissiveAlphaMesh : MonoBehaviour
{
	private Vector4 m_uvOffset = Vector4.zero;

	private static int prop_UVOffset;

	private static int prop_EmissiveScroll;

	private static int prop_AlphaScroll;

	private void Awake()
	{
		prop_UVOffset = Shader.PropertyToID("_UVOffset");
		prop_EmissiveScroll = Shader.PropertyToID("_EmissiveScroll");
		prop_AlphaScroll = Shader.PropertyToID("_AlphaScroll");
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		Material[] sharedMaterials = GetComponent<Renderer>().sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			if ((bool)material && material.HasProperty(prop_EmissiveScroll))
			{
				float num = Time.deltaTime;
				if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject))
				{
					num = Time.unscaledDeltaTime;
				}
				Vector4 vector = material.GetVector(prop_EmissiveScroll);
				Vector4 vector2 = material.GetVector(prop_AlphaScroll);
				Vector2 vectorFromAngle = PE_Math.GetVectorFromAngle(vector.x);
				Vector2 vectorFromAngle2 = PE_Math.GetVectorFromAngle(vector2.x);
				Vector2 vector3 = new Vector2(vectorFromAngle.x * vector.y, vectorFromAngle.y * vector.y);
				Vector2 vector4 = new Vector2(vectorFromAngle2.x * vector2.y, vectorFromAngle2.y * vector2.y);
				m_uvOffset.x += vector3.x * num;
				m_uvOffset.y += vector3.y * num;
				m_uvOffset.z += vector4.x * num;
				m_uvOffset.w += vector4.y * num;
				m_uvOffset.x = Mathf.Repeat(m_uvOffset.x, 1f);
				m_uvOffset.y = Mathf.Repeat(m_uvOffset.y, 1f);
				m_uvOffset.z = Mathf.Repeat(m_uvOffset.z, 1f);
				m_uvOffset.w = Mathf.Repeat(m_uvOffset.w, 1f);
				material.SetVector(prop_UVOffset, m_uvOffset);
			}
		}
	}
}

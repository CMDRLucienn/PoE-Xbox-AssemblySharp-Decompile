using UnityEngine;

public class PE_TextureSettings : MonoBehaviour
{
	public float mipMapBias = -1f;

	private static bool m_errorLogged;

	private void Start()
	{
		bool flag = false;
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (null != component && null != component.material)
		{
			Texture2D texture2D = component.material.mainTexture as Texture2D;
			if (null != texture2D)
			{
				texture2D.mipMapBias = mipMapBias;
				flag = true;
			}
		}
		if (!flag && !m_errorLogged)
		{
			Debug.LogError("PE_TextureSettings::Start() - failed to set mipmap bias", base.gameObject);
			m_errorLogged = true;
		}
	}
}

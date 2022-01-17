using UnityEngine;

[RequireComponent(typeof(UITexture))]
[ExecuteInEditMode]
public class UIClippedTexture : MonoBehaviour
{
	public Texture2D ClipMask;

	private void Awake()
	{
		UITexture component = base.gameObject.GetComponent<UITexture>();
		if ((bool)component.material)
		{
			component.material.SetTexture("_MaskTex", ClipMask);
		}
	}

	public void OnTextureChanged()
	{
		UITexture component = base.gameObject.GetComponent<UITexture>();
		if ((bool)component.material)
		{
			component.material.SetTexture("_MaskTex", ClipMask);
		}
	}
}

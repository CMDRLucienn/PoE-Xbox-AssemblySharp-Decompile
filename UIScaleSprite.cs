using UnityEngine;

[ExecuteInEditMode]
public class UIScaleSprite : MonoBehaviour
{
	[Range(0.01f, 1f)]
	public float Scale = 1f;

	private void Start()
	{
		UpdateScale();
	}

	private void UpdateScale()
	{
		NGUITools.MakePixelPerfect(base.transform);
		base.transform.localScale *= Scale;
	}
}

using UnityEngine;

public class UIDissolve : MonoBehaviour
{
	public Texture DissolveTexture;

	public float DissolveValue;

	public float DissolveScale;

	private TweenValue tweenControlled;

	private float prevDissolveValue;

	private float prevDissolveScale;

	private void Start()
	{
		Shader.SetGlobalTexture("_DissolveTexture", DissolveTexture);
		Shader.SetGlobalFloat("_DissolveAlphaControl", DissolveValue);
		Shader.SetGlobalFloat("_DissolveScale", DissolveScale);
		prevDissolveValue = DissolveValue;
		prevDissolveScale = DissolveScale;
		tweenControlled = GetComponent<TweenValue>();
	}

	public void RandomOffsetDissolveTexture()
	{
		Shader.SetGlobalVector("_DissolveOffset", new Vector4(OEIRandom.FloatValue(), OEIRandom.FloatValue(), 0f, 0f));
	}

	private void Update()
	{
		if ((bool)tweenControlled)
		{
			DissolveValue = tweenControlled.Value;
		}
		if (prevDissolveValue != DissolveValue)
		{
			Shader.SetGlobalFloat("_DissolveAlphaControl", DissolveValue);
			prevDissolveValue = DissolveValue;
		}
		if (prevDissolveScale != DissolveScale)
		{
			Shader.SetGlobalFloat("_DissolveScale", DissolveScale);
			prevDissolveScale = DissolveScale;
		}
	}
}

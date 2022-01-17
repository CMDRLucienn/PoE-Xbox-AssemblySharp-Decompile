using System;
using UnityEngine;

public class UIHealthObfuscator : MonoBehaviour
{
	[Serializable]
	public class ScrollingMaskedTexture
	{
		public UITexture ScrollingTexture;

		public Vector2 ScrollRate;
	}

	public ScrollingMaskedTexture[] ScrollingTextures;

	public Texture2D ClipMask;

	public TweenColor PulseOverlay;

	private CharacterStats m_character;

	private void Awake()
	{
		ScrollingMaskedTexture[] scrollingTextures = ScrollingTextures;
		for (int i = 0; i < scrollingTextures.Length; i++)
		{
			UITexture scrollingTexture = scrollingTextures[i].ScrollingTexture;
			scrollingTexture.material = new Material(scrollingTexture.material);
			scrollingTexture.mainTexture = scrollingTexture.mainTexture;
			scrollingTexture.material.SetTexture("_MaskTex", ClipMask);
		}
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		int num = 0;
		if ((bool)m_character)
		{
			StatusEffect statusEffect = m_character.FindFirstStatusEffectOfType(StatusEffect.ModifiedStat.HidesHealthStamina);
			if (statusEffect != null)
			{
				num = (int)statusEffect.ParamsExtraValue();
				if (num < 0 || num >= UIPartyPortraitBar.Instance.ObfusticatorColors.Length)
				{
					Debug.LogError(string.Concat("HideHealthStamina effect from '", statusEffect.Origin, "' has a bad index in ExtraValue (", num, ")."));
					num = 0;
				}
			}
		}
		for (int i = 0; i < ScrollingTextures.Length; i++)
		{
			ScrollingMaskedTexture scrollingMaskedTexture = ScrollingTextures[i];
			UITexture scrollingTexture = scrollingMaskedTexture.ScrollingTexture;
			scrollingTexture.mainTexture = UIPartyPortraitBar.Instance.ObfusticatorColors[num].FlameImage;
			scrollingTexture.material.SetFloat("_ScrollY", scrollingTexture.material.GetFloat("_ScrollY") + scrollingMaskedTexture.ScrollRate.y * Time.unscaledDeltaTime);
			scrollingTexture.material.SetFloat("_ScrollX", scrollingTexture.material.GetFloat("_ScrollX") + scrollingMaskedTexture.ScrollRate.x * Time.unscaledDeltaTime);
		}
		PulseOverlay.from = UIPartyPortraitBar.Instance.ObfusticatorColors[num].PulseColor1;
		PulseOverlay.to = UIPartyPortraitBar.Instance.ObfusticatorColors[num].PulseColor2;
	}

	public void LoadCharacter(CharacterStats cs)
	{
		m_character = cs;
	}
}

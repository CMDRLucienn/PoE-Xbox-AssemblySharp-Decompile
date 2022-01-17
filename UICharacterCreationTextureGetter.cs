using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class UICharacterCreationTextureGetter : UICharacterCreationElement
{
	public enum TextureType
	{
		PORTRAIT,
		PORTRAIT_BIG
	}

	public TextureType Texture;

	private Coroutine m_loadPortraitTexture;

	public override void SignalValueChanged(ValueType type)
	{
		if (base.gameObject.activeInHierarchy && (type == ValueType.Portrait || type == ValueType.All))
		{
			if (m_loadPortraitTexture != null)
			{
				StopCoroutine(m_loadPortraitTexture);
			}
			switch (Texture)
			{
			case TextureType.PORTRAIT:
				m_loadPortraitTexture = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(base.Owner.Character.CharacterPortraitSmallPath, PortraitLoaded));
				break;
			case TextureType.PORTRAIT_BIG:
				m_loadPortraitTexture = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(base.Owner.Character.CharacterPortraitLargePath, PortraitLoaded));
				break;
			}
		}
	}

	private void PortraitLoaded(Texture2D loadedTexture)
	{
		GetComponent<UITexture>().mainTexture = loadedTexture;
		m_loadPortraitTexture = null;
	}
}

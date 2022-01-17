using UnityEngine;

public class Portrait : MonoBehaviour
{
	public delegate void PortraitChanged();

	public Texture2D TextureLarge;

	public Texture2D TextureSmall;

	public static int TextureLargeWidth = 210;

	public static int TextureLargeHeight = 331;

	public static int TextureSmallWidth = 76;

	public static int TextureSmallHeight = 96;

	[Persistent]
	public string m_textureLargePath;

	[Persistent]
	public string m_textureSmallPath;

	private Coroutine m_loadSmallTextureCoroutine;

	private Coroutine m_loadLargeTextureCoroutine;

	private Coroutine m_SetTexturesCoroutine;

	public string TextureSmallPath
	{
		get
		{
			return m_textureSmallPath;
		}
		set
		{
			m_textureSmallPath = value;
			LoadSmallTexture();
		}
	}

	public string TextureLargePath
	{
		get
		{
			return m_textureLargePath;
		}
		set
		{
			m_textureLargePath = value;
			LoadLargeTexture();
		}
	}

	public event PortraitChanged OnPortraitChanged;

	private void Start()
	{
		LoadSmallTexture();
		LoadLargeTexture();
	}

	private void LoadSmallTexture()
	{
		if (string.IsNullOrEmpty(m_textureSmallPath))
		{
			CompanionInstanceID component = GetComponent<CompanionInstanceID>();
			if ((bool)component)
			{
				m_textureSmallPath = GetPortraitPathForCompanion(component.Companion, largeTexture: false);
			}
		}
		if (!string.IsNullOrEmpty(m_textureSmallPath))
		{
			if (m_loadSmallTextureCoroutine != null)
			{
				StopCoroutine(m_loadSmallTextureCoroutine);
			}
			m_loadSmallTextureCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(m_textureSmallPath, LoadedSmallTexture));
		}
	}

	private void LoadLargeTexture()
	{
		if (string.IsNullOrEmpty(m_textureLargePath))
		{
			CompanionInstanceID component = GetComponent<CompanionInstanceID>();
			if ((bool)component)
			{
				m_textureLargePath = GetPortraitPathForCompanion(component.Companion, largeTexture: true);
			}
		}
		if (!string.IsNullOrEmpty(m_textureLargePath))
		{
			if (m_loadLargeTextureCoroutine != null)
			{
				StopCoroutine(m_loadLargeTextureCoroutine);
			}
			m_loadLargeTextureCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(m_textureLargePath, LoadedLargeTexture));
		}
	}

	private void LoadedSmallTexture(Texture2D loadedTexture)
	{
		TextureSmall = loadedTexture;
		NotifyChanged();
	}

	private void LoadedLargeTexture(Texture2D loadedTexture)
	{
		TextureLarge = loadedTexture;
		NotifyChanged();
	}

	public void Restored()
	{
		LoadSmallTexture();
		LoadLargeTexture();
	}

	public void CopyTo(Portrait p)
	{
		p.m_textureLargePath = m_textureLargePath;
		p.m_textureSmallPath = m_textureSmallPath;
		p.TextureLarge = TextureLarge;
		p.TextureSmall = TextureSmall;
	}

	public void SetTextures(string newSmallTexturePath, string newLargeTexturePath)
	{
		if (m_SetTexturesCoroutine != null)
		{
			StopCoroutine(m_SetTexturesCoroutine);
		}
		if (!string.IsNullOrEmpty(newSmallTexturePath))
		{
			m_textureSmallPath = newSmallTexturePath;
			m_SetTexturesCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(m_textureSmallPath, TextureSmallLoaded));
		}
		if (!string.IsNullOrEmpty(newLargeTexturePath))
		{
			m_textureLargePath = newLargeTexturePath;
			m_SetTexturesCoroutine = StartCoroutine(GUIUtils.LoadTexture2DFromPathCallback(m_textureLargePath, TextureLargeLoaded));
		}
	}

	private void TextureSmallLoaded(Texture2D loadedTexture)
	{
		TextureSmall = loadedTexture;
		NotifyChanged();
	}

	private void TextureLargeLoaded(Texture2D loadedTexture)
	{
		TextureLarge = loadedTexture;
		NotifyChanged();
	}

	public void NotifyChanged()
	{
		if (this.OnPortraitChanged != null)
		{
			this.OnPortraitChanged();
		}
	}

	public static Texture2D GetTextureSmall(MonoBehaviour mb)
	{
		if (mb == null)
		{
			return null;
		}
		return GetTextureSmall(mb.gameObject);
	}

	public static Texture2D GetTextureSmall(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		Portrait component = go.GetComponent<Portrait>();
		if ((bool)component)
		{
			return component.TextureSmall;
		}
		return null;
	}

	public static Texture2D GetTextureLarge(MonoBehaviour mb)
	{
		if (mb == null)
		{
			return null;
		}
		return GetTextureLarge(mb.gameObject);
	}

	public static Texture2D GetTextureLarge(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		Portrait component = go.GetComponent<Portrait>();
		if ((bool)component)
		{
			return component.TextureLarge;
		}
		return null;
	}

	public static string GetPortraitPathForCompanion(CompanionNames.Companions companion, bool largeTexture)
	{
		string text = "Data/Art/GUI/Portraits/Companion/portrait_";
		switch (companion)
		{
		case CompanionNames.Companions.Eder:
			text += "eder";
			break;
		case CompanionNames.Companions.Caroc:
			text += "devil_of_caroc";
			break;
		case CompanionNames.Companions.Aloth:
			text += "aloth";
			break;
		case CompanionNames.Companions.Kana:
			text += "kana";
			break;
		case CompanionNames.Companions.Sagani:
			text += "sagani";
			break;
		case CompanionNames.Companions.Pallegina:
			text += "pallegina";
			break;
		case CompanionNames.Companions.Mother:
			text += "grieving_mother";
			break;
		case CompanionNames.Companions.Hiravias:
			text += "hiravias";
			break;
		case CompanionNames.Companions.Calisca:
			text += "calisca";
			break;
		case CompanionNames.Companions.Heodan:
			text += "heodan";
			break;
		case CompanionNames.Companions.Maneha:
			text += "maneha";
			break;
		default:
			return null;
		}
		if (largeTexture)
		{
			return text + "_lg.png";
		}
		return text + "_sm.png";
	}
}

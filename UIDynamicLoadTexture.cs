using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(UITexture))]
public class UIDynamicLoadTexture : MonoBehaviour
{
	private class DynamicTexture
	{
		public Texture2D Texture;

		public int RefCount;

		public DynamicTexture(Texture2D tex)
		{
			Texture = tex;
		}
	}

	public enum UnloadMode
	{
		OnDisable,
		OnLevelUnload
	}

	private static Dictionary<string, DynamicTexture> m_LoadedTextures = new Dictionary<string, DynamicTexture>();

	[ResourcesImageProperty]
	public string Path;

	public UnloadMode UnloadWhen;

	public bool MakePixelPerfect;

	private string TransparentPath = "Art/UI/ItemSketches/spells_empty_image";

	private string m_OldPath;

	private Texture2D m_LoadedTexture;

	private static Texture2D Load(string path, string tranparentPath)
	{
		string text = path;
		if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(tranparentPath))
		{
			return null;
		}
		if (string.IsNullOrEmpty(path))
		{
			text = tranparentPath;
		}
		if (!m_LoadedTextures.ContainsKey(text))
		{
			Texture2D texture2D = Resources.Load(text) as Texture2D;
			if (!texture2D)
			{
				return null;
			}
			m_LoadedTextures[text] = new DynamicTexture(texture2D);
		}
		DynamicTexture dynamicTexture = m_LoadedTextures[text];
		dynamicTexture.RefCount++;
		return dynamicTexture.Texture;
	}

	private static void Unload(string path)
	{
		if (m_LoadedTextures.ContainsKey(path))
		{
			DynamicTexture dynamicTexture = m_LoadedTextures[path];
			dynamicTexture.RefCount--;
			if (dynamicTexture.RefCount <= 0)
			{
				Resources.UnloadAsset(dynamicTexture.Texture);
				m_LoadedTextures.Remove(path);
			}
		}
	}

	private void OnEnable()
	{
		Load();
	}

	private void OnDisable()
	{
		if (UnloadWhen == UnloadMode.OnDisable)
		{
			Unload();
		}
	}

	private void Start()
	{
		GameState.OnLevelUnload += OnLevelUnload;
	}

	public void SetPath(string path)
	{
		if (Path != path)
		{
			Path = path;
			Unload();
			Load();
		}
	}

	private void Update()
	{
		if (Path != m_OldPath)
		{
			Unload();
			Load();
		}
	}

	private void OnDestroy()
	{
		Unload();
		GameState.OnLevelUnload -= OnLevelUnload;
		m_LoadedTexture = null;
	}

	private void OnLevelUnload(object sender, EventArgs e)
	{
		if (Application.isPlaying && UnloadWhen == UnloadMode.OnLevelUnload)
		{
			Unload();
		}
	}

	private void Load()
	{
		if (!m_LoadedTexture)
		{
			UITexture component = GetComponent<UITexture>();
			m_LoadedTexture = Load(Path, TransparentPath);
			component.mainTexture = m_LoadedTexture;
			if (MakePixelPerfect)
			{
				component.MakePixelPerfect();
			}
			m_OldPath = Path;
		}
	}

	private void Unload()
	{
		if ((bool)m_LoadedTexture)
		{
			GetComponent<UITexture>().mainTexture = null;
			m_LoadedTexture = null;
		}
	}
}

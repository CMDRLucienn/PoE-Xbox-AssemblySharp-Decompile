using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	private static Localization mInstance;

	public string startingLanguage = "English";

	public TextAsset[] languages;

	private Dictionary<string, string> mDictionary = new Dictionary<string, string>();

	private string mLanguage;

	public static bool isActive => mInstance != null;

	public static Localization instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Object.FindObjectOfType(typeof(Localization)) as Localization;
				if (mInstance == null)
				{
					GameObject obj = new GameObject("_Localization");
					Object.DontDestroyOnLoad(obj);
					mInstance = obj.AddComponent<Localization>();
				}
			}
			return mInstance;
		}
	}

	public string currentLanguage
	{
		get
		{
			return mLanguage;
		}
		set
		{
			if (!(mLanguage != value))
			{
				return;
			}
			startingLanguage = value;
			if (!string.IsNullOrEmpty(value))
			{
				if (languages != null)
				{
					int i = 0;
					for (int num = languages.Length; i < num; i++)
					{
						TextAsset textAsset = languages[i];
						if (textAsset != null && textAsset.name == value)
						{
							Load(textAsset);
							return;
						}
					}
				}
				TextAsset textAsset2 = Resources.Load(value, typeof(TextAsset)) as TextAsset;
				if (textAsset2 != null)
				{
					Load(textAsset2);
					return;
				}
			}
			mDictionary.Clear();
			PlayerPrefs.DeleteKey("Language");
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			currentLanguage = PlayerPrefs.GetString("Language", startingLanguage);
			if (string.IsNullOrEmpty(mLanguage) && languages != null && languages.Length != 0)
			{
				currentLanguage = languages[0].name;
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Load(TextAsset asset)
	{
		mLanguage = asset.name;
		PlayerPrefs.SetString("Language", mLanguage);
		ByteReader byteReader = new ByteReader(asset);
		mDictionary = byteReader.ReadDictionary();
		UIRoot.Broadcast("OnLocalize", this);
	}

	public string Get(string key)
	{
		if (!mDictionary.TryGetValue(key, out var value))
		{
			return key;
		}
		return value;
	}

	public static string Localize(string key)
	{
		if (!(instance != null))
		{
			return key;
		}
		return instance.Get(key);
	}
}

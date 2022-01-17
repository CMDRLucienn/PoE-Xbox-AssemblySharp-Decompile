using System.IO;
using UnityEngine;

public class StrongholdTileFlipper : MonoBehaviour
{
	public StrongholdUpgrade.InteriorLocation StrongholdLocation;

	private PE_StreamTileManager m_tileManager;

	private LevelInfo m_levelInfo;

	private string m_levelPathOverride;

	private string m_levelNameOverride;

	private AssetBundle m_flipAssetBundle;

	private bool m_flipTiles;

	private void Start()
	{
		m_tileManager = PE_StreamTileManager.Instance;
		m_levelInfo = LevelInfo.Instance;
		m_levelPathOverride = m_levelInfo.m_LevelDataPath.Insert(m_levelInfo.m_LevelDataPath.Length - 1, "Pristine");
		m_levelNameOverride = m_levelInfo.GetLevelName() + "_Pristine";
		m_flipTiles = true;
	}

	private void OnDestroy()
	{
		if (m_flipAssetBundle != null)
		{
			m_flipAssetBundle.Unload(unloadAllLoadedObjects: true);
			GameUtilities.Destroy(m_flipAssetBundle);
			m_flipAssetBundle = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (m_flipTiles)
		{
			m_flipTiles = !FlipTilesInternal();
		}
	}

	public void FlipTiles()
	{
		m_flipTiles = !FlipTilesInternal();
	}

	private bool FlipTilesInternal()
	{
		Stronghold stronghold = Object.FindObjectOfType(typeof(Stronghold)) as Stronghold;
		if (m_flipAssetBundle == null)
		{
			string text = m_tileManager.gameObject.name;
			string text2 = "assetbundles/ST_" + text + "_Pristine";
			text2 = text2.ToLower();
			m_flipAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + Path.DirectorySeparatorChar + text2);
			if (m_flipAssetBundle == null)
			{
				Debug.LogError("Error trying to load asset bundle " + text2);
				return true;
			}
		}
		for (int i = 0; i < stronghold.Upgrades.Length; i++)
		{
			StrongholdUpgrade strongholdUpgrade = stronghold.Upgrades[i];
			if (!stronghold.HasUpgrade(strongholdUpgrade.UpgradeType))
			{
				continue;
			}
			if (StrongholdLocation == StrongholdUpgrade.InteriorLocation.None)
			{
				for (int j = 0; j < strongholdUpgrade.Tiles.Length; j++)
				{
					FlipTile((int)strongholdUpgrade.Tiles[j].x, (int)strongholdUpgrade.Tiles[j].y);
				}
			}
			else
			{
				if (strongholdUpgrade.InteriorUpgradeLocation != StrongholdLocation)
				{
					continue;
				}
				if (strongholdUpgrade.FullMapInteriorUpgrade)
				{
					if (!FlipAllTiles())
					{
						return false;
					}
					continue;
				}
				for (int k = 0; k < strongholdUpgrade.InteriorTiles.Length; k++)
				{
					if (!FlipTile((int)strongholdUpgrade.InteriorTiles[k].x, (int)strongholdUpgrade.InteriorTiles[k].y))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool FlipTile(int x, int y)
	{
		PE_StreamTile tile = m_tileManager.GetTile(x, y);
		if ((bool)tile)
		{
			tile.LoadTextures(m_flipAssetBundle, m_levelPathOverride, m_levelNameOverride, m_levelInfo);
			return true;
		}
		return false;
	}

	private bool FlipAllTiles()
	{
		if (m_tileManager == null)
		{
			return false;
		}
		PE_StreamTile[] array = m_tileManager.TilePool();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == null))
				{
					array[i].LoadTextures(m_flipAssetBundle, m_levelPathOverride, m_levelNameOverride, m_levelInfo);
				}
			}
			return true;
		}
		return false;
	}
}

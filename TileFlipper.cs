using UnityEngine;

public class TileFlipper : MonoBehaviour
{
	public Vector2[] Tiles;

	private PE_StreamTileManager m_tileManager;

	private LevelInfo m_levelInfo;

	[Persistent]
	public int Frame;

	private int m_frame;

	private bool m_flipTiles;

	private void Start()
	{
		m_tileManager = PE_StreamTileManager.Instance;
		m_levelInfo = LevelInfo.Instance;
	}

	private void Update()
	{
		if (m_tileManager.FinishedLoading())
		{
			if (Frame != m_frame)
			{
				m_flipTiles = true;
			}
			if (m_flipTiles)
			{
				m_frame = Frame;
				FlipTiles(Frame);
				m_flipTiles = false;
			}
		}
	}

	public void Flip(int frame)
	{
		Frame = frame;
	}

	private void FlipTiles(int frame)
	{
		for (int i = 0; i < Tiles.Length; i++)
		{
			FlipTile((int)Tiles[i].x, (int)Tiles[i].y, frame);
		}
	}

	private void FlipTile(int x, int y, int frame)
	{
		PE_StreamTile tile = m_tileManager.GetTile(x, y);
		if ((bool)tile)
		{
			if (frame > 0)
			{
				string levelName = m_levelInfo.GetLevelName() + "_f" + $"{frame:000}";
				tile.LoadTextures(m_tileManager.GetAssetBundle(), m_levelInfo.m_LevelDataPath, levelName, m_levelInfo);
			}
			else
			{
				tile.LoadTextures(m_tileManager.GetAssetBundle(), m_levelInfo.m_LevelDataPath, m_levelInfo.GetLevelName(), m_levelInfo);
			}
		}
	}
}

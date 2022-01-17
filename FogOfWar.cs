using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FogOfWar : MonoBehaviour
{
	[Flags]
	public enum FogOfWarFlags : byte
	{
		FOG_FLAG_NOTHING = 0,
		FOG_FLAG_FLOOR = 1,
		FOG_FLAG_WALL = 2,
		FOG_FLAG_VISIBLE = 4,
		FOG_FLAG_EXPLORED = 8,
		FOG_FLAG_DOOR_CLOSED = 0x10
	}

	private enum UpdateState
	{
		REVEAL,
		COPY_TO_TEXTURE,
		BLEND
	}

	public class Revealer
	{
		public Vector3 WorldPos;

		public Vector3 CameraPos;

		public float LOSDistance;

		public bool TriggersBoxColliders;

		public bool HasTrigger;

		public bool RevealOnly;

		public bool RespectLOS;

		public bool RequiresRefresh;

		public Vector3 TriggerPos = Vector3.zero;

		public Vector3 TriggerBoxSize = Vector3.zero;

		public bool TriggerFullyRevealed;
	}

	public class DoorBlocker
	{
		public Door door;

		public int x;

		public int y;
	}

	private class RevealerCandidate
	{
		public Revealer Revealer;

		public int OriginX;

		public int OriginY;

		public int OriginIndex;

		public Vector3 WorldPos;

		public bool UseBacksideVerts;
	}

	public class LOSVertex
	{
		public class Intersection
		{
			public Vector3 Position = Vector3.zero;

			public Vector3 Normal = Vector3.zero;
		}

		public class Visibility
		{
			public bool IsVisible;

			public List<Door> Doors = new List<Door>();
		}

		public Vector3 NavPosition = Vector3.zero;

		public List<Intersection> Intersections;

		public Visibility[,] Visible;

		public LOSVertex()
		{
			Intersections = new List<Intersection>();
		}
	}

	public const int VERSION_NUMBER = 3;

	public const float FOG_SCALE_FACTOR = 1f;

	private const float LOS_ATTENUATION_DISTANCE = 3f;

	private const float REVEAL_ATTENUATION_DISTANCE = 3f;

	public const float FULL_FOG_ALPHA = 0.7f;

	private const float RANGED_ATTACKER_FOG_ALPHA = 0.4f;

	private const float LOS_BUFFER = 0.01f;

	public const float RANGED_ATTACK_LOS_DISTANCE = 11f;

	public const float MAX_LOS_DISTANCE = 19f;

	public bool CompletelyExplored;

	public bool Disabled;

	public Color FogColor = Color.black;

	[HideInInspector]
	public bool DebugActive;

	private int m_fogMapWidth;

	private int m_fogMapHeight;

	private bool m_fogIsInitialized;

	private float m_systemTime;

	private float m_refreshTime;

	private float m_deltaTime = 0.01f;

	private Vector3 m_cameraForward = Vector3.zero;

	private Vector2 m_cameraForward2D = Vector3.zero;

	private Texture2D m_fogTextureA;

	private LevelInfo m_levelInfo;

	private Color[] m_colorBuffer;

	private Vector3 m_fogWorldBoundsX;

	private Vector3 m_fogWorldBoundsY;

	private Vector3 m_fogWorldBoundsXNormalized;

	private Vector3 m_fogWorldBoundsYNormalized;

	private Vector3 m_fogWorldBoundsOrigin;

	private Vector3 m_worldOrigin;

	private Vector3 m_worldWidth;

	private Vector3 m_worldHeight;

	private Mesh m_fogMesh;

	private int m_fogVertsPerColumn;

	private int m_fogVertsPerRow;

	private Vector3[] m_worldVertices;

	private Vector3[] m_fogVertices;

	private Vector2[] m_fogAlphas;

	private Vector2[] m_fogAlphasStaging;

	private float[] m_fogDesiredAlphas;

	private bool[] m_fogUpdateState;

	private bool[] m_fogLOSRevealed;

	private float m_fogTileWidth = 1f;

	private float m_fogTileHeight = 1f;

	private Plane m_cameraPlane;

	private List<int> m_prevUpdateList = new List<int>();

	private List<int> m_fadeOutList = new List<int>();

	private bool m_queueDisable;

	private bool m_queueRevealAll;

	private FogOfWarLOS m_los;

	private Door[] m_doorLookup;

	private bool[] m_doorLookupValidation;

	private Vector2[] m_doorLookupForward;

	private Guid[] m_doorLookupGuid;

	private Vector3[] m_doorLookupPos;

	private List<Revealer> m_revealList = new List<Revealer>();

	private List<Revealer> m_revealAddList = new List<Revealer>();

	private List<Revealer> m_revealRemoveList = new List<Revealer>();

	private List<int> m_updateList = new List<int>(1024);

	private List<int> m_underRevealerList = new List<int>(1024);

	private List<RevealerCandidate> m_candidates = new List<RevealerCandidate>(16);

	private Plane m_groundPlane = new Plane(Vector3.up, 0f);

	private Thread m_updateThread;

	private bool m_fogUpdateHandled;

	private bool m_disableFogFading;

	public int FogMapWidth => m_fogMapWidth;

	public int FogMapHeight => m_fogMapHeight;

	public Vector3 FogWorldBoundsX => m_fogWorldBoundsX;

	public Vector3 FogWorldBoundsY => m_fogWorldBoundsY;

	public Vector3 FogWorldBoundsOrigin => m_fogWorldBoundsOrigin;

	public Vector3 WorldOrigin => m_worldOrigin;

	public Vector3 WorldWidth => m_worldWidth;

	public Vector3 WorldHeight => m_worldHeight;

	public Texture2D FogTextureA => m_fogTextureA;

	public Mesh FogMesh => m_fogMesh;

	public int FogVertsPerColumn => m_fogVertsPerColumn;

	public int FogVertsPerRow => m_fogVertsPerRow;

	public Vector3[] WorldVertices => m_worldVertices;

	public Vector3[] FogVertices => m_fogVertices;

	public Vector2[] FogAlphas => m_fogAlphas;

	public Plane CameraPlane => m_cameraPlane;

	public static FogOfWar Instance { get; private set; }

	public Revealer AddRevealer(bool triggersBoxColliders, float losDistance, Vector3 worldPosition, FogOfWarTrigger trigger, bool revealOnly, bool respectLOS)
	{
		Revealer revealer = new Revealer();
		revealer.TriggersBoxColliders = triggersBoxColliders;
		revealer.LOSDistance = losDistance;
		revealer.RevealOnly = revealOnly;
		revealer.RespectLOS = respectLOS;
		revealer.TriggerBoxSize = Vector3.zero;
		revealer.WorldPos = worldPosition;
		if (trigger != null)
		{
			revealer.HasTrigger = true;
			revealer.TriggerPos = trigger.gameObject.transform.position;
			revealer.TriggerFullyRevealed = trigger.FullyRevealed;
			BoxCollider component = trigger.gameObject.GetComponent<BoxCollider>();
			if (component != null)
			{
				revealer.TriggerPos += component.center;
				revealer.TriggerBoxSize = component.size;
			}
		}
		else
		{
			revealer.HasTrigger = false;
			revealer.TriggerPos = Vector3.zero;
		}
		lock (m_revealAddList)
		{
			m_revealAddList.Add(revealer);
			return revealer;
		}
	}

	public void RemoveRevealer(Revealer revealer)
	{
		lock (m_revealRemoveList)
		{
			m_revealRemoveList.Add(revealer);
		}
	}

	public void ClearInitialized()
	{
		m_fogIsInitialized = false;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'FogOfWar' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	public static void Save()
	{
		FogOfWar instance = Instance;
		if (!(instance == null))
		{
			string text = Path.Combine(Application.persistentDataPath, "CurrentGame");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string filename = Path.Combine(text, SceneManager.GetActiveScene().name + ".fog");
			instance.Save(filename);
		}
	}

	public static void Load()
	{
		FogOfWar instance = Instance;
		if (!(instance == null))
		{
			string text = Path.Combine(Application.persistentDataPath, "CurrentGame");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string filename = Path.Combine(text, SceneManager.GetActiveScene().name + ".fog");
			instance.Load(filename);
		}
	}

	private void Save(string filename)
	{
		BinaryWriter binaryWriter = null;
		try
		{
			binaryWriter = new BinaryWriter(File.Open(filename, FileMode.Create));
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, this);
			Debug.LogError("FAILED TO SAVE FOG OF WAR Error = " + ex.Message);
		}
		if (binaryWriter != null)
		{
			binaryWriter.Write(3);
			int value = m_fogAlphas.Length;
			binaryWriter.Write(value);
			float[] array = new float[m_fogAlphas.Length];
			int num = 0;
			Vector2[] fogAlphas = m_fogAlphas;
			for (int i = 0; i < fogAlphas.Length; i++)
			{
				Vector2 vector = fogAlphas[i];
				array[num] = vector.y;
				num++;
			}
			int num2 = m_fogAlphas.Length * 4;
			binaryWriter.Write(num2);
			byte[] array2 = new byte[num2];
			Buffer.BlockCopy(array, 0, array2, 0, num2);
			binaryWriter.Write(array2);
			binaryWriter.Close();
		}
	}

	private void Load(string filename)
	{
		BinaryReader binaryReader = null;
		if (!File.Exists(filename))
		{
			return;
		}
		try
		{
			binaryReader = new BinaryReader(File.Open(filename, FileMode.Open));
		}
		catch (Exception ex)
		{
			Debug.LogException(ex, this);
			Debug.LogError("FAILED TO LOAD FOG OF WAR Error = " + ex.Message);
		}
		if (binaryReader == null)
		{
			return;
		}
		switch (binaryReader.ReadInt32())
		{
		case 3:
		{
			int num3 = binaryReader.ReadInt32();
			int count2 = binaryReader.ReadInt32();
			if (num3 < m_fogVertsPerColumn * m_fogVertsPerRow)
			{
				m_fogAlphas = new Vector2[num3];
				m_fogAlphasStaging = new Vector2[num3];
				m_fogDesiredAlphas = new float[num3];
			}
			byte[] src2 = binaryReader.ReadBytes(count2);
			float[] array2 = new float[num3 * 4];
			Buffer.BlockCopy(src2, 0, array2, 0, count2);
			for (int j = 0; j < num3; j++)
			{
				float num4 = array2[j];
				m_fogAlphas[j].x = (Disabled ? 0f : num4);
				m_fogAlphas[j].y = (Disabled ? 0f : num4);
				m_fogAlphasStaging[j].x = (Disabled ? 0f : num4);
				m_fogAlphasStaging[j].y = (Disabled ? 0f : num4);
				m_fogDesiredAlphas[j] = (Disabled ? 0f : num4);
			}
			array2 = null;
			break;
		}
		case 2:
		{
			int num = binaryReader.ReadInt32();
			int count = binaryReader.ReadInt32();
			if (num < m_fogVertsPerColumn * m_fogVertsPerRow)
			{
				m_fogAlphas = new Vector2[num];
				m_fogAlphasStaging = new Vector2[num];
				m_fogDesiredAlphas = new float[num];
			}
			byte[] src = binaryReader.ReadBytes(count);
			float[] array = new float[num * 4 * 2];
			Buffer.BlockCopy(src, 0, array, 0, count);
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				m_fogAlphas[i].x = (Disabled ? 0f : array[num2]);
				m_fogAlphas[i].y = (Disabled ? 0f : array[num2 + 1]);
				m_fogAlphasStaging[i].x = (Disabled ? 0f : array[num2]);
				m_fogAlphasStaging[i].y = (Disabled ? 0f : array[num2 + 1]);
				m_fogDesiredAlphas[i] = (Disabled ? 0f : array[num2]);
				num2 += 2;
			}
			array = null;
			break;
		}
		}
		binaryReader.Close();
	}

	private void Start()
	{
		InitFogOfWar();
		if (Application.isEditor && !Application.isPlaying)
		{
			return;
		}
		m_fogTextureA = new Texture2D(m_fogMapWidth, m_fogMapHeight, TextureFormat.Alpha8, mipChain: false);
		m_fogTextureA.wrapMode = TextureWrapMode.Clamp;
		m_colorBuffer = new Color[m_fogMapWidth * m_fogMapHeight];
		for (int i = 0; i < m_fogMapWidth * m_fogMapHeight; i++)
		{
			m_colorBuffer[i].a = 1f;
		}
		m_fogTextureA.SetPixels(m_colorBuffer);
		m_fogTextureA.Apply();
		string text = Application.dataPath + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "fogofwar" + Path.DirectorySeparatorChar + GameState.ApplicationLoadedLevelName.ToLower() + ".los";
		if (File.Exists(text))
		{
			try
			{
				m_los = FogOfWarLOS.Load(text);
				Door[] array = UnityEngine.Object.FindObjectsOfType(typeof(Door)) as Door[];
				m_doorLookup = new Door[m_los.DoorIDs.Length];
				m_doorLookupValidation = new bool[m_los.DoorIDs.Length];
				m_doorLookupForward = new Vector2[m_los.DoorIDs.Length];
				m_doorLookupGuid = new Guid[m_los.DoorIDs.Length];
				m_doorLookupPos = new Vector3[m_los.DoorIDs.Length];
				if (array.Length != 0)
				{
					Guid[] array2 = new Guid[array.Length];
					for (int j = 0; j < array.Length; j++)
					{
						Guid guid = Guid.Empty;
						InstanceID component = array[j].GetComponent<InstanceID>();
						if (component != null)
						{
							guid = component.Guid;
						}
						array2[j] = guid;
					}
					for (int k = 0; k < m_los.DoorIDs.Length; k++)
					{
						Guid guid2 = m_los.DoorIDs[k];
						m_doorLookupValidation[k] = false;
						for (int l = 0; l < array.Length; l++)
						{
							if (array2[l] == guid2)
							{
								m_doorLookup[k] = array[l];
								m_doorLookupValidation[k] = true;
								m_doorLookupGuid[k] = array2[l];
								m_doorLookupPos[k] = array[l].transform.position;
								m_doorLookupForward[k] = GameUtilities.V3ToV2(array[l].transform.forward);
								m_doorLookupForward[k].Normalize();
								if (Vector2.Dot(-m_cameraForward2D, m_doorLookupForward[k]) < 0f)
								{
									m_doorLookupForward[k] = -m_doorLookupForward[k];
								}
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex, this);
				Debug.LogError("Failed to load fog of war line of sight data. " + ex.Message);
			}
		}
		m_updateThread = new Thread(FogUpdateThread);
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI == null))
			{
				partyMemberAI.CreateFogRevealer();
			}
		}
		m_updateThread.Start();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if (Application.isEditor && !Application.isPlaying)
		{
			return;
		}
		if (m_updateThread != null)
		{
			m_updateThread.Abort();
			while (m_updateThread.IsAlive)
			{
				Thread.Sleep(10);
			}
			m_updateThread = null;
		}
		GameUtilities.Destroy(m_fogTextureA);
		m_fogTextureA = null;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		m_systemTime = Time.unscaledTime;
		m_deltaTime = Time.unscaledDeltaTime;
	}

	public void InitFogOfWar()
	{
		InitFogOfWar(LevelInfo.Instance);
	}

	public void InitFogOfWar(LevelInfo levelInfo)
	{
		if (m_fogIsInitialized)
		{
			return;
		}
		m_fogIsInitialized = true;
		m_levelInfo = levelInfo;
		if (m_levelInfo == null)
		{
			return;
		}
		Camera main = Camera.main;
		m_cameraForward = main.transform.forward;
		m_cameraForward2D = GameUtilities.V3ToV2(m_cameraForward);
		m_cameraForward2D.Normalize();
		m_cameraPlane = default(Plane);
		m_cameraPlane.SetNormalAndPosition(m_levelInfo.m_CameraRotation * Vector3.forward, m_levelInfo.m_CameraPos);
		m_worldOrigin = m_levelInfo.m_backgroundQuadOrigin;
		m_worldWidth = m_levelInfo.m_backgroundQuadAxisX * m_levelInfo.m_backgroundQuadWidth;
		m_worldHeight = m_levelInfo.m_backgroundQuadAxisY * m_levelInfo.m_backgroundQuadHeight;
		Vector3 origin = m_worldOrigin + m_worldWidth;
		Vector3 origin2 = m_worldOrigin + m_worldHeight;
		Ray theRay = new Ray(m_worldOrigin, -main.transform.forward);
		Ray theRay2 = new Ray(origin, -main.transform.forward);
		Ray theRay3 = new Ray(origin2, -main.transform.forward);
		m_fogWorldBoundsOrigin = GetCameraPlaneRayIntersectionPosition(m_cameraPlane, theRay);
		Vector3 cameraPlaneRayIntersectionPosition = GetCameraPlaneRayIntersectionPosition(m_cameraPlane, theRay2);
		Vector3 cameraPlaneRayIntersectionPosition2 = GetCameraPlaneRayIntersectionPosition(m_cameraPlane, theRay3);
		m_fogWorldBoundsX = cameraPlaneRayIntersectionPosition - m_fogWorldBoundsOrigin;
		m_fogWorldBoundsY = cameraPlaneRayIntersectionPosition2 - m_fogWorldBoundsOrigin;
		m_fogWorldBoundsXNormalized = m_fogWorldBoundsX;
		m_fogWorldBoundsYNormalized = m_fogWorldBoundsY;
		m_fogWorldBoundsXNormalized.Normalize();
		m_fogWorldBoundsYNormalized.Normalize();
		m_fogMapWidth = (int)(m_fogWorldBoundsX.magnitude * 1f);
		m_fogMapHeight = (int)(m_fogWorldBoundsY.magnitude * 1f);
		m_fogMesh = new Mesh();
		float backgroundQuadWidth = m_levelInfo.m_backgroundQuadWidth;
		float backgroundQuadHeight = m_levelInfo.m_backgroundQuadHeight;
		m_fogVertsPerColumn = (int)((backgroundQuadWidth + 2f) * 1f);
		m_fogVertsPerRow = (int)((backgroundQuadHeight + 2f) * 1f);
		m_worldVertices = null;
		m_fogVertices = null;
		m_fogAlphas = null;
		m_fogAlphasStaging = null;
		m_fogDesiredAlphas = null;
		m_fogUpdateState = null;
		m_fogLOSRevealed = null;
		m_worldVertices = new Vector3[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogVertices = new Vector3[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogAlphas = new Vector2[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogAlphasStaging = new Vector2[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogDesiredAlphas = new float[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogUpdateState = new bool[m_fogVertsPerColumn * m_fogVertsPerRow];
		m_fogLOSRevealed = new bool[m_fogVertsPerColumn * m_fogVertsPerRow];
		Vector3 worldOrigin = m_worldOrigin;
		worldOrigin -= m_levelInfo.m_backgroundQuadAxisX * 0.1f + m_levelInfo.m_backgroundQuadAxisY * 0.1f;
		for (int i = 0; i < m_fogVertsPerColumn; i++)
		{
			for (int j = 0; j < m_fogVertsPerRow; j++)
			{
				int num = i * m_fogVertsPerRow + j;
				m_worldVertices[num] = worldOrigin + m_levelInfo.m_backgroundQuadAxisX * ((float)i * 1f) + m_levelInfo.m_backgroundQuadAxisY * ((float)j * 1f);
				if (i == m_fogVertsPerColumn - 1)
				{
					m_worldVertices[num] += m_levelInfo.m_backgroundQuadAxisX * 1f;
				}
				Ray theRay4 = new Ray(m_worldVertices[num], -main.transform.forward);
				m_fogVertices[num] = GetCameraPlaneRayIntersectionPosition(m_cameraPlane, theRay4) + main.transform.forward * 0.5f;
				m_fogAlphas[num] = new Vector2(1f, 1f);
				if (CompletelyExplored)
				{
					m_fogAlphas[num].x = 0.7f;
					m_fogAlphas[num].y = 0.7f;
				}
				if (Disabled)
				{
					m_fogAlphas[num].x = 0f;
					m_fogAlphas[num].y = 0f;
				}
				m_fogAlphasStaging[num] = m_fogAlphas[num];
				m_fogDesiredAlphas[num] = m_fogAlphas[num].x;
			}
		}
		m_fogTileWidth = (cameraPlaneRayIntersectionPosition - m_fogVertices[0]).magnitude / (float)m_fogVertsPerColumn;
		m_fogTileHeight = (cameraPlaneRayIntersectionPosition2 - m_fogVertices[0]).magnitude / (float)m_fogVertsPerRow;
		m_fogMesh.vertices = m_fogVertices;
		int num2 = (int)((backgroundQuadWidth + 1f) * 1f);
		int num3 = (int)((backgroundQuadHeight + 1f) * 1f);
		int[] array = new int[num2 * num3 * 6];
		int num4 = 0;
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num3; l++)
			{
				int num5 = (array[num4] = k * m_fogVertsPerRow + l);
				array[num4 + 1] = num5 + 1;
				array[num4 + 2] = num5 + m_fogVertsPerRow;
				array[num4 + 3] = num5 + 1;
				array[num4 + 4] = num5 + m_fogVertsPerRow + 1;
				array[num4 + 5] = num5 + m_fogVertsPerRow;
				num4 += 6;
			}
		}
		m_fogMesh.triangles = array;
		if (CompletelyExplored)
		{
			ExploreAll();
		}
	}

	private void FogUpdateThread()
	{
		while (true)
		{
			FogUpdate();
			Thread.Sleep(1);
		}
	}

	private void SignalFogUpdateEnd()
	{
		m_fogUpdateHandled = true;
	}

	public void WaitForFogUpdate()
	{
		m_disableFogFading = true;
		m_fogUpdateHandled = false;
		while (!m_fogUpdateHandled)
		{
		}
		m_disableFogFading = false;
	}

	private void FogUpdate()
	{
		if (m_queueDisable)
		{
			Disable();
			m_queueDisable = false;
		}
		if (m_queueRevealAll)
		{
			RevealAll();
			m_queueRevealAll = false;
		}
		if (m_systemTime != m_refreshTime && !GameState.IsLoading)
		{
			m_refreshTime = m_systemTime;
			if (!Disabled)
			{
				UpdateRevealers();
				UpdateVertices();
			}
			lock (m_fogAlphasStaging)
			{
				Array.Copy(m_fogAlphasStaging, m_fogAlphas, m_fogAlphas.Length);
			}
			SignalFogUpdateEnd();
		}
	}

	public void RefreshFogTexture()
	{
		UpdateColorBuffer();
		m_fogTextureA.SetPixels(m_colorBuffer);
		m_fogTextureA.Apply();
	}

	private void UpdateRevealers()
	{
		if (m_revealAddList.Count > 0)
		{
			lock (m_revealAddList)
			{
				for (int num = m_revealAddList.Count - 1; num >= 0; num--)
				{
					m_revealList.Add(m_revealAddList[num]);
					m_revealAddList.RemoveAt(num);
				}
			}
		}
		if (m_revealRemoveList.Count <= 0)
		{
			return;
		}
		lock (m_revealRemoveList)
		{
			for (int num2 = m_revealRemoveList.Count - 1; num2 >= 0; num2--)
			{
				m_revealList.Remove(m_revealRemoveList[num2]);
				m_revealRemoveList.RemoveAt(num2);
			}
		}
	}

	private void ExploreAll()
	{
		lock (m_fogAlphasStaging)
		{
			for (int i = 0; i < m_fogAlphas.Length; i++)
			{
				if (m_fogAlphasStaging[i].y > 0.7f)
				{
					m_fogAlphasStaging[i].y = 0.7f;
				}
			}
		}
	}

	public void QueueDisable()
	{
		m_queueDisable = true;
	}

	public void QueueRevealAll()
	{
		m_queueRevealAll = true;
	}

	public void Disable()
	{
		lock (m_fogAlphasStaging)
		{
			lock (m_fogDesiredAlphas)
			{
				for (int i = 0; i < m_fogAlphasStaging.Length; i++)
				{
					m_fogAlphasStaging[i].x = 0f;
					m_fogAlphasStaging[i].y = 0f;
					m_fogDesiredAlphas[i] = 0f;
				}
			}
		}
		Disabled = true;
	}

	public void RevealAll()
	{
		lock (m_fogAlphasStaging)
		{
			lock (m_fogDesiredAlphas)
			{
				for (int i = 0; i < m_fogAlphasStaging.Length; i++)
				{
					if (m_fogAlphasStaging[i].y > 0.7f)
					{
						m_fogAlphasStaging[i].y = 0.7f;
					}
					if (m_fogAlphasStaging[i].x > 0.7f)
					{
						m_fogAlphasStaging[i].x = 0.7f;
					}
					if (m_fogDesiredAlphas[i] > 0.7f)
					{
						m_fogDesiredAlphas[i] = 0.7f;
					}
				}
			}
		}
		CompletelyExplored = true;
	}

	public void ClearFogOfWar()
	{
		lock (m_fogDesiredAlphas)
		{
			for (int i = 0; i < m_fogAlphas.Length; i++)
			{
				m_fogAlphas[i].x = 0.7f;
				m_fogDesiredAlphas[i] = 0.7f;
			}
		}
	}

	public void UpdateVertices()
	{
		if (m_revealList.Count <= 0 || m_fogVertices == null || m_fogAlphasStaging == null || m_fogDesiredAlphas == null)
		{
			return;
		}
		for (int i = 0; i < m_prevUpdateList.Count; i++)
		{
			m_fogUpdateState[m_prevUpdateList[i]] = false;
		}
		float num = m_deltaTime / 0.5f;
		if (m_disableFogFading || num > 1f)
		{
			num = 1f;
		}
		m_updateList.Clear();
		m_underRevealerList.Clear();
		m_candidates.Clear();
		int num2 = 19;
		int num3 = num2 * 2 + 1;
		int num4 = num3 * num3;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		float num8 = 0f;
		float num9 = 1f;
		float num10 = 0f;
		float num11 = 1f;
		float num12 = 0f;
		float num13 = 0f;
		float num14 = 0f;
		float num15 = 0f;
		float num16 = 0f;
		m_groundPlane.normal = Vector3.up;
		m_groundPlane.distance = 0f;
		for (int j = 0; j < m_revealList.Count; j++)
		{
			Revealer revealer = m_revealList[j];
			if (revealer.RequiresRefresh)
			{
				continue;
			}
			if (revealer.HasTrigger)
			{
				if (revealer.TriggerBoxSize.sqrMagnitude > float.Epsilon)
				{
					float num17 = revealer.TriggerPos.x - revealer.TriggerBoxSize.x * 0.5f;
					float num18 = revealer.TriggerPos.x + revealer.TriggerBoxSize.x * 0.5f;
					float num19 = revealer.TriggerPos.z - revealer.TriggerBoxSize.z * 0.5f;
					float num20 = revealer.TriggerPos.z + revealer.TriggerBoxSize.z * 0.5f;
					bool flag = false;
					for (int k = 0; k < m_revealList.Count; k++)
					{
						Revealer revealer2 = m_revealList[k];
						if (revealer2.TriggersBoxColliders && revealer2.WorldPos.x > num17 && revealer2.WorldPos.x < num18 && revealer2.WorldPos.z > num19 && revealer2.WorldPos.z < num20)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				else
				{
					if (revealer.TriggerFullyRevealed)
					{
						if (!PointFullyVisible(revealer.TriggerPos))
						{
							continue;
						}
					}
					else if (!PointVisible(revealer.TriggerPos))
					{
						continue;
					}
					int x = 0;
					int y = 0;
					WorldToFogOfWar(revealer.TriggerPos, out x, out y);
					if (!m_fogLOSRevealed[x * m_fogVertsPerRow + y])
					{
						continue;
					}
				}
			}
			int x2 = 0;
			int y2 = 0;
			WorldToFogOfWar(revealer.WorldPos, out x2, out y2);
			int num21 = (int)((revealer.LOSDistance + 3f) * 1f / m_fogTileWidth) + 1;
			int num22 = (int)((revealer.LOSDistance + 3f) * 1f / m_fogTileHeight) + 1;
			int num23 = x2 - num21;
			int num24 = y2 - num22;
			int num25 = x2 + num21;
			int num26 = y2 + num22;
			RevealerCandidate revealerCandidate = new RevealerCandidate();
			revealerCandidate.Revealer = revealer;
			revealerCandidate.UseBacksideVerts = false;
			revealerCandidate.OriginX = x2;
			revealerCandidate.OriginY = y2;
			revealerCandidate.OriginIndex = x2 * m_fogVertsPerRow + y2;
			if (m_los != null && (revealerCandidate.OriginIndex < 0 || revealerCandidate.OriginIndex >= m_los.Vertices.Length))
			{
				continue;
			}
			m_candidates.Add(revealerCandidate);
			Ray theRay = new Ray(revealer.WorldPos, -CameraPlane.normal);
			revealer.CameraPos = GetCameraPlaneRayIntersectionPosition(CameraPlane, theRay) + CameraPlane.normal * 0.5f;
			Ray theRay2 = new Ray(revealer.WorldPos - m_cameraForward * 500f, m_cameraForward);
			revealerCandidate.WorldPos = m_levelInfo.GetGroundPlaneRayIntersectionPosition(m_groundPlane, theRay2);
			if (m_los != null && m_los.BacksideVertices.ContainsKey(revealerCandidate.OriginIndex))
			{
				int num27 = -1;
				for (int l = 0; l < m_doorLookupGuid.Length; l++)
				{
					if (m_doorLookupGuid[l] == m_los.BacksideVertices[revealerCandidate.OriginIndex].DoorID)
					{
						num27 = l;
						break;
					}
				}
				if (num27 >= 0 && m_doorLookupValidation[num27])
				{
					Vector2 rhs = GameUtilities.V3Subtract2D(revealer.WorldPos, m_doorLookupPos[num27]);
					rhs.Normalize();
					if (Vector2.Dot(m_doorLookupForward[num27], rhs) < 0f)
					{
						revealerCandidate.UseBacksideVerts = true;
					}
				}
			}
			if (num23 < 0)
			{
				num23 = 0;
			}
			if (num24 < 0)
			{
				num24 = 0;
			}
			if (num25 >= m_fogVertsPerColumn)
			{
				num25 = m_fogVertsPerColumn - 1;
			}
			if (num26 >= m_fogVertsPerRow)
			{
				num26 = m_fogVertsPerRow - 1;
			}
			for (int m = num23; m <= num25; m++)
			{
				for (int n = num24; n <= num26; n++)
				{
					int num28 = m * m_fogVertsPerRow + n;
					if (!m_fogUpdateState[num28])
					{
						m_updateList.Add(num28);
						m_underRevealerList.Add(num28);
						m_fogUpdateState[num28] = true;
						m_fogDesiredAlphas[num28] = m_fogAlphasStaging[num28].y;
					}
				}
			}
		}
		m_fadeOutList.Clear();
		for (int num29 = 0; num29 < m_prevUpdateList.Count; num29++)
		{
			int num30 = m_prevUpdateList[num29];
			if (!m_fogUpdateState[num30])
			{
				m_updateList.Add(num30);
				m_fadeOutList.Add(num30);
				m_fogDesiredAlphas[num30] = m_fogAlphasStaging[num30].y;
			}
		}
		for (int num31 = 0; num31 < m_prevUpdateList.Count; num31++)
		{
			m_fogLOSRevealed[m_prevUpdateList[num31]] = false;
		}
		m_prevUpdateList.Clear();
		m_prevUpdateList.AddRange(m_underRevealerList);
		for (int num32 = 0; num32 < m_updateList.Count; num32++)
		{
			int num33 = m_updateList[num32];
			num8 = m_fogDesiredAlphas[num33];
			num10 = m_fogAlphasStaging[num33].y;
			num9 = num10;
			num11 = num10;
			for (int num34 = 0; num34 < m_candidates.Count; num34++)
			{
				RevealerCandidate revealerCandidate2 = m_candidates[num34];
				num14 = 0f;
				if (m_los != null && (!revealerCandidate2.Revealer.HasTrigger || revealerCandidate2.Revealer.RespectLOS))
				{
					FogOfWarLOS.VertexData[] array = m_los.Vertices[revealerCandidate2.OriginIndex];
					if (revealerCandidate2.UseBacksideVerts)
					{
						array = m_los.BacksideVertices[revealerCandidate2.OriginIndex].VertexData;
					}
					if (array != null)
					{
						int num35 = num33 / m_fogVertsPerRow;
						num5 = num33 % m_fogVertsPerRow;
						int num36 = num35 - revealerCandidate2.OriginX + num2;
						num6 = num5 - revealerCandidate2.OriginY + num2;
						num7 = num36 * num3 + num6;
						if (num7 < 0 || num7 >= num4)
						{
							continue;
						}
						if (array[num7] == null)
						{
							num14 = 0f;
						}
						else
						{
							num14 = array[num7].MinAlpha;
							if (array[num7].DoorIndices != null)
							{
								for (int num37 = 0; num37 < array[num7].DoorIndices.Length; num37++)
								{
									byte b = array[num7].DoorIndices[num37];
									if (m_doorLookupValidation[b] && m_doorLookup[b].CurrentState != OCL.State.Open && m_doorLookup[b].CurrentState != OCL.State.SealedOpen)
									{
										num14 = m_fogAlphasStaging[num33].y;
										break;
									}
								}
							}
						}
						if (num14 > m_fogAlphasStaging[num33].y)
						{
							num14 = m_fogAlphasStaging[num33].y;
						}
					}
				}
				num15 = Mathf.Max(0.7f, num14);
				float num38 = revealerCandidate2.Revealer.LOSDistance + 3f;
				float num39 = revealerCandidate2.Revealer.LOSDistance - 3f;
				num12 = GameUtilities.V3Distance2D(m_worldVertices[num33], revealerCandidate2.WorldPos);
				if (num12 > num38)
				{
					num8 = m_fogAlphasStaging[num33].y;
				}
				else if (num12 > revealerCandidate2.Revealer.LOSDistance)
				{
					num13 = (1f - (num38 - num12) / 3f) * 0.3f + 0.7f;
					if (num13 < num14)
					{
						num13 = num14;
					}
					if (num13 < m_fogAlphasStaging[num33].y)
					{
						num10 = ((!(num13 > num15)) ? num15 : num13);
					}
					if (num13 < m_fogDesiredAlphas[num33])
					{
						num8 = num13;
					}
				}
				else if (num12 < num39 - 1f)
				{
					num8 = num14;
					num10 = num15;
				}
				else
				{
					num13 = (1f - (revealerCandidate2.Revealer.LOSDistance - num12) / 3f) * 0.7f;
					if (num13 < num14)
					{
						num13 = num14;
					}
					if (num13 < m_fogAlphasStaging[num33].y)
					{
						num10 = ((!(num13 > num15)) ? num15 : num13);
					}
					if (num13 < m_fogDesiredAlphas[num33])
					{
						num8 = num13;
					}
				}
				if (!revealerCandidate2.Revealer.HasTrigger && num8 < 0.7f)
				{
					m_fogLOSRevealed[num33] = true;
				}
				if (revealerCandidate2.Revealer.RevealOnly && num8 < 0.7f)
				{
					num8 = 0.7f;
				}
				if (num8 < num9)
				{
					num9 = num8;
				}
				if (num10 < num11)
				{
					num11 = num10;
				}
			}
			if (num9 > num11)
			{
				num9 = num11;
			}
			m_fogDesiredAlphas[num33] = num9;
			m_fogAlphasStaging[num33].y = num11;
			if (!Disabled && m_fogAlphasStaging[num33].y < 0.7f)
			{
				m_fogAlphasStaging[num33].y = 0.7f;
			}
			num16 = m_fogDesiredAlphas[num33] - m_fogAlphasStaging[num33].x;
			if (Mathf.Abs(num16) <= num)
			{
				m_fogAlphasStaging[num33].x = m_fogDesiredAlphas[num33];
				continue;
			}
			if (num16 > float.Epsilon)
			{
				m_fogAlphasStaging[num33].x += num;
			}
			else
			{
				m_fogAlphasStaging[num33].x -= num;
			}
			m_fogUpdateState[num33] = true;
		}
		for (int num40 = 0; num40 < m_fadeOutList.Count; num40++)
		{
			int num41 = m_fadeOutList[num40];
			if (m_fogAlphasStaging[num41].x < 0.7f)
			{
				m_prevUpdateList.Add(num41);
			}
		}
	}

	private void UpdateColorBuffer()
	{
		for (int i = 0; i < m_fogMapWidth; i++)
		{
			int num = (int)((float)i / (float)m_fogMapWidth * (float)m_fogVertsPerColumn + 0.5f);
			for (int j = 0; j < m_fogMapHeight; j++)
			{
				int num2 = (int)((float)j / (float)m_fogMapHeight * (float)m_fogVertsPerRow + 0.5f);
				int num3 = j * m_fogMapWidth + i;
				int num4 = num * m_fogVertsPerRow + num2;
				m_colorBuffer[num3].a = m_fogAlphas[num4].x;
			}
		}
	}

	public static bool PointVisibleInFog(Vector3 worldPosition)
	{
		if (Instance == null || Instance.PointVisible(worldPosition))
		{
			return true;
		}
		return false;
	}

	public bool PointVisible(Vector3 worldPosition)
	{
		WorldToFogOfWar(worldPosition, out var x, out var y);
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			return m_fogAlphas[x * m_fogVertsPerRow + y].x + 0.01f < 0.7f;
		}
		return false;
	}

	public bool PointFullyVisible(Vector3 worldPosition)
	{
		WorldToFogOfWar(worldPosition, out var x, out var y);
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			return m_fogAlphas[x * m_fogVertsPerRow + y].x + 0.01f < 0.05f;
		}
		return false;
	}

	public bool RangedAttackerVisible(Vector3 worldPosition)
	{
		WorldToFogOfWar(worldPosition, out var x, out var y);
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			return m_fogAlphas[x * m_fogVertsPerRow + y].x + 0.01f < 0.4f;
		}
		return false;
	}

	public float FogValue(Vector3 worldPosition)
	{
		WorldToFogOfWar(worldPosition, out var x, out var y);
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			return m_fogAlphas[x * m_fogVertsPerRow + y].x + 0.01f;
		}
		return 1f;
	}

	public bool PointRevealed(Vector3 worldPosition)
	{
		WorldToFogOfWar(worldPosition, out var x, out var y);
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			if (!(m_fogAlphas[x * m_fogVertsPerRow + y].x <= 0.7f))
			{
				return m_fogAlphas[x * m_fogVertsPerRow + y].y <= 0.7f;
			}
			return true;
		}
		return false;
	}

	public void WorldToFogOfWar(Vector3 worldPos, out int x, out int y)
	{
		if (m_fogVertices == null)
		{
			x = 0;
			y = 0;
			return;
		}
		Ray theRay = new Ray(worldPos, -CameraPlane.normal);
		Vector3 lhs = GetCameraPlaneRayIntersectionPosition(CameraPlane, theRay) + CameraPlane.normal * 0.5f - m_fogVertices[0];
		x = (int)(Vector3.Dot(lhs, m_fogWorldBoundsXNormalized) * 1f / m_fogTileWidth);
		y = (int)(Vector3.Dot(lhs, m_fogWorldBoundsYNormalized) * 1f / m_fogTileHeight);
	}

	public Vector3 FogOfWarToWorld(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < m_fogVertsPerColumn && y < m_fogVertsPerRow)
		{
			return m_worldVertices[x * m_fogVertsPerRow + y];
		}
		return m_worldVertices[0];
	}

	public static Vector3 GetCameraPlaneRayIntersectionPosition(Plane cameraPlane, Ray theRay)
	{
		if (!cameraPlane.Raycast(theRay, out var enter))
		{
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	public Vector3 GetCameraPlaneRayIntersectionPosition(Ray ray)
	{
		return GetCameraPlaneRayIntersectionPosition(Instance.m_cameraPlane, ray);
	}

	public Vector3 GetBackgroundQuadOrigin()
	{
		if (m_levelInfo != null)
		{
			return m_levelInfo.m_backgroundQuadOrigin;
		}
		return Vector3.zero;
	}
}

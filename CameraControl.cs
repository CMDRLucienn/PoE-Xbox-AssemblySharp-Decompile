using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraControl : MonoBehaviour
{
	public enum ScreenShakeValues
	{
		None,
		Miniscule,
		Small,
		Medium,
		Large,
		Catastrophic
	}

	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}

	public RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityZ = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	public float MoveAmount = 5f;

	private float BufferTop;

	private float BufferRight;

	private float BufferLeft;

	public float BufferBottom;

	private float m_mouseScrollBuffer = 5f;

	private float m_mouseScrollBufferOuter = -30f;

	private float m_zoomRes = 0.25f;

	private Vector3 position_offset;

	private Vector3 lastPosition;

	private Vector3 m_mouseDrag_lastMousePos = Vector3.zero;

	private Vector3 m_worldBoundsX = Vector3.one;

	private Vector3 m_worldBoundsY = Vector3.one;

	private Vector3 m_worldBoundsOrigin = Vector3.zero;

	public Transform Audio;

	private float m_lastAudioY;

	private int PlayerControlDisableLevel;

	private int PlayerScrollDisableLevel;

	private Vector3 MoveToPointSrc = Vector3.zero;

	private Vector3 MoveToPointDest = Vector3.zero;

	private float MoveTotalTime;

	private float MoveTime;

	private List<GameObject> m_FollowingUnits = new List<GameObject>(6);

	private bool m_blockoutMode;

	private const float OPTIMAL_FRAME_RATE = 30f;

	private float m_screenShakeTimer;

	private float m_screenShakeTotalTime;

	private float m_screenShakeStrength;

	public bool InterpolatingToTarget;

	public float ScreenShakeMinisculeDuration = 0.01f;

	public float ScreenShakeSmallDuration = 0.05f;

	public float ScreenShakeMediumDuration = 0.2f;

	public float ScreenShakeLargeDuration = 0.5f;

	public float ScreenShakeCatastrophicDuration = 1f;

	public float ScreenShakeMinisculeStrength = 0.01f;

	public float ScreenShakeSmallStrength = 0.05f;

	public float ScreenShakeMediumStrength = 0.1f;

	public float ScreenShakeLargeStrength = 0.2f;

	public float ScreenShakeCatastrophicStrength = 0.5f;

	private bool m_forceReset = true;

	private bool m_atLeft;

	private bool m_atRight;

	private bool m_atTop;

	private bool m_atBottom;

	private bool m_testLeft;

	private bool m_testRight;

	private bool m_testTop;

	private bool m_testBottom;

	public static CameraControl Instance { get; private set; }

	private bool PlayerControlEnabled => PlayerControlDisableLevel == 0;

	private bool PlayerScrollEnabled => PlayerScrollDisableLevel == 0;

	public Vector3 CamNearWorldBoundsOrigin => m_worldBoundsOrigin;

	public Vector3 CamNearWorldBoundsX => m_worldBoundsX;

	public Vector3 CamNearWorldBoundsY => m_worldBoundsY;

	public SyncCameraOrthoSettings OrthoSettings => SyncCameraOrthoSettings.Instance;

	public float CameraMoveDelta => MoveAmount * 30f * GetDeltaTime() * GameState.Option.ScrollSpeed;

	public Vector3 CameraPanDeltaX { get; private set; }

	public Vector3 CameraPanDeltaY { get; private set; }

	public static Plane groundPlane => new Plane(Vector3.up, Vector3.zero);

	private static float GetDeltaTime()
	{
		if (GameState.IsLoading)
		{
			return 0f;
		}
		if (GameState.Paused && Cutscene.CutsceneActive)
		{
			return 0f;
		}
		return Time.unscaledDeltaTime;
	}

	public void DoUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Camera main = Camera.main;
		if (m_forceReset)
		{
			m_testLeft = true;
			m_testRight = true;
			m_testTop = true;
			m_testBottom = true;
			m_forceReset = false;
		}
		else
		{
			m_testLeft = false;
			m_testRight = false;
			m_testTop = false;
			m_testBottom = false;
		}
		if (PlayerControlEnabled && GameState.ApplicationIsFocused)
		{
			if (PlayerScrollEnabled)
			{
				float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
				if (axisRaw != 0f)
				{
					OrthoSettings.SetZoomLevelDelta(axisRaw);
					ResetAtEdges();
				}
			}
			if (GameInput.GetDoublePressed(KeyCode.Mouse0, handle: true) && !UINoClick.MouseOverUI)
			{
				Vector3 point = GameInput.WorldMousePosition;
				if ((bool)GameCursor.CharacterUnderCursor)
				{
					point = GameCursor.CharacterUnderCursor.transform.position;
					ResetAtEdges();
				}
				if ((bool)Instance)
				{
					Instance.FocusOnPoint(point, 0.4f);
					ResetAtEdges();
				}
			}
			if (GameInput.GetControlDownWithRepeat(MappedControl.ZOOM_IN, handle: true))
			{
				OrthoSettings.SetZoomLevelDelta(m_zoomRes);
				ResetAtEdges();
			}
			if (GameInput.GetControlDownWithRepeat(MappedControl.ZOOM_OUT, handle: true))
			{
				OrthoSettings.SetZoomLevelDelta(0f - m_zoomRes);
				ResetAtEdges();
			}
			if (GameInput.GetControlUp(MappedControl.RESET_ZOOM))
			{
				OrthoSettings.SetZoomLevel(1f, force: false);
				ResetAtEdges();
			}
			if (GameInput.GetControlUp(MappedControl.FOLLOW_CAM))
			{
				m_FollowingUnits.Clear();
				for (int i = 0; i < PartyMemberAI.SelectedPartyMembers.Length; i++)
				{
					if ((bool)PartyMemberAI.SelectedPartyMembers[i])
					{
						m_FollowingUnits.Add(PartyMemberAI.SelectedPartyMembers[i]);
					}
				}
			}
			if (GameInput.GetControlDown(MappedControl.PAN_CAMERA))
			{
				m_mouseDrag_lastMousePos = GameInput.MousePosition;
				float num = (float)main.pixelWidth * 0.5f;
				float num2 = (float)main.pixelHeight * 0.5f;
				Vector3 vector = ProjectScreenCoordsToGroundPlane(main, new Vector3(num + 1f, num2, main.nearClipPlane));
				Vector3 vector2 = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2, main.nearClipPlane));
				CameraPanDeltaX = vector2 - vector;
				vector = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2 + 1f, main.nearClipPlane));
				vector2 = ProjectScreenCoordsToGroundPlane(main, new Vector3(num, num2, main.nearClipPlane));
				CameraPanDeltaY = vector2 - vector;
			}
			else if (GameInput.GetControl(MappedControl.PAN_CAMERA))
			{
				CancelFollow();
				Vector3 vector3 = GameInput.MousePosition - m_mouseDrag_lastMousePos;
				m_mouseDrag_lastMousePos = GameInput.MousePosition;
				if (vector3.x < 0f)
				{
					m_atLeft = false;
				}
				else if (vector3.x > 0f)
				{
					m_atRight = false;
				}
				if (vector3.y < 0f)
				{
					m_atBottom = false;
				}
				else if (vector3.y > 0f)
				{
					m_atTop = false;
				}
				if (m_atRight && vector3.x < 0f)
				{
					vector3.x = 0f;
				}
				else if (m_atLeft && vector3.x > 0f)
				{
					vector3.x = 0f;
				}
				if (m_atTop && vector3.y < 0f)
				{
					vector3.y = 0f;
				}
				else if (m_atBottom && vector3.y > 0f)
				{
					vector3.y = 0f;
				}
				if (vector3.x < 0f)
				{
					m_testRight = true;
				}
				else if (vector3.x > 0f)
				{
					m_testLeft = true;
				}
				if (vector3.y < 0f)
				{
					m_testTop = true;
				}
				else if (vector3.y > 0f)
				{
					m_testBottom = true;
				}
				position_offset += -main.transform.right * CameraPanDeltaX.magnitude * vector3.x;
				position_offset += -main.transform.up * Vector3.Dot(-main.transform.up, CameraPanDeltaY) * vector3.y;
			}
			else
			{
				bool option = GameState.Option.GetOption(GameOption.BoolOption.SCREEN_EDGE_SCROLLING);
				bool flag = GameInput.MousePosition.x > 0f + m_mouseScrollBufferOuter && GameInput.MousePosition.x < (float)Screen.width - m_mouseScrollBufferOuter;
				bool flag2 = GameInput.MousePosition.y > 0f + m_mouseScrollBufferOuter && GameInput.MousePosition.y < (float)Screen.height - m_mouseScrollBufferOuter;
				if (GameInput.GetControl(MappedControl.PAN_CAMERA_LEFT) || (flag2 && option && GameInput.MousePosition.x < m_mouseScrollBuffer && GameInput.MousePosition.x > m_mouseScrollBufferOuter))
				{
					CancelFollow();
					m_atRight = false;
					if (!m_atLeft)
					{
						position_offset -= Camera.main.transform.right * CameraMoveDelta;
						m_testLeft = true;
					}
				}
				else if (GameInput.GetControl(MappedControl.PAN_CAMERA_RIGHT) || (flag2 && option && GameInput.MousePosition.x > (float)Screen.width - m_mouseScrollBuffer && GameInput.MousePosition.x < (float)Screen.width - m_mouseScrollBufferOuter))
				{
					CancelFollow();
					m_atLeft = false;
					if (!m_atRight)
					{
						position_offset += Camera.main.transform.right * CameraMoveDelta;
						m_testRight = true;
					}
				}
				if (GameInput.GetControl(MappedControl.PAN_CAMERA_DOWN) || (flag && option && GameInput.MousePosition.y < m_mouseScrollBuffer && GameInput.MousePosition.y > m_mouseScrollBufferOuter))
				{
					CancelFollow();
					m_atTop = false;
					if (!m_atBottom)
					{
						position_offset -= Camera.main.transform.up * CameraMoveDelta;
						m_testBottom = true;
					}
				}
				else if (GameInput.GetControl(MappedControl.PAN_CAMERA_UP) || (flag && option && GameInput.MousePosition.y > (float)Screen.height - m_mouseScrollBuffer && GameInput.MousePosition.y < (float)Screen.height - m_mouseScrollBufferOuter))
				{
					CancelFollow();
					m_atBottom = false;
					if (!m_atTop)
					{
						position_offset += Camera.main.transform.up * CameraMoveDelta;
						m_testTop = true;
					}
				}
			}
		}
		if (m_FollowingUnits.Count > 0 && GetDeltaTime() > 0f)
		{
			Vector3 zero = Vector3.zero;
			int num3 = 0;
			for (int num4 = m_FollowingUnits.Count - 1; num4 >= 0; num4--)
			{
				if ((bool)m_FollowingUnits[num4])
				{
					zero += m_FollowingUnits[num4].transform.position;
					num3++;
				}
			}
			if (num3 > 0)
			{
				FocusOnPoint(zero / num3);
			}
		}
		if (InterpolatingToTarget)
		{
			if ((bool)GameState.s_playerCharacter)
			{
				MoveTime -= GetDeltaTime();
			}
			Vector3 position = Vector3.zero;
			if (MoveTime <= 0f)
			{
				MoveTime = 0f;
				InterpolatingToTarget = false;
				position = MoveToPointDest;
			}
			else
			{
				float t = MoveTime / MoveTotalTime;
				position.x = Mathf.SmoothStep(MoveToPointDest.x, MoveToPointSrc.x, t);
				position.y = Mathf.SmoothStep(MoveToPointDest.y, MoveToPointSrc.y, t);
				position.z = Mathf.SmoothStep(MoveToPointDest.z, MoveToPointSrc.z, t);
			}
			lastPosition = position;
			base.transform.position = position;
			position_offset = Vector3.zero;
			ResetAtEdges();
		}
		Vector3 zero2 = Vector3.zero;
		if (m_screenShakeTimer > 0f && GameState.s_playerCharacter != null)
		{
			m_screenShakeTimer -= Time.unscaledDeltaTime;
			Vector3 vector4 = Random.onUnitSphere * m_screenShakeStrength * (m_screenShakeTimer / m_screenShakeTotalTime);
			zero2 += vector4.x * -main.transform.right;
			zero2 += vector4.y * -main.transform.up;
		}
		Vector3 zero3 = Vector3.zero;
		base.transform.position = lastPosition + position_offset + zero2;
		if (!m_blockoutMode)
		{
			Vector3 vector5 = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane));
			Vector3 vector6 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane));
			Vector3 vector7 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, Camera.main.nearClipPlane)) - vector5;
			Vector3 vector8 = Camera.main.ScreenToWorldPoint(new Vector3(0f, Camera.main.pixelHeight, Camera.main.nearClipPlane)) - vector5;
			float magnitude = vector7.magnitude;
			float magnitude2 = vector8.magnitude;
			vector5 -= m_worldBoundsOrigin;
			Vector3 lhs = vector6 - m_worldBoundsOrigin;
			Vector3 worldBoundsX = m_worldBoundsX;
			Vector3 worldBoundsY = m_worldBoundsY;
			worldBoundsX.Normalize();
			worldBoundsY.Normalize();
			float num5 = Vector3.Dot(vector5, worldBoundsX);
			float num6 = Vector3.Dot(vector5, worldBoundsY);
			float num7 = Vector3.Dot(lhs, worldBoundsX);
			float num8 = Vector3.Dot(lhs, worldBoundsY);
			float magnitude3 = m_worldBoundsX.magnitude;
			float magnitude4 = m_worldBoundsY.magnitude;
			float num9 = BufferLeft * magnitude;
			float num10 = BufferRight * magnitude;
			float num11 = BufferTop * magnitude2;
			float num12 = BufferBottom * magnitude2;
			if (magnitude > magnitude3)
			{
				float num13 = (magnitude - magnitude3) / 2f;
				num9 += num13;
				num10 += num13;
			}
			if (magnitude2 > magnitude4)
			{
				float num14 = (magnitude2 - magnitude4) / 2f;
				num11 += num14;
				num12 += num14;
			}
			if (m_testLeft && num5 < 0f - num9)
			{
				zero3 += (0f - num5 - num9) * worldBoundsX;
				m_atLeft = true;
				m_atRight = false;
			}
			else if (m_testRight && num7 > magnitude3 + num10)
			{
				zero3 -= (num7 - (magnitude3 + num10)) * worldBoundsX;
				m_atRight = true;
				m_atLeft = false;
			}
			if (m_testBottom && num6 < 0f - num12)
			{
				zero3 += (0f - num6 - num12) * worldBoundsY;
				m_atBottom = true;
				m_atTop = false;
			}
			else if (m_testTop && num8 > magnitude4 + num11)
			{
				zero3 -= (num8 - (magnitude4 + num11)) * worldBoundsY;
				m_atTop = true;
				m_atBottom = false;
			}
			base.transform.position = lastPosition + position_offset + zero3;
			lastPosition = base.transform.position;
			base.transform.position += zero2;
			position_offset = Vector3.zero;
		}
		if (Audio != null)
		{
			Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
			int layerMask = 1 << LayerMask.NameToLayer("Walkable");
			if (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, layerMask))
			{
				Audio.position = hitInfo.point;
				m_lastAudioY = Audio.position.y;
			}
			else
			{
				Plane cameraPlane = new Plane(Vector3.up, new Vector3(0f, m_lastAudioY, 0f));
				Audio.position = GetPlaneRayIntersectionPosition(cameraPlane, ray);
			}
		}
	}

	public void CancelFollow()
	{
		m_FollowingUnits.Clear();
	}

	public void ResetAtEdges()
	{
		m_atLeft = false;
		m_atRight = false;
		m_atTop = false;
		m_atBottom = false;
		m_testLeft = true;
		m_testRight = true;
		m_testTop = true;
		m_testBottom = true;
		m_forceReset = true;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void ForceFullEnablePlayerControl()
	{
		PlayerControlDisableLevel = 0;
		InterpolatingToTarget = false;
	}

	public void EnablePlayerControl(bool enableControl)
	{
		if (enableControl)
		{
			PlayerControlDisableLevel = Mathf.Max(PlayerControlDisableLevel - 1, 0);
		}
		else
		{
			PlayerControlDisableLevel++;
		}
	}

	public void EnablePlayerScroll(bool enableScroll)
	{
		if (enableScroll)
		{
			PlayerScrollDisableLevel = Mathf.Max(PlayerScrollDisableLevel - 1, 0);
		}
		else
		{
			PlayerScrollDisableLevel++;
		}
	}

	public void FocusOnObjectOffsetLimited(GameObject focus, float time, Vector3 offset, float limit)
	{
		Vector3 hitPoint = GetHitPoint(base.transform.position);
		if ((GetHitPoint(focus.transform.position + offset) - hitPoint).magnitude <= limit)
		{
			FocusOnObjectOffset(focus, time, offset);
		}
	}

	public void FocusOnObjectOffset(GameObject focus, float time, Vector3 offset)
	{
		FocusOnPoint(focus.transform.position + offset, time);
	}

	public void FocusOnObject(GameObject focus, float time)
	{
		FocusOnPoint(focus.transform.position, time);
	}

	public void FocusOnPoint(Vector3 point, float time)
	{
		FocusOnPoint(base.transform.position, point, time);
	}

	public void FocusOnPoint(Vector3 start, Vector3 end, float time)
	{
		CancelFollow();
		MoveToPointSrc = start;
		MoveTotalTime = time;
		MoveTime = time;
		MoveToPointDest = end;
		MoveToPointSrc = GetHitPoint(MoveToPointSrc);
		MoveToPointDest = GetHitPoint(MoveToPointDest);
		InterpolatingToTarget = true;
		ResetAtEdges();
	}

	private Vector3 GetHitPoint(Vector3 cameraPosition)
	{
		Plane plane = new Plane(Camera.main.transform.forward, Camera.main.transform.transform.position);
		Vector3 vector = base.transform.forward * -1f;
		Ray ray = new Ray(cameraPosition, vector);
		_ = Vector3.zero;
		if (plane.Raycast(ray, out var enter))
		{
			cameraPosition = vector * enter + cameraPosition;
		}
		return cameraPosition;
	}

	public void FocusOnPlayer()
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if (s_playerCharacter != null)
		{
			FocusOnPoint(s_playerCharacter.transform.position);
		}
		else
		{
			FocusOnPartySpawner();
		}
	}

	public void FocusOnPartySpawner()
	{
		PartySpawner partySpawner = Object.FindObjectOfType(typeof(PartySpawner)) as PartySpawner;
		if (partySpawner != null)
		{
			FocusOnPoint(partySpawner.transform.position);
		}
	}

	public void FocusOnPoint(Vector3 point)
	{
		Vector3 vector = base.transform.forward * -1f;
		Plane plane = new Plane(Camera.main.transform.forward, Camera.main.transform.position);
		Ray ray = new Ray(point, vector);
		float enter = 0f;
		Vector3 zero = Vector3.zero;
		if (plane.Raycast(ray, out enter))
		{
			zero = vector * enter;
			zero += point;
			Camera.main.transform.position = (lastPosition = zero);
		}
		ResetAtEdges();
	}

	public void ScreenShake(float duration, float strength)
	{
		if (!(strength < m_screenShakeStrength) || !(m_screenShakeTimer > 0f))
		{
			m_screenShakeStrength = strength;
			m_screenShakeTimer = (m_screenShakeTotalTime = duration);
		}
	}

	public float GetShakeDuration(ScreenShakeValues duration)
	{
		return duration switch
		{
			ScreenShakeValues.Miniscule => ScreenShakeMinisculeDuration, 
			ScreenShakeValues.Small => ScreenShakeSmallDuration, 
			ScreenShakeValues.Medium => ScreenShakeMediumDuration, 
			ScreenShakeValues.Large => ScreenShakeLargeDuration, 
			ScreenShakeValues.Catastrophic => ScreenShakeCatastrophicDuration, 
			_ => 0f, 
		};
	}

	public float GetShakeStrength(ScreenShakeValues strength)
	{
		return strength switch
		{
			ScreenShakeValues.Miniscule => ScreenShakeMinisculeStrength, 
			ScreenShakeValues.Small => ScreenShakeSmallStrength, 
			ScreenShakeValues.Medium => ScreenShakeMediumStrength, 
			ScreenShakeValues.Large => ScreenShakeLargeStrength, 
			ScreenShakeValues.Catastrophic => ScreenShakeCatastrophicStrength, 
			_ => 0f, 
		};
	}

	public Vector3 ProjectScreenCoordsToCameraPlane(Camera theCamera, Vector3 screenPoint)
	{
		Vector3 vector = theCamera.transform.forward * -1f;
		Vector3 vector2 = theCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, theCamera.nearClipPlane));
		Plane plane = new Plane(Vector3.up, theCamera.transform.position);
		Ray ray = new Ray(vector2, vector);
		float enter = 0f;
		plane.Raycast(ray, out enter);
		return vector2 + enter * vector;
	}

	public Vector3 ProjectScreenCoordsToGroundPlane(Camera theCamera, Vector3 screenPoint)
	{
		Vector3 vector = theCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, theCamera.nearClipPlane));
		Ray ray = new Ray(vector, theCamera.transform.forward);
		float enter = 0f;
		groundPlane.Raycast(ray, out enter);
		return vector + enter * theCamera.transform.forward;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'CameraControl' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		Start();
	}

	private void Start()
	{
		m_forceReset = true;
		if (PE_GameRender.Instance != null)
		{
			m_blockoutMode = PE_GameRender.Instance.m_blockoutMode;
		}
		if (Application.isEditor && Application.isPlaying && m_blockoutMode)
		{
			VerifyBlockoutModeStaticScenePhysics();
		}
		LevelInfo levelInfo = null;
		LevelInfo[] array = Object.FindObjectsOfType(typeof(LevelInfo)) as LevelInfo[];
		int num = array.Length;
		if (num <= 0)
		{
			Debug.LogError("MainCameraSetup: cannot find LevelInfo object in scene!");
		}
		else if (num > 1)
		{
			Debug.LogError("Error: Level contains more than one LevelInfo!");
		}
		else
		{
			Camera.main.transform.position = (lastPosition = array[0].m_CameraPos);
			levelInfo = array[0];
			Camera.main.transform.localRotation = array[0].m_CameraRotation;
		}
		if (!(levelInfo == null))
		{
			Camera main = Camera.main;
			Plane cameraPlane = new Plane(main.transform.forward, main.transform.position + main.transform.forward * main.nearClipPlane);
			Vector3 backgroundQuadOrigin = levelInfo.m_backgroundQuadOrigin;
			Vector3 origin = levelInfo.m_backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisX * levelInfo.m_backgroundQuadWidth;
			Vector3 origin2 = levelInfo.m_backgroundQuadOrigin + levelInfo.m_backgroundQuadAxisY * levelInfo.m_backgroundQuadHeight;
			Ray theRay = new Ray(backgroundQuadOrigin, -main.transform.forward);
			Ray theRay2 = new Ray(origin, -main.transform.forward);
			Ray theRay3 = new Ray(origin2, -main.transform.forward);
			m_worldBoundsOrigin = GetPlaneRayIntersectionPosition(cameraPlane, theRay);
			Vector3 planeRayIntersectionPosition = GetPlaneRayIntersectionPosition(cameraPlane, theRay2);
			Vector3 planeRayIntersectionPosition2 = GetPlaneRayIntersectionPosition(cameraPlane, theRay3);
			m_worldBoundsX = planeRayIntersectionPosition - m_worldBoundsOrigin;
			m_worldBoundsY = planeRayIntersectionPosition2 - m_worldBoundsOrigin;
			FocusOnPlayer();
		}
	}

	private Vector3 GetPlaneRayIntersectionPosition(Plane cameraPlane, Ray theRay)
	{
		if (!cameraPlane.Raycast(theRay, out var enter))
		{
			Debug.LogError("CameraControl::GetPlaneRayIntersectionPosition() - failed to find intersection with plane");
			return Vector3.zero;
		}
		return theRay.origin + theRay.direction * enter;
	}

	private void VerifyBlockoutModeStaticScenePhysics()
	{
		MeshRenderer[] array = Object.FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer.gameObject.name.StartsWith("T_") && meshRenderer.gameObject.isStatic && null == meshRenderer.gameObject.GetComponent<MeshCollider>())
			{
				Debug.LogError("Tile mesh (" + meshRenderer.gameObject.name + ") is missing a mesh collider. Scene picking will not work without it. Use \"Add Mesh Colliders to Tiles\" in the blockout tool");
				break;
			}
		}
	}
}

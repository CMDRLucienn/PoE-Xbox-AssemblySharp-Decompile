using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PE_Collider2D : MonoBehaviour
{
	[Tooltip("List of vertices defining the shape of this collider.")]
	public List<Vector3> VertList = new List<Vector3>();

	private Mesh m_Mesh;

	private Trigger[] m_triggers;

	private Trap[] m_traps;

	private SceneTransition m_sceneTransition;

	private Switch m_switch;

	private Usable[] m_usables;

	private Detectable m_detectable;

	private Container m_Container;

	private OCL m_ocl;

	private Vector3[] m_screenVerts;

	private List<GameObject> m_objectsToCheckInBounds = new List<GameObject>();

	private List<GameObject> m_objectsInBounds = new List<GameObject>();

	public Texture2D DisplayIcon;

	private UITriggerIcon m_ActiveIcon;

	public bool m_neverRender;

	[Range(-1f, 2f)]
	public float IconXOffset = 0.5f;

	[Range(-1f, 2f)]
	public float IconYOffset = 0.5f;

	[Range(-1f, 2f)]
	public float IconZOffset = 0.5f;

	[Range(0.01f, 1f)]
	public float IconScale = 1f;

	public GameCursor.CursorType MouseOverCursor = GameCursor.CursorType.Interact;

	[Persistent]
	public Color LineColor = Color.red;

	[Persistent]
	public bool UsePlayerColor;

	private bool m_renderSubscribed;

	private bool m_recalculateVerts = true;

	private bool m_ManualHideIcon;

	public Bounds bounds { get; set; }

	public bool m_shouldRender { get; set; }

	public bool ShouldRender
	{
		get
		{
			if (!m_neverRender && !GameState.IsLoading && (m_shouldRender || RenderLines) && (FogOfWar.Instance == null || FogOfWar.Instance.PointRevealed(Center)) && !UILootManager.Instance.WindowActive() && base.isActiveAndEnabled)
			{
				return IsDetected;
			}
			return false;
		}
	}

	public Color DrawColor
	{
		get
		{
			if ((bool)m_Container && m_Container.HasInteracted)
			{
				if (m_Container.IsEmpty)
				{
					return Color.gray;
				}
				return new Color(LineColor.r * 0.75f, LineColor.g * 0.75f, LineColor.b * 0.75f);
			}
			return LineColor;
		}
	}

	public bool MouseOver { get; private set; }

	public bool RenderLines { get; set; }

	public Vector3 Center { get; private set; }

	public Vector3 PreviewPoint { get; set; }

	public bool DrawPreview { get; set; }

	public bool ManualHideIcon
	{
		get
		{
			if (!m_ManualHideIcon && !(FogOfWar.Instance == null))
			{
				return !FogOfWar.Instance.PointRevealed(base.transform.position);
			}
			return true;
		}
		set
		{
			m_ManualHideIcon = value;
			RefreshIcon();
		}
	}

	public bool HasTrap
	{
		get
		{
			if (m_traps != null)
			{
				return m_traps.Length != 0;
			}
			return false;
		}
	}

	public Usable FirstUsable
	{
		get
		{
			for (int i = 0; i < m_usables.Length; i++)
			{
				if (m_usables[i].IsUsable)
				{
					return m_usables[i];
				}
			}
			return null;
		}
	}

	public bool CanUse
	{
		get
		{
			Usable firstUsable = FirstUsable;
			if (!firstUsable || !firstUsable.IsUsable)
			{
				return false;
			}
			return IsDetected;
		}
	}

	public bool IsDetected
	{
		get
		{
			if ((bool)m_detectable && !m_detectable.Detected)
			{
				if (HasTrap)
				{
					return m_Container;
				}
				return false;
			}
			return true;
		}
	}

	public float UsableRadius
	{
		get
		{
			if (GetComponent<Collider>() is SphereCollider)
			{
				return (GetComponent<Collider>() as SphereCollider).radius;
			}
			return 1f;
		}
	}

	public event UIEventListener.BoolDelegate OnHover;

	private void OnEnable()
	{
		if ((bool)InGameHUD.Instance)
		{
			InGameHUD instance = InGameHUD.Instance;
			instance.OnHighlightBegin = (InGameHUD.HighlightEvent)Delegate.Combine(instance.OnHighlightBegin, new InGameHUD.HighlightEvent(RefreshIcon));
			InGameHUD instance2 = InGameHUD.Instance;
			instance2.OnHighlightEnd = (InGameHUD.HighlightEvent)Delegate.Combine(instance2.OnHighlightEnd, new InGameHUD.HighlightEvent(RefreshIcon));
		}
		RefreshIcon();
	}

	private void OnDisable()
	{
		if ((bool)InGameHUD.Instance)
		{
			InGameHUD instance = InGameHUD.Instance;
			instance.OnHighlightBegin = (InGameHUD.HighlightEvent)Delegate.Remove(instance.OnHighlightBegin, new InGameHUD.HighlightEvent(RefreshIcon));
			InGameHUD instance2 = InGameHUD.Instance;
			instance2.OnHighlightEnd = (InGameHUD.HighlightEvent)Delegate.Remove(instance2.OnHighlightEnd, new InGameHUD.HighlightEvent(RefreshIcon));
		}
		if (MouseOver)
		{
			NotifyHoverOff();
		}
		RefreshIcon(onDisable: true);
	}

	private void Start()
	{
		m_Container = GetComponent<Container>();
		if (VertList.Count == 0)
		{
			Debug.LogError("No vert list has been created for the 2d collider " + base.gameObject.name, base.gameObject);
			base.gameObject.SetActive(value: false);
			return;
		}
		m_screenVerts = new Vector3[VertList.Count];
		m_Mesh = InteriorTriangulation.Triangulate(VertList, base.transform.rotation);
		m_Mesh.name = "TriangulatedMesh";
		m_Mesh.uv = new Vector2[VertList.Count];
		m_Mesh.tangents = new Vector4[VertList.Count];
		GameObject obj = new GameObject("Mesh");
		obj.layer = LayerUtility.FindLayerValue("InGameUI");
		obj.transform.parent = base.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = Vector3.zero;
		obj.AddComponent<MeshFilter>().mesh = m_Mesh;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		meshRenderer.material.shader = Shader.Find("Trenton/PE_Collider2D");
		CalculateBounds();
		m_triggers = GetComponents<Trigger>();
		m_sceneTransition = GetComponent<SceneTransition>();
		m_usables = GetComponents<Usable>();
		m_detectable = GetComponent<Detectable>();
		m_switch = GetComponent<Switch>();
		m_ocl = GetComponent<OCL>();
		m_traps = GetComponents<Trap>();
		if (!m_sceneTransition)
		{
			SphereCollider sphereCollider = GetComponent<Collider>() as SphereCollider;
			if (sphereCollider == null)
			{
				sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			}
			sphereCollider.isTrigger = true;
			sphereCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			base.gameObject.layer = 0;
		}
		if ((bool)m_sceneTransition)
		{
			ManualHideIcon = true;
		}
		if (VertList.Count > 1)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < VertList.Count; i++)
			{
				zero += VertList[i];
			}
			zero /= (float)VertList.Count;
			Center = zero + base.transform.position;
		}
	}

	private void OnDestroy()
	{
		if (InGameHUD.Instance != null)
		{
			InGameHUD instance = InGameHUD.Instance;
			instance.OnHighlightBegin = (InGameHUD.HighlightEvent)Delegate.Remove(instance.OnHighlightBegin, new InGameHUD.HighlightEvent(RefreshIcon));
			InGameHUD instance2 = InGameHUD.Instance;
			instance2.OnHighlightEnd = (InGameHUD.HighlightEvent)Delegate.Remove(instance2.OnHighlightEnd, new InGameHUD.HighlightEvent(RefreshIcon));
		}
		if ((bool)InGameUILayout.NGUICamera && m_renderSubscribed)
		{
			InGameUILayout.NGUICamera.GetComponent<UICamera>().OnPreRenderCallbacks -= OnGuiPreRender;
			m_renderSubscribed = false;
		}
	}

	private void Update()
	{
		m_recalculateVerts = true;
		if (!m_renderSubscribed && (bool)InGameUILayout.NGUICamera)
		{
			InGameUILayout.NGUICamera.GetComponent<UICamera>().OnPreRenderCallbacks += OnGuiPreRender;
			m_renderSubscribed = true;
		}
		if (m_screenVerts == null)
		{
			Debug.LogError("No vert list has been created for the 2d collider '" + base.gameObject.name + "'.", base.gameObject);
			base.gameObject.SetActive(value: false);
			return;
		}
		bool flag = false;
		if (MouseInPolygon() && !GameState.s_playerCharacter.IsActionCursor())
		{
			if (GameCursor.GenericUnderCursor == base.gameObject || GameCursor.GenericUnderCursor == null || GameCursor.GenericUnderCursor.GetComponent<Container>() == null)
			{
				MouseOver = true;
				if (this.OnHover != null)
				{
					this.OnHover(base.gameObject, state: true);
				}
				RefreshIcon();
				if (CanUse)
				{
					GameCursor.GenericUnderCursor = base.gameObject;
					GameCursor.CursorOverride = MouseOverCursor;
					if (GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
					{
						GameInput.HandleAllClicks();
						GameState.s_playerCharacter.ObjectClicked(FirstUsable);
					}
				}
			}
			else
			{
				flag = true;
			}
		}
		else if (MouseOver)
		{
			NotifyHoverOff();
			if (this.OnHover != null)
			{
				this.OnHover(base.gameObject, state: false);
			}
			RefreshIcon();
		}
		if ((m_triggers != null && m_triggers.Length != 0) || HasTrap)
		{
			for (int num = m_objectsToCheckInBounds.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = m_objectsToCheckInBounds[num];
				if (!(gameObject == null) && gameObject.activeInHierarchy && PointInPolygon(gameObject.transform.position))
				{
					Trigger[] triggers = m_triggers;
					for (int i = 0; i < triggers.Length; i++)
					{
						triggers[i].NotifyTriggerEnter(gameObject.GetComponent<Collider>());
					}
					if (m_traps != null && !m_ocl)
					{
						Trap[] traps = m_traps;
						for (int i = 0; i < traps.Length; i++)
						{
							traps[i].ActivateTrap(gameObject);
						}
					}
					m_objectsToCheckInBounds.Remove(gameObject);
					m_objectsInBounds.Add(gameObject);
				}
			}
			for (int num2 = m_objectsInBounds.Count - 1; num2 >= 0; num2--)
			{
				GameObject gameObject2 = m_objectsInBounds[num2];
				if (gameObject2 == null || !gameObject2.activeInHierarchy)
				{
					if ((bool)gameObject2)
					{
						Trigger[] triggers = m_triggers;
						for (int i = 0; i < triggers.Length; i++)
						{
							triggers[i].NotifyTriggerExit(gameObject2.GetComponent<Collider>());
						}
					}
					m_objectsInBounds.RemoveAt(num2);
				}
				else if (!PointInPolygon(gameObject2.transform.position))
				{
					Trigger[] triggers = m_triggers;
					for (int i = 0; i < triggers.Length; i++)
					{
						triggers[i].NotifyTriggerExit(gameObject2.GetComponent<Collider>());
					}
					m_objectsInBounds.RemoveAt(num2);
					m_objectsToCheckInBounds.Add(gameObject2);
				}
			}
		}
		if (!IsDetected)
		{
			m_shouldRender = false;
		}
		else
		{
			m_shouldRender = InGameHUD.Instance.HighlightActive || (MouseOver && !flag);
			if (!m_shouldRender && Stealth.AnyStealthInStealthMode())
			{
				if ((bool)m_switch)
				{
					m_shouldRender = true;
				}
				else if (m_traps != null)
				{
					for (int num3 = m_traps.Length - 1; num3 >= 0; num3--)
					{
						if ((bool)m_traps[num3] && m_traps[num3].Visible)
						{
							m_shouldRender = true;
							break;
						}
					}
				}
			}
			if (m_shouldRender)
			{
				bool shouldRender = false;
				for (int j = 0; j < m_usables.Length; j++)
				{
					if (m_usables[j].IsUsable && m_usables[j].IsVisible)
					{
						shouldRender = true;
						break;
					}
				}
				m_shouldRender = shouldRender;
			}
		}
		if (UsePlayerColor)
		{
			LineColor = (InGameHUD.Instance.UseColorBlindSettings ? InGameHUD.Instance.FriendlyColorBlind.color : InGameHUD.Instance.Friendly.color);
		}
	}

	private void NotifyHoverOff()
	{
		MouseOver = false;
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
			GameCursor.CursorOverride = GameCursor.CursorType.None;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		AIController component = other.gameObject.GetComponent<AIController>();
		if ((!(component != null) || !component.IsPet) && !m_objectsToCheckInBounds.Contains(other.gameObject))
		{
			m_objectsToCheckInBounds.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (m_objectsToCheckInBounds.Contains(other.gameObject))
		{
			m_objectsToCheckInBounds.Remove(other.gameObject);
		}
		if (m_objectsInBounds.Contains(other.gameObject))
		{
			Trigger[] triggers = m_triggers;
			for (int i = 0; i < triggers.Length; i++)
			{
				triggers[i].NotifyTriggerExit(other);
			}
			m_objectsInBounds.Remove(other.gameObject);
		}
	}

	private Vector3 GetIconWorldPosition()
	{
		Vector3 center = Center;
		float y = bounds.size.y;
		center.y = base.transform.position.y - y * 0.5f + y * IconYOffset;
		float z = bounds.size.z;
		center.z = base.transform.position.z - z * 0.5f + z * IconZOffset;
		center.x = base.transform.position.x - z * 0.5f + z * IconXOffset;
		return center;
	}

	public void RefreshIcon()
	{
		RefreshIcon(onDisable: false);
	}

	public void RefreshIcon(bool onDisable)
	{
		if (!(DisplayIcon != null))
		{
			return;
		}
		if ((MouseOver || ((bool)InGameHUD.Instance && InGameHUD.Instance.HighlightActive)) && !ManualHideIcon && base.isActiveAndEnabled && !onDisable)
		{
			if (m_ActiveIcon == null)
			{
				Detectable component = base.gameObject.GetComponent<Detectable>();
				if (component == null || component.Detected)
				{
					m_ActiveIcon = UITriggerManager.Instance.Show(GetIconWorldPosition(), DisplayIcon, null, null, m_sceneTransition);
				}
			}
		}
		else if (m_ActiveIcon != null)
		{
			UITriggerManager.Instance.Hide(m_ActiveIcon);
			m_ActiveIcon = null;
		}
	}

	private void OnGuiPreRender(object sender, EventArgs args)
	{
		if (ShouldRender && (bool)base.transform && VertList != null && VertList.Count >= 2)
		{
			GL.PushMatrix();
			GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
			GL.modelview = Camera.main.worldToCameraMatrix;
			Color drawColor = DrawColor;
			for (int i = 1; i < VertList.Count; i++)
			{
				GUIHelper.DrawLine(VertList[i - 1] + base.transform.position, VertList[i] + base.transform.position, drawColor);
			}
			GUIHelper.DrawLine(VertList[0] + base.transform.position, VertList[VertList.Count - 1] + base.transform.position, drawColor);
			GL.PopMatrix();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		Vector3 vector = Vector3.up * 3f;
		Vector3 to = vector - Vector3.up * 0.5f;
		to.x -= 0.25f;
		Vector3 to2 = vector - Vector3.up * 0.5f;
		to2.x += 0.25f;
		Gizmos.DrawLine(Vector3.zero, vector);
		Gizmos.DrawLine(vector, to);
		Gizmos.DrawLine(vector, to2);
		Gizmos.color = new Color(0.6f, 0.6f, 0.8f, 0.15f);
		Vector3 size = new Vector3(10f, 0.02f, 10f);
		Gizmos.DrawCube(Vector3.zero, size);
		Gizmos.color = new Color(0.6f, 0.6f, 0.8f, 1f);
		Gizmos.DrawWireCube(Vector3.zero, size);
		Gizmos.matrix = Matrix4x4.identity;
	}

	private void OnDrawGizmos()
	{
		if (VertList.Count >= 1 && (VertList.Count != 1 || DrawPreview))
		{
			Gizmos.color = DrawColor;
			for (int i = 1; i < VertList.Count; i++)
			{
				Gizmos.DrawLine(VertList[i - 1] + base.transform.position, VertList[i] + base.transform.position);
			}
			Vector3 vector = VertList[VertList.Count - 1];
			Gizmos.color = DrawColor;
			Gizmos.DrawLine(VertList[0] + base.transform.position, vector + base.transform.position);
			if (DisplayIcon != null && m_sceneTransition == null)
			{
				CalculateBounds();
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(GetIconWorldPosition(), 0.5f * IconScale);
			}
		}
	}

	public bool MouseInPolygon()
	{
		if (UINoClick.MouseOverUI || (bool)GameCursor.OverrideCharacterUnderCursor)
		{
			return false;
		}
		if (VertList.Count <= 2 || !UIWindowManager.MouseInputAvailable)
		{
			return false;
		}
		if (!FogOfWar.PointVisibleInFog(base.transform.position))
		{
			return false;
		}
		Ray ray = Camera.main.ScreenPointToRay(GameInput.MousePosition);
		Plane plane = new Plane(base.transform.up, base.transform.position);
		Vector3 vector = GameInput.WorldMousePosition;
		if (plane.Raycast(ray, out var enter))
		{
			vector = ray.origin + ray.direction * enter;
		}
		if (m_sceneTransition == null && (vector - Center).sqrMagnitude > UsableRadius * UsableRadius)
		{
			return false;
		}
		return PointInPolygon(vector);
	}

	public bool PointInPolygon(Vector3 point)
	{
		if (m_screenVerts == null)
		{
			return false;
		}
		if (m_recalculateVerts)
		{
			Vector3 position = base.transform.position;
			for (int i = 0; i < VertList.Count; i++)
			{
				Vector3 vector = VertList[i];
				Vector3 position2 = new Vector3(vector.x + position.x, vector.y + position.y, vector.z + position.z);
				m_screenVerts[i] = Camera.main.WorldToScreenPoint(position2);
			}
			m_recalculateVerts = false;
		}
		Vector3 vector2 = Camera.main.WorldToScreenPoint(point);
		float x = vector2.x;
		float y = vector2.y;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		num = 0;
		num2 = m_screenVerts.Length - 1;
		while (num < m_screenVerts.Length)
		{
			if (m_screenVerts[num].y > y != m_screenVerts[num2].y > y && x < (m_screenVerts[num2].x - m_screenVerts[num].x) * (y - m_screenVerts[num].y) / (m_screenVerts[num2].y - m_screenVerts[num].y) + m_screenVerts[num].x)
			{
				flag = !flag;
			}
			num2 = num++;
		}
		return flag;
	}

	public void CalculateBounds()
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Vector3 vert in VertList)
		{
			if (vert.x < vector.x)
			{
				vector.x = vert.x;
			}
			if (vert.x > vector2.x)
			{
				vector2.x = vert.x;
			}
			if (vert.y < vector.y)
			{
				vector.y = vert.y;
			}
			if (vert.y > vector2.y)
			{
				vector2.y = vert.y;
			}
			if (vert.z < vector.z)
			{
				vector.z = vert.z;
			}
			if (vert.z > vector2.z)
			{
				vector2.z = vert.z;
			}
		}
		vector2.y = Mathf.Max(vector2.y, 0.25f);
		bounds = new Bounds(base.transform.position, vector2 - vector);
	}
}

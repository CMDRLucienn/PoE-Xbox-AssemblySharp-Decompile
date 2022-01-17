using UnityEngine;

public class TrapTriggerGeneric : MonoBehaviour
{
	public CharacterDatabaseString DisplayText = new CharacterDatabaseString();

	public bool TrapActive = true;

	public float TrapDamage;

	public float DisplayTime;

	private bool m_mouseOver;

	private bool b_trapTriggered;

	private float f_trapDamage;

	private float f_messageTimer;

	private void Start()
	{
		base.gameObject.layer = LayerUtility.FindLayerValue("Dynamics");
	}

	private void Update()
	{
		if (m_mouseOver && GameInput.GetControlUp(MappedControl.INTERACT, handle: true))
		{
			f_messageTimer = DisplayTime;
		}
		if (f_messageTimer > 0f)
		{
			f_messageTimer -= Time.deltaTime;
		}
	}

	private void OnGUI()
	{
		if (f_messageTimer > 0f)
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
			gUIStyle.alignment = TextAnchor.MiddleCenter;
			gUIStyle.fontSize = 18;
			Vector3 vector = Camera.main.WorldToScreenPoint(base.transform.position);
			GUI.Label(new Rect(vector.x, (float)Camera.main.pixelHeight - vector.y, 100f, 200f), DisplayText.GetText(), gUIStyle);
		}
	}

	private void OnTriggerEnter(Collider victim)
	{
		if (!b_trapTriggered && victim.gameObject.GetComponent<Player>() != null)
		{
			f_trapDamage = TrapDamage * -1f;
			victim.gameObject.GetComponent<Health>().ApplyHealthChangeDirectly(f_trapDamage, applyIfDead: false);
			b_trapTriggered = true;
			TrapActive = false;
		}
	}

	private void OnMouseEnter()
	{
		if (FogOfWar.PointVisibleInFog(base.transform.position))
		{
			GameCursor.GenericUnderCursor = base.gameObject;
			m_mouseOver = true;
		}
	}

	private void OnMouseExit()
	{
		if (GameCursor.GenericUnderCursor == base.gameObject)
		{
			GameCursor.GenericUnderCursor = null;
		}
		m_mouseOver = false;
	}

	private void OnDrawGizmos()
	{
		if (!(GetComponent<Collider>() == null))
		{
			DrawUtility.DrawCollider(base.transform, GetComponent<Collider>(), Color.blue);
		}
	}
}

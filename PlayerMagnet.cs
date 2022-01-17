using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
	[Tooltip("Distance it finds the player from")]
	[Range(0f, 40f)]
	public float AttractDistance = 2f;

	[Tooltip("Ramp up speed in meters per second")]
	[Range(1f, 100f)]
	public float Acceleration = 5f;

	[Tooltip("Object move speed in meters per second")]
	[Range(1f, 100f)]
	public float MoveSpeed = 20f;

	private bool m_caught;

	private float m_currentSpeed;

	private bool m_done;

	private void Update()
	{
		if (!(GameState.s_playerCharacter == null) && !m_done)
		{
			if (m_caught)
			{
				MoveToPlayer();
			}
			if ((GameState.s_playerCharacter.transform.position - base.transform.position).sqrMagnitude < AttractDistance * AttractDistance)
			{
				m_caught = true;
			}
		}
	}

	private void MoveToPlayer()
	{
		Vector3 vector = GameState.s_playerCharacter.transform.position - base.transform.position;
		float magnitude = vector.magnitude;
		vector.Normalize();
		if (magnitude < 0.25f || magnitude < MoveSpeed * Time.deltaTime)
		{
			base.transform.position = GameState.s_playerCharacter.transform.position;
			base.transform.parent = GameState.s_playerCharacter.transform;
			m_done = true;
			GameUtilities.ShutDownLoopingEffect(base.gameObject);
			GameUtilities.Destroy(base.gameObject, 1f);
			return;
		}
		if (m_currentSpeed < MoveSpeed)
		{
			m_currentSpeed += Acceleration * Time.deltaTime;
			if (m_currentSpeed > MoveSpeed)
			{
				m_currentSpeed = MoveSpeed;
			}
		}
		base.transform.position = base.transform.position + vector * m_currentSpeed * Time.deltaTime;
	}
}

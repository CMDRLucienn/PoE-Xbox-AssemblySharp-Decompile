using UnityEngine;

public class CreateWallAttack : HazardAttack
{
	public float WallLength = 20f;

	private float length;

	private Vector3 placed_position;

	private Vector3 rightAngleVec;

	private BoxCollider box;

	private Quaternion rotation;

	private int trapID;

	private float runtime;

	private float spawntime_mod;

	private void StopAudio()
	{
		AudioBank component = GetComponent<AudioBank>();
		if (component != null)
		{
			component.enabled = false;
		}
		AudioSource[] components = GetComponents<AudioSource>();
		if (components == null)
		{
			return;
		}
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource != null)
			{
				audioSource.enabled = false;
				audioSource.Stop();
			}
		}
	}

	private void VolumeMod(float percVolume)
	{
		if (!(GetComponent<AudioBank>() != null))
		{
			return;
		}
		AudioSource[] components = GetComponents<AudioSource>();
		if (components == null)
		{
			return;
		}
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource != null)
			{
				audioSource.volume = percVolume;
			}
		}
	}

	private void StartAudio()
	{
		AudioBank component = GetComponent<AudioBank>();
		if (component != null)
		{
			component.enabled = true;
		}
		AudioSource[] components = GetComponents<AudioSource>();
		if (components == null)
		{
			return;
		}
		AudioSource[] array = components;
		foreach (AudioSource audioSource in array)
		{
			if (audioSource != null)
			{
				audioSource.enabled = false;
				audioSource.enabled = true;
				audioSource.transform.position = m_parent.transform.position;
			}
		}
	}

	public override void Update()
	{
		if (box != null)
		{
			if (length < WallLength / 2f)
			{
				if (!m_first_update)
				{
					Vector3 position = placed_position + rightAngleVec * length;
					Trap trap = HazardAttack.PlaceTrap(TrapPrefab, position, rotation, m_parent, trapID, GetComponent<Consumable>() != null);
					trap.SuppressTriggerBark = true;
					trap.IsWallTrap = true;
					length += box.size.x;
					spawntime_mod += Time.deltaTime;
				}
			}
			else
			{
				box = null;
			}
		}
		runtime += Time.deltaTime;
		float num = TrapPrefab.SelfDestructTime - runtime;
		if (num <= 0f)
		{
			if (num < 0f - spawntime_mod)
			{
				StopAudio();
			}
			else
			{
				VolumeMod(num / (0f - spawntime_mod));
			}
		}
		base.Update();
	}

	protected override void PlaceTrap(Vector3 position)
	{
		Transform transform = m_parent.transform;
		placed_position = position;
		if (transform.position == placed_position)
		{
			placed_position += transform.forward;
		}
		Vector3 lhs = transform.position - placed_position;
		rightAngleVec = Vector3.Cross(lhs, Vector3.up).normalized;
		box = TrapPrefab.GetComponent<BoxCollider>();
		if (box != null)
		{
			rotation = Quaternion.LookRotation((placed_position - transform.position).normalized);
			length = 0f - WallLength / 2f;
			Vector3 position2 = placed_position + rightAngleVec * length;
			trapID = HazardAttack.PlacedTrapID();
			Trap trap = HazardAttack.PlaceTrap(TrapPrefab, position2, rotation, m_parent, trapID, GetComponent<Consumable>() != null);
			trap.SuppressTriggerBark = true;
			trap.IsWallTrap = true;
			StartAudio();
			length += box.size.x;
		}
		runtime = 0f;
	}

	public void ComputeBounds(Vector3 position, out Vector3 left, out Vector3 right)
	{
		left = position;
		right = position;
		Vector3 normalized = Vector3.Cross(m_parent.transform.position - position, Vector3.up).normalized;
		if (TrapPrefab.GetComponent<BoxCollider>() != null)
		{
			float num = WallLength / 2f;
			left = position + normalized * (0f - num);
			right = position + normalized * num;
		}
	}

	protected override void OnDestroy()
	{
		StopAudio();
		base.OnDestroy();
	}

	protected override string GetAoeString(GenericAbility ability, GameObject character)
	{
		return GUIUtils.Format(1596, GUIUtils.Format(1533, WallLength.ToString("####0")));
	}
}

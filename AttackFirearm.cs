using UnityEngine;

[RequireComponent(typeof(AnimationController))]
[AddComponentMenu("Attacks/Firearm")]
public class AttackFirearm : AttackRanged
{
	public enum FirearmType
	{
		Pistol = 1,
		Rifle,
		Crossbow,
		Arbalest
	}

	public int ClipSize = 10;

	public float ReloadTime = 5f;

	public FirearmType ReloadAnim;

	protected int m_remainingAmmo = 10;

	public bool RequiresReload => m_remainingAmmo < ProjectileCount;

	public int RemainingShots
	{
		get
		{
			return m_remainingAmmo / ProjectileCount;
		}
		set
		{
			m_remainingAmmo = value * ProjectileCount;
		}
	}

	public float RemainingReloadTime { get; set; }

	protected override void Start()
	{
		base.Start();
		ClipSize = Mathf.Max(ClipSize, ProjectileCount);
		m_remainingAmmo = ClipSize;
		RemainingReloadTime = ReloadTime;
	}

	protected override void DecrementAmmo()
	{
		m_remainingAmmo -= ProjectileCount;
	}

	public override GameObject Launch(GameObject enemy, int variationOverride)
	{
		if (!RequiresReload)
		{
			base.Launch(enemy, variationOverride);
		}
		else
		{
			Debug.LogWarning(base.name + " out of ammo.");
		}
		return enemy;
	}

	public override void Launch(Vector3 location, GameObject enemy, int variationOverride)
	{
		if (!RequiresReload)
		{
			base.Launch(location, enemy, variationOverride);
		}
		else
		{
			Debug.LogWarning(base.name + " out of ammo.");
		}
	}

	public void Reload()
	{
		m_remainingAmmo = ClipSize;
		RemainingReloadTime = ReloadTime;
	}

	public override bool IsReady()
	{
		if (!RequiresReload)
		{
			return base.IsReady();
		}
		return false;
	}

	public bool BaseIsReady()
	{
		return base.IsReady();
	}
}

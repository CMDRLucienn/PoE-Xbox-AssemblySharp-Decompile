using UnityEngine;

public abstract class Usable : MonoBehaviour
{
	public enum UseAnimation
	{
		None,
		High,
		Low
	}

	public GameObject InteractionObject;

	public GameObject[] AdditionalInteractionObjects = new GameObject[0];

	public UseAnimation Animation;

	public abstract float UsableRadius { get; }

	public abstract float ArrivalRadius { get; }

	public abstract bool IsUsable { get; }

	public bool IsVisible
	{
		get
		{
			if (!(FogOfWar.Instance == null) && (!FogOfWarRender.Instance || !FogOfWarRender.Instance.gameObject || FogOfWarRender.Instance.gameObject.activeInHierarchy))
			{
				return FogOfWar.Instance.PointVisible(base.transform.position);
			}
			return true;
		}
	}

	public bool IsRevealed
	{
		get
		{
			if (!(FogOfWar.Instance == null) && (!FogOfWarRender.Instance || !FogOfWarRender.Instance.gameObject || FogOfWarRender.Instance.gameObject.activeInHierarchy))
			{
				return FogOfWar.Instance.PointRevealed(base.transform.position);
			}
			return true;
		}
	}

	public virtual bool HasInteractionObject
	{
		get
		{
			if (InteractionObject != null)
			{
				return true;
			}
			GameObject[] additionalInteractionObjects = AdditionalInteractionObjects;
			for (int i = 0; i < additionalInteractionObjects.Length; i++)
			{
				if (additionalInteractionObjects[i] != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public abstract bool Use(GameObject user);

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	protected void FireUseAudio()
	{
		AudioBank component = GetComponent<AudioBank>();
		if ((bool)component)
		{
			component.PlayFrom("Used");
		}
	}

	protected virtual void Start()
	{
		if (!GetComponent<HighlightCharacter>())
		{
			HighlightCharacter highlightCharacter = base.gameObject.AddComponent<HighlightCharacter>();
			if ((bool)GetComponent<Door>())
			{
				highlightCharacter.OccludedByScene = true;
			}
		}
	}

	public virtual Vector3 GetClosestInteractionPoint(Vector3 worldPos)
	{
		float num = float.MaxValue;
		Vector3 position = base.transform.position;
		if (InteractionObject != null)
		{
			num = GameUtilities.V3SqrDistance2D(InteractionObject.transform.position, worldPos);
			position = InteractionObject.transform.position;
		}
		GameObject[] additionalInteractionObjects = AdditionalInteractionObjects;
		foreach (GameObject gameObject in additionalInteractionObjects)
		{
			if (gameObject != null)
			{
				float num2 = GameUtilities.V3SqrDistance2D(gameObject.transform.position, worldPos);
				if (num2 < num)
				{
					num = num2;
					position = gameObject.transform.position;
				}
			}
		}
		return position;
	}

	public virtual void NotifyMouseOver(bool state)
	{
	}
}

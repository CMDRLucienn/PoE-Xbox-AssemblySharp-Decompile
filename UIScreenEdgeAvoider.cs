using UnityEngine;

public abstract class UIScreenEdgeAvoider : MonoBehaviour
{
	public UIAnchor Anchor;

	public virtual Bounds Bounds => new Bounds(base.transform.position, base.transform.lossyScale);

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}

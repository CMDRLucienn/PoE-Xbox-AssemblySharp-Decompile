using UnityEngine;

public class ObjectReplicator : MonoBehaviour
{
	public GameObject ObjectToReplicate;

	public int NumObjectsDesired;

	private void Start()
	{
		for (int i = 0; i < NumObjectsDesired - 1; i++)
		{
			GameObject obj = Object.Instantiate(ObjectToReplicate);
			obj.transform.parent = ObjectToReplicate.transform.parent;
			obj.transform.localScale = ObjectToReplicate.transform.localScale;
			obj.transform.localPosition = ObjectToReplicate.transform.localPosition;
			obj.transform.localRotation = Quaternion.identity;
		}
		GetComponent<UIGrid>().Reposition();
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}
}

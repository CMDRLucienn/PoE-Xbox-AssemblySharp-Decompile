using UnityEngine;

public class GibSpawner : MonoBehaviour
{
	public ObjectList GibList;

	public GameObject GibTrailEffect;

	public const int GIB_COUNT = 6;

	public float Scale = 1f;

	public void Spawn(bool destroy)
	{
		if (GameState.Mode.Option.GetOption(GameOption.BoolOption.GIBS))
		{
			int count = GibList.list.Count;
			for (int i = 0; i < 6; i++)
			{
				GameObject gameObject = Object.Instantiate(GibList.list[OEIRandom.Index(count)], Vector3.zero, Quaternion.identity) as GameObject;
				Transform transform = gameObject.transform;
				transform.position = base.transform.position + Vector3.down * (i - 3) * 0.35f;
				Vector3 force = new Vector3(Random.Range(-2, 3) * 30, Random.Range(1, 7) * 50, Random.Range(-2, 3) * 30);
				gameObject.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
				transform.localScale = new Vector3(Scale, Scale, Scale);
				if ((bool)GibTrailEffect)
				{
					Transform obj = Object.Instantiate(GibTrailEffect).transform;
					obj.position = transform.position;
					obj.rotation = transform.rotation;
					obj.parent = transform;
				}
				if (!Health.BloodyMess)
				{
					GameUtilities.Destroy(gameObject, 10f);
				}
			}
		}
		if (destroy)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}
}

using System;
using UnityEngine;

public class UIStrongholdCompanionBox : MonoBehaviour
{
	public UITexture Icon;

	public GameObject Selected;

	public GameObject Unavailable;

	public GameObject PartyMember { get; private set; }

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Icon.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClick));
	}

	private void OnChildClick(GameObject sender)
	{
		if (!Unavailable.activeSelf)
		{
			UIStrongholdCompanionPicker.Instance.Select(PartyMember);
		}
	}

	public void Select(GameObject select)
	{
		Selected.SetActive(select == PartyMember);
	}

	public void Load(GameObject companion)
	{
		StoredCharacterInfo component = companion.GetComponent<StoredCharacterInfo>();
		if (component == null)
		{
			Debug.LogError("UIStrongholdCompanionBox isn't using a stored companion!");
			return;
		}
		PartyMember = companion;
		Icon.mainTexture = component.SmallPortrait;
		Unavailable.SetActive(!UIStrongholdManager.Instance.Stronghold.IsAvailable(component));
	}
}

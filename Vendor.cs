using UnityEngine;

public class Vendor : MonoBehaviour
{
	public DatabaseString StoreName = new DatabaseString(DatabaseString.StringTableType.Interactables);

	public Texture2D StoreIcon;

	private void Open()
	{
		UIWindowManager.Instance.SuspendFor(UIStoreManager.Instance);
		UIStoreManager.Instance.Vendor = this;
		UIStoreManager.Instance.ShowWindow();
	}

	public void OpenAny()
	{
		if ((bool)GetComponent<Store>())
		{
			OpenStore();
		}
		else if ((bool)GetComponent<Inn>())
		{
			OpenInn();
		}
		else if ((bool)GetComponent<Recruiter>())
		{
			OpenRecruitment();
		}
	}

	public void OpenStore()
	{
		UIStoreManager.Instance.Page = UIStorePageType.Store;
		Open();
	}

	public void OpenStore(float buyRate, float sellRate)
	{
		Store component = GetComponent<Store>();
		if ((bool)component)
		{
			component.buyMultiplier = buyRate;
			component.sellMultiplier = sellRate;
		}
		OpenStore();
	}

	public void OpenInn()
	{
		UIStoreManager.Instance.Page = UIStorePageType.Inn;
		Open();
	}

	public void OpenInn(float rate)
	{
		Inn component = GetComponent<Inn>();
		if ((bool)component)
		{
			component.multiplier = rate;
		}
		OpenInn();
	}

	public void OpenRecruitment()
	{
		UIStoreManager.Instance.Page = UIStorePageType.Recruit;
		Open();
	}
}

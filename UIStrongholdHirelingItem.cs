using System;
using UnityEngine;

public class UIStrongholdHirelingItem : MonoBehaviour
{
	public UILabel NameLabel;

	public UILabel PrestigeLabel;

	public UILabel SecurityLabel;

	public UILabel PriceLabel;

	public GameObject Unpaid;

	public UITexture Texture;

	public UIMultiSpriteImageButton HireButton;

	public UIMultiSpriteImageButton DismissButton;

	public GameObject Hired;

	public GameObject Unhired;

	private StrongholdHireling m_Hireling;

	private bool m_Guest;

	public bool IsVisible { get; private set; }

	private void Start()
	{
		UIMultiSpriteImageButton hireButton = HireButton;
		hireButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(hireButton.onClick, new UIEventListener.VoidDelegate(OnHire));
		UIMultiSpriteImageButton dismissButton = DismissButton;
		dismissButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(dismissButton.onClick, new UIEventListener.VoidDelegate(OnDismiss));
		Stronghold stronghold = UIStrongholdManager.Instance.Stronghold;
		stronghold.OnHirelingStatusChanged = (Stronghold.HirelingStatusChanged)Delegate.Combine(stronghold.OnHirelingStatusChanged, new Stronghold.HirelingStatusChanged(OnStatusChanged));
	}

	private void OnStatusChanged(StrongholdHireling hireling)
	{
		if (hireling == m_Hireling)
		{
			Reload();
		}
	}

	private void OnHire(GameObject gameObject)
	{
		Stronghold.WhyCantHire whyCantHire = GameState.Stronghold.WhyCantHireHireling(m_Hireling);
		if (whyCantHire == Stronghold.WhyCantHire.NONE)
		{
			GameState.Stronghold.HireHireling(m_Hireling);
			return;
		}
		UIStrongholdManager.Instance.ShowMessage(StringUtility.Format(GUIUtils.GetWhyCantHireString(whyCantHire, CharacterStats.GetGender(m_Hireling.HirelingPrefab)), m_Hireling.Name));
	}

	private void OnDismiss(GameObject sender)
	{
		GameState.Stronghold.DismissHireling(m_Hireling);
	}

	public void Reload()
	{
		Set(m_Hireling, m_Guest);
	}

	public void SetGuest(StrongholdHireling hireling)
	{
		Set(hireling, guest: true);
	}

	public void SetStandard(StrongholdHireling hireling)
	{
		Set(hireling, guest: false);
	}

	private void Set(StrongholdHireling hireling, bool guest)
	{
		m_Hireling = hireling;
		m_Guest = guest;
		if (hireling == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		IsVisible = true;
		if (UIStrongholdManager.Instance.Stronghold.HasHireling(hireling))
		{
			Hired.SetActive(value: true);
			Unhired.SetActive(value: false);
			base.gameObject.SetActive(value: true);
		}
		else if (!UIStrongholdManager.Instance.Stronghold.CanSeeHireling(hireling))
		{
			IsVisible = false;
			base.gameObject.SetActive(value: false);
		}
		else if (m_Guest)
		{
			IsVisible = false;
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Hired.SetActive(value: false);
			Unhired.SetActive(value: true);
			base.gameObject.SetActive(value: true);
		}
		NameLabel.text = hireling.Name;
		PrestigeLabel.text = TextUtils.NumberBonus(hireling.PrestigeAdjustment);
		SecurityLabel.text = TextUtils.NumberBonus(hireling.SecurityAdjustment);
		if (m_Guest)
		{
			NameLabel.text += GUIUtils.Format(1731, GUIUtils.GetText(809, CharacterStats.GetGender(hireling.HirelingPrefab)));
		}
		PriceLabel.text = GUIUtils.Format(655, GUIUtils.Format(466, hireling.CostPerDay), GUIUtils.Format(466, UIStrongholdManager.Instance.Stronghold.CostToHire(hireling)));
		Unpaid.SetActive(!m_Hireling.Paid && UIStrongholdManager.Instance.Stronghold.HasHireling(hireling));
		if ((bool)Texture)
		{
			Texture.mainTexture = Portrait.GetTextureSmall(hireling.HirelingPrefab);
		}
	}
}

using System;
using UnityEngine;

public class UIStoreManager : UIHudWindow, ISelectACharacterMutable, ISelectACharacter
{
	private UIStorePageType m_Page;

	public UILabel StoreTitle;

	public UILabel PageLabel;

	public UIStoreInnPage InnPage;

	public UIStoreStorePage StorePage;

	public UIStoreRecruitPage RecruitPage;

	public UIStoreRespecPage RespecPage;

	public UIGrid LeftGrid;

	public UIMultiSpriteImageButton RecruitButton;

	public UIMultiSpriteImageButton InnButton;

	public UIMultiSpriteImageButton StoreButton;

	public UIMultiSpriteImageButton ManagePartyButton;

	public UIMultiSpriteImageButton RespecPartyButton;

	public UIGrid RightGrid;

	public UITexture[] StoreLogos;

	private Vendor m_Vendor;

	private bool m_PendingConfirmed;

	public static UIStoreManager Instance { get; private set; }

	public UIStorePageType Page
	{
		get
		{
			return m_Page;
		}
		set
		{
			m_Page = value;
			TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.STORE_SCREEN_OPENED);
			trigger.StoreTab = value;
			TutorialManager.STriggerTutorialsOfType(trigger);
			StorePage.gameObject.SetActive(m_Page == UIStorePageType.Store);
			InnPage.gameObject.SetActive(m_Page == UIStorePageType.Inn);
			RecruitPage.gameObject.SetActive(m_Page == UIStorePageType.Recruit);
			RespecPage.gameObject.SetActive(m_Page == UIStorePageType.Respec);
			if ((bool)Inn && Inn.IsPlayerOwned && m_Page == UIStorePageType.Inn)
			{
				PageLabel.text = GUIUtils.GetText(885);
			}
			else
			{
				PageLabel.text = GUIUtils.GetStoreTypeTitle(m_Page);
			}
			if (this.OnPageChanged != null)
			{
				this.OnPageChanged(m_Page);
			}
			RightGrid.Reposition();
		}
	}

	public CharacterStats SelectedCharacter
	{
		get
		{
			return UIGlobalSelectAPartyMember.Instance.SelectedCharacter;
		}
		set
		{
			UIGlobalSelectAPartyMember.Instance.SelectedCharacter = value;
		}
	}

	public Vendor Vendor
	{
		get
		{
			return m_Vendor;
		}
		set
		{
			m_Vendor = value;
			if ((bool)m_Vendor)
			{
				Store = m_Vendor.GetComponent<Store>();
				Inn = m_Vendor.GetComponent<Inn>();
				Recruiter = m_Vendor.GetComponent<Recruiter>();
				if ((bool)Store)
				{
					Store.NotifyOpened();
					Store.Sort(BaseInventory.CompareItemsForShop);
				}
				InnButton.gameObject.SetActive(Inn);
				StoreButton.gameObject.SetActive(Store);
				RecruitButton.gameObject.SetActive(Recruiter);
				ManagePartyButton.gameObject.SetActive(Recruiter);
				RespecPartyButton.gameObject.SetActive(value: true);
				LeftGrid.Reposition();
				LeftGrid.repositionNow = true;
				if (m_Vendor.StoreName != null && m_Vendor.StoreName.IsValidString)
				{
					StoreTitle.text = m_Vendor.StoreName.GetText();
				}
				else
				{
					StoreTitle.text = CharacterStats.Name(m_Vendor.gameObject);
				}
				UITexture[] storeLogos = StoreLogos;
				foreach (UITexture obj in storeLogos)
				{
					obj.mainTexture = m_Vendor.StoreIcon;
					obj.transform.parent.gameObject.SetActive(m_Vendor.StoreIcon != null);
				}
				if ((bool)Store)
				{
					StorePage.Set(Store);
				}
				if ((bool)Inn)
				{
					InnPage.Set(Inn);
				}
				if (HasContentForPage(Page))
				{
					Page = Page;
				}
				else if ((bool)Store)
				{
					Page = UIStorePageType.Store;
				}
				else if ((bool)Inn)
				{
					Page = UIStorePageType.Inn;
				}
				else if ((bool)Recruiter)
				{
					Page = UIStorePageType.Recruit;
				}
				else
				{
					Debug.LogError(string.Concat("Store UI error: no valid page for vendor '", m_Vendor, "'"));
				}
			}
			else
			{
				Store = null;
				Inn = null;
				Recruiter = null;
			}
		}
	}

	public Store Store { get; private set; }

	public Inn Inn { get; private set; }

	public Recruiter Recruiter { get; private set; }

	public event Action<UIStorePageType> OnPageChanged;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private bool HasContentForPage(UIStorePageType tab)
	{
		return tab switch
		{
			UIStorePageType.Inn => Inn, 
			UIStorePageType.Recruit => Recruiter, 
			UIStorePageType.Store => Store, 
			_ => false, 
		};
	}

	private void Awake()
	{
		Instance = this;
		UIGlobalSelectAPartyMember.Instance.OnSelectedCharacterChanged += OnGlobalSelectionChanged;
	}

	private void Start()
	{
		UIMultiSpriteImageButton managePartyButton = ManagePartyButton;
		managePartyButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(managePartyButton.onClick, new UIEventListener.VoidDelegate(OnManageParty));
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnGlobalSelectionChanged(CharacterStats character)
	{
		if (WindowActive())
		{
			SelectedCharacter = character;
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
		}
	}

	private void OnManageParty(GameObject sender)
	{
		UIWindowManager.Instance.SuspendFor(UIPartyManager.Instance);
		UIPartyManager.Instance.ToggleAlt();
	}

	private void OnConfirmClose(UIMessageBox.Result result, UIMessageBox sender)
	{
		if (result == UIMessageBox.Result.AFFIRMATIVE)
		{
			m_PendingConfirmed = true;
			HideWindow();
		}
	}

	public void CancelTransaction()
	{
		StorePage.CancelTransaction();
	}

	protected override void Show()
	{
		m_PendingConfirmed = false;
		StorePage.Show();
		if (this.OnPageChanged != null)
		{
			this.OnPageChanged(m_Page);
		}
	}

	protected override bool Hide(bool forced)
	{
		if (UIGlobalInventory.Instance.DraggingItem)
		{
			if (!forced)
			{
				return false;
			}
			UIGlobalInventory.Instance.CancelDrag();
		}
		if (!forced && !m_PendingConfirmed && StorePage.TradeIsPending)
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, "", GUIUtils.GetText(1769)).OnDialogEnd = OnConfirmClose;
			return false;
		}
		StorePage.CancelTransaction();
		UIInventoryFilterManager.ClearFilters();
		return base.Hide(forced);
	}
}

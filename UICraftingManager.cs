using System;
using UnityEngine;

public class UICraftingManager : UIHudWindow
{
	public UICraftingRecipeList RecipeList;

	public UICraftingWorkspace WorkspaceCrafting;

	public UIEnchantingWorkspace WorkspaceEnchanting;

	public UIMultiSpriteImageButton CraftButton;

	public UIMultiSpriteImageButton CancelButton;

	public UILabel TitleLabel;

	public UIWidget RecipesBackground;

	private Recipe m_CurrentRecipe;

	private Recipe m_LastLoadRecipe;

	private UICraftingWorkspace m_ActiveWorkspace;

	public UIIntegerBox CraftingAmt;

	public Color ColorInvalid;

	public Color ColorDisabled;

	public Color ColorEnchantEnabled;

	public Color ColorSelected;

	[HideInInspector]
	public bool EnchantMode;

	private Item m_EnchantTarget;

	public static UICraftingManager Instance { get; private set; }

	public string ForLocation { get; set; }

	public int RecipeAmount => CraftingAmt.Value;

	public int ItemAmount
	{
		get
		{
			if ((bool)m_CurrentRecipe)
			{
				if (m_CurrentRecipe.Output.Length != 0)
				{
					return m_CurrentRecipe.Output.Length * RecipeAmount;
				}
				return RecipeAmount;
			}
			return 0;
		}
	}

	public Item EnchantTarget
	{
		get
		{
			return m_EnchantTarget;
		}
		set
		{
			m_EnchantTarget = value;
			RefreshActiveWorkspace(null);
			RecipeList.Reload();
		}
	}

	private void Awake()
	{
		Instance = this;
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

	private void Start()
	{
		mInit();
		UIMultiSpriteImageButton craftButton = CraftButton;
		craftButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(craftButton.onClick, new UIEventListener.VoidDelegate(OnCraft));
		UIMultiSpriteImageButton cancelButton = CancelButton;
		cancelButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(cancelButton.onClick, new UIEventListener.VoidDelegate(OnCancel));
		CraftingAmt.OnValueChanged += OnAmountChanged;
		LoadRecipe(null);
	}

	private void mInit()
	{
	}

	private void OnAmountChanged(int amount)
	{
		if (WorkspaceCrafting.gameObject.activeInHierarchy)
		{
			WorkspaceCrafting.RefreshRecipeQty();
		}
	}

	protected override void Show()
	{
		mInit();
		CraftingAmt.gameObject.SetActive(!EnchantMode);
		WorkspaceCrafting.gameObject.SetActive(!EnchantMode);
		WorkspaceEnchanting.gameObject.SetActive(EnchantMode);
		if (EnchantMode)
		{
			m_ActiveWorkspace = WorkspaceEnchanting;
			CraftButton.Label.text = GUIUtils.GetText(1096);
			TitleLabel.text = GUIUtils.GetText(1097);
		}
		else
		{
			m_ActiveWorkspace = WorkspaceCrafting;
			CraftButton.Label.text = GUIUtils.GetText(592);
			TitleLabel.text = GUIUtils.GetText(39);
		}
		m_ActiveWorkspace.OutputText(string.Empty);
		RecipeList.Reload();
		RefreshActiveWorkspace(null);
	}

	private void RefreshActiveWorkspace(Recipe recipe)
	{
		if (m_ActiveWorkspace != null)
		{
			m_ActiveWorkspace.LoadRecipe(recipe);
		}
	}

	private InventoryItem MakeInventoryItem(Item baseItem)
	{
		return new InventoryItem(baseItem);
	}

	protected override bool Hide(bool forced)
	{
		EnchantTarget = null;
		ForLocation = "";
		return base.Hide(forced);
	}

	public override void HandleInput()
	{
		RecipeList.HandleInput();
	}

	private void OnCraft(GameObject sender)
	{
		if (!(m_CurrentRecipe != null))
		{
			return;
		}
		if (m_CurrentRecipe.ModifiableItem == Recipe.ModificationType.None)
		{
			if (m_CurrentRecipe.CanCreate(RecipeAmount))
			{
				for (int i = 0; i < RecipeAmount; i++)
				{
					m_CurrentRecipe.Create();
				}
				if (RecipeAmount > 0)
				{
					string msg = Console.Format(GUIUtils.GetTextWithLinks(1752), m_CurrentRecipe.DisplayName.GetText());
					m_ActiveWorkspace.OutputText(msg);
					Console.AddMessage(Console.Format(GUIUtils.GetTextWithLinks(1752), m_CurrentRecipe.DisplayName.GetText()));
				}
			}
			else
			{
				string msg2 = "";
				switch (Recipe.CantCreateReason)
				{
				case Recipe.WhyCantCreate.COST:
					msg2 = GUIUtils.Format(1333, ItemAmount, m_CurrentRecipe.DisplayName.GetText());
					break;
				case Recipe.WhyCantCreate.INGREDIENTS:
					msg2 = GUIUtils.Format(1332, ItemAmount, m_CurrentRecipe.DisplayName.GetText());
					break;
				case Recipe.WhyCantCreate.REQUIREMENTS:
					msg2 = GUIUtils.Format(1343);
					break;
				}
				m_ActiveWorkspace.OutputErrorText(msg2);
			}
		}
		else if ((bool)EnchantTarget)
		{
			if (m_CurrentRecipe.CanCreate(EnchantTarget))
			{
				m_CurrentRecipe.Create(EnchantTarget);
				m_ActiveWorkspace.OutputText(GUIUtils.Format(95, EnchantTarget.Name, m_CurrentRecipe.DisplayName));
			}
			else
			{
				m_ActiveWorkspace.OutputErrorText(GetRecipeFailReasonDescription());
			}
		}
		else
		{
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.OK, GUIUtils.GetText(39), GUIUtils.GetText(1336));
		}
		RecipeList.RefreshUsability();
		LoadRecipe(RecipeList.SelectedRecipe.Recipe);
	}

	private void OnCancel(GameObject sender)
	{
		HideWindow();
	}

	public void LoadRecipe(Recipe recipe)
	{
		CraftingAmt.Value = 1;
		if ((bool)recipe)
		{
			m_CurrentRecipe = recipe;
			RefreshActiveWorkspace(recipe);
			bool flag = recipe.CanCreate(EnchantTarget);
			CraftButton.enabled = flag;
			CraftButton.enabled = flag;
			if (!flag && m_LastLoadRecipe != recipe)
			{
				m_ActiveWorkspace.OutputErrorText(GetRecipeFailReasonDescription());
			}
			else if (m_LastLoadRecipe != recipe)
			{
				m_ActiveWorkspace.OutputText(string.Empty);
			}
		}
		else
		{
			RefreshActiveWorkspace(null);
			CraftButton.enabled = true;
			CraftButton.enabled = false;
		}
		m_LastLoadRecipe = recipe;
	}

	public static string GetCategorySpriteName(ItemMod.EnchantCategory cat)
	{
		return cat switch
		{
			ItemMod.EnchantCategory.Attributes => "MOD_attribute", 
			ItemMod.EnchantCategory.Lashing => "MOD_lash", 
			ItemMod.EnchantCategory.Proofing => "MOD_proofing", 
			ItemMod.EnchantCategory.Quality => "MOD_quality", 
			ItemMod.EnchantCategory.Slaying => "MOD_slaying", 
			ItemMod.EnchantCategory.WhiteForge => "MOD_white_forge", 
			_ => string.Empty, 
		};
	}

	private string GetRecipeFailReasonDescription()
	{
		string result = string.Empty;
		switch (Recipe.CantCreateReason)
		{
		case Recipe.WhyCantCreate.COST:
			result = GUIUtils.Format(1335, m_CurrentRecipe.DisplayName.GetText());
			break;
		case Recipe.WhyCantCreate.INGREDIENTS:
			result = GUIUtils.Format(1334, m_CurrentRecipe.DisplayName.GetText());
			break;
		case Recipe.WhyCantCreate.REQUIREMENTS:
			result = GUIUtils.Format(1343);
			break;
		case Recipe.WhyCantCreate.ALREADY_HAS_MOD:
			result = GUIUtils.Format(1339, EnchantTarget.Name, m_CurrentRecipe.DisplayName.GetText());
			break;
		case Recipe.WhyCantCreate.MAXIMUM_ENCHANTMENTS:
			result = GUIUtils.Format(1341, EnchantTarget.Name);
			break;
		case Recipe.WhyCantCreate.MAXIMUM_MOD_VALUE:
			result = GUIUtils.Format(1342, EnchantTarget.Name);
			break;
		case Recipe.WhyCantCreate.MAXIMUM_QUALITY_MODS:
			result = GUIUtils.Format(1340, EnchantTarget.Name);
			break;
		case Recipe.WhyCantCreate.NOT_MODDABLE:
			result = GUIUtils.Format(1337, m_CurrentRecipe.DisplayName.GetText(), EnchantTarget.Name);
			break;
		case Recipe.WhyCantCreate.EXISTING_QUALITY_MOD_IS_BETTER:
			result = GUIUtils.GetText(1097) + " " + GUIUtils.GetText(1425);
			break;
		}
		return result;
	}
}

public class UIStrongholdAdventureManager : UIHudWindow
{
	public UILabel TitleLabel;

	public UICapitularLabel IntroLabel;

	public UILabel ResolutionLabel;

	public UILabel RewardsLabel;

	public UIItemReadOnlyPopulator ItemRewards;

	public UITable Layout;

	public UIDraggablePanel DragPanel;

	public static UIStrongholdAdventureManager Instance { get; private set; }

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
	}

	protected override void Show()
	{
		DragPanel.ResetPosition();
		base.Show();
	}

	public void ShowAdventure(StrongholdAdventureCompletion adventureCompletion)
	{
		if (adventureCompletion.PremadeAdventureIndex < 0)
		{
			return;
		}
		StrongholdPremadeAdventure strongholdPremadeAdventure = Stronghold.Instance.PremadeAdventures.Adventures[adventureCompletion.PremadeAdventureIndex];
		TitleLabel.text = strongholdPremadeAdventure.Title.GetText();
		IntroLabel.text = strongholdPremadeAdventure.Description.GetText();
		ResolutionLabel.text = StringUtility.Format(strongholdPremadeAdventure.Resolution, adventureCompletion.AdventurerName);
		RewardsLabel.text = TextUtils.FuncJoin((string t) => "• " + t, adventureCompletion.RewardStrings, "\n");
		ItemRewards.Clear();
		for (int i = 0; i < strongholdPremadeAdventure.RewardList.Length; i++)
		{
			if (strongholdPremadeAdventure.RewardList[i].RewardType == StrongholdAdventure.RewardType.SpecificItem)
			{
				Item item = GameResources.LoadPrefab<Item>(strongholdPremadeAdventure.RewardList[i].SpecificItemName, instantiate: false);
				ItemRewards.AddItem(item, 1);
			}
		}
		Layout.Reposition();
		Layout.repositionNow = true;
		ShowWindow();
	}
}

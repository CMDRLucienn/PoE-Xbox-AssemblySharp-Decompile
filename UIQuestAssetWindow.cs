public class UIQuestAssetWindow : UIHudWindow
{
	public UIDynamicLoadTexture Texture;

	public static UIQuestAssetWindow Instance { get; private set; }

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

	public void LoadAsset(QuestAsset value)
	{
		Texture.SetPath(value.Image);
	}
}

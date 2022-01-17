using System;
using UnityEngine;

public class UIFormationsManager : UIHudWindow
{
	public UITexture DraggedGraphic;

	public GameObject DraggedObject;

	private UIClippedTexture m_clippedComp;

	public int[] StandardFormationNameIds;

	public UIFormationsTabs FormationTabs;

	public UIFormationsGrid FormationGrid;

	private int m_DraggedPartyMember = -1;

	public static UIFormationsManager Instance { get; private set; }

	public int NumStandardSets => StandardFormationNameIds.Length;

	public void BeginDrag(int dragged)
	{
		m_DraggedPartyMember = dragged;
		DraggedObject.SetActive(value: true);
		PartyMemberAI partyMemberAtFormationIndex = PartyMemberAI.GetPartyMemberAtFormationIndex(dragged);
		DraggedGraphic.mainTexture = Portrait.GetTextureSmall(partyMemberAtFormationIndex);
		if (m_clippedComp != null)
		{
			m_clippedComp.OnTextureChanged();
		}
	}

	private void Awake()
	{
		Instance = this;
		m_clippedComp = DraggedGraphic.gameObject.GetComponent<UIClippedTexture>();
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
		DraggedObject.SetActive(value: false);
		UIFormationsTabs formationTabs = FormationTabs;
		formationTabs.OnTabChanged = (UIFormationsTabs.TabChanged)Delegate.Combine(formationTabs.OnTabChanged, new UIFormationsTabs.TabChanged(OnTabChanged));
	}

	private void Update()
	{
		if (m_DraggedPartyMember >= 0)
		{
			Camera nGUICamera = InGameUILayout.NGUICamera;
			Vector3 vector = Window.transform.worldToLocalMatrix.MultiplyPoint3x4(nGUICamera.ScreenToWorldPoint(GameInput.MousePosition));
			DraggedObject.transform.localPosition = new Vector3(vector.x, vector.y, DraggedObject.transform.localPosition.z);
		}
		if (!GameInput.GetMouseButton(0, setHandled: false))
		{
			DraggedObject.SetActive(value: false);
		}
	}

	private void OnTabChanged(int index)
	{
		FormationGrid.LoadFormation(index + NumStandardSets);
	}

	public void Save()
	{
		FormationGrid.SetFormation(FormationTabs.CurrentTab + NumStandardSets);
	}

	public void ShowFormation(int formationIndex)
	{
		if (formationIndex >= NumStandardSets)
		{
			ShowWindow();
			FormationTabs.SetTab(formationIndex - NumStandardSets);
		}
	}

	protected override void Show()
	{
		FormationGrid.LoadFormation(FormationTabs.CurrentTab + NumStandardSets);
	}
}

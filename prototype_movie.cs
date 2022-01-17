using AI.Achievement;
using UnityEngine;

public class prototype_movie : MonoBehaviour
{
	public GameObject Wisp;

	public GameObject WispEffect;

	public GameObject Firefly;

	public GameObject Mist;

	public GameObject Water;

	public GameObject DayNight;

	public SyncCameraOrthoSettings Zoom;

	public float ZoomValue = 1f;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			StartSequenceOverview();
			InGameHUD.Instance.ShowHUD = false;
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			StartSequenceWater();
			InGameHUD.Instance.ShowHUD = false;
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			StartSequenceShadows();
			InGameHUD.Instance.ShowHUD = false;
		}
		if (Input.GetKeyDown(KeyCode.N))
		{
			StartSequenceDayNight();
			InGameHUD.Instance.ShowHUD = false;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			StartSequenceDynamicLights();
			InGameHUD.Instance.ShowHUD = false;
		}
		if (GetComponent<Animation>().isPlaying)
		{
			Zoom.SetZoomLevel(ZoomValue, force: false);
		}
	}

	public void StartWaterAnimation()
	{
		Water.GetComponent<Animation>().Play();
	}

	public void StartRunInAnimation()
	{
		Player s_playerCharacter = GameState.s_playerCharacter;
		if ((bool)s_playerCharacter)
		{
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			pathToPosition.Parameters.Destination = GameObject.Find("PlayerMoveA").transform.position;
			pathToPosition.Parameters.Range = 0.1f;
			pathToPosition.Parameters.MovementType = AnimationController.MovementType.Run;
		}
		Object[] array = Object.FindObjectsOfType(typeof(PartyMemberAI));
		int num = 0;
		Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (((PartyMemberAI)array2[i]).gameObject != s_playerCharacter.gameObject)
			{
				PathToPosition pathToPosition2 = AIStateManager.StatePool.Allocate<PathToPosition>();
				pathToPosition2.Parameters.Destination = GameObject.Find((num == 0) ? "NPCMoveA" : "NPCMoveB").transform.position;
				pathToPosition2.Parameters.Range = 0.1f;
				pathToPosition2.Parameters.MovementType = AnimationController.MovementType.Run;
				num++;
			}
		}
	}

	public void StartWalkAnimation()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
			pathToPosition.Parameters.Destination = GameObject.Find("PlayerMoveB").transform.position;
			pathToPosition.Parameters.Range = 0.1f;
			pathToPosition.Parameters.MovementType = AnimationController.MovementType.Walk;
		}
	}

	public void StartDayNightAnimation()
	{
		DayNight.GetComponent<Animation>().Play();
	}

	public void StartNighttimeThings()
	{
		Firefly.SetActive(value: true);
		Mist.SetActive(value: true);
	}

	public void StartWispAnimation()
	{
		Wisp.SetActive(value: true);
		Wisp.GetComponent<Animation>().Play();
	}

	public void TurnOnWisp()
	{
		WispEffect.SetActive(value: true);
	}

	private void StartSequenceOverview()
	{
		GetComponent<Animation>().Play("prototype_camera_a");
	}

	private void StartSequenceWater()
	{
		GetComponent<Animation>().Play("prototype_camera_b");
	}

	private void StartSequenceShadows()
	{
		GetComponent<Animation>().Play("prototype_camera_c");
	}

	private void StartSequenceDayNight()
	{
		GetComponent<Animation>().Play("prototype_camera");
		DayNight.GetComponent<Animation>().Play();
	}

	private void StartSequenceDynamicLights()
	{
		GetComponent<Animation>().Play("prototype_camera_d");
	}
}

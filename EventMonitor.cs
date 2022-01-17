using UnityEngine;

public class EventMonitor : MonoBehaviour
{
	public bool _OnEnable = true;

	public bool _OnDisable = true;

	public bool _Awake = true;

	public bool _Start = true;

	public bool _OnDestroy = true;

	public bool _Update;

	public bool _LateUpdate;

	public bool _OnMouseDown = true;

	public bool _OnMouseUp = true;

	public bool _OnMouseEnter = true;

	public bool _OnMouseExit = true;

	private void OnEnable()
	{
		if (_OnEnable)
		{
			Debug.Log(base.name + ": OnEnable");
		}
	}

	private void OnDisable()
	{
		if (_OnDisable)
		{
			Debug.Log(base.name + ": OnDisable");
		}
	}

	private void Awake()
	{
		if (_Awake)
		{
			Debug.Log(base.name + ": Awake");
		}
	}

	private void Start()
	{
		if (_Start)
		{
			Debug.Log(base.name + ": Start");
		}
	}

	private void OnDestroy()
	{
		if (_OnDestroy)
		{
			Debug.Log(base.name + ": OnDestroy");
		}
	}

	private void Update()
	{
		if (_Update)
		{
			Debug.Log(base.name + ": Update");
		}
	}

	private void LateUpdate()
	{
		if (_LateUpdate)
		{
			Debug.Log(base.name + ": LateUpdate");
		}
	}

	private void OnMouseDown()
	{
		if (_OnMouseDown)
		{
			Debug.Log(base.name + ": OnMouseDown");
		}
	}

	private void OnMouseUp()
	{
		if (_OnMouseUp)
		{
			Debug.Log(base.name + ": OnMouseUp");
		}
	}

	private void OnMouseEnter()
	{
		if (_OnMouseEnter)
		{
			Debug.Log(base.name + ": OnMouseEnter");
		}
	}

	private void OnMouseExit()
	{
		if (_OnMouseExit)
		{
			Debug.Log(base.name + ": OnMouseExit");
		}
	}
}

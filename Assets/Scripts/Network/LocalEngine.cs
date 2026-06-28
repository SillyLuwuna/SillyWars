#nullable enable

using UnityEngine;
using RtsEngine;
using System;
using System.Text;
using System.IO;

public class LocalEngine : MonoBehaviour
{
	public int ServerTps = 20;
	private const string _mapsPath = "Maps";

	private static LocalEngine? _instance = null;
	private static bool _awoken = false;

	private RtsEngine.RtsEngine? _engine = null!;

	public event EventHandler<WorldState>? TickEnded;
	public event Action? Started;
	public event Action? Stopped;

	private LocalEngine() { }

	public static LocalEngine Instance
	{
		get
		{
			if (!_awoken || _instance == null)
			{
				throw new MethodAccessException("Instance was not initialized yet");
			}

			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
		_awoken = true;

		Console.SetOut(new UnityTextWriter());
	}

    void Start()
    {
		if (this != _instance) return;
    }

	public void StartEngine(string stateFileName)
	{
		if (_engine != null) return;

		WorldState state = WorldState.Load($"{Environment.CurrentDirectory}/{_mapsPath}/{stateFileName}");

		_engine = RtsEngine.RtsEngine.StartInstance(state, ServerTps);
		_engine.TickEnded += OnTickEnded;
		_ = _engine.Start(isServer: false, useInternalClock: true);
		OnStarted();
	}

	public void StopEngine()
	{
		if (_engine == null) return;

		_engine.Stop();
		_engine = null;
		OnStopped();
	}

	public RtsEngine.RtsEngine? Engine => _engine;

	private void OnTickEnded(object? sender, WorldState state)
	{
		TickEnded?.Invoke(this, state);
	}

	private void OnStarted()
	{
		Started?.Invoke();
	}

	private void OnStopped()
	{
		Stopped?.Invoke();
	}

	public bool IsRunning => _engine != null;

	private class UnityTextWriter : TextWriter
	{
		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(string value)
		{
			Debug.Log(value);
		}

		public override void WriteLine(string value)
		{
			Debug.Log(value);
		}
	}

	void OnDisable()
	{
		_engine?.Stop();
	}

	void OnDestroy()
	{
		_engine?.Stop();
	}
}

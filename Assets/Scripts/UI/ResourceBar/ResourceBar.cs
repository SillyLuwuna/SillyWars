using RtsEngine;
using RtsEngine.Resources;
using RtsEngine.Units;
using UnityEngine;

public class ResourceBar : MonoBehaviour
{
	private const string _iconPath = "Tiny Swords/UI Elements/Icons";
	private const string _unitIconPath = "Tiny Swords/UI Elements/Human Avatars";

	[SerializeField] private Transform _resourcePrefab;
	[SerializeField] private Transform _resourceContainer;

	private bool _init = true;
	private ResourceBarUnit _goldDisplay;
	private ResourceBarUnit _workerDisplay;
	private ResourceBarUnit _knightDisplay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;
		NetworkClient.Instance.ConnectionLost += OnDisconnect;
		this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnNewState(object sender, WorldState state)
	{
		if (_init)
		{
			_init = false;
			this.gameObject.SetActive(true);
			ClearDisplays();
			InitResources();
		}

		UpdateResourceAmounts(state);
	}

	private void InitResources()
	{
		AssetLoader.Instance.LoadAssets(_iconPath);
		AssetLoader.Instance.LoadAssets(_unitIconPath);

		ColorVariant color = WorldStateManager.GetColorVariant(WorldStateManager.Instance.PlayerId);

		InitGoldDisplay();
		InitWorkerDisplay(color);
		InitKnightDisplay(color);
	}

	private void InitGoldDisplay()
	{
		GameObject display = Instantiate(_resourcePrefab.gameObject, _resourceContainer);
		_goldDisplay = display.GetComponent<ResourceBarUnit>();
		_goldDisplay.SetIcon(AssetLoader.Instance.GetSprite($"{_iconPath}/Coin"));
	}

	private void InitWorkerDisplay(ColorVariant color)
	{
		GameObject display = Instantiate(_resourcePrefab.gameObject, _resourceContainer);
		_workerDisplay = display.GetComponent<ResourceBarUnit>();
		_workerDisplay.SetIcon(AssetLoader.Instance.GetSprite($"{_unitIconPath}/{color} Worker"));
	}

	private void InitKnightDisplay(ColorVariant color)
	{
		GameObject display = Instantiate(_resourcePrefab.gameObject, _resourceContainer);
		_knightDisplay = display.GetComponent<ResourceBarUnit>();
		_knightDisplay.SetIcon(AssetLoader.Instance.GetSprite($"{_unitIconPath}/{color} Knight"));
	}

	private void UpdateResourceAmounts(WorldState state)
	{
		int numWorkers = 0;
		int numKnights = 0;
		foreach (BaseUnit unit in state.Units)
		{
			if (unit.OwnerId != state.PlayerVersion) continue;

			if (unit is Worker)
			{
				numWorkers++;
			}
			else if (unit is Knight)
			{
				numKnights++;
			}
		}

		_workerDisplay.SetAmount(numWorkers);
		_knightDisplay.SetAmount(numKnights);
		_goldDisplay.SetAmount(state.GetResource((uint)state.PlayerVersion, Resource.Gold));
	}

	private void ClearDisplays()
	{
		foreach (Transform child in _resourceContainer)
		{
			Destroy(child.gameObject);
		}
	}

	private void OnDisconnect()
	{
		_init = true;
		this.gameObject.SetActive(false);
	}
}

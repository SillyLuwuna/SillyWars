using System.Collections.Generic;
using RtsEngine;
using RtsEngine.Structures;
using RtsEngine.Units;
using UnityEngine;

public class TrainQueueManager : MonoBehaviour
{
	[SerializeField] private Transform _trainQueueItemPrefab;
	[SerializeField] private Transform _trainQueueItemHead;

	[SerializeField] private Transform _trainQueueContainer;

	private UnitProducer _structure = null;
	private TrainQueueHead _queueHead = null;
	private ColorVariant _color = ColorVariant.Invalid;


    void Start()
    {
		WorldStateManager.Instance.NewState += OnNewState;
		// Close();
    }

    void Update()
    {
    }

	private void OnNewState(object sender, WorldState state)
	{
		if (_structure == null) return;
		_structure = (UnitProducer)WorldStateManager.Instance.GetEntity(_structure.Id);
		if (_structure == null) return;

		UpdateUI();
	}

	public void Open(UnitProducer structure)
	{
		_trainQueueContainer.gameObject.SetActive(true);
		_structure = structure;
		_color = WorldStateManager.GetColorVariant(_structure.OwnerId);


		ClearTrainQueue();
		InitTrainQueue(structure);

		UpdateUI();
	}

	private void ClearTrainQueue()
	{
		foreach (Transform child in _trainQueueContainer)
		{
			Destroy(child.gameObject);
		}
	}

	private void InitTrainQueue(UnitProducer producer)
	{
		InitHead();
		InitItems(producer.MaxUnitProduction - 1);
	}

	private void InitItems(int items)
	{
		for (int i = 0; i < items; i++)
		{
			Instantiate(_trainQueueItemPrefab.gameObject, _trainQueueContainer);
		}
	}

	private void InitHead()
	{
		GameObject trainItem = Instantiate(_trainQueueItemHead.gameObject, _trainQueueContainer);
		_queueHead = trainItem.GetComponent<TrainQueueHead>();
	}

	public void Close()
	{
		_trainQueueContainer.gameObject.SetActive(false);
		_structure = null;
		_queueHead = null;
		_color = ColorVariant.Invalid;
	}

	private void UpdateUI()
	{
		_queueHead.UpdateUI(_structure);
		UpdateItems();
	}

	private void UpdateItems()
	{
		Queue<UnitType> productionQueue = _structure.ProductionQueue;

		foreach (Transform child in _trainQueueContainer)
		{
			TrainQueueItem queueItem = child.GetComponent<TrainQueueItem>();
			if (queueItem == null) continue;

			if (productionQueue.Count > 0)
			{
				UnitType currType = productionQueue.Dequeue();
				queueItem.UpdateUI(currType, _color);
			}
			else
			{
				queueItem.UpdateUI(null, ColorVariant.Invalid);
			}
		}
	}
}

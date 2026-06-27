using RtsEngine;
using RtsEngine.Math;
using RtsEngine.Structures;
using RtsEngine.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainQueueHead : MonoBehaviour
{
	private const string _unitPortraitsPath = "Tiny Swords/UI Elements/Human Avatars";

	[SerializeField] private Image _unitIcon;
	[SerializeField] private Transform _progressContainer;
	[SerializeField] private RectTransform _progressBar;
	[SerializeField] private TMP_Text _progressText;
	[SerializeField] private float _maxProgressBarWidth = 154;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		AssetLoader.Instance.LoadAssets(_unitPortraitsPath);
    }

    // Update is called once per frame
    void Update()
    {
    }

	public void UpdateUI(UnitProducer structure)
	{
		UnitType? head = structure.ProductionQueueHead;
		if (head == null)
		{
			_progressContainer.gameObject.SetActive(false);
			_unitIcon.sprite = null;
			_unitIcon.color = Color.clear;
			_progressText.text = "";
			SetProgressBarPercentage(0);
			return;
		}

		_progressContainer.gameObject.SetActive(true);

		int maxUnitProductionTicks = BaseUnit.FromUnitType(head.Value, structure.OwnerId, Vec2.Zero).ProductionTime;
		int ticksLeft = structure.TicksLeftForProduction;
		int ticksDone = maxUnitProductionTicks - ticksLeft;
		float progress = (float)ticksDone / (float)maxUnitProductionTicks;
		ColorVariant color = WorldStateManager.GetColorVariant(structure.OwnerId);

		_unitIcon.sprite = AssetLoader.Instance.GetSprite($"{_unitPortraitsPath}/{color} {head}");
		_unitIcon.color = Color.white;
		_progressText.text = $"{Mathf.Ceil((float)ticksLeft / (float)NetworkClient.SERVER_TPS)}s";

		SetProgressBarPercentage(progress);
	}

	private void SetProgressBarPercentage(float percentage)
	{
		Vector2 delta = _progressBar.sizeDelta;
		delta.x = _maxProgressBarWidth * percentage;
		_progressBar.sizeDelta = delta;
	}
}

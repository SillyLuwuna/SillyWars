using RtsEngine.Units;
using UnityEngine;
using UnityEngine.UI;

public class TrainQueueItem : MonoBehaviour
{
	private const string _unitPortraitsPath = "Tiny Swords/UI Elements/Human Avatars";

	[SerializeField] private static readonly Color _defaultColor = new Color(1f, 1f, 1f, 0.75f);

	[SerializeField] private Image _unitIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		AssetLoader.Instance.LoadAssets(_unitPortraitsPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void UpdateUI(UnitType? unitType, ColorVariant color)
	{
		if (unitType == null)
		{
			_unitIcon.sprite = null;
			_unitIcon.color = Color.clear;
			return;
		}

		_unitIcon.sprite = AssetLoader.Instance.GetSprite($"{_unitPortraitsPath}/{color} {unitType}");
		_unitIcon.color = _defaultColor;
	}
}

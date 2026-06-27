using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityActionButton : MonoBehaviour
{
	private const string _buttonsPath = "Tiny Swords/UI Elements/Buttons";
	private const string _iconsPath = "Tiny Swords/UI Elements/Icons";
	private const string _unitIconsPath = "Tiny Swords/UI Elements/Human Avatars";

	[SerializeField] private Image _icon;
	[SerializeField] private Image _buttonImage;

	public event EventHandler<EntityActionButton> Pressed;

	public EntityAction Action { get; private set; }
	private PlayerActionController _playerActionController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetEntityAction(PlayerActionController playerActionController, EntityAction action, ColorVariant color)
	{
		AssetLoader.Instance.LoadAssets(_buttonsPath);
		AssetLoader.Instance.LoadAssets(_iconsPath);
		AssetLoader.Instance.LoadAssets(_unitIconsPath);

		Action = action;
		_playerActionController = playerActionController;

		string iconPath = action switch
		{
			EntityAction.Mine => $"{_iconsPath}/Coin",
			EntityAction.Move => $"{_iconsPath}/Play",
			EntityAction.Build => $"{_iconsPath}/Hammer",
			EntityAction.Attack => $"{_iconsPath}/Sword",
			EntityAction.Repair => $"{_iconsPath}/Hammer",
			EntityAction.Halt => $"{_iconsPath}/Cancel",
			EntityAction.EnqueueWorker => $"{_unitIconsPath}/{color} Worker",
			EntityAction.EnqueueKnight => $"{_unitIconsPath}/{color} Knight",
			_ => "",
		};

		_icon.sprite = AssetLoader.Instance.GetSprite(iconPath);
	}

	public void OnPress()
	{
		switch (Action)
		{
			case EntityAction.Halt:
				_playerActionController.OnHaltInput();
				break;
			case EntityAction.Move:
				_playerActionController.IsWalkAttack = false;
				_playerActionController.BuildBarracks = false;
				_playerActionController.BuildCastle = false;
				break;
			case EntityAction.Attack:
				_playerActionController.IsWalkAttack = true;
				break;
			case EntityAction.EnqueueWorker:
				_playerActionController.OnEnqueueWorkerInput();
				break;
			case EntityAction.EnqueueKnight:
				_playerActionController.OnEnqueueKnightInput();
				break;
		}
		Pressed?.Invoke(this, this);
	}
}

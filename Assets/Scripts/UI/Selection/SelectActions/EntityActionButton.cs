using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityActionButton : MonoBehaviour
{
	private const string _buttonsPath = "Tiny Swords/UI Elements/Buttons";
	private const string _iconsPath = "Tiny Swords/UI Elements/Icons";

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

	public void SetEntityAction(PlayerActionController playerActionController, EntityAction action)
	{
		AssetLoader.Instance.LoadAssets(_buttonsPath);
		AssetLoader.Instance.LoadAssets(_iconsPath);

		Action = action;
		_playerActionController = playerActionController;

		string iconName = action switch
		{
			EntityAction.Mine => "Coin",
			EntityAction.Move => "Play",
			EntityAction.Build => "Hammer",
			EntityAction.Attack => "Sword",
			EntityAction.Repair => "Hammer",
			EntityAction.Halt => "Cancel",
			_ => "",
		};

		_icon.sprite = AssetLoader.Instance.GetSprite($"{_iconsPath}/{iconName}");
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
				_playerActionController.BuildBarracks = false;
				_playerActionController.BuildCastle = false;
				break;
		}
		Pressed?.Invoke(this, this);
	}
}

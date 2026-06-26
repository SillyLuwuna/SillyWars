using UnityEngine;
using UnityEngine.UI;

public class EntityActionButton : MonoBehaviour
{
	private const string _buttonsPath = "Tiny Swords/UI Elements/Buttons";
	private const string _iconsPath = "Tiny Swords/UI Elements/Icons";

	[SerializeField] private Image _icon;
	[SerializeField] private Image _buttonImage;

	private EntityAction _action;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		AssetLoader.Instance.LoadAssets(_buttonsPath);
		AssetLoader.Instance.LoadAssets(_iconsPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetEntityAction(EntityAction action)
	{
		_action = action;

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
		Debug.Log($"Pressed! {_action}");
	}
}

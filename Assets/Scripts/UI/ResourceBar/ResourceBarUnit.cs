using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarUnit : MonoBehaviour
{
	[SerializeField] private Image _icon;
	[SerializeField] private TMP_Text _text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetIcon(Sprite sprite)
	{
		_icon.sprite = sprite;
	}

	public void SetAmount(int amount)
	{
		_text.text = $"{amount}";
	}
}

using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
	[SerializeField] private MainMenu _mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Open()
	{
		this.gameObject.SetActive(true);
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
	}

	public bool IsOpen => this.gameObject.activeSelf;

	public void OnBackButtonPressed()
	{
		_mainMenu.Open();
		this.Close();
	}
}

using UnityEngine;

public class OutcomeScreen : MonoBehaviour
{
	[SerializeField] private GameplayMenu _gameplayMenu;

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

	public void OnExitButtonPressed()
	{
		_gameplayMenu.ReturnToMainMenu();
		this.Close();
	}
}

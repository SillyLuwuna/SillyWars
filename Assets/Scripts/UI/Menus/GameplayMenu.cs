using UnityEngine;

public class GameplayMenu : MonoBehaviour
{
	[SerializeField] private InputManager _inputManager;
	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private GameplayUI _gameplayUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		NetworkClient.Instance.ConnectionLost += OnConnectionLost;
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

	public void OnCloseButtonPressed()
	{
		this.Close();
	}

	public void OnExitButtonPressed()
	{
		if (NetworkClient.Instance.IsConnected)
		{
			NetworkClient.Instance.Disconnect();
			return;
		}

		ReturnToMainMenu();
	}

	private void ReturnToMainMenu()
	{
		_inputManager.InGameInputs = false;
		_mainMenu.Open();

		_gameplayUI.Close();
		this.Close();
	}

	public void OnConnectionLost()
	{
		if (!this.gameObject.activeSelf) return;

		ReturnToMainMenu();
	}
}

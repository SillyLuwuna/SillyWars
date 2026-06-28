using TMPro;
using UnityEngine;

public class NetworkMenu : MonoBehaviour
{
	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private TMP_InputField _serverIpInput;
	[SerializeField] private TMP_InputField _serverPortInput;
	[SerializeField] private TMP_Text _connectionErrorText;
	[SerializeField] private GameplayUI _gameplayUI;
	[SerializeField] private InputManager _inputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		NetworkClient.Instance.ConnectionEstablished += OnConnectionEstablished;
		NetworkClient.Instance.ConnectionLost += OnConnectionLost;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Open()
	{
		_connectionErrorText.text = "";
		_serverIpInput.text = "localhost";
		_serverPortInput.text = "13774";
		this.gameObject.SetActive(true);
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
	}

	public bool IsOpen => this.gameObject.activeSelf;

	public void OnConnectButtonPressed()
	{
		string ip = _serverIpInput.text;
		bool success = int.TryParse(_serverPortInput.text, out int port);

		if (!success)
		{
			_connectionErrorText.text = "invalid port";
			return;
		}

		NetworkClient.Instance.TryConnect(ip, port);
	}

	private void OnConnectionEstablished()
	{
		if (!this.gameObject.activeSelf) return;

		_inputManager.InGameInputs = true;
		_gameplayUI.Open();

		this.Close();
	}

	private void OnConnectionLost()
	{
		if (!this.gameObject.activeSelf) return;

		_connectionErrorText.text = "could not connect to server";
	}

	public void OnBackButtonPressed()
	{
		_mainMenu.Open();
		this.Close();
	}
}

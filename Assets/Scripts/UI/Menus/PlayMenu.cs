using UnityEngine;

public class PlayMenu : MonoBehaviour
{
	[SerializeField] private const string _easyMap = "map1.sstate";
	[SerializeField] private const string _mediumMap = "map2.sstate";
	[SerializeField] private const string _hardMap = "map3.sstate";

	[SerializeField] private MainMenu _mainMenu;
	[SerializeField] private GameplayUI _gameplayUI;
	[SerializeField] private InputManager _inputManager;

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

	public void OnEasyButtonPressed()
	{
		LocalEngine.Instance.StartEngine(_easyMap);

		if (!LocalEngine.Instance.IsRunning) return;

		OpenGameplay();
	}

	public void OnMediumButtonPressed()
	{
		LocalEngine.Instance.StartEngine(_easyMap);

		if (!LocalEngine.Instance.IsRunning) return;

		OpenGameplay();
	}

	public void OnHardButtonPressed()
	{
		LocalEngine.Instance.StartEngine(_easyMap);

		if (!LocalEngine.Instance.IsRunning) return;

		OpenGameplay();
	}

	private void OpenGameplay()
	{
		_inputManager.InGameInputs = true;
		_gameplayUI.Open();

		this.Close();
	}

	public void OnBackButtonPressed()
	{
		_mainMenu.Open();
		this.Close();
	}
}

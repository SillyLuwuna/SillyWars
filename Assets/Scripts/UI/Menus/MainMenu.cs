using UnityEngine;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private PlayMenu _playMenu;
	[SerializeField] private NetworkMenu _networkMenu;
	[SerializeField] private CreditsMenu _creditsMenu;
	[SerializeField] private InfoMenu _infoMenu;

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

	public void OnPlayButtonPressed()
	{
		_playMenu.Open();
		this.Close();
	}

	public void OnMultiplayerButtonPressed()
	{
		_networkMenu.Open();
		this.Close();
	}

	public void OnCreditsButtonPressed()
	{
		_creditsMenu.Open();
		this.Close();
	}

	public void OnQuitButtonPressed()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public void OnInfoButtonPressed()
	{
		_infoMenu.Open();
	}
}

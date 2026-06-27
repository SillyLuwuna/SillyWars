using UnityEngine;

public class ConnectButton : MonoBehaviour
{
	[SerializeField] GameObject selectionUI;

	public void OnButtonClickConnect()
	{
		NetworkClient.Instance.TryConnect();
		selectionUI.SetActive(true);
	}

	public void OnButtonClickDisconnect()
	{
		NetworkClient.Instance.Disconnect();
		selectionUI.SetActive(false);
	}
}

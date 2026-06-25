using UnityEngine;

public class ConnectButton : MonoBehaviour
{
	public void OnButtonClickConnect()
	{
		NetworkClient.Instance.TryConnect();
	}

	public void OnButtonClickDisconnect()
	{
		NetworkClient.Instance.Disconnect();
	}
}

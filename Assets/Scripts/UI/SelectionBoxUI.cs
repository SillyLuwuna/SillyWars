using UnityEngine;
using UnityEngine.UI;

public class SelectionBoxUI : MonoBehaviour
{
	[SerializeField] private RectTransform selectionBox;
	[SerializeField] private Image selectionBoxImage;
	[SerializeField] private Canvas canvas;

	private void Awake()
	{
		if (selectionBox == null)
		{
			selectionBox = GetComponent<RectTransform>();
		}

		if (selectionBoxImage == null)
		{
			selectionBoxImage = GetComponent<Image>();
		}

		SetBoxActive(false);
	}

	public void SetBoxActive(bool active)
	{
		selectionBox.gameObject.SetActive(active);
	}

	public void UpdateBox(Vector2 startScreenPos, Vector2 currentScreenPos)
	{
		Vector2 startCanvas = ScreenToCanvasPosition(startScreenPos);
        Vector2 currentCanvas = ScreenToCanvasPosition(currentScreenPos);
        
        Vector2 center = (startCanvas + currentCanvas) / 2f;
        Vector2 size = new Vector2(
            Mathf.Abs(startCanvas.x - currentCanvas.x),
            Mathf.Abs(startCanvas.y - currentCanvas.y)
        );
        
        selectionBox.anchoredPosition = center;
        selectionBox.sizeDelta = size;

		SetBoxActive(true);
	}

	private Vector2 ScreenToCanvasPosition(Vector2 screenPos)
    {
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvas.transform as RectTransform,
			screenPos,
			null,
			out Vector2 localPoint
		);
		return localPoint;
    }

	public void HideBox()
	{
		SetBoxActive(false);
	}
}

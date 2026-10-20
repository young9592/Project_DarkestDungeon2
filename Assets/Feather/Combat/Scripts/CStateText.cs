using TMPro;
using UnityEngine;

public class CStateText : MonoBehaviour
{
	#region Inspector

	[SerializeField] private TextMeshProUGUI textMesh;
	[SerializeField] private float _duration = 2f;
	[SerializeField] private float _speed = 30f;
	#endregion

	#region Field
	private RectTransform _rectTr;
    #endregion

    private void Awake()
    {
		_rectTr = GetComponent<RectTransform>();
		gameObject.SetActive(false);
    }

    private void Update()
    {
		Move();
    }
    public void SetText(Canvas canvas, Vector3 screenPos, string text)
	{
		RectTransform canvasRect = canvas.GetComponent<RectTransform>();
		Vector2 uiLocalPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out uiLocalPos);

		textMesh.text = text;
        _rectTr.anchoredPosition = uiLocalPos;

		gameObject.SetActive(true);
		Destroy(gameObject, _duration);
	}
	public void Move()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		_rectTr.anchoredPosition += Vector2.up * _speed * Time.deltaTime;
	}
}

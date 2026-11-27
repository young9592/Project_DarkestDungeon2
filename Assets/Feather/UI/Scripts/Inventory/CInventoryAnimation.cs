using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CInventoryAnimation : MonoBehaviour
{
    [SerializeField] RectTransform _inventoryMenu;

    [SerializeField] private float _startX = 226;
    [SerializeField] private float _targetX = -217f;
    [SerializeField] private float _moveDuration = 1.0f;

    private bool _isInn;
    private bool _isInventoryOpen = false;

    private void Start()
    {
        if (_isInn)
        {
            EnterInn();
        }
    }
    private void Update()
    {
        if (_isInn)
            return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_isInventoryOpen)
            {
                InventoryMenuExit();
                _isInventoryOpen = false;
            }
            else
            {
                InventoryMenuEnter();
                _isInventoryOpen = true;
            }
        }
    }
    public void SetInn(bool isInn)
    {
        _isInn = isInn;
    }
    private void EnterInn()
    {
        Vector2 targetPosition = new Vector2(_targetX, 2);
        _inventoryMenu.anchoredPosition = targetPosition;
    }
    private IEnumerator MoveInventoryMenuEnter()
    {
        float elapsedTime = 0f;

        Vector2 startPosition = new Vector2(_startX, 2);
        Vector2 targetPosition = new Vector2(_targetX, 2);

        _inventoryMenu.anchoredPosition = startPosition;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            _inventoryMenu.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }
        _inventoryMenu.anchoredPosition = targetPosition;

    }

    //Ã©ÅÍ ¸Þ´º ÅðÀå
    private IEnumerator MoveInventoryMenuExit()
    {
        float elapsedTime = 0f;

        Vector2 startPosition = new Vector2(_startX, 2);
        Vector2 targetPosition = new Vector2(_targetX, 2);

        _inventoryMenu.anchoredPosition = targetPosition;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            _inventoryMenu.anchoredPosition =
                Vector2.Lerp(targetPosition, startPosition, t);

            yield return null;
        }
        _inventoryMenu.anchoredPosition = startPosition;
    }

    //Ã©ÅÍ ÄÚ·çÆ¾ ½ÃÀÛ ÇÔ¼ö
    public void InventoryMenuEnter()
    {
        StartCoroutine(MoveInventoryMenuEnter());
    }

    //Ã©ÅÍ ÄÚ·çÆ¾ ÅðÀå
    public void InventoryMenuExit()
    {
        StartCoroutine(MoveInventoryMenuExit());
    }
}

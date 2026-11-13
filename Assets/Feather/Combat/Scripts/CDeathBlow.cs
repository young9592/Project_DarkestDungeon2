using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CDeathBlow : CIcon
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _iconImage;
    #endregion

    private void Awake()
    {
        if (_iconImage == null)
        {
            enabled = false;
            return;
        }

        _rectTr = GetComponent<RectTransform>();
    }

    public override void SetIcon(Canvas canvas, GameObject target)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;

        // 캔버스 랜더러 모드 체크
        if (!_init)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _rendererCameraMode = true;
            }
        }

        Vector3 screenPos = Vector3.zero;

        SkinnedMeshRenderer mesh = target.GetComponentInChildren<SkinnedMeshRenderer>();

        _rectTr.localScale = Vector3.one * 5f;
        screenPos = Camera.main.WorldToScreenPoint(mesh.bounds.center);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _rendererCameraMode ? Camera.main : null, out uiLocalPos);
        _rectTr.anchoredPosition = uiLocalPos;

        _init = true;

        StartCoroutine(CoAnimation());
    }

    private IEnumerator CoAnimation()
    {
        float delay = 1f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            _rectTr.localScale = Vector3.Lerp(_rectTr.localScale, Vector3.one, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}

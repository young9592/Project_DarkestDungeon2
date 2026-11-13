using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CStress : CIcon
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _iconImage;

    [Header("Data")]
    [SerializeField] private Material _shader;
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
        Vector2 headPos = new Vector2(mesh.bounds.center.x, mesh.bounds.max.y);
        screenPos = Camera.main.WorldToScreenPoint(headPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _rendererCameraMode ? Camera.main : null, out uiLocalPos);
        _rectTr.anchoredPosition = uiLocalPos;

        _init = true;

        StartCoroutine(CoAnimation());
    }

    private IEnumerator CoAnimation()
    {
        float delay = 0.5f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            _rectTr.localScale = Vector3.Lerp(_rectTr.localScale, Vector3.one, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        delay = 0.5f;
        nextTime = Time.time + delay;

        _iconImage.material = _shader;
        _iconImage.material.SetFloat("_prograss", 0);
        _iconImage.sprite = null;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            _rectTr.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, Mathf.Clamp01(2 * t));
            _iconImage.material.SetFloat("_prograss", t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
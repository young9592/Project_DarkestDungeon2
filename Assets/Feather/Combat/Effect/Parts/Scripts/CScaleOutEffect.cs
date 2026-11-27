using UnityEngine;
using UnityEngine.UI;

// 크기가 변경되면서 사라지는 연출
public class CScaleOutEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _desireScale = 1.2f;
    #endregion

    #region Field
    private float _nextTime;
    private Color _initColor;
    private Vector3 _initScale;
    #endregion

    private void Awake()
    {
        if (_image == null || _tr == null)
        {
            CPrint.Error($"{gameObject.name} : Effect Null Find");
            enabled = false;
            return;
        }

        _initColor = _image.color;
        _nextTime = Time.time + _delay;
        _initScale = _tr.localScale;
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);

        _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, t));
        _tr.localScale = Vector3.Lerp(_initScale, _desireScale * _initScale, t);
        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, Space.World);
    }
}

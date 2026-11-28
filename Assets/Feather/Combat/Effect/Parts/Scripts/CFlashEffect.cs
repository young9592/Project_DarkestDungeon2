using UnityEngine;
using UnityEngine.UI;

public class CFlashEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _desireScale = 1.2f;
    #endregion

    #region Field
    private Color _initColor;
    private float _nextTime;
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
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);
        _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, t));

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, Space.World);
        _tr.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _desireScale, 1 - t);
        _tr.rotation *= Quaternion.Euler(0, 0, _rotationSpeed);
    }
}

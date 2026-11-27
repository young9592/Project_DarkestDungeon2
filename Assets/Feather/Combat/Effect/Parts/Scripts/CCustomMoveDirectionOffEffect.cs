using UnityEngine;
using UnityEngine.UI;

public class CCustomMoveDirectionOffEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private Transform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private Vector3 _moveDir = Vector3.zero;
    #endregion

    #region Field
    private float _nextTime;
    private Color _initColor;

    #endregion

    private void Awake()
    {
        if (_image == null || _tr == null)
        {
            CPrint.Error($"{gameObject.name} : Effect Null Find");
            enabled = false;
            return;
        }

        _nextTime = Time.time + _delay;
        _initColor = _image.color;
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);

        _tr.Translate(_moveDir * _moveSpeed * Time.deltaTime, Space.Self);
        _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, t));
    }
}

using UnityEngine;
using UnityEngine.UI;

public class CDirectionMoveOff : CSkillEffectPart
{

    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _dirMoveSpeed = 0.3f;
    [SerializeField] private Vector3 _moveDir = Vector3.right;
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
        _tr.Translate(_moveDir * _dirMoveSpeed * Time.deltaTime, Space.Self);
    }
}

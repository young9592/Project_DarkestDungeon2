using UnityEngine;
using UnityEngine.UI;

public class CMuzzleEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private Space _space = Space.Self;
    [SerializeField] private float _customDelay = 0f;
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

        if (_customDelay == 0f)
        {
            _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, t));

        }
        else
        {
            if (t >= _customDelay)
            {
                float nextT = Mathf.Clamp01((t - _customDelay) / (1f - _customDelay));

                _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, nextT));

            }
            else
            {
                _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, 0);
            }
        }

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, _space);

    }
}

using UnityEngine;
using UnityEngine.UI;

public class CTrackingShardEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;



    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _customWaitDelay = 0.2f;
    [SerializeField] private Space _space = Space.World;
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
        _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, 0);
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, _space);

        if (t > _customWaitDelay && t < 0.8f)
        {
            float nextT = Mathf.Clamp01((t - _customWaitDelay) / (0.5f - _customWaitDelay));

            _image.color = _initColor;
            CPrint.Log(_initColor.a.ToString());
            _tr.localScale = Vector3.Lerp(new Vector3(0.1f, 1f, 1f), Vector3.one, nextT);
            _tr.Translate(Vector3.left * 0.2f * Time.deltaTime, Space.Self);
        }

        if (t >= 0.8f)
        {
            float lastT = Mathf.Clamp01((t - 0.8f) / 0.2f);

            _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, lastT));
        }
    }
}

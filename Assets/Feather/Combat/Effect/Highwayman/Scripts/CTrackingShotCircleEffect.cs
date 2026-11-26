using UnityEngine;
using UnityEngine.UI;

public class CTrackingShotCircleEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _customWaitDelay = 0.2f;
    [SerializeField] private float _scale = 1.2f;
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

        if (t > _customWaitDelay)
        {

            if (t < 0.8f)
            {
                _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, _initColor.a);
            }
            else
            {
                float nextT = Mathf.Clamp01((t - 0.8f) / 0.2f);

                _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, nextT));
                _tr.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _scale, nextT);
            }
        }

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, _space);
    }
}

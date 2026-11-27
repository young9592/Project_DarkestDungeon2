using UnityEngine;
using UnityEngine.UI;

public class CFlareHorizontalEffect02 : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;
    
    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _dirSpeed = 0.2f;
    // 카메라에 맞춰서 이동할 방향기준
    [SerializeField] private Space _cameraSpace = Space.World;
    // 이펙트가 이동할 방향기준
    [SerializeField] private Space _dirSpace = Space.Self;

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

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, _cameraSpace);
        _tr.Translate(Vector3.right * _dirSpeed * Time.deltaTime, _dirSpace);
    }
}

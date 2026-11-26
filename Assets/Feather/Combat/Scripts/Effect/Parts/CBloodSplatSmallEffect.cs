
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CBloodSplatSmallEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] private float _desireScale = 1.2f;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private float _bloodDirectionMoveSpeed = 0.1f;
    #endregion

    #region Field
    private float _nextTime;
    private Color _initColor;
    private Vector3 _initScale;
    #endregion

    private void Awake()
    {
        if (_image == null || _tr == null || _sprites == null || _sprites.Count == 0)
        {
            CPrint.Error($"{gameObject.name} : Effect Null Find");
            enabled = false;
            return;
        }

        _initColor = _image.color;
        _nextTime = Time.time + _delay;
        _initScale = _tr.localScale;
        _image.sprite = _sprites[Random.Range(0, _sprites.Count)];
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);
        if (t > 0.3f)
        {
            // 투명을 거의 끝나감과 동시에 사라지게 하기 위함
            float speedTime = Mathf.Clamp01((t - 0.3f) / 0.3f);
            _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, speedTime));
        }
        _tr.localScale = Vector3.Lerp(_initScale, _desireScale * _initScale, t);
        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, Space.World);
        _tr.Translate(Vector3.right * _bloodDirectionMoveSpeed * Time.deltaTime, Space.Self);
    }
}

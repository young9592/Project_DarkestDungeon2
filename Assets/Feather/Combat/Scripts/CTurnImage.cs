using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class CTurnImage : MonoBehaviour
{
    #region Inspector
    [Header("Turn Image")]
    [SerializeField] private Image _entityImage;
    [SerializeField] private Image _markerImage;

    [Header("Maker Color")]
    [SerializeField] private Color _heroMakerColor;
    [SerializeField] private Color _enemyMakerColor;

    [Header("Option")]
    // 오른쪽 상단 턴이 진행될 때 움직이는 애니메이션 진행 시간
    // 2초면 2초간 Lerp가 실행되면서 목표위치로 이동합니다.
    [SerializeField] private float _moveTime = 2f;
    #endregion

    #region Field
    private RectTransform _rectTr;
    private Coroutine _coroutine;
    #endregion

    #region Property
    public float MoveTime => _moveTime;
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
    }

    public void Init(Vector2 vec, CTempEntity.EntityType type, Sprite sprite)
    {
        _rectTr.anchoredPosition = vec;

        _entityImage.sprite = sprite;

        switch (type)
        {
            case CTempEntity.EntityType.Player:
                _markerImage.color = _heroMakerColor;
                break;
            case CTempEntity.EntityType.Enemy:
                _markerImage.color = _enemyMakerColor;
                break;
        }
    }

    // 보간작업 해야겠지?
    public void Move(Vector2 vec, bool direct)
    {
        // 즉발이동도 필요할거같아요 [ 엔티티가 여러마리 한방에 죽었을 때? ]
        if (direct)
        {
            _rectTr.anchoredPosition = vec;
        }

        if (_coroutine != null)
        {
            return;
        }

        _coroutine = StartCoroutine(CoLerpMove(vec));
    }

    private IEnumerator CoLerpMove(Vector2 vec)
    {
        float endTime = Time.time + _moveTime;

        while (endTime > Time.time)
        {
            _rectTr.anchoredPosition = Vector2.Lerp(_rectTr.anchoredPosition, vec, Mathf.Max(1f - (endTime - Time.time) / _moveTime));

            yield return null;
        }
        _coroutine = null;
    }
}

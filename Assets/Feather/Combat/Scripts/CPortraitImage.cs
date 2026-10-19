using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CPortraitImage : MonoBehaviour
{
    #region Inspector
    [Header("Turn Portrait Image")]
    [SerializeField] private Image _entityPortrait;
    [SerializeField] private Image _markerImage;

    [Header("Turnorder Pos")]
    // 턴오더 배치 위치
    [SerializeField] private List<Vector2> _turnOrderPos;

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
    private int _currentTurnorder = 0;
    #endregion

    #region Property
    public int CurrentTurnorder => _currentTurnorder;
    public float MoveTime => _moveTime;
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
    }
    // 초상화 초기화
    public void Initialize(int index, CTempEntity.EEntityType type, Sprite sprite)
    {
        index = Mathf.Clamp(index, 0, _turnOrderPos.Count - 1);

        _currentTurnorder = index;

        _rectTr.anchoredPosition = _turnOrderPos[index];

        _entityPortrait.sprite = sprite;

        switch (type)
        {
            case CTempEntity.EEntityType.Player:
                _markerImage.color = _heroMakerColor;
                break;
            case CTempEntity.EEntityType.Enemy:
                _markerImage.color = _enemyMakerColor;
                break;
        }
    }
    
    // 턴순서 초상화 이동
    public void Move(int index)
    {
        if (_coroutine != null)
        {
            return;
        }


        if (index < _turnOrderPos.Count - 1)
        {
            if (_currentTurnorder < index)
            {
                _rectTr.anchoredPosition = _turnOrderPos[index];
            }
            else if (_currentTurnorder == _turnOrderPos.Count - 1 && _currentTurnorder - index >= 2)
            {
                _rectTr.anchoredPosition = _turnOrderPos[index];
            }
            else
            {
                _coroutine = StartCoroutine(CoLerpMove(_turnOrderPos[index]));
            }
        }
        else
        {
            _rectTr.anchoredPosition = _turnOrderPos[_turnOrderPos.Count - 1];
        }

        _currentTurnorder = Mathf.Clamp(index, 0, _turnOrderPos.Count - 1);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CPortraitImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private CCombatManager _combatManager;

    [Header("Turn Entity Id")]
    [SerializeField] private int _id;

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
    [SerializeField] private float _moveTime = 1.5f;
    #endregion

    #region Field
    // 연결된 지시등
    private CIndicator _connectIndicator = null;
    private RectTransform _rectTr;
    private Coroutine _moveCoroutine;
    private int _prevTurnorder = 0;
    private int _currentTurnorder = 0;
    // 최신 상태인지 체크
    private static bool _isDirty = false;
    #endregion

    #region Property
    public float MoveTime => _moveTime;
    public static bool IsDirty { get { return _isDirty; } set { _isDirty = value; } }
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
    }
    // 초상화 초기화
    public void Initialize(CCombatManager combatManager, int id, int index, EEntityType entityType, Sprite sprite)
    {
        _combatManager = combatManager;

        _id = id;

        index = Mathf.Clamp(index, 0, _turnOrderPos.Count - 1);

        _currentTurnorder = index;
        _prevTurnorder = index;

        _rectTr.anchoredPosition = _turnOrderPos[index];

        _entityPortrait.sprite = sprite;

        switch (entityType)
        {
            case EEntityType.Hero:
                _markerImage.color = _heroMakerColor;
                break;
            case EEntityType.Enemy:
                _markerImage.color = _enemyMakerColor;
                break;
        }
    }

    // 턴순서 초상화 이동
    public void Move(int index)
    {
        // 코루틴이 돌고있는데 들어왔다는건 Move가 두번 호출된 상황인 것
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
            _currentTurnorder = _prevTurnorder;
        }

        if (index < _turnOrderPos.Count - 1)
        {
            if (_currentTurnorder < index)
            {
                _rectTr.anchoredPosition = _turnOrderPos[index];
            }
            else if (_currentTurnorder == _turnOrderPos.Count - 1 && _currentTurnorder - index >= 2)
            {
                _rectTr.anchoredPosition = _turnOrderPos[_turnOrderPos.Count - 2];
                _moveCoroutine = StartCoroutine(CoLerpMove(_turnOrderPos[index]));
            }
            else
            {
                _moveCoroutine = StartCoroutine(CoLerpMove(_turnOrderPos[index]));
            }
        }
        else
        {
            _rectTr.anchoredPosition = _turnOrderPos[_turnOrderPos.Count - 1];
        }

        // Move가 짧은시간에 두번 들어오는 경우를 위한 변수
        _prevTurnorder = _currentTurnorder;

        _currentTurnorder = Mathf.Clamp(index, 0, _turnOrderPos.Count - 1);
    }

    private IEnumerator CoLerpMove(Vector2 vec)
    {
        float nextTime = Time.time + _moveTime;

        while (nextTime > Time.time)
        {
            _rectTr.anchoredPosition = Vector2.Lerp(_rectTr.anchoredPosition, vec, Mathf.Clamp01(1f - (nextTime - Time.time) / _moveTime));

            yield return null;
        }
        _moveCoroutine = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_connectIndicator == null)
        {
            CEntity entity = null;

            // 동일한 개체인가?
            for (int i = 0; i < _combatManager.EntityList.Count; i++)
            {
                if (_id == _combatManager.EntityList[i].id)
                {
                    entity = _combatManager.EntityList[i].entity;
                }
            }

            _combatManager.EntitiesIndicator.TryGetValue(entity, out CIndicator indicator);

            _connectIndicator = indicator;
        }

        // 여전히 null인지 확인후 실행
        if (_connectIndicator != null)
        {
            _connectIndicator.EnterPortrait();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_connectIndicator == null)
        {
            CEntity entity = null;

            // 동일한 개체인가?
            for (int i = 0; i < _combatManager.EntityList.Count; i++)
            {
                if (_id == _combatManager.EntityList[i].id)
                {
                    entity = _combatManager.EntityList[i].entity;
                }
            }

            _combatManager.EntitiesIndicator.TryGetValue(entity, out CIndicator indicator);

            _connectIndicator = indicator;
        }

        // 여전히 null인지 확인후 실행
        if (_connectIndicator != null)
        {
            _connectIndicator.ExitPortrait();
        }
    }
}

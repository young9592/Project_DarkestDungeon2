using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CBasicCombatManager : MonoBehaviour
{
    public enum RoundState
    {
        None,
        Ready,
        Start
    }

    public enum EntityState
    {
        Success,
        Dead,
        Stun,
        Daze,
    }

    #region Inspector
    // 현재 필드에 서있는 엔티티 리스트
    [SerializeField] private List<CTempEntity> _fieldEntityList;
    // 현재 라운드 상태
    [SerializeField] private RoundState _state = RoundState.None;

    // 턴 초상화 이미지 프리팹과 보관 위치
    [Header("Turnorder Prefab")]
    [SerializeField] private GameObject _turnOrderPrefab;
    [SerializeField] private GameObject _turnOrderRoot;

    [Header("Option")]
    [SerializeField] private float _actionDelay = 3.5f;
    #endregion

    #region Field
    private List<(CTempEntity entity, int turnSpeed)> _turnList = new List<(CTempEntity, int)>();
    private Coroutine _coroutine = null;
    private string _message = "";
    private int _round = 0;

    // UI 가지고 있다가 삭제
    private List<GameObject> _turnPortraitList = new List<GameObject>();
    #endregion

    private void Update()
    {
        ChangeState();
        ActiveState();
    }

    private void ChangeState()
    {
        if (_state == RoundState.None || _state == RoundState.Start)
        {
            if (_turnList.Count == 0)
            {
                _state = RoundState.Ready;
            }
        }

        if (_state == RoundState.Ready)
        {
            if (_turnList.Count != 0)
            {
                _state = RoundState.Start;
            }
        }
    }
    private void ActiveState()
    {
        switch (_state)
        {
            case RoundState.Ready:
                BuildTurnOrderList();
                break;
            case RoundState.Start:
                ActionEntity();
                break;
        }
    }

    private void BuildTurnOrderList()
    {
        // 모션이 끝날때까지 기다리기
        if (_coroutine != null)
        {
            return;
        }

        for (int i = 0; i < _fieldEntityList.Count; i++)
        {
            // 임의 버프 디버프 걸어보기
            _fieldEntityList[i].TempInitState();

            // 보스 개체는 행동 횟수를 여러번 가집니다.
            for (int actions = 0; actions < _fieldEntityList[i].Actions; actions++)
            {
                _turnList.Add((_fieldEntityList[i], _fieldEntityList[i].Speed + Random.Range(0, 7)));
            }
        }

        // 선택정렬
        for (int i = 0; i < _turnList.Count - 1; i++)
        {
            (int index, int speed) max = (i, _turnList[i].turnSpeed);

            for (int j = i + 1; j < _turnList.Count; j++)
            {
                if (max.speed < _turnList[j].turnSpeed)
                {
                    // 최소나 최대를 찾아서 체크
                    max = (j, _turnList[j].turnSpeed);
                }
            }

            // 순서 변경
            (_turnList[i], _turnList[max.index]) = (_turnList[max.index], _turnList[i]);
        }

        // 턴 이미지 세팅 및 생성
        for (int i = 0; i < _turnList.Count; i++)
        {
            GameObject obj = Instantiate(_turnOrderPrefab, _turnOrderRoot.transform);

            CPortraitImage portraitImage = obj.GetComponent<CPortraitImage>();


            // 4 3 2 1 초상화 배치 및 이동
            if (i < 4)
            {
                portraitImage.Initialize(4, _turnList[i].entity.Type, _turnList[i].entity.PortraitImage);
                portraitImage.Move(i + 1);
            }
            else
            {
                portraitImage.Initialize(i + 1, _turnList[i].entity.Type, _turnList[i].entity.PortraitImage);
            }

            _turnPortraitList.Add(obj);
        }

        // Debug
        for (int i = 0; i < _turnList.Count; i++)
        {
            CPrint.Log($"index[{i}] :  [{_turnList[i].entity.name} / {_turnList[i].turnSpeed}]");
        }

        // 라운드 올리기
        _round++;
    }

    private void ActionEntity()
    {
        if (_coroutine != null)
        {
            return;
        }

        if (_turnList.Count == 0)
        {
            return;
        }

        // 행동하기전 상태 체크
        EntityState entityState = CheckState();

        // 상태에 맞는 행동 실행
        switch (entityState)
        {
            case EntityState.Success:
                _coroutine = StartCoroutine(CoAction());
                break;
            case EntityState.Dead:
                // TODO : 죽음 상태 코루틴 만들어야합니다.
                break;
            case EntityState.Stun:
                _coroutine = StartCoroutine(CoStun());
                break;
            case EntityState.Daze:
                _coroutine = StartCoroutine(CoDaze());
                break;
        }
    }

    private EntityState CheckState()
    {
        // TODO : DOT 데미지 들어오는거 체크 후 죽음 상태로 변경
        // 맨 우선순위인 이유는 DOT 데미지로 죽었을 시 다른 처리는 필요없기 때문입니다.

        if (false)
        {
            return EntityState.Dead;
        }

        bool isStuned = _turnList[0].entity.TokenManager.ConsumeToken(EEffectType.Stun);

        if (isStuned)
        {
            return EntityState.Stun;
        }

        bool isDazed = _turnList[0].entity.TokenManager.ConsumeToken(EEffectType.Daze);

        if (isDazed)
        {
            return EntityState.Daze;
        }

        return EntityState.Success;
    }

    // 기절 : 현재 라운드의 차례를 넘깁니다.
    private IEnumerator CoStun()
    {
        _message = $"{_turnList[0].entity.name} 기절";
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_actionDelay);
        MoveTurnOrder();
        Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
        _turnPortraitList.RemoveAt(0);
        _coroutine = null;
    }
    // 현기증 : 자신의 차례를 맨 뒤로 보냅니다. 2중첩시 기절로 변경됩니다.
    private IEnumerator CoDaze()
    {
        _message = $"{_turnList[0].entity.name} 현기증";
        _turnList.Add(_turnList[0]);
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_actionDelay);
        GameObject copyObj = Instantiate(_turnPortraitList[0], _turnOrderRoot.transform);
        _turnPortraitList.Add(copyObj);
        MoveTurnOrder();
        _turnPortraitList.RemoveAt(0);
        _coroutine = null;
    }
    // 행동
    // TODO : 추가행동은 주로 행동을 하고나서 추가행동이 가능한지 확인하는 느낌입니다. 변경필요
    private IEnumerator CoAction()
    {

        _message = $"{_turnList[0].entity.name}의 턴";

        bool doAction = false;

        // 액션할때까지 기다리기
        while (!doAction)
        {
            doAction = _turnList[0].entity.Action();
            yield return null;
        }

        // TODO : 행동 후 추가행동 확인이 필요. 로직수정 필요
        // 엔티티 상태에 따른 후처리
        if (_turnList[0].entity.HaveFreeAction)
        {
            _message = $"{_turnList[0].entity.name} 추가행동";
            _turnList[0].entity.UseFreeAction();
            yield return new WaitForSeconds(_actionDelay);
            _coroutine = null;
        }
        else
        {
            _message = $"{_turnList[0].entity.name} 행동";
            _turnList.RemoveAt(0);
            yield return new WaitForSeconds(_actionDelay);
            MoveTurnOrder();
            Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
            _turnPortraitList.RemoveAt(0);
            _coroutine = null;
        }
    }

    // TODO : 엔티티들끼리 상호작용하고 죽은 엔티티들을 제거해야합니다.
    private void CheckEntitiesDeath()
    {

    }

    private void MoveTurnOrder()
    {
        for (int i = 0; i < _turnPortraitList.Count; i++)
        {
            if (_turnPortraitList.Count - 1 < i)
            {
                break;
            }

            CPortraitImage portraitImage = _turnPortraitList[i].GetComponent<CPortraitImage>();

            portraitImage.Move(i);
        }
    }

    private void OnGUI()
    {
        GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
        gUIStyle.alignment = TextAnchor.MiddleRight;
        gUIStyle.fontSize = 50;
        gUIStyle.fontStyle = FontStyle.Bold;


        GUI.Label(new Rect(Screen.width - 700, 20, 300, 120), $"라운드 : {_round}", gUIStyle);

        if (_message != "")
        {
            GUI.color = Color.black;
            gUIStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.2f, Screen.width, 100), $"{_message}", gUIStyle);
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBasicCombatManager : MonoBehaviour
{
    public enum State
    {
        None,
        Ready,
        Active
    }

    #region Inspector
    [SerializeField] private List<CTempEntity> _entityList;
    [SerializeField] private State _state = State.None;

    // 턴 이미지 프리팹과 보관할 위치
    [Header("Turnorder Prefab")]
    [SerializeField] private GameObject _turnOrderPrefab;
    [SerializeField] private GameObject _turnOrderRoot;

    // 턴오더 배치 위치
    [Header("Turnorder Data")]
    [SerializeField] private List<Vector2> _turnOrderPos;

    [Header("Option")]
    [SerializeField] private float _subDelay = 0.5f;
    #endregion

    #region Field
    private List<(CTempEntity entity, int extraTurn)> _turnList = new List<(CTempEntity, int)>();
    private Coroutine _coroutine = null;
    private string _message = "";
    private int _round = 0;

    // UI 가지고 있다가 삭제
    private List<GameObject> _turnOrdertList = new List<GameObject>();
    #endregion

    private void Update()
    {
        ChangeState();
        ActiveState();
    }

    private void ChangeState()
    {
        if (_state == State.None || _state == State.Active)
        {
            if (_turnList.Count == 0)
            {
                _state = State.Ready;
            }
        }

        if (_state == State.Ready)
        {
            if (_turnList.Count != 0)
            {
                _state = State.Active;
            }
        }

    }
    private void ActiveState()
    {
        switch (_state)
        {
            case State.Ready:
                SetTurnList();
                break;
            case State.Active:
                ActiveEntity();
                break;
        }
    }


    private void SetTurnList()
    {
        // 모션이 끝날때까지 기다리기
        if (_coroutine != null)
        {
            return;
        }

        for (int i = 0; i < _entityList.Count; i++)
        {
            _entityList[i].SetRoundRandomSpeed();
            // 임의 버프 디버프 걸어보기
            _entityList[i].TempInitState();

            // 누군간 추가행동을 가지고 있습니다 [ 보스라던가 보스라던가 ]
            // 추가 행동마다 속도를 다르게 주고 해당 요소들을 모두 리스트에 넣겠습니다.
            for (int extraTurn = 0; extraTurn <= _entityList[i].ExtraTurn; extraTurn++)
            {
                _turnList.Add((_entityList[i], extraTurn));
            }
        }

        // 일단 선택정렬 [ 무려 시간복잡도 O^2 와! ]
        for (int i = 0; i < _turnList.Count - 1; i++)
        {
            (int index, int speed) max = (i, _turnList[i].entity.Speed(_turnList[i].extraTurn));

            for (int j = i + 1; j < _turnList.Count; j++)
            {
                if (max.speed < _turnList[j].entity.Speed(_turnList[j].extraTurn))
                {
                    // 최소나 최대를 찾아서 체크
                    max = (j, _turnList[j].entity.Speed(_turnList[j].extraTurn));
                }
            }

            // 순서 변경
            (_turnList[i], _turnList[max.index]) = (_turnList[max.index], _turnList[i]);
        }

        // 턴 이미지 세팅 및 생성
        for (int i = 0; i < _turnList.Count; i++)
        {
            GameObject obj = Instantiate(_turnOrderPrefab, _turnOrderRoot.transform);

            CTurnImage cTurnImage = obj.GetComponent<CTurnImage>();

            int posIdx = i > 3 ? 0 : 1;
            cTurnImage.Init(_turnOrderPos[posIdx], _turnList[i].entity.Type, _turnList[i].entity.EntityTurnIamge);

            if (i < 3)
            {
                cTurnImage.Move(_turnOrderPos[4 - i], false);
            }

            _turnOrdertList.Add(obj);
        }

        // Debug
        for (int i = 0; i < _turnList.Count; i++)
        {
            CPrint.Log($"index[{i}] :  {_turnList[i].entity.name}/{_turnList[i].entity.Speed(_turnList[i].extraTurn)}");
        }

        _round++;
    }

    private void ActiveEntity()
    {
        if (_coroutine != null)
        {
            return;
        }

        if (_turnList.Count == 0)
        {
            return;
        }

        _coroutine = StartCoroutine(CoActive());
    }

    private IEnumerator CoActive()
    { 
        CTempEntity.ActiveState activeState = CTempEntity.ActiveState.Wait;

        _message = $"{_turnList[0].entity.name}의 턴!";

        // 일단 동작만 시켜보기
        while (activeState == CTempEntity.ActiveState.Wait)
        {
            activeState = _turnList[0].entity.Active();
            yield return null;
        }




        switch (activeState)
        {
            case CTempEntity.ActiveState.Success:
                if (!_turnList[0].entity.FreeTurn)
                {
                    _message = $"{_turnList[0].entity.name} 행동!";
                    _turnList.RemoveAt(0);
                    yield return new WaitForSeconds(CTempEntity.ActiveDelay + _subDelay);
                    SetTurnOrderPos();
                    Destroy(_turnOrdertList[0], _turnOrdertList[0].GetComponent<CTurnImage>().MoveTime + _subDelay);
                    _turnOrdertList.RemoveAt(0);
                    _coroutine = null;
                }
                else
                {
                    _message = $"{_turnList[0].entity.name} 추가행동!";
                    _turnList[0].entity.UseFreeAction();
                    yield return new WaitForSeconds(CTempEntity.ActiveDelay + _subDelay);
                    _coroutine = null;
                    // 현재 추가턴만 할 수 있도록..
                    // 죽는 상황은 고려되지않음
                }
                break;
            case CTempEntity.ActiveState.Stun:
                _message = $"{_turnList[0].entity.name} 기절!";
                _turnList.RemoveAt(0);
                yield return new WaitForSeconds(CTempEntity.ActiveDelay + _subDelay);
                SetTurnOrderPos();
                Destroy(_turnOrdertList[0], _turnOrdertList[0].GetComponent<CTurnImage>().MoveTime + _subDelay);
                _turnOrdertList.RemoveAt(0);
                _coroutine = null;
                break;
            case CTempEntity.ActiveState.Daze:
                _message = $"{_turnList[0].entity.name} 현기증!";
                _turnList.Add(_turnList[0]);
                _turnList.RemoveAt(0);
                yield return new WaitForSeconds(CTempEntity.ActiveDelay + _subDelay);
                SetTurnOrderPos();
                _turnOrdertList.Add(_turnOrdertList[0]);
                _turnOrdertList.RemoveAt(0);
                _coroutine = null;
                break;
            case CTempEntity.ActiveState.Dead:
                // _turnOrdertList에서 죽은 개체들을 검사 후 제거하는 작업 필요
                break;
        }
    }



    private void SetTurnOrderPos()
    {
        for (int i = 0; i < _turnOrdertList.Count; i++)
        {
            if (_turnOrdertList.Count - 1 < i)
            {
                break;
            }
            CTurnImage cTurnImage = _turnOrdertList[i].GetComponent<CTurnImage>();

            if (i < 5)
            {
                cTurnImage.Move(_turnOrderPos[_turnOrderPos.Count - 1 - i], false);
            }
            else
            {
                cTurnImage.Move(_turnOrderPos[0], true);
            }

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

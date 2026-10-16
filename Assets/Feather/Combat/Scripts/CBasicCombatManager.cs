using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region 전투
/*
전투를 만들어야 합니다.

라운드당 한턴씩 행동할 수 있는듯하고 [ 물론 추가 행동이란게 있기도 합니다만 ㅡㅡ;; ]

그 라운드동안 행동할 수 있는 인원들을 행동시키는 느낌입니다.

이미지출력은 ㄴ형태로 나가는거 같습니다. 일단 지금은 일자로? ]

 */
#endregion

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

    // 턴오더 위치
    [Header("Turnorder Data")]
    [SerializeField] private List<Vector2> _turnOrderPos;
    #endregion

    #region Field
    private List<CTempEntity> _turnList = new List<CTempEntity>();
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
        // 공격 모션이 끝날때까지 기다리기
        if (_coroutine != null)
        {
            return;
        }

        for (int i = 0; i < _entityList.Count; i++)
        {
            _entityList[i].RoundRandomSpeed();
            _turnList.Add(_entityList[i]);
        }

        // 일단 선택정렬 [ 무려 시간복잡도 O^2 와! ]
        for (int i = 0; i < _turnList.Count - 1; i++)
        {
            (int index, int value) max = (i, _turnList[i].Speed);

            for (int j = i + 1; j < _turnList.Count; j++)
            {
                if (max.value < _turnList[j].Speed)
                {
                    // 최소나 최대를 찾아서 체크
                    max = (j, _turnList[j].Speed);
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
            cTurnImage.Init(_turnOrderPos[posIdx], _turnList[i].Type, _turnList[i].EntityTurnIamge);

            if (i < 3)
            {
                cTurnImage.Move(_turnOrderPos[4 - i], false);
            }

            _turnOrdertList.Add(obj);
        }

        // Debug
        for (int i = 0; i < _turnList.Count; i++)
        {
            CPrint.Log($"index[{i}] :  {_turnList[i].name}/{_turnList[i].Speed}");
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
        bool isActive = false;

        _message = $"{_turnList[0].name}의 턴!";

        // 일단 동작만 시켜보기
        while (!isActive)
        {
            if (_turnList[0].Attack())
            {
                isActive = true;
                _turnList.RemoveAt(0);
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(3.5f);
        _message = "";
        _coroutine = null;
        SetTurnOrderPos();
        Destroy(_turnOrdertList[0], _turnOrdertList[0].GetComponent<CTurnImage>().MoveTime + 1f);
        _turnOrdertList.RemoveAt(0);
    }

    private void SetTurnOrderPos()
    {
        for (int i = 0; i < 5; i++)
        {
            if(_turnOrdertList.Count - 1 < i)
            {
                break;
            }

            CTurnImage cTurnImage = _turnOrdertList[i].GetComponent<CTurnImage>();
            cTurnImage.Move(_turnOrderPos[_turnOrderPos.Count - 1 - i], false);
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

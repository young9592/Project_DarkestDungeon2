using UnityEngine;

public class CStageCoach : MonoBehaviour
{
    public enum ECoachState
    {
        None,
        Auto,
        Stop
    }

    #region Inspector
    [Header("StageCoach Connect")]
    [SerializeField] private Rigidbody _horse;

    
    [Header("StageCoach State")]
    [SerializeField] private ECoachState _coachState = ECoachState.None;
    [SerializeField] private float _currentSpeed = 0f; 

    [Header("Option")]
    [SerializeField] private float _maxSpeed = 10f;
    #endregion

    #region Field
    private Vector2 _input = Vector2.zero;
    private bool _connect = false;
    #endregion

    private void Awake()
    {
        if (CInputDispatcher.Instance != null && !_connect)
        {
            CInputDispatcher.Instance.OnMove += OnMoveHandler;
            CPrint.Log("디스패쳐와 연결 완료");
            _connect = true;
        }
    }

    private void Start()
    {
        _coachState = ECoachState.Stop;
    }

    private void OnEnable()
    {
        if (CInputDispatcher.Instance != null && !_connect)
        {
            CInputDispatcher.Instance.OnMove += OnMoveHandler;
            CPrint.Log("디스패쳐와 연결 완료");
            _connect = true;
        }
    }

    private void OnDisable()
    {
        CInputDispatcher.Instance.OnMove -= OnMoveHandler;
    }

    private void Update()
    {
        Vector3 dir = new Vector3(_input.x, 0, 0);

        if (_coachState == ECoachState.Auto)
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed + 0.1f, 0, _maxSpeed);
        }
        else if (_coachState == ECoachState.Stop)
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed - 0.1f, 0, _maxSpeed);
        }

        Vector3 moveVec = new Vector3(dir.x, 0, _currentSpeed);

        _horse.velocity = moveVec;
    }

    // 마차 이동
    private void OnMoveHandler(Vector2 vec)
    {
        if (_coachState == ECoachState.Auto)
        {
            if (vec.y < 0)
            {
                _coachState = ECoachState.Stop;
            }
        }
        else if(_coachState == ECoachState.Stop)
        {
            if (vec.y > 0)
            {
                _coachState = ECoachState.Auto;
            }
        }

        _input = vec;
    }
}

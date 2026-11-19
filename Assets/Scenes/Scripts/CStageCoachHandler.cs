using UnityEngine;

public class CStageCoach : MonoBehaviour
{
    public enum ECoachState
    {
        None,
        Walk,
        Run,
        Stop
    }

    #region Inspector
    [Header("Horse Rigidbody")]
    [SerializeField] private Rigidbody _leftHorse;
    [SerializeField] private Rigidbody _rightHorse;

    [Header("Body Rigidbody")]
    [SerializeField] private Rigidbody _body;

    [Header("Stair Rigidbody")]
    [SerializeField] private Rigidbody _stair;

    [Header("Horse Animator")]
    [SerializeField] private Animator _leftAnimator;
    [SerializeField] private Animator _rightAnimator;
    [SerializeField] private string _speed = "fSpeed";
    [SerializeField] private string _turnSpeed = "fTurnSpeed";

    [Header("StageCoach State")]
    [SerializeField] private ECoachState _coachState = ECoachState.None;
    [SerializeField] private float _currentSpeed = 0f;

    [Header("Option")]
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _rotatePower = 10f;

    [Header("Runtime Check")]
    [SerializeField] private int _pressCount;    // 걷기, 달리기 구분위한 함수
    #endregion

    #region Field
    private Vector2 _input = Vector2.zero;
    private bool _connect = false;

    private int _hashSpeed;
    private int _hashTurnSpeed;

    private Transform _leftTr;
    private Transform _rightTr;

    #endregion

    private void Awake()
    {
        // null check


        if (CInputDispatcher.Instance != null && !_connect)
        {
            CInputDispatcher.Instance.OnMove += OnMoveHandler;
            CPrint.Log("디스패쳐와 연결 완료");
            _connect = true;
        }

        _hashSpeed = Animator.StringToHash(_speed);
        _hashTurnSpeed = Animator.StringToHash(_turnSpeed);

        _leftTr = _leftHorse.gameObject.transform;
        _rightTr = _rightHorse.gameObject.transform;
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
        if (CInputDispatcher.Instance != null)
        {
            CInputDispatcher.Instance.OnMove -= OnMoveHandler;
        }
    }


    private void Update()
    {
        Vector3 moveVec = Vector3.zero;

        if (_coachState == ECoachState.Walk)
        {
            _currentSpeed = _maxSpeed * 0.4f;

            moveVec = new Vector3(0, 0, _currentSpeed);

            _leftHorse.velocity = moveVec;
            _rightHorse.velocity = moveVec;
        }
        else if (_coachState == ECoachState.Run)
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed + 0.05f, 0, _maxSpeed);

            moveVec = new Vector3(0, 0, _currentSpeed);

            _leftHorse.velocity = moveVec;
            _rightHorse.velocity = moveVec;
        }
        else if (_coachState == ECoachState.Stop)
        {
            _currentSpeed = 0;
            
            _body.velocity = new Vector3(_body.velocity.x * 0.9f, _body.velocity.y, _body.velocity.z * 0.9f);
        }

        float rot = _input.x * _rotatePower;

        if (_currentSpeed / _maxSpeed >= 0.3f)
        {
            _leftTr.position += new Vector3(rot, 0, 0);
            _rightTr.position += new Vector3(rot, 0, 0);
        }



        // Speed Animator
        float speedFloat = Mathf.Clamp01(_currentSpeed / _maxSpeed);
        _leftAnimator.SetFloat(_hashSpeed, speedFloat);
        _rightAnimator.SetFloat(_hashSpeed, speedFloat);

        // TurnSpeed Animator
        float turnSpeedFloat = 0;
        if (_input.x > 0)
        {
            turnSpeedFloat = 1;
        }
        else if (_input.x < 0)
        {
            turnSpeedFloat = -1;
        }

        _leftAnimator.SetFloat(_hashTurnSpeed, turnSpeedFloat);
        _rightAnimator.SetFloat(_hashTurnSpeed, turnSpeedFloat);
    }

    // 마차 이동
    private void OnMoveHandler(Vector2 vec)
    {
        if (_coachState == ECoachState.Walk || _coachState == ECoachState.Run)
        {
            if (vec.y < 0)
            {
                _coachState = ECoachState.Stop;
                _pressCount = 0;
            }
            else if (vec.y > 0)
            {
                _pressCount++;

                if (_pressCount == 1)
                {
                    _coachState = ECoachState.Walk;
                }
                else if (_pressCount > 1)
                {
                    _coachState = ECoachState.Run;
                }
            }
        }
        else if (_coachState == ECoachState.Stop)
        {
            if (vec.y > 0)
            {
                _pressCount++;

                if (_pressCount == 1)
                {
                    _coachState = ECoachState.Walk; 
                }
                else if(_pressCount > 1)
                {
                    _coachState = ECoachState.Run;
                }
            }
        }


            _input = vec;
    }
}

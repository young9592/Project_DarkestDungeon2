using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 먼저 생성해주세요.
[DefaultExecutionOrder(-100)]
public class CInputDispatcher : MonoBehaviour
{
    #region Inspector
    // 새로운 입력값을 참조
    [Header("InputAction")]
    [SerializeField] private InputActionReference _move;
    [SerializeField] private InputActionReference _skill;

    // 키 입력이 제대로 들어오는지 체크
    [SerializeField] private bool _keyInputLog = true;
    #endregion

    #region Field
    private static CInputDispatcher _instance;
    public static CInputDispatcher Instance => _instance;
    #endregion

    #region Event
    public event Action<Vector2> OnMove;
    public event Action<int> OnSkill;

    private bool _isReady = false;
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            CPrint.Warn($"{gameObject.name} : 중복 발견");
            Destroy(gameObject);
            return;
        }

        _instance = this;

        TryBind();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnBind();
        }

        _instance = null;
    }

    private void OnEnable()
    {
        TryBind();
        EnableAction(true);
    }

    private void OnDisable()
    {
        EnableAction(false);
    }

    private void TryBind()
    {
        if (_isReady)
        {
            return;
        }

        if (_move == null || _move.action == null)
        {
            return;
        }

        if(_skill == null || _skill.action == null)
        {
            return;
        }

        _move.action.performed += OnMovePerformed;
        _move.action.performed += OnMoveCanceled;
        _skill.action.performed += OnSkillSelected;

        _isReady = true;
        CPrint.Log("디스패쳐 바인딩");
    }

    private void UnBind()
    {
        if (!_isReady)
        {
            return;
        }

        if (_move == null || _move.action == null)
        {
            _move.action.performed -= OnMovePerformed;
            _move.action.performed -= OnMoveCanceled;
        }
        if (_skill == null || _skill.action == null)
        {
            _skill.action.performed -= OnSkillSelected;
        }
        _isReady = false;
        CPrint.Log("디스패쳐 언바인딩");
    }

    private void EnableAction(bool enable)
    {
        if (!_isReady)
        {
            return;
        }

        if (enable)
        {
            _move.action.Enable();
            _skill.action.Enable();
        }
        else
        {
            _move.action.Disable();
            _skill.action.Disable();
        }
    }

    // 마차이동
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        if (_keyInputLog)
        {
            CPrint.Log($"Input {v}");
        }

        OnMove?.Invoke(v);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        if (_keyInputLog)
        {
            CPrint.Log("Input (0,0)");
        }

        OnMove?.Invoke(Vector2.zero);
    }

    // 스킬 단축키
    private void OnSkillSelected(InputAction.CallbackContext ctx)
    {
        int.TryParse(ctx.control.name, out int selectNumber);
        OnSkill?.Invoke(selectNumber - 1);
    }
}

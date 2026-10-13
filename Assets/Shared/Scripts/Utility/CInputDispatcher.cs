using System;
using System.Collections;
using System.Collections.Generic;
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

    // 키 입력이 제대로 들어오는지 체크
    [SerializeField] private bool _keyInputLog = true;
    #endregion

    #region Field
    private static CInputDispatcher _instance;
    public static CInputDispatcher Instance => _instance;
    #endregion

    #region Event
    public event Action<Vector2> OnMove;

    private bool _isReady = false;
    #endregion

    private void Awake()
    {
        if(_instance != null && _instance != this)
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
        if(Instance == this)
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

        if(_move == null || _move.action == null)
        {
            return;
        }

        _move.action.performed += OnMovePerformed;
        _move.action.performed += OnMoveCanceled;

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

        _isReady = false;
    }

    private void EnableAction(bool enable)
    {
        if(!_isReady)
        {
            return;
        }

        if(enable)
        {
            _move.action.Enable();
        }
        else
        {
            _move.action.Disable();
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
        if(_keyInputLog)
        {
            CPrint.Log("Input (0,0)");
        }

        OnMove?.Invoke(Vector2.zero);
    }

    // 추가적으로 필요할때 등록


    // 미완성입니다. 구독과 해지를 하는걸 여기서 할려고합니다.
    public void SubscribeMove(Action<Vector2> callback)
    {
        OnMove += callback;
    }

    public void UnsubscribeMove(Action<Vector2> callback)
    {
        OnMove -= callback;
    }
}

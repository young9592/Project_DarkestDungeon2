using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CIndicator : MonoBehaviour
{
    #region Inspector
    [Header("Connect Object")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _target;

    [Header("Indicator Type")]
    [SerializeField] private GameObject _myturnIndicator;
    [SerializeField] private GameObject _buffIndicator;
    [SerializeField] private GameObject _targetIndicator;

    [Header("Light Type")]
    [SerializeField] private GameObject _heroFullLight;
    [SerializeField] private GameObject _heroSemiLight;
    [SerializeField] private GameObject _enemyFullLight;

    [Header("MouseEvent")]
    [SerializeField] private GameObject _mouseEvent;

    [Header("Option")]
    [SerializeField] private Vector3 _offset = new Vector3(0, -1, 0);
    #endregion

    #region Field
    private CHeroTargetView _view;
    private RectTransform _rectTr;
    private RectTransform _canvasRectTr;
    private bool _doFollow = true;
    private bool _init = false;
    private bool _rendererCameraMode = false;
    private int _formationSize = 1;
    private bool _rankHover = false;
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public bool isHighlight
    {
        get
        {
            if (_view != null)
            {
                return _view.IsHighlighted;
            }

            return false;
        }
    }
    #endregion

    private void Awake()
    {
        _checkList.Add(_myturnIndicator);
        _checkList.Add(_buffIndicator);
        _checkList.Add(_targetIndicator);
        _checkList.Add(_heroFullLight);
        _checkList.Add(_heroSemiLight);
        _checkList.Add(_enemyFullLight);

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }

        _rectTr = GetComponent<RectTransform>();
    }

    public void Initialize(Canvas canvas, GameObject target)
    {
        _canvas = canvas;
        _target = target;

        if (_canvas == null || _target == null)
        {
            CPrint.Warn($"{gameObject.name} : Initialize 값 전달 오류");
            return;
        }

        // 랜더링 모드 체크
        if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            _rendererCameraMode = true;
        }


        CEntity entity = _target.GetComponent<CEntity>();

        _canvasRectTr = _canvas.GetComponent<RectTransform>();

        bool isHero = entity is CPlayer;

        if (isHero)
        {
            _formationSize = 1;
        }
        else
        {
            _formationSize = (entity as CEnemy).EnemyData.FormationSize;

            if (_formationSize > 1)
            {
                _rectTr.localScale = new Vector3(_rectTr.localScale.x * _formationSize, _rectTr.localScale.y, _rectTr.localScale.z);
            }
        }

        _init = true;
    }

    private void Update()
    {
        if (!_init)
        {
            return;
        }

        CheckEntityDead();
        FollowEntity();
        ViewHover();
        RankChangeViewHover();
    }

    // 추적이동
    private void FollowEntity()
    {
        if (!_doFollow)
        {
            return;
        }

        if (_target == null)
        {
            return;
        }

        MoveState();
    }

    // 이동
    private void MoveState()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 targetVec = _target.transform.position;
        Vector3 bottomPos = _target.transform.position + _offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(bottomPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTr, screenPos, _rendererCameraMode ? Camera.main : null, out Vector2 uiLocalPos);

        _rectTr.anchoredPosition = uiLocalPos;
    }

    private void CheckEntityDead()
    {
        if (!_init)
        {
            return;
        }

        // 엔티티가 연결이 끊겼다.
        if (_target == null)
        {
            Destroy(gameObject);
        }
    }

    public void ViewMyturnIndicator()
    {
        _myturnIndicator.SetActive(true);
        _heroSemiLight.SetActive(true);

        _targetIndicator.SetActive(false);
        _buffIndicator.SetActive(false);
    }

    public void HideIndicator()
    {
        _myturnIndicator.SetActive(false);
        _buffIndicator.SetActive(false);
        _targetIndicator.SetActive(false);

        _heroFullLight.SetActive(false);
        _heroSemiLight.SetActive(false);
        _enemyFullLight.SetActive(false);
    }

    public void ViewBuffIndicator()
    {
        _buffIndicator.SetActive(true);
        _heroSemiLight.SetActive(true);

        _myturnIndicator.SetActive(false);
        _targetIndicator.SetActive(false);
    }

    public void ViewTargetIndicator()
    {
        _targetIndicator.SetActive(true);

        _buffIndicator.SetActive(false);
        _myturnIndicator.SetActive(false);

        _heroFullLight.SetActive(false);
        _heroSemiLight.SetActive(false);
    }

    public void SetView(CHeroTargetView view)
    {
        _view = view;
    }

    private void ViewHover()
    {
        if (_view == null)
        {
            return;
        }

        bool myTurnIndicator = _myturnIndicator.activeSelf;
        bool targetIndicator = _targetIndicator.activeSelf;
        bool buffIndicator = _buffIndicator.activeSelf;


        if (myTurnIndicator || buffIndicator)
        {
            if (_view.IsHovered)
            {
                _heroFullLight.SetActive(true);
                _heroSemiLight.SetActive(false);
            }
            else
            {
                _heroFullLight.SetActive(false);
                _heroSemiLight.SetActive(true);
            }
        }
        else if (targetIndicator)
        {
            if (_view.IsHovered)
            {
                _enemyFullLight.SetActive(true);
                _heroSemiLight.SetActive(false);
            }
            else
            {
                _enemyFullLight.SetActive(false);
                _heroSemiLight.SetActive(false);
            }
        }
    }
    public void EnableRankChange()
    {

    }

    public void DisableRankChange()
    {

    }

    // 자리 이동을 위한 함수
    private void RankChangeViewHover()
    {
        if (_view != null)
        {
            return;
        }

        bool myTurnIndicator = _myturnIndicator.activeSelf;
        bool buffIndicator = _buffIndicator.activeSelf;

        if (myTurnIndicator || buffIndicator)
        {
            if (_rankHover)
            {
                _heroFullLight.SetActive(true);
                _heroSemiLight.SetActive(false);
            }
            else
            {
                _heroFullLight.SetActive(false);
                _heroSemiLight.SetActive(true);
            }
        }
    }
}

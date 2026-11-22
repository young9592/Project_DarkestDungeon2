using UnityEngine;

public class CStageCoachFollowCamera : MonoBehaviour
{
    public enum ECinematicType
    {
        Default,
        Inn,
        Dungeon,
    }

    #region Inspector
    [Header("Camera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private ECinematicType _cinematicType;

    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Option")]
    [SerializeField] private Vector3 _thirdOffset = new Vector3(0, 10, -20);
    [SerializeField] private float _rotX = 15f;
    [SerializeField] private float _delay = 3f;
    #endregion

    #region Field
    private Transform _camTr;
    private Vector3 _innInit;
    private Vector3 _innDesire;
    private float _nextTime;
    private ESceneId _nextCustomDesire = ESceneId.Inn;
    #endregion
    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        _camTr = _camera.transform;

        if (CPrint.NullUnityObjectCheck(gameObject.name, _target))
        {
            enabled = false;
            return;
        }
    }


    private void Update()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
            _camTr = _camera.transform;
        }

        switch (_cinematicType)
        {
            case ECinematicType.Default:
                _camTr.position = new Vector3(0, 0, _target.position.z) + _thirdOffset;
                _camTr.rotation = Quaternion.Euler(_rotX, 0, 0);
                break;
            case ECinematicType.Inn:
                float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay );
                _camTr.position = Vector3.Lerp(_innInit, _innDesire, t);

                if (_nextTime > Time.time + 2f)
                {
                    CSceneFlowManager.Instance.LoadScene(_nextCustomDesire , CSceneFlowManager.EFadeType.Default);
                }
                break;
            case ECinematicType.Dungeon:
                break;
            default:
                break;
        }
    }

    public void ChangeCinematicType(ECinematicType cinematicType, ESceneId id)
    {
        if (_cinematicType == cinematicType)
        {
            return;
        }

        if (id != ESceneId.Inn)
        {
            _nextCustomDesire = id;
        }

        switch (cinematicType)
        {
            case ECinematicType.Default:
                break;
            case ECinematicType.Inn:
                _nextTime = Time.time + _delay;
                _innInit = _camTr.position;
                _innDesire = _camTr.position + new Vector3(0, 10, 0);
                break;
            case ECinematicType.Dungeon:
                break;
            default:
                break;
        }

        _cinematicType = cinematicType;
    }



}

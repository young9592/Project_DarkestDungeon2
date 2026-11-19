using UnityEngine;

public class CStageCoachFollowCamera : MonoBehaviour
{
    #region Inspector
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Option")]
    [SerializeField] private Vector3 _thirdOffset = new Vector3(0, 10, -20);
    [SerializeField] private float _rotX = 15f;
    #endregion

    #region Field
    private Transform _camTr;
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
        _camTr.position = new Vector3(0, 0, _target.position.z) + _thirdOffset;
        _camTr.rotation = Quaternion.Euler(_rotX, 0, 0);
    }
}

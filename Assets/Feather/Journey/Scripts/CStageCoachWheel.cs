using UnityEngine;

public class CStageCoachWheel : MonoBehaviour
{
    #region Inspector
    [Header("Connect Wheel")]
    [SerializeField] private WheelCollider _wheelFL;
    [SerializeField] private WheelCollider _wheelFR;
    [SerializeField] private WheelCollider _wheelBL;
    [SerializeField] private WheelCollider _wheelBR;
    [SerializeField] private Transform _trFL;
    [SerializeField] private Transform _trFR;
    [SerializeField] private Transform _trBL;
    [SerializeField] private Transform _trBR;
    #endregion

    private void Awake()
    {
        if (CPrint.NullUnityObjectCheck(gameObject.name, _wheelFL))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _wheelFR))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _wheelBL))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _wheelBR))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _trFL))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _trFR))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _trBL))
        {
            enabled = false;
            return;
        }
        if (CPrint.NullUnityObjectCheck(gameObject.name, _trBR))
        {
            enabled = false;
            return;
        }


    }

    private void FixedUpdate()
    {
        UpdateWheel(_wheelFL,  _trFL);
        UpdateWheel(_wheelFR,  _trFR);
        UpdateWheel(_wheelBL,  _trBL);
        UpdateWheel(_wheelBR,  _trBR);
    }

    private void UpdateWheel(WheelCollider wheel, Transform tr)
    {
        Vector3 position;
        Quaternion rotation;

        wheel.GetWorldPose(out position, out rotation);

        tr.position = position;
        tr.rotation = rotation;
    }
}

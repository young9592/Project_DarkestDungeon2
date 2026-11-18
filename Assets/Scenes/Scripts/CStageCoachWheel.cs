using UnityEngine;

public class CStageCoachWheel : MonoBehaviour
{
    #region Inspector
    [Header("Connect Wheel")]
    [SerializeField] private WheelCollider _wheel;
    [SerializeField] private Transform _tr;
    #endregion

    private void Awake()
    {
        if (CPrint.NullUnityObjectCheck(gameObject.name, _wheel))
        {
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        UpdateWheel(_wheel, _tr);
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

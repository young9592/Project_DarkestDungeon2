using UnityEngine;

public abstract class CIcon : MonoBehaviour
{
    public enum IconType
    {
        DeathBlow,
        Stress,
        StressHeal,
    }

    #region Field
    protected bool _init = false;
    protected bool _rendererCameraMode = false;
    protected RectTransform _rectTr;
    #endregion
    public abstract void SetIcon(Canvas canvas, GameObject target);
}

using System.Collections.Generic;
using UnityEngine;

public class CCombatCameraManager : MonoBehaviour
{
    #region Inspector
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _combatPanel;
    [SerializeField] private GameObject _leftHit;
    [SerializeField] private GameObject _rightHit;
    #endregion

    #region Field
    private Transform _cameraTr;
    private Vector3 _initVec;
    #endregion

    private void Awake()
    {
        if(_camera == null)
        {
            _camera = Camera.main;
        }            

        _cameraTr = _camera.transform;
        _initVec = _cameraTr.position;
    }

    public void ViewReset()
    {
        _cameraTr.position = _initVec;
    }
    public void ViewLeft()
    {

    }

    public void ViewRight()
    {

    }

    public void CombatZoomIn(List<CEntity> entities)
    {

    }

    public void MetlDownZoomIn(CPlayer hero)
    {

    }
}

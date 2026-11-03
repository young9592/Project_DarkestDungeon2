using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CCombatCameraManager : MonoBehaviour
{
    public enum CameraState
    {
        Default,
        Hero,
        Enemy
    }

    #region Inspector

    [Header("Connect")]
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private Camera _camera;

    #endregion

    #region Field
    private Transform _cameraTr;
    private Vector3 _initVec;
    private Coroutine _coroutine;
    //private bool _meltdownActionCompleted = false;
    private bool _meltdownCoroutineCompleted = false;
    #endregion

    #region Property
    public bool MeltdownCompleted => _meltdownCoroutineCompleted;
    #endregion

    private void Awake()
    {
        if (_camera == null)
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

    // 행동 시 카메라 연출
    public void CombatZoomIn(CEntity actor, IReadOnlyList<CEntity> entities, CSkillDataSO skillDataSO)
    {
        List<Vector3> initPos = new List<Vector3>();

        for (int i = 0; i < entities.Count; i++)
        {
            initPos.Add(entities[i].gameObject.transform.position);
        }


    }

    public void HeroMetlDown(CPlayer hero)
    {
        if (_coroutine != null)
        {
            return;
        }

        if (_meltdownCoroutineCompleted)
        {
            return;
        }

        _meltdownCoroutineCompleted = false;
        _coroutine = StartCoroutine(CoHeroTemptation(hero));
    }

    /*
     영상을 면밀히 관찰해보니 멘탈붕괴 애니메이션이 끝나기전에 돌아가는 것을 확인.
    private void OnMeltDownAction(CPlayer hero)
    {
        _meltdownActionCompleted = true;
    }
    */

    // 누군가의 의지가 시험받고 있습니다.
    public IEnumerator CoHeroTemptation(CPlayer hero)
    {
        // 시험 메세지 열기
        _combatUI.OpenTemptation(hero);

        // 시험 메세지 연출
        RectTransform tr = _combatUI.HeroTemptationTextTr;
        tr.localScale = Vector3.one;

        float delay = 3f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float dt = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            tr.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, dt);
            yield return null;
        }

        // TODO : 확률에 따라 영웅의 기상 혹은 붕괴 연출 나눠야합니다.
        _combatUI.OpenMeltDown();

        // 붕괴 연출
        _camera.orthographicSize = 1f;


        // 영웅 초기값 저장
        Vector3 initPos = hero.gameObject.transform.position;
        Quaternion initQuat = hero.gameObject.transform.rotation;
        Vector3 initScale = hero.gameObject.transform.localScale;

        Transform heroTr = hero.gameObject.transform;

        // 카메라 앞으로 영웅 끌어오기
        if (hero.HeroData.Id == "hero_man_at_arms")
        {
            heroTr.position = new Vector3(0, -1, 0);
        }
        else if (hero.HeroData.Id == "hero_highwayman")
        {
            heroTr.position = new Vector3(0.35f, -1.25f, 0);
        }
        else if (hero.HeroData.Id == "hero_grave_robber")
        {
            heroTr.position = new Vector3(0, -1.2f, 0);
        }
        else if(hero.HeroData.Id == "hero_plague_doctor")
        {
            heroTr.position = new Vector3(0.2f, -1.1f, 0);
        }


        // 임시 설정
        hero.ModifyStress(10);



        delay = 2f;
        nextTime = Time.time + delay;
        bool cameraBraveEffect = false;
        float bravePower = 5f;
        int tick = 0;

        Quaternion rot = initQuat * Quaternion.Euler(0, 0, 0.5f);
        Quaternion _cameraRot = _cameraTr.rotation;

        _combatUI.TogglePrefabRoot(false);

        while (nextTime > Time.time)
        {
            float dt = 1f - Mathf.Clamp01((nextTime - Time.time) / delay);
            tick++;

            // 연출
            heroTr.localRotation = Quaternion.Lerp(initQuat, rot, dt);
            heroTr.localScale = Vector3.Lerp(initScale, initScale * 1.03f, dt);

            if (tick > 30)
            {
                _cameraTr.rotation = Quaternion.Euler(cameraBraveEffect ? Random.Range(0, bravePower) / 100f : -Random.Range(0, bravePower) / 100f, 0, 0);
                cameraBraveEffect = !cameraBraveEffect;
                tick = Random.Range(0, 5);
            }

            yield return null;
        }

        _combatUI.TogglePrefabRoot(true);
        _cameraTr.rotation = _cameraRot;

        _combatUI.ResetCombatPanel();

        // 영웅 원래위치로 돌리기
        delay = 3f;
        nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float dt = 1f - Mathf.Clamp01((nextTime - Time.time) / delay);

            heroTr.position = Vector3.Lerp(heroTr.position, initPos, dt);
            heroTr.rotation = Quaternion.Lerp(heroTr.rotation, initQuat, dt);
            heroTr.localScale = Vector3.Lerp(heroTr.localScale, initScale, dt);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, 5f, dt);
            yield return null;
        }

        heroTr.position = initPos;
        heroTr.rotation = initQuat;
        heroTr.localScale = initScale;
        _camera.orthographicSize = 5f;

        _meltdownCoroutineCompleted = true;
        _coroutine = null;
    }
}
